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
using Burrows.Configuration.Configuration;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.SubscriptionBuilders;
using Magnum.Extensions;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    public class UntypedConsumerSubscriptionConfigurator<TConsumer> :
        SubscriptionConfigurator<IConsumerSubscriptionConfigurator>,
        IConsumerSubscriptionConfigurator,
        ISubscriptionBuilderConfigurator
        where TConsumer : class
    {
        private readonly IConsumerFactory<TConsumer> _consumerFactory;

        public UntypedConsumerSubscriptionConfigurator(Func<Type, object> consumerFactory)
        {
            _consumerFactory =
                new DelegateConsumerFactory<TConsumer>(() => (TConsumer)consumerFactory(typeof(TConsumer)));
        }

        public IEnumerable<IValidationResult> Validate()
        {
            if (_consumerFactory == null)
                yield return this.Failure("The consumer factory cannot be null.");

            if (!typeof(TConsumer).Implements<IConsumer>())
                yield return
                    this.Warning(string.Format("The consumer class {0} does not implement any IConsumer interfaces",
                        typeof(TConsumer).ToShortTypeName()));
        }

        public ISubscriptionBuilder Configure()
        {
            return new ConsumerSubscriptionBuilder<TConsumer>(_consumerFactory, ReferenceFactory);
        }
    }
}