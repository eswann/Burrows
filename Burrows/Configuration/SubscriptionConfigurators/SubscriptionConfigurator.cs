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

using Burrows.Subscriptions;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    /// <summary>
    /// The base configuration interface for a subscription
    /// </summary>
    public interface ISubscriptionConfigurator<out TInterface>
        where TInterface : class
    {
        TInterface Permanent();
        TInterface Transient();
        TInterface SetReferenceFactory(ReferenceFactory referenceFactory);
    }

    public class SubscriptionConfigurator<TInterface> :
        ISubscriptionConfigurator<TInterface>
        where TInterface : class, ISubscriptionConfigurator<TInterface>
    {
        ReferenceFactory _referenceFactory;

        protected SubscriptionConfigurator()
        {
            Permanent();
        }

        protected ReferenceFactory ReferenceFactory
        {
            get { return _referenceFactory; }
        }

        public TInterface Permanent()
        {
            _referenceFactory = PermanentSubscriptionReference.Create;

            return this as TInterface;
        }

        public TInterface Transient()
        {
            _referenceFactory = TransientSubscriptionReference.Create;

            return this as TInterface;
        }

        public TInterface SetReferenceFactory(ReferenceFactory referenceFactory)
        {
            _referenceFactory = referenceFactory;

            return this as TInterface;
        }
    }
}