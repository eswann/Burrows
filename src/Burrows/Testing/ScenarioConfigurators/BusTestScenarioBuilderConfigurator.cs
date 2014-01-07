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

namespace Burrows.Testing.ScenarioConfigurators
{
    using System;
    using System.Collections.Generic;
    using Configurators;
    using ScenarioBuilders;

    public class BusTestScenarioBuilderConfigurator :
		IScenarioBuilderConfigurator<IBusTestScenario>
	{
		readonly Action<IServiceBusConfigurator> _configureAction;

		public BusTestScenarioBuilderConfigurator(Action<IServiceBusConfigurator> configureAction)
		{
			_configureAction = configureAction;
		}

		public IEnumerable<ITestConfiguratorResult> Validate()
		{
			if (_configureAction == null)
				yield return this.Failure("The scenario configuration action cannot be null");
		}

		public IScenarioBuilder<IBusTestScenario> Configure(IScenarioBuilder<IBusTestScenario> builder)
		{
			var busBuilder = builder as IBusScenarioBuilder;
			if (busBuilder != null)
			{
				busBuilder.ConfigureBus(_configureAction);
			}

			return builder;
		}
	}
}