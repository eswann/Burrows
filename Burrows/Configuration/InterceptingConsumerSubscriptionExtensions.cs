// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Burrows.Configuration.Configuration;
using Burrows.Configuration.SubscriptionConfigurators;
using Burrows.Configuration.SubscriptionConnectors;
using Burrows.Logging;
using Burrows.Util;

namespace Burrows.Configuration
{
    public static class InterceptingConsumerSubscriptionExtensions
    {
        private static readonly ILog _log = Logger.Get(typeof(ConsumerSubscriptionExtensions));

        public static IConsumerSubscriptionConfigurator<TConsumer> InterceptingConsumer<TConsumer>(
            [NotNull] this ISubscriptionBusServiceConfigurator configurator,
            [NotNull] IConsumerFactory<TConsumer> consumerFactory, [NotNull] Action<Action> interceptor)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Intercepting Consumer: {0} (using supplied consumer factory)",
                    typeof(TConsumer));

            var interceptingConsumerFactory = new InterceptingConsumerFactory<TConsumer>(consumerFactory, interceptor);

            return configurator.Consumer(interceptingConsumerFactory);
        }

        public static IConsumerSubscriptionConfigurator<TConsumer> InterceptingConsumer<TConsumer>(
            [NotNull] this ISubscriptionBusServiceConfigurator configurator, [NotNull] Action<Action> interceptor)
            where TConsumer : class, IConsumer, new()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Intercepting Consumer: {0} (using default consumer factory)",
                    typeof(TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(() => new TConsumer());

            var interceptingConsumerFactory = new InterceptingConsumerFactory<TConsumer>(delegateConsumerFactory,
                interceptor);

            return configurator.Consumer(interceptingConsumerFactory);
        }

        public static IConsumerSubscriptionConfigurator<TConsumer> InterceptingConsumer<TConsumer>(
            [NotNull] this ISubscriptionBusServiceConfigurator configurator, [NotNull] Func<TConsumer> consumerFactory,
            [NotNull] Action<Action> interceptor)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using delegate consumer factory)", typeof(TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(consumerFactory);

            var interceptingConsumerFactory = new InterceptingConsumerFactory<TConsumer>(delegateConsumerFactory,
                interceptor);

            return configurator.Consumer(interceptingConsumerFactory);
        }

        public static UnsubscribeAction SubscribeInterceptingConsumer<TConsumer>([NotNull] this IServiceBus bus,
            [NotNull] Action<Action> interceptor)
            where TConsumer : class, IConsumer, new()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using default consumer factory)", typeof(TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(() => new TConsumer());

            var interceptingConsumerFactory = new InterceptingConsumerFactory<TConsumer>(delegateConsumerFactory,
                interceptor);

            IConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<TConsumer>();

            return bus.Configure(x => connector.Connect(x, interceptingConsumerFactory));
        }

        public static UnsubscribeAction SubscribeInterceptingConsumer<TConsumer>([NotNull] this IServiceBus bus,
            [NotNull] Func<TConsumer> consumerFactory, [NotNull] Action<Action> interceptor)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using delegate consumer factory)", typeof(TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(consumerFactory);

            var interceptingConsumerFactory = new InterceptingConsumerFactory<TConsumer>(delegateConsumerFactory,
                interceptor);
            IConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<TConsumer>();

            return bus.Configure(x => connector.Connect(x, interceptingConsumerFactory));
        }

        public static UnsubscribeAction SubscribeInterceptingConsumer<TConsumer>([NotNull] this IServiceBus bus,
            [NotNull] IConsumerFactory<TConsumer> consumerFactory, [NotNull] Action<Action> interceptor)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using supplied consumer factory)", typeof(TConsumer));

            var interceptingConsumerFactory = new InterceptingConsumerFactory<TConsumer>(consumerFactory,
                interceptor);
            IConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<TConsumer>();

            return bus.Configure(x => connector.Connect(x, interceptingConsumerFactory));
        }
    }
}