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

using Burrows.Configuration;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.EndpointConfigurators;
using Burrows.Endpoints;

namespace Burrows.Testing.ScenarioBuilders
{
    using System;
    using Exceptions;
    using Scenarios;
    using Serialization;
    using Transports.Loopback;

    /// <summary>
    /// And endpoint scenario builder implementation ties together the scenario 
    /// with the underlying infrastructure.
    /// </summary>
    /// <typeparam name="TScenario">See <see cref="IBusTestScenario"/>, <see cref="IEndpointTestScenario"/> and <see cref="ITestScenario"/>
    /// for feeding as the generic parameter.</typeparam>
    public interface IEndpointScenarioBuilder<TScenario> :
        IScenarioBuilder<TScenario>
        where TScenario : ITestScenario
    {
        /// <summary>
        /// Endpoint scenario builders may call this method to configure the endpoint factory. Call this method
        /// to customize how the endpoint uris are built. Example:
        /// <code>
        /// ConfigureEndpointFactory(x =>
        ///    {
        ///    	x.UseRabbitMq();
        ///    });
        /// </code>
        /// </summary>
        /// <param name="configureCallback"></param>
        void ConfigureEndpointFactory(Action<IEndpointFactoryConfigurator> configureCallback);
    }

	public abstract class EndpointScenarioBuilder<TScenario> :
		IEndpointScenarioBuilder<TScenario>
		where TScenario : ITestScenario
	{
		readonly IEndpointFactoryConfigurator _endpointFactoryConfigurator;

		public EndpointScenarioBuilder()
		{
			var settings = new EndpointFactoryDefaultSettings
				{
					CreateMissingQueues = true,
					PurgeOnStartup = true,
					Serializer = new JsonMessageSerializer(),
				};

			_endpointFactoryConfigurator = new EndpointFactoryConfigurator(settings);

			_endpointFactoryConfigurator.AddTransportFactory<LoopbackTransportFactory>();
		}

		public void ConfigureEndpointFactory(Action<IEndpointFactoryConfigurator> configureCallback)
		{
			configureCallback(_endpointFactoryConfigurator);
		}

		protected IEndpointFactory BuildEndpointFactory()
		{
			IConfigurationResult result = ConfigurationResult.CompileResults(_endpointFactoryConfigurator.Validate());

			IEndpointFactory endpointFactory;
			try
			{
				endpointFactory = _endpointFactoryConfigurator.CreateEndpointFactory();
			}
			catch (Exception ex)
			{
				throw new ConfigurationException(result, "An exception was thrown during endpoint cache creation", ex);
			}
			return endpointFactory;
		}

		public abstract TScenario Build();
	}
}