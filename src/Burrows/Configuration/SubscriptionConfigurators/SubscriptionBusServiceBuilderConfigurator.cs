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

using System.Collections.Generic;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.SubscriptionBuilders;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    public interface ISubscriptionBusServiceBuilderConfigurator :
    IConfigurator
    {
        ISubscriptionBusServiceBuilder Configure(ISubscriptionBusServiceBuilder builder);
    }

	public class SubscriptionBusServiceBuilderConfigurator :
		ISubscriptionBusServiceBuilderConfigurator
	{
		readonly ISubscriptionBuilderConfigurator _configurator;

		public SubscriptionBusServiceBuilderConfigurator(ISubscriptionBuilderConfigurator configurator)
		{
			_configurator = configurator;
		}

		public IEnumerable<IValidationResult> Validate()
		{
			return _configurator.Validate();
		}

		public ISubscriptionBusServiceBuilder Configure(ISubscriptionBusServiceBuilder builder)
		{
			ISubscriptionBuilder subscriptionBuilder = _configurator.Configure();

			builder.AddSubscriptionBuilder(subscriptionBuilder);

			return builder;
		}
	}
}