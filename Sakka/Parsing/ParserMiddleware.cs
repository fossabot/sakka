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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace Sakka.Parsing
{
    public class ParserMiddleware
    {
        private readonly ILogger<ParserMiddleware> _logger;

        public ParserMiddleware(ILogger<ParserMiddleware> logger)
        {
            _logger = logger;
        }

        public Func<Context, Func<Task>, Task> Parser()
        {
            return async (context, next) =>
            {
                if (context.CallbackQuery != null)
                {
                    var callbackQueryArgs = context.CallbackQuery.Data.Split(' ');
                    context.MiddlewareData.Add("callbackQueryArgs", callbackQueryArgs);

                    _logger.LogDebug($"{callbackQueryArgs.Length} arguments parsed from callback query");
                }

                if (context.Message.Type == MessageType.TextMessage)
                {
                    var messageArgs = context.Message.Text.Split(' ');
                    context.MiddlewareData.Add("messageArgs", messageArgs);

                    _logger.LogDebug($"{messageArgs.Length} arguments parsed from message");
                }

                await next();
            };
        }
    }
}
