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

using System;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.EndpointConfigurators;
using Burrows.Endpoints;

namespace Burrows.Transports.Configuration.Configurators
{
    using System.Collections.Generic;
    using System.Linq;
    using Builders;

    public interface ITransportFactoryConfigurator : IConfigurator
    {
        void ConfigureHost(Uri hostAddress, Action<IConnectionFactoryConfigurator> configureHost);

        void UsePublisherConfirms(Action<IEnumerable<string>> acktion, Action<IEnumerable<string>> nacktion, int testNacks);
    }

	public class TransportFactoryConfigurator : ITransportFactoryConfigurator
	{
		readonly IList<ITransportFactoryBuilderConfigurator> _transportFactoryConfigurators;

		public TransportFactoryConfigurator()
		{
			_transportFactoryConfigurators = new List<ITransportFactoryBuilderConfigurator>();
		}

		public IEnumerable<IValidationResult> Validate()
		{
			return _transportFactoryConfigurators.SelectMany(x => x.Validate());
		}

        public void ConfigureHost(Uri hostAddress, Action<IConnectionFactoryConfigurator> configureHost)
        {
            var hostConfigurator = new ConnectionFactoryConfigurator(RabbitEndpointAddress.Parse(hostAddress));
            configureHost(hostConfigurator);

            AddConfigurator(hostConfigurator);
        }

        public void UsePublisherConfirms(Action<IEnumerable<string>> acktion, Action<IEnumerable<string>> nacktion, int testNacks)
        {
            var hostConfigurator = new PublisherConfirmFactoryConfigurator(true, acktion, nacktion, testNacks);

            AddConfigurator(hostConfigurator);
        }

		public TransportFactory Build()
		{
			var builder = new TransportFactoryBuilder();

			_transportFactoryConfigurators.Aggregate((ITransportFactoryBuilder) builder,
				(seed, configurator) => configurator.Configure(seed));

			return builder.Build();
		}

        private void AddConfigurator(ITransportFactoryBuilderConfigurator configurator)
        {
            _transportFactoryConfigurators.Add(configurator);
        }
	}
}