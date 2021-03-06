﻿// Copyright 2018 Victorique Ko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sakka
{
    public class Client
    {
        private readonly ILogger<Client> _logger;
        private readonly IList<Func<MessageDelegate, MessageDelegate>> _components;
        private ITelegramBotClient _client;
        private MessageDelegate _application;

        public Client(ILogger<Client> logger)
        {
            _logger = logger;
            _components = new List<Func<MessageDelegate, MessageDelegate>>();
        }

        public Client Use(Func<Context, Func<Task>, Task> middleware)
        {
            _logger.LogDebug("Adding middleware...");

            _components.Add(next =>
            {
                return context =>
                {
                    return middleware(context, () => next(context));
                };
            });

            _logger.LogDebug("Middleware added");

            return this;
        }

        public void Run(IConfigurationRoot config)
        {
            _logger.LogInformation("Starting...");

            _logger.LogTrace($"Token: {config["token"]}");
            _logger.LogTrace($"Proxy: {config["proxy"]}");
            
            _client = new TelegramBotClient(config["token"],
                config["proxy"] != null ? new WebProxy(config["proxy"]) : null);

            _logger.LogDebug("Composing components...");

            Task App(Context context) => Task.CompletedTask;
            _application = _components
                .Reverse()
                .Aggregate((MessageDelegate) App, (current, component) => component(current));

            _logger.LogDebug($"{_components.Count + 1} components composed");

            _client.OnMessage += MessageReceived;
            _client.OnCallbackQuery += CallbackQueryReceived;
            _client.StartReceiving();

            _logger.LogInformation("Started");
        }

        public async Task SendTextAsync(ChatId chatId, string text,
            ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false, bool disableNotification = false,
            int replyToMessageId = 0, IReplyMarkup replyMarkup = null)
        {
            _logger.LogDebug("Sending text message...");

            await _client.SendChatActionAsync(chatId, ChatAction.Typing);
            var message = await _client.SendTextMessageAsync(chatId, text,
                parseMode, disableWebPagePreview, disableNotification,
                replyToMessageId, replyMarkup);

            _logger.LogDebug("Message sent");
            _logger.LogTrace($"<< {message.Chat.Id} {message.From.Id} {message.MessageId} {message.Type} {message.Text}");
        }

        public async Task SendPhotoAsync(ChatId chatId, FileToSend photo,
            string caption = "",
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null)
        {
            _logger.LogDebug("Sending photo message...");

            await _client.SendChatActionAsync(chatId, ChatAction.UploadPhoto);
            var message = await _client.SendPhotoAsync(chatId, photo,
                caption, disableNotification, replyToMessageId, replyMarkup);

            _logger.LogDebug("Message sent");
            _logger.LogTrace($"<< {message.Chat.Id} {message.From.Id} {message.MessageId} {message.Type}");
        }

        public async Task SendDocumentAsync(ChatId chatId, FileToSend document,
            string caption = "",
            bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null)
        {
            _logger.LogDebug("Sending document message...");

            await _client.SendChatActionAsync(chatId, ChatAction.UploadDocument);
            var message = await _client.SendDocumentAsync(chatId, document,
                caption, disableNotification, replyToMessageId, replyMarkup);

            _logger.LogDebug("Message sent");
            _logger.LogTrace($"<< {message.Chat.Id} {message.From.Id} {message.MessageId} {message.Type}");
        }

        public async Task EditTextAsync(ChatId chatId, int messageId, string text,
            ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false, IReplyMarkup replyMarkup = null)
        {
            _logger.LogDebug("Editing text message...");

            var message = await _client.EditMessageTextAsync(chatId, messageId, text,
                parseMode, disableWebPagePreview, replyMarkup);

            _logger.LogDebug("Message Edited");
            _logger.LogTrace($"<< {message.Chat.Id} {message.From.Id} {message.MessageId} {message.Type} {message.Text}");
        }

        private void MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            _logger.LogDebug("Message received");
            _logger.LogTrace($">> {message.Chat.Id} {message.From.Id} {message.MessageId} {message.Type} {message.Text}");

            var context = new Context(message);
            _application(context);
        }

        private void CallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            var callbackQuery = e.CallbackQuery;

            _logger.LogDebug("Callback query received");
            _logger.LogTrace($">> {callbackQuery.Message.Chat.Id} {callbackQuery.From.Id} {callbackQuery.Message.MessageId} CallbackQuery {callbackQuery.Data}");

            var context = new Context(callbackQuery);
            _application(context);
        }
    }
}
