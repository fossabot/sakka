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

using System.Collections.Generic;
using Telegram.Bot.Types;

namespace Sakka
{
    public class Context
    {
        internal Context(Message message)
        {
            Message = message;
            MiddlewareData = new Dictionary<string, object>();
        }

        internal Context(CallbackQuery callbackQuery)
        {
            CallbackQuery = callbackQuery;
            MiddlewareData = new Dictionary<string, object>();

            Message = callbackQuery.Message;
        }

        public Message Message { get; }

        public CallbackQuery CallbackQuery { get; }

        public IDictionary<string, object> MiddlewareData { get; }
    }
}
