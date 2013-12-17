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
using Burrows.Configuration.Builders;
using Burrows.Configuration.Configurators;

namespace Burrows.Configuration.EndpointConfigurators
{
    public class DefaultRetryLimitEndpointFactoryConfigurator :
        IEndpointFactoryBuilderConfigurator
    {
        private readonly int _retryLimit;

        public DefaultRetryLimitEndpointFactoryConfigurator(int retryLimit)
        {
            _retryLimit = retryLimit;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            if (_retryLimit < 0)
                yield return this.Failure("RetryLimit",
                    "must be >= 0.");
        }

        public IEndpointFactoryBuilder Configure(IEndpointFactoryBuilder builder)
        {
            builder.SetDefaultRetryLimit(_retryLimit);

            return builder;
        }
    }
}