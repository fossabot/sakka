// Copyright 2018 Victorique Ko
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

namespace Sakka
{
    public class Client
    {
        private readonly ILogger<Client> _logger;
        private readonly IList<Func<RequestDelegate, RequestDelegate>> _components = new List<Func<RequestDelegate, RequestDelegate>>();

        private ITelegramBotClient _client;
        private string _username;
        private RequestDelegate _application;

        public Client(ILogger<Client> logger)
        {
            _logger = logger;
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
            
            if (config["token"] == null)
            {
                _logger.LogCritical("You have to specifies the token of your Telegram bot.");
                Environment.Exit(1);
            }

            _logger.LogTrace($"Token: {config["token"]}");
            _logger.LogTrace($"Proxy: {config["proxy"]}");

            var proxy = config["proxy"] != null ? new WebProxy(config["proxy"]) : null;
            _client = new TelegramBotClient(config["token"], proxy);

            _logger.LogDebug("Getting username...");

            var me = _client.GetMeAsync();
            me.Wait();
            _username = me.Result.Username;

            _logger.LogDebug("username got");
            _logger.LogTrace($"Username: {_username}");

            _logger.LogDebug("Composing components...");

            Task App(Context context) => Task.CompletedTask;
            _application = _components.Reverse().Aggregate((RequestDelegate) App, (current, component) => component(current));

            _logger.LogDebug($"{_components.Count + 1} components composed");

            _client.OnMessage += MessageReceived;
            _client.StartReceiving();

            _logger.LogInformation("Started");
        }

        public async Task SendTextAsync(ChatId chatId, string text, int replyToMessageId = 0, bool isMarkdown = false)
        {
            _logger.LogDebug("Sending text message...");

            await _client.SendChatActionAsync(chatId, ChatAction.Typing);
            var msg = await _client.SendTextMessageAsync(
                chatId,
                text,
                isMarkdown ? ParseMode.Markdown : ParseMode.Default,
                replyToMessageId: replyToMessageId);

            _logger.LogDebug("Message sent");
            _logger.LogTrace($"<< {msg.Chat.Id} {msg.From.Id} {msg.MessageId} {msg.Type} {msg.Text}");
        }

        public async Task SendPhotoAsync(ChatId chatId, Uri uri)
        {
            _logger.LogDebug("Sending photo message...");

            await _client.SendChatActionAsync(chatId, ChatAction.UploadPhoto);
            var photo = new FileToSend(uri);
            var msg = await _client.SendPhotoAsync(chatId, photo);

            _logger.LogDebug("Message sent");
            _logger.LogTrace($"<< {msg.Chat.Id} {msg.From.Id} {msg.MessageId} {msg.Type}");
        }

        private void MessageReceived(object sender, MessageEventArgs e)
        {
            var msg = e.Message;

            _logger.LogDebug("Message received");
            _logger.LogTrace($">> {msg.Chat.Id} {msg.From.Id} {msg.MessageId} {msg.Type} {msg.Text}");

            var context = new Context(msg);
            _application(context);
        }
    }
}
