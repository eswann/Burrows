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
using Burrows.Configuration.Configuration;
using Burrows.Configuration.SubscriptionConfigurators;
using Burrows.Configuration.SubscriptionConnectors;
using Burrows.Logging;
using Magnum.Reflection;
using Burrows.Util;

namespace Burrows.Configuration
{
    public static class ConsumerSubscriptionExtensions
    {
        private static readonly ILog _log = Logger.Get(typeof (ConsumerSubscriptionExtensions));

        public static IConsumerSubscriptionConfigurator<TConsumer> Consumer<TConsumer>(
            [NotNull] this ISubscriptionBusServiceConfigurator configurator,
            [NotNull] IConsumerFactory<TConsumer> consumerFactory)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using supplied consumer factory)", typeof (TConsumer));

            var consumerConfigurator = new ConsumerSubscriptionConfigurator<TConsumer>(consumerFactory);

            var busServiceConfigurator = new SubscriptionBusServiceBuilderConfigurator(consumerConfigurator);

            configurator.AddConfigurator(busServiceConfigurator);

            return consumerConfigurator;
        }

        public static IConsumerSubscriptionConfigurator<TConsumer> Consumer<TConsumer>(
            [NotNull] this ISubscriptionBusServiceConfigurator configurator)
            where TConsumer : class, IConsumer, new()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using default consumer factory)", typeof (TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(() => new TConsumer());

            var consumerConfigurator = new ConsumerSubscriptionConfigurator<TConsumer>(delegateConsumerFactory);

            var busServiceConfigurator = new SubscriptionBusServiceBuilderConfigurator(consumerConfigurator);

            configurator.AddConfigurator(busServiceConfigurator);

            return consumerConfigurator;
        }

        public static IConsumerSubscriptionConfigurator<TConsumer> Consumer<TConsumer>(
            [NotNull] this ISubscriptionBusServiceConfigurator configurator, [NotNull] Func<TConsumer> consumerFactory)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using delegate consumer factory)", typeof (TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(consumerFactory);

            var consumerConfigurator = new ConsumerSubscriptionConfigurator<TConsumer>(delegateConsumerFactory);

            var busServiceConfigurator = new SubscriptionBusServiceBuilderConfigurator(consumerConfigurator);

            configurator.AddConfigurator(busServiceConfigurator);

            return consumerConfigurator;
        }

        public static IConsumerSubscriptionConfigurator Consumer(
            [NotNull] this ISubscriptionBusServiceConfigurator configurator,
            [NotNull] Type consumerType,
            [NotNull] Func<Type, object> consumerFactory)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (by type, using object consumer factory)", consumerType);

            var consumerConfigurator =
                (ISubscriptionBuilderConfigurator)
                FastActivator.Create(typeof (UntypedConsumerSubscriptionConfigurator<>),
                    new[] {consumerType}, new object[] {consumerFactory});

            var busServiceConfigurator = new SubscriptionBusServiceBuilderConfigurator(consumerConfigurator);

            configurator.AddConfigurator(busServiceConfigurator);

            return consumerConfigurator as IConsumerSubscriptionConfigurator;
        }

        public static UnsubscribeAction SubscribeConsumer<TConsumer>([NotNull] this IServiceBus bus)
            where TConsumer : class, IConsumer, new()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using default consumer factory)", typeof (TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(() => new TConsumer());

            IConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<TConsumer>();

            return bus.Configure(x => connector.Connect(x, delegateConsumerFactory));
        }

        public static UnsubscribeAction SubscribeConsumer<TConsumer>([NotNull] this IServiceBus bus,
                                                                     [NotNull] Func<TConsumer> consumerFactory)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using delegate consumer factory)", typeof (TConsumer));

            var delegateConsumerFactory = new DelegateConsumerFactory<TConsumer>(consumerFactory);

            IConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<TConsumer>();

            return bus.Configure(x => connector.Connect(x, delegateConsumerFactory));
        }

        public static UnsubscribeAction SubscribeConsumer<TConsumer>([NotNull] this IServiceBus bus,
                                                                     [NotNull] IConsumerFactory<TConsumer>
                                                                         consumerFactory)
            where TConsumer : class, IConsumer
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (using supplied consumer factory)", typeof (TConsumer));

            IConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector<TConsumer>();

            return bus.Configure(x => connector.Connect(x, consumerFactory));
        }

        public static UnsubscribeAction SubscribeConsumer([NotNull] this IServiceBus bus, [NotNull] Type consumerType,
                                                          [NotNull] Func<Type, object> consumerFactory)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscribing Consumer: {0} (by type, using object consumer factory)", consumerType);

            object factory = FastActivator.Create(typeof (ObjectConsumerFactory<>), new[] {consumerType},
                new object[] {consumerFactory});

            IConsumerConnector connector = ConsumerConnectorCache.GetConsumerConnector(consumerType);

            return bus.Configure(x => connector.FastInvoke<IConsumerConnector, UnsubscribeAction>("Connect", x, factory));
        }
    }
}