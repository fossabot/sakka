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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sakka.Parsing;
using Sakka.Routing;
using Sakka.Routing.Extensions;

namespace Sakka.Example
{
    internal class Startup
    {
        private readonly IConfigurationRoot _config;
        private ILogger<Startup> _logger;

        internal Startup(IConfigurationRoot config)
        {
            _config = config;
        }

        internal void Run()
        {
            var services = ConfigureServices();

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            Configure(loggerFactory);
            
            var client = services.GetRequiredService<Client>();
            var parserMiddleware = services.GetRequiredService<ParserMiddleware>();
            var routerMiddleware = services.GetRequiredService<RouterMiddleware>();

            routerMiddleware.AddCommand("ping", async context =>
            {
                await client.SendTextAsync(context.Message.Chat, "pong",
                    replyToMessageId: context.Message.MessageId);
            });

            client
                .Use(parserMiddleware.Parser())
                .Use(routerMiddleware.Routes())
                .Run(_config);
        }

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace))
                .AddSingleton<Client>()
                .AddSingleton<ParserMiddleware>()
                .AddSingleton<RouterMiddleware>()
                .BuildServiceProvider();
        }

        private void Configure(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory
                .AddConsole(LogLevel.Trace)
                .CreateLogger<Startup>();
            
            _logger.LogInformation("Startup Configured");
        }
    }
}
