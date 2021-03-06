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
using Burrows.Configuration.BusServiceConfigurators;
using Burrows.Endpoints;

namespace Burrows.Services.Routing.Configuration
{
    using System;
    using System.Collections.Generic;
    using Pipeline;
    using Util;

    public class RoutingConfigurator :
		IBusServiceConfigurator
	{
		readonly IList<Func<IServiceBus, UnsubscribeAction>> _routes = new List<Func<IServiceBus, UnsubscribeAction>>();

		public Type ServiceType
		{
			get { return typeof (RoutingService); }
		}

		public IBusServiceLayer Layer
		{
			get { return IBusServiceLayer.Session; }
		}

		public IBusService Create(IServiceBus bus)
		{
			return new RoutingService(_routes);
		}

		public RouteTo Route<TMessage>() where TMessage : class
		{
			return new Router<TMessage>(this);
		}

		class Router<TMessage> :
			RouteTo
			where TMessage : class
		{
			readonly RoutingConfigurator _boss;

			public Router(RoutingConfigurator boss)
			{
				_boss = boss;
			}

			public void To(Uri address)
			{
				_boss._routes.Add(bus =>
					{
						IEndpoint endpoint = bus.GetEndpoint(address);
						return bus.OutboundPipeline.ConnectEndpoint<TMessage>(endpoint);
					});
			}

			public void To(string addressUri)
			{
				To(addressUri.ToUri("Invalid URI specified in route"));
			}
		}
	}
}