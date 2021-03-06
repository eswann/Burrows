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
namespace Burrows.Testing.Builders
{
    using System.Collections.Generic;
    using Instances;
    using Saga;
    using Scenarios;
    using TestActions;

    public interface ISagaTestBuilder<TScenario, TSaga> :
    ITestInstanceBuilder<TScenario>
        where TSaga : class, ISaga
        where TScenario : ITestScenario
    {
        ISagaTest<TScenario, TSaga> Build();

        void SetSagaRepository(ISagaRepository<TSaga> sagaRepository);
    }

	public class SagaTestBuilder<TScenario, TSaga> :
		ISagaTestBuilder<TScenario, TSaga>
		where TSaga : class, ISaga
		where TScenario : ITestScenario
	{
		readonly IList<TestAction<TScenario>> _actions;
		readonly TScenario _scenario;
		ISagaRepository<TSaga> _sagaRepository;

		public SagaTestBuilder(TScenario scenario)
		{
			_scenario = scenario;

			_actions = new List<TestAction<TScenario>>();
		}

		public ISagaTest<TScenario, TSaga> Build()
		{
			if(_sagaRepository == null)
			{
				_sagaRepository = new InMemorySagaRepository<TSaga>();
			}

			var test = new SagaTestInstance<TScenario, TSaga>(_scenario, _actions, _sagaRepository);

			return test;
		}

		public void SetSagaRepository(ISagaRepository<TSaga> sagaRepository)
		{
			_sagaRepository = sagaRepository;
		}

		public void AddTestAction(TestAction<TScenario> testAction)
		{
			_actions.Add(testAction);
		}
	}
}