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
using Burrows.Configuration.Builders;
using Burrows.Configuration.Configurators;
using Burrows.Exceptions;
using Burrows.Serialization;

namespace Burrows.Configuration.EndpointConfigurators
{
    public class DefaultSerializerEndpointFactoryConfigurator :
        IEndpointFactoryBuilderConfigurator
    {
        private readonly Func<IMessageSerializer> _serializerFactory;

        public DefaultSerializerEndpointFactoryConfigurator(Func<IMessageSerializer> serializerFactory)
        {
            _serializerFactory = serializerFactory;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            if (_serializerFactory == null)
                yield return this.Failure("DefaultSerializer",
                    "was not configured (it was null). The factory method should have been specified.");
        }

        public IEndpointFactoryBuilder Configure(IEndpointFactoryBuilder builder)
        {
            IMessageSerializer serializer = _serializerFactory();
            if (serializer == null)
                throw new ConfigurationException("The configured default serializer was not created");

            builder.SetDefaultSerializer(serializer);

            return builder;
        }
    }
}