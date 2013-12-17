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
namespace Burrows.Testing.TestInstanceConfigurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BuilderConfigurators;
    using Builders;
    using Configurators;
    using ScenarioBuilders;
    using Scenarios;

    public interface IConsumerTestInstanceConfigurator<TScenario, TConsumer> :
    ITestInstanceConfigurator<TScenario>
        where TConsumer : class
        where TScenario : ITestScenario
    {
        void UseBuilder(Func<TScenario, IConsumerTestBuilder<TScenario, TConsumer>> builderFactory);
        void AddConfigurator(ConsumerTestBuilderConfigurator<TScenario, TConsumer> configurator);

        void UseConsumerFactory(IConsumerFactory<TConsumer> consumerFactory);
    }

	public class ConsumerTestInstanceConfigurator<TScenario, TConsumer> :
		TestInstanceConfigurator<TScenario>,
		IConsumerTestInstanceConfigurator<TScenario, TConsumer>
		where TConsumer : class, IConsumer
	    where TScenario : ITestScenario
	{
		readonly IList<ConsumerTestBuilderConfigurator<TScenario, TConsumer>> _configurators;

		Func<TScenario, IConsumerTestBuilder<TScenario, TConsumer>> _builderFactory;
		IConsumerFactory<TConsumer> _consumerFactory;

		public ConsumerTestInstanceConfigurator(Func<IScenarioBuilder<TScenario>> scenarioBuilderFactory)
			: base(scenarioBuilderFactory)
		{
			_configurators = new List<ConsumerTestBuilderConfigurator<TScenario, TConsumer>>();

			_builderFactory = scenario => new ConsumerTestBuilder<TScenario, TConsumer>(scenario);
		}

		public void UseBuilder(Func<TScenario, IConsumerTestBuilder<TScenario, TConsumer>> builderFactory)
		{
			_builderFactory = builderFactory;
		}

		public void AddConfigurator(ConsumerTestBuilderConfigurator<TScenario, TConsumer> configurator)
		{
			_configurators.Add(configurator);
		}

		public void UseConsumerFactory(IConsumerFactory<TConsumer> consumerFactory)
		{
			_consumerFactory = consumerFactory;
		}

		public new IEnumerable<ITestConfiguratorResult> Validate()
		{
			if (_consumerFactory == null)
				yield return this.Failure("ConsumerFactory", "The consumer factory must be configured (using ConstructedBy)");

			IEnumerable<ITestConfiguratorResult> results = base.Validate().Concat(_configurators.SelectMany(x => x.Validate()));
			foreach (ITestConfiguratorResult result in results)
			{
				yield return result;
			}
		}

		public ConsumerTest<TScenario, TConsumer> Build()
		{
			TScenario context = BuildTestScenario();

			IConsumerTestBuilder<TScenario, TConsumer> builder = _builderFactory(context);

			builder.SetConsumerFactory(_consumerFactory);

			builder = _configurators.Aggregate(builder, (current, configurator) => configurator.Configure(current));

			BuildTestActions(builder);

			return builder.Build();
		}
	}
}