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
    using Context;
    using ScenarioBuilders;
    using Scenarios;

    public interface IHandlerTestInstanceConfigurator<TScenario, TMessage> :
    ITestInstanceConfigurator<TScenario>
        where TMessage : class
        where TScenario : ITestScenario
    {
        void UseBuilder(Func<TScenario, IHandlerTestBuilder<TScenario, TMessage>> builderFactory);
        void AddConfigurator(HandlerTestBuilderConfigurator<TScenario, TMessage> configurator);

        void Handler(Action<IConsumeContext<TMessage>, TMessage> handler);
    }

	public class HandlerTestInstanceConfigurator<TScenario, TMessage> :
		TestInstanceConfigurator<TScenario>,
		IHandlerTestInstanceConfigurator<TScenario, TMessage>
		where TMessage : class
		where TScenario : ITestScenario
	{
		readonly IList<HandlerTestBuilderConfigurator<TScenario, TMessage>> _configurators;

		Func<TScenario, IHandlerTestBuilder<TScenario, TMessage>> _builderFactory;
		Action<IConsumeContext<TMessage>, TMessage> _handler;

		public HandlerTestInstanceConfigurator(Func<IScenarioBuilder<TScenario>> scenarioBuilderFactory)
			: base(scenarioBuilderFactory)
		{
			_configurators = new List<HandlerTestBuilderConfigurator<TScenario, TMessage>>();

			_builderFactory = scenario => new HandlerTestBuilder<TScenario, TMessage>(scenario);
		}

		public void UseBuilder(Func<TScenario, IHandlerTestBuilder<TScenario, TMessage>> builderFactory)
		{
			_builderFactory = builderFactory;
		}

		public void AddConfigurator(HandlerTestBuilderConfigurator<TScenario, TMessage> configurator)
		{
			_configurators.Add(configurator);
		}

		public void Handler(Action<IConsumeContext<TMessage>, TMessage> handler)
		{
			_handler = handler;
		}

		public override IEnumerable<ITestConfiguratorResult> Validate()
		{
			return base.Validate().Concat(_configurators.SelectMany(x => x.Validate()));
		}

		public HandlerTest<TScenario, TMessage> Build()
		{
			TScenario scenario = BuildTestScenario();

			IHandlerTestBuilder<TScenario, TMessage> builder = _builderFactory(scenario);

			if (_handler != null)
				builder.SetHandler(_handler);

			builder = _configurators.Aggregate(builder, (current, configurator) => configurator.Configure(current));

			BuildTestActions(builder);

			return builder.Build();
		}
	}
}