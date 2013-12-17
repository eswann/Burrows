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
using Burrows.Configuration.Configurators;
using Burrows.Transports.Configuration.Builders;
using Burrows.Transports.Configuration.Configurators;
using Burrows.Transports.PublisherConfirm;
using System.Collections.Generic;

namespace Burrows.Configuration.EndpointConfigurators
{
    /// <summary>
    /// Configures SSL/TLS for RabbitMQ. See http://www.rabbitmq.com/ssl.html
    /// for details on how to set up RabbitMQ for SSL.
    /// </summary>
    public interface IPublisherConfirmFactoryConfigurator
    {
    }

    public class PublisherConfirmFactoryConfigurator :
        IPublisherConfirmFactoryConfigurator,
        ITransportFactoryBuilderConfigurator
    {
        private readonly bool _usePublisherConfirms;
        private readonly int _testNacks;
        private readonly Action<IEnumerable<string>> _acktion;
        private readonly Action<IEnumerable<string>> _nacktion;

        public PublisherConfirmFactoryConfigurator(bool usePublisherConfirms, 
            Action<IEnumerable<string>> acktion, Action<IEnumerable<string>> nacktion, int testNacks)
        {
            _usePublisherConfirms = usePublisherConfirms;
            _acktion = acktion;
            _nacktion = nacktion;
            _testNacks = testNacks;
        }

        public ITransportFactoryBuilder Configure(ITransportFactoryBuilder builder)
        {
            builder.SetPublisherConfirmSettings(
                new PublisherConfirmSettings
                    {
                        UsePublisherConfirms = _usePublisherConfirms,
                        Acktion = _acktion,
                        Nacktion = _nacktion,
                        TestNacks = _testNacks
                    }
                );

            return builder;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            if (_usePublisherConfirms)
            {
                if (_acktion == null)
                    yield return this.Failure("Acktion", "Acktion must be specified if publisher confirms are enabled");
                if (_nacktion == null)
                    yield return
                        this.Failure("Nacktion", "Nacktion must be specified if publisher confirms are enabled");
            }
        }
    }
}