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
    public interface IInstanceConnector
    {
        UnsubscribeAction Connect(IInboundPipelineConfigurator configurator, object instance);
    }

    public class InstanceConnector<T> :
        IInstanceConnector
        where T : class
    {
        private readonly IEnumerable<IInstanceSubscriptionConnector> _connectors;

        public InstanceConnector()
        {
            Type[] interfaces = typeof(T).GetInterfaces();

            if (interfaces.Contains(typeof(ISaga)))
                throw new ConfigurationException("A saga cannot be registered as a consumer");

            if (interfaces.Implements(typeof(InitiatedBy<>))
                || interfaces.Implements(typeof(IOrchestrate<>))
                || interfaces.Implements(typeof(IObserve<,>)))
                throw new ConfigurationException("InitiatedBy, Orchestrates, and Observes can only be used with sagas");

            _connectors = /* Distributors()
                .Concat(Workers())
                .Concat(*/
                ConsumesCorrelated()
                    .Concat(ConsumesSelectedContext())
                    .Concat(ConsumesContext())
                    .Concat(ConsumesSelected())
                    .Concat(ConsumesAll())
                    .Distinct((x, y) => x.MessageType == y.MessageType)
                    .ToList();
        }


        public IEnumerable<IInstanceSubscriptionConnector> Connectors
        {
            get { return _connectors; }
        }

        public UnsubscribeAction Connect(IInboundPipelineConfigurator configurator, object instance)
        {
            return _connectors.Select(x => x.Connect(configurator, instance))
                .Aggregate<UnsubscribeAction, UnsubscribeAction>(() => true, (seed, x) => () => seed() && x());
        }

        IEnumerable<IInstanceSubscriptionConnector> ConsumesContext()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesContextTypes()
                .Select(CreateContextConnector);
        }

        IEnumerable<IInstanceSubscriptionConnector> ConsumesSelectedContext()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesSelectedContextTypes()
                .Select(CreateSelectedContextConnector);
        }

        static IInstanceSubscriptionConnector CreateContextConnector(MessageInterfaceType x)
        {
            return (IInstanceSubscriptionConnector)
                   FastActivator.Create(typeof(ContextInstanceSubscriptionConnector<,>),
                       new[] {typeof(T), x.MessageType});
        }

        static IInstanceSubscriptionConnector CreateSelectedContextConnector(MessageInterfaceType x)
        {
            return (IInstanceSubscriptionConnector)
                   FastActivator.Create(typeof(SelectedContextInstanceSubscriptionConnector<,>),
                       new[] {typeof(T), x.MessageType});
        }

        static IEnumerable<IInstanceSubscriptionConnector> ConsumesAll()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesAllTypes()
                .Select(CreateConnector);
        }

        static IInstanceSubscriptionConnector CreateConnector(MessageInterfaceType x)
        {
            return (IInstanceSubscriptionConnector)
                   FastActivator.Create(typeof(InstanceSubscriptionConnector<,>), new[] {typeof(T), x.MessageType});
        }

        static IEnumerable<IInstanceSubscriptionConnector> ConsumesSelected()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesSelectedTypes()
                .Select(CreateSelectedConnector);
        }

        static IInstanceSubscriptionConnector CreateSelectedConnector(MessageInterfaceType x)
        {
            return (IInstanceSubscriptionConnector)
                   FastActivator.Create(typeof(SelectedInstanceSubscriptionConnector<,>),
                       new[] {typeof(T), x.MessageType});
        }

        static IEnumerable<IInstanceSubscriptionConnector> ConsumesCorrelated()
        {
            return MessageInterfaceTypeReflector<T>.GetConsumesCorrelatedTypes()
                .Select(CreateCorrelatedConnector);
        }

        static IInstanceSubscriptionConnector CreateCorrelatedConnector(CorrelatedMessageInterfaceType x)
        {
            return (IInstanceSubscriptionConnector)
                   FastActivator.Create(typeof(CorrelatedInstanceSubscriptionConnector<,,>),
                       new[] {typeof(T), x.MessageType, x.CorrelationType});
        }


//        static IEnumerable<InstanceSubscriptionConnector> Distributors()
//        {
//            return MessageInterfaceTypeReflector<T>.GetDistributorTypes()
//                .Select(x => FastActivator.Create(typeof (DistributorSubscriptionConnector<>), new[] {x.MessageType}))
//                .Cast<InstanceSubscriptionConnector>();
//        }
//
//        static IEnumerable<InstanceSubscriptionConnector> Workers()
//        {
//            return MessageInterfaceTypeReflector<T>.GetWorkerTypes()
//                .Select(x => FastActivator.Create(typeof (WorkerSubscriptionConnector<>), new[] {x.MessageType}))
//                .Cast<InstanceSubscriptionConnector>();
//        }
    }
}