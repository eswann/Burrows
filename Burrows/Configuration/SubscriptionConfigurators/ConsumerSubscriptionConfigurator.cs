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

using System.Collections.Generic;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.SubscriptionBuilders;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    public interface IConsumerSubscriptionConfigurator :
    ISubscriptionConfigurator<IConsumerSubscriptionConfigurator>
    {

    }

    public interface IConsumerSubscriptionConfigurator<TConsumer> :
        ISubscriptionConfigurator<IConsumerSubscriptionConfigurator<TConsumer>>
        where TConsumer : class
    {
    }

    public class ConsumerSubscriptionConfigurator<TConsumer> :
        SubscriptionConfigurator<IConsumerSubscriptionConfigurator<TConsumer>>,
        IConsumerSubscriptionConfigurator<TConsumer>,
        ISubscriptionBuilderConfigurator
        where TConsumer : class
    {
        private readonly IConsumerFactory<TConsumer> _consumerFactory;

        public ConsumerSubscriptionConfigurator(IConsumerFactory<TConsumer> consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            return _consumerFactory.Validate();
        }

        public ISubscriptionBuilder Configure()
        {
            return new ConsumerSubscriptionBuilder<TConsumer>(_consumerFactory, ReferenceFactory);
        }
    }
}