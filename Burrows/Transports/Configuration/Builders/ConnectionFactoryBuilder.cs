// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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

using Burrows.Endpoints;

namespace Burrows.Transports.Configuration.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Magnum.Extensions;
    using RabbitMQ.Client;

    public interface IConnectionFactoryBuilder
    {
        ConnectionFactory Build();

        void Add(Func<ConnectionFactory, ConnectionFactory> callback);
    }

    public class ConnectionFactoryBuilder : IConnectionFactoryBuilder
    {
        private readonly IRabbitEndpointAddress _address;
        private readonly IList<Func<ConnectionFactory, ConnectionFactory>> _connectionFactoryConfigurators;

        public ConnectionFactoryBuilder(IRabbitEndpointAddress address)
        {
            _address = address;
            _connectionFactoryConfigurators = new List<Func<ConnectionFactory, ConnectionFactory>>();
        }

        public ConnectionFactory Build()
        {
            ConnectionFactory connectionFactory = _address.ConnectionFactory;

            var clientProperties = new Dictionary<string, object>
                {
                    {"client_api", "Burrows"},
                    {"burrows_version", typeof (IServiceBus).Assembly.GetName().Version.ToString()},
                    {"net_version", Environment.Version.ToString()},
                    {"hostname", Environment.MachineName},
                    {"connected", DateTimeOffset.Now.ToString("R")},
                    {"process_id", Process.GetCurrentProcess().Id.ToString()},
                    {"process_name", Process.GetCurrentProcess().ProcessName}
                };


            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                var assemblyName = entryAssembly.GetName();
                clientProperties.Add("entry_assembly", assemblyName.Name);
            }

            connectionFactory.ClientProperties = clientProperties;

            _connectionFactoryConfigurators.Each(x => x(connectionFactory));

            if (string.IsNullOrEmpty(connectionFactory.UserName))
                connectionFactory.UserName = "guest";
            if (string.IsNullOrEmpty(connectionFactory.Password))
                connectionFactory.Password = "guest";

            return connectionFactory;
        }

        public void Add(Func<ConnectionFactory, ConnectionFactory> callback)
        {
            _connectionFactoryConfigurators.Add(callback);
        }
    }
}