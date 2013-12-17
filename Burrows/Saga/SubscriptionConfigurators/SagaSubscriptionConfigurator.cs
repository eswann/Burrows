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
using Burrows.Configuration.SubscriptionBuilders;
using Burrows.Configuration.SubscriptionConfigurators;

namespace Burrows.Saga.SubscriptionConfigurators
{
    using System.Collections.Generic;
    using SubscriptionBuilders;

    public interface ISagaSubscriptionConfigurator<TSaga> :
    ISubscriptionConfigurator<ISagaSubscriptionConfigurator<TSaga>>
    where TSaga : class, ISaga
    {
    }

	public class SagaSubscriptionConfigurator<TSaga> :
		SubscriptionConfigurator<ISagaSubscriptionConfigurator<TSaga>>,
		ISagaSubscriptionConfigurator<TSaga>,
		ISubscriptionBuilderConfigurator
		where TSaga : class, ISaga
	{
		readonly ISagaRepository<TSaga> _sagaRepository;

		public SagaSubscriptionConfigurator(ISagaRepository<TSaga> sagaRepository)
		{
			_sagaRepository = sagaRepository;
		}

		public IEnumerable<IValidationResult> Validate()
		{
			if (_sagaRepository == null)
				yield return this.Failure("The saga repository cannot be null. How else are we going to save stuff? #facetopalm");
		}

		public ISubscriptionBuilder Configure()
		{
			return new SagaSubscriptionBuilder<TSaga>(_sagaRepository, ReferenceFactory);
		}
	}
}