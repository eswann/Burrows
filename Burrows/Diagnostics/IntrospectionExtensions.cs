// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using Burrows.Configuration.BusConfigurators;
using Burrows.Configuration.BusServiceConfigurators;

namespace Burrows.Diagnostics
{
    using System;
    using Introspection;
    using Magnum.FileSystem;

    public static class IntrospectionExtensions
    {
        public static IServiceBusConfigurator EnableRemoteIntrospection(this IServiceBusConfigurator configurator)
        {
            var serviceConfigurator = new IntrospectionServiceConfigurator();

            var busConfigurator = new CustomBusServiceConfigurator(serviceConfigurator);

            configurator.AddBusConfigurator(busConfigurator);
            return configurator;
        }

        public static void WriteIntrospectionToConsole(this IServiceBus bus)
        {
            IDiagnosticsProbe probe = bus.Probe();
            Console.Write(probe);
        }

        public static void WriteIntrospectionToFile(this IServiceBus bus, string fileName)
        {
            IDiagnosticsProbe probe = bus.Probe();
            var fs = new DotNetFileSystem();
            fs.DeleteFile(fileName);
            fs.Write(fileName, probe.ToString());
        }

        /// <summary>
        /// A convenience method for inspecting an active service bus instance.
        /// </summary>
        public static IDiagnosticsProbe Probe(this IServiceBus bus)
        {
            var probe = new InMemoryDiagnosticsProbe();
            bus.Inspect(probe);
            return probe;
        }
    }
}