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

using Burrows.Configuration.Configurators;
using Burrows.Endpoints;

namespace Burrows.Transports.Configuration.Configurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;

    public interface IConnectionFactoryConfigurator : IConfigurator
    {
        void UseSsl(Action<ISslConnectionFactoryConfigurator> configureSsl);
        void SetRequestedHeartbeat(ushort requestedHeartbeat);
        void SetUsername(string username);
        void SetPassword(string password);
    }

	public class ConnectionFactoryConfigurator :
		IConnectionFactoryConfigurator,
		ITransportFactoryBuilderConfigurator
	{
        private readonly IRabbitEndpointAddress _address;

		readonly List<IConnectionFactoryBuilderConfigurator> _configurators;

        public ConnectionFactoryConfigurator(IRabbitEndpointAddress address)
		{
			_address = address;
			_configurators = new List<IConnectionFactoryBuilderConfigurator>();
		}


		public IEnumerable<IValidationResult> Validate()
		{
			return _configurators.SelectMany(x => x.Validate());
		}

		public void UseSsl(Action<ISslConnectionFactoryConfigurator> configureSsl)
		{
			var configurator = new SslConnectionFactoryConfigurator();

			configureSsl(configurator);

			_configurators.Add(configurator);
		}

		public void SetRequestedHeartbeat(ushort requestedHeartbeat)
		{
			_configurators.Add(new RequestedHeartbeatConnectionFactoryConfigurator(requestedHeartbeat));
		}

	    public void SetUsername(string username)
	    {
	        _configurators.Add(new UsernameConnectionFactoryConfigurator(username));
	    }

	    public void SetPassword(string password)
	    {
	        _configurators.Add(new PasswordConnectionFactoryConfigurator(password));
	    }

	    public ITransportFactoryBuilder Configure(ITransportFactoryBuilder builder)
		{
			IConnectionFactoryBuilder connectionFactoryBuilder = CreateBuilder();

			builder.AddConnectionFactoryBuilder(_address.Uri, connectionFactoryBuilder);

			return builder;
		}

		public IConnectionFactoryBuilder CreateBuilder()
		{
			var connectionFactoryBuilder = new ConnectionFactoryBuilder(_address);

			_configurators.Aggregate((IConnectionFactoryBuilder) connectionFactoryBuilder,
				(seed, configurator) => configurator.Configure(seed));
			return connectionFactoryBuilder;
		}
	}
}