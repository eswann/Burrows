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
using System.Collections.Generic;
using System.Linq;
using Burrows.Configuration.Builders;
using Burrows.Configuration.BusConfigurators;
using Burrows.Configuration.BusServiceConfigurators;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.SubscriptionBuilders;
using Burrows.Subscriptions;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    /// <summary>
    /// The configuration scope for subscriptions on the bus
    /// </summary>
    public interface ISubscriptionBusServiceConfigurator :
        IConfigurator
    {
        void AddConfigurator(ISubscriptionBusServiceBuilderConfigurator configurator);
    }

	/// <summary>
	/// Handles the configuration of subscriptions as part of the bus
	/// 
	/// </summary>
	public class SubscriptionBusServiceConfigurator :
		ISubscriptionBusServiceConfigurator,
		IBusServiceConfigurator,
		IBusBuilderConfigurator
	{
		readonly IList<ISubscriptionBusServiceBuilderConfigurator> _configurators;

		public SubscriptionBusServiceConfigurator()
		{
			_configurators = new List<ISubscriptionBusServiceBuilderConfigurator>();
		}

		public IEnumerable<IValidationResult> Validate()
		{
			return from configurator in _configurators
			       from result in configurator.Validate()
			       select result.WithParentKey("Subscribe");
		}

		public void AddConfigurator(ISubscriptionBusServiceBuilderConfigurator configurator)
		{
			_configurators.Add(configurator);
		}

		public IBusBuilder Configure(IBusBuilder builder)
		{
			builder.AddBusServiceConfigurator(this);

			return builder;
		}

		public Type ServiceType
		{
			get { return typeof (SubscriptionBusService); }
		}

		public IBusServiceLayer Layer
		{
			get { return IBusServiceLayer.Application; }
		}

		public IBusService Create(IServiceBus bus)
		{
			var subscriptionServiceBuilder = new SubscriptionBusServiceBuilder();

			foreach (ISubscriptionBusServiceBuilderConfigurator configurator in _configurators)
			{
				configurator.Configure(subscriptionServiceBuilder);
			}

			return subscriptionServiceBuilder.Build();
		}
	}
}