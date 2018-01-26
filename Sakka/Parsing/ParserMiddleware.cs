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
                if (context.Request.Type != MessageType.TextMessage)
                {
                    return;
                }

                var args = context.Request.Text.Split(' ');
                context.MiddlewareData.Add("args", args);

                _logger.LogDebug($"{args.Length} arguments parsed");

                await next();
            };
        }
    }
}
