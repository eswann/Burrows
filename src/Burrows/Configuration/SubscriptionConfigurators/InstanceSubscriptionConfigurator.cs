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

using System.Collections.Generic;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.SubscriptionBuilders;
using Magnum.Extensions;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    public interface IInstanceSubscriptionConfigurator :
    ISubscriptionConfigurator<IInstanceSubscriptionConfigurator>
    {
    }

    public class InstanceSubscriptionConfigurator :
        SubscriptionConfigurator<IInstanceSubscriptionConfigurator>,
        IInstanceSubscriptionConfigurator,
        ISubscriptionBuilderConfigurator
    {
        private readonly object _instance;

        public InstanceSubscriptionConfigurator(object instance)
        {
            _instance = instance;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            if (_instance == null)
                yield return this.Failure("The instance cannot be null. This should have come in the ctor.");

            if (_instance != null && !_instance.GetType().Implements<IConsumer>())
                yield return
                    this.Warning(string.Format("The instance of {0} does not implement any IConsumer interfaces",
                        _instance.GetType().ToShortTypeName()));
        }

        public ISubscriptionBuilder Configure()
        {
            return new InstanceSubscriptionBuilder(_instance, ReferenceFactory);
        }
    }
}