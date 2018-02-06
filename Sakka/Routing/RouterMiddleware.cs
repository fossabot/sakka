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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace Sakka.Routing
{
    public class RouterMiddleware
    {
        private readonly ILogger<RouterMiddleware> _logger;
        private readonly IList<Route> _routes = new List<Route>();

        public RouterMiddleware(ILogger<RouterMiddleware> logger)
        {
            _logger = logger;
        }

        public RouterMiddleware AddTextRoute(string pattern, MessageDelegate middleware)
        {
            _logger.LogDebug("Adding route...");
            _logger.LogTrace($"Type: {MessageType.TextMessage}");
            _logger.LogTrace($"Pattern: {pattern}");

            _routes.Add(new Route(MessageType.TextMessage, pattern, middleware));

            _logger.LogDebug("Route added");

            return this;
        }

        public RouterMiddleware AddCallbackQueryRoute(MessageDelegate middleware)
        {
            _logger.LogDebug("Adding route...");
            _logger.LogTrace("Type: CallbackQuery");

            _routes.Add(new Route(middleware));

            _logger.LogDebug("Route added");

            return this;
        }

        public Func<Context, Func<Task>, Task> Routes()
        {
            return async (context, next) =>
            {
                Route matchedRoute = null;

                if (context.CallbackQuery != null)
                {
                    matchedRoute = _routes.First(x => x.IsCallbackQueryRoute);
                }
                else
                {
                    foreach (var route in _routes)
                    {
                        if (context.Message.Type != route.Type)
                        {
                            continue;
                        }

                        if (context.Message.Type == MessageType.TextMessage &&
                            !Regex.IsMatch(context.Message.Text, route.Pattern))
                        {
                            continue;
                        }

                        matchedRoute = route;
                        break;
                    }
                }

                if (matchedRoute != null)
                {
                    _logger.LogDebug("Route matched");
                    await matchedRoute.Middleware(context);
                }

                await next();
            };
        }
    }
}
