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

using Telegram.Bot.Types.Enums;

namespace Sakka.Routing
{
    internal class Route
    {
        internal Route(MessageDelegate middleware)
        {
            IsCallbackQueryRoute = true;
            Middleware = middleware;
        }

        internal Route(MessageType type, string pattern, MessageDelegate middleware)
        {
            Type = type;
            Pattern = pattern;
            Middleware = middleware;
        }

        public MessageType Type { get; }

        public string Pattern { get; }

        public bool IsCallbackQueryRoute { get; }

        public MessageDelegate Middleware { get; }
    }
}
