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
using Burrows.Exceptions;
using Magnum.Extensions;
using Magnum.Reflection;
using Burrows.Pipeline;
using Burrows.Saga;
using Burrows.Util;

namespace Burrows.Configuration.SubscriptionConnectors
{
    /// <summary>
    /// Interface implemented by objects that tie an inbound pipeline together with
    /// consumers (by means of calling a consumer factory).
    /// </summary>
    public interface IConsumerConnector
    {
        UnsubscribeAction Connect<TConsumer>(IInboundPipelineConfigurator configurator,
                                             IConsumerFactory<TConsumer> consumerFactory)
            where TConsumer : class;
    }

    public class ConsumerConnector<T> : IConsumerConnector where T : class
    {
        private readonly IEnumerable<IConsumerSubscriptionConnector> _connectors;

        public ConsumerConnector()
        {
            Type[] interfaces = typeof (T).GetInterfaces();

            if (interfaces.Contains(typeof (ISaga)))
                throw new ConfigurationException("A saga cannot be registered as a consumer");

            if (interfaces.Implements(typeof (InitiatedBy<>))
                || interfaces.Implements(typeof (IOrchestrate<>))
                || interfaces.Implements(typeof (IObserve<,>)))
                throw new ConfigurationException("InitiatedBy, Orchestrates, and Observes can only be used with sagas");

            _connectors = ConsumesSelectedContext()
                .Concat(ConsumesContext())
                .Concat(ConsumesSelected())
                .Concat(ConsumesAll())
                .Distinct((x, y) => x.MessageType == y.MessageType)
                .ToList();
        }

        public IEnumerable<IConsumerSubscriptionConnector> Connectors
        {
            get { return _connectors; }
        }

        public UnsubscribeAction Connect<TConsumer>(IInboundPipelineConfigurator configurator,
                                                    IConsumerFactory<TConsumer> consumerFactory)
            where TConsumer : class
        {
            return _connectors.Select(x => x.Connect(configurator, consumerFactory))
                .Aggregate<UnsubscribeAction, UnsubscribeAction>(() => true, (seed, x) => () => seed() && x());
        }

        static IEnumerable<IConsumerSubscriptionConnector> ConsumesContext()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesContextTypes()
                .Select(CreateContextConnector);
        }

        static IConsumerSubscriptionConnector CreateContextConnector(MessageInterfaceType x)
        {
            return (IConsumerSubscriptionConnector)
                   FastActivator.Create(typeof (ContextConsumerSubscriptionConnector<,>),
                       new[] {typeof (T), x.MessageType});
        }

        static IEnumerable<IConsumerSubscriptionConnector> ConsumesSelectedContext()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesSelectedContextTypes()
                .Select(CreateSelectedContextConnector);
        }

        static IConsumerSubscriptionConnector CreateSelectedContextConnector(MessageInterfaceType x)
        {
            return (IConsumerSubscriptionConnector)
                   FastActivator.Create(typeof (SelectedContextConsumerSubscriptionConnector<,>),
                       new[] {typeof (T), x.MessageType});
        }

        static IEnumerable<IConsumerSubscriptionConnector> ConsumesAll()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesAllTypes()
                .Select(CreateConnector);
        }

        static IConsumerSubscriptionConnector CreateConnector(MessageInterfaceType x)
        {
            return (IConsumerSubscriptionConnector)
                   FastActivator.Create(typeof (ConsumerSubscriptionConnector<,>),
                       new[] {typeof (T), x.MessageType});
        }

        static IEnumerable<IConsumerSubscriptionConnector> ConsumesSelected()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesSelectedTypes()
                .Select(CreateSelectedConnector);
        }

        static IConsumerSubscriptionConnector CreateSelectedConnector(MessageInterfaceType x)
        {
            return (IConsumerSubscriptionConnector)
                   FastActivator.Create(typeof (SelectedConsumerSubscriptionConnector<,>),
                       new[] {typeof (T), x.MessageType});
        }
    }
}