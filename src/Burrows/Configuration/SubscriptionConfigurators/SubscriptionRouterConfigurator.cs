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
using Burrows.Subscriptions.Coordinator;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    public class SubscriptionRouterConfigurator :
		IBusServiceConfigurator,
		IBusBuilderConfigurator
	{
		readonly IList<ISubscriptionRouterBuilderConfigurator> _configurators;
		string _network;

		public SubscriptionRouterConfigurator(string network)
		{
			_configurators = new List<ISubscriptionRouterBuilderConfigurator>();
			_network = network;
		}

		public IEnumerable<IValidationResult> Validate()
		{
			return _configurators.SelectMany(x => x.Validate());
		}

		public IBusBuilder Configure(IBusBuilder builder)
		{
			builder.AddBusServiceConfigurator(this);

			return builder;
		}

		public Type ServiceType
		{
			get { return typeof (SubscriptionRouterService); }
		}

		public IBusServiceLayer Layer
		{
			get { return IBusServiceLayer.Session; }
		}

		public IBusService Create(IServiceBus bus)
		{
			ISubscriptionRouterBuilder builder = new SubscriptionRouterBuilder(bus, _network);

			builder = _configurators.Aggregate(builder, (seed, next) => next.Configure(seed));

			return builder.Build();
		}

		public void SetNetwork(string network)
		{
			_network = network;
		}

		public void AddConfigurator(ISubscriptionRouterBuilderConfigurator configurator)
		{
			_configurators.Add(configurator);
		}
	}
}