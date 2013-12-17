// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Burrows.Configuration.BusConfigurators;
using Burrows.Configuration.SubscriptionConfigurators;

namespace Burrows.Services.Subscriptions.Configuration
{
    using System;
    using Util;

    public static class SubscriptionClientConfiguratorExtensions
    {
        /// <summary>
        /// Sets the address of the Subscription Service which routes messages to multiple subscribers
        /// from a single publisher.  The address of the Subscription Service is used by both 
        /// publishers and subscribers, while routing is carried out by Burrows.RuntimeServices
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="subscriptionServiceUri"></param>
        [Obsolete("The extension method on UseMsmq/UseRabbitMQ should be used instead")]
        public static IServiceBusConfigurator UseSubscriptionService(this IServiceBusConfigurator configurator,
            string subscriptionServiceUri)
        {
            configurator.UseSubscriptionService(x => x.SetSubscriptionServiceEndpoint(subscriptionServiceUri.ToUri()));
            return configurator;
        }

        /// <summary>
        /// Sets the address of the Subscription Service which routes messages to multiple subscribers
        /// from a single publisher.  The address of the Subscription Service is used by both 
        /// publishers and subscribers, while routing is carried out by Burrows.RuntimeServices
        /// </summary>
        [Obsolete("The extension method on UseMsmq/UseRabbitMQ should be used instead")]
        public static IServiceBusConfigurator UseSubscriptionService(this IServiceBusConfigurator configurator, Uri subscriptionServiceUri)
        {
            configurator.UseSubscriptionService(x => x.SetSubscriptionServiceEndpoint(subscriptionServiceUri));
            return configurator;
        }

        /// <summary>
        /// Sets the address of the Subscription Service which routes messages to multiple subscribers
        /// from a single publisher.  The address of the Subscription Service is used by both 
        /// publishers and subscribers, while routing is carried out by Burrows.RuntimeServices
        /// </summary>
        [Obsolete("The extension method on UseMsmq/UseRabbitMQ should be used instead")]
        public static IServiceBusConfigurator UseSubscriptionService(this IServiceBusConfigurator configurator,
            Action<ISubscriptionClientConfigurator> configureCallback)
        {
            var clientConfigurator = new SubscriptionClientConfigurator();

            configureCallback(clientConfigurator);

            var routerBuilderConfigurator = new SubscriptionRouterBuilderConfigurator(x => x.SetNetwork(null));

            configurator.AddSubscriptionRouterConfigurator(routerBuilderConfigurator);

            configurator.AddSubscriptionObserver(clientConfigurator.Create);
            return configurator;
        }
    }
}