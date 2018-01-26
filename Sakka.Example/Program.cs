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

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Sakka.Example
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "token", "373144766:AAHoUJfXNf822nfneApmwAiphKS_r1dCIcs" },
                })
                .Build();

            new Startup(config)
                .Run();

            new ManualResetEvent(false)
                .WaitOne();
        }
    }
}
