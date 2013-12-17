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

using Burrows.Configuration.SubscriptionConnectors;
using Burrows.Pipeline;
using Burrows.Subscriptions;

namespace Burrows.Configuration.SubscriptionBuilders
{
    public class ConsumerSubscriptionBuilder<TConsumer> :
        ISubscriptionBuilder
        where TConsumer : class
    {
        private readonly IConsumerConnector _consumerConnector;
        private readonly IConsumerFactory<TConsumer> _consumerFactory;
        private readonly ReferenceFactory _referenceFactory;

        public ConsumerSubscriptionBuilder(IConsumerFactory<TConsumer> consumerFactory,
            ReferenceFactory referenceFactory)
        {
            _consumerFactory = consumerFactory;
            _referenceFactory = referenceFactory;

            _consumerConnector = ConsumerConnectorCache.GetConsumerConnector<TConsumer>();
        }

        public ISubscriptionReference Subscribe(IInboundPipelineConfigurator configurator)
        {
            UnsubscribeAction unsubscribe = _consumerConnector.Connect(configurator, _consumerFactory);

            return _referenceFactory(unsubscribe);
        }
    }
}