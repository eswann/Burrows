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

using Burrows.Configuration.Builders;
using Burrows.Configuration.Configurators;
using Burrows.Endpoints;
using System;
using System.Collections.Generic;
using Magnum.Extensions;
using Burrows.Serialization;
using Burrows.Transports;
using Burrows.Util;

namespace Burrows.Configuration.EndpointConfigurators
{
    /// <summary>
    /// Configure the endpoint
    /// </summary>
    public interface IEndpointConfigurator
    {
        /// <summary>
        /// Specify a serializer for this endpoint (overrides the default)
        /// </summary>
        IEndpointConfigurator UseSerializer(IMessageSerializer serializer);

        /// <summary>
        /// Overrides the default error address with a new error address
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IEndpointConfigurator SetErrorAddress(Uri uri);

        /// <summary>
        /// Overrides the transport factory when the error transport is created, to modify the behavior
        /// </summary>
        /// <param name="transportFactory"></param>
        IEndpointConfigurator SetTransportFactory(DuplexTransportFactory transportFactory);

        /// <summary>
        /// Overrides the transport factory when the error transport is created, to modify the behavior
        /// </summary>
        /// <param name="errorTransportFactory"></param>
        IEndpointConfigurator SetErrorTransportFactory(OutboundTransportFactory errorTransportFactory);

        /// <summary>
        /// Remove any existing messages from the endpoint when it is created
        /// </summary>
        IEndpointConfigurator PurgeExistingMessages();

        /// <summary>
        /// Creates the endpoint if it is missing
        /// </summary>
        IEndpointConfigurator CreateIfMissing();

        /// <summary>
        /// Set the retry limit for inbound messages in the event of an exception
        /// </summary>
        /// <param name="retryLimit">The number of attempts to process the inbound message</param>
        IEndpointConfigurator SetMessageRetryLimit(int retryLimit);

        /// <summary>
        /// Overrides the default message tracker with a custom factory provider
        /// </summary>
        /// <param name="messageTrackerFactory"></param>
        /// <returns></returns>
        IEndpointConfigurator SetInboundMessageTrackerFactory(MessageTrackerFactory messageTrackerFactory);

    }


    public class EndpointConfigurator :
        IEndpointConfigurator,
        IEndpointFactoryBuilderConfigurator
    {
        private readonly Uri _baseUri;
        private readonly EndpointSettings _settings;
        IEndpointAddress _errorAddress;
        OutboundTransportFactory _errorTransportFactory;
        DuplexTransportFactory _transportFactory;

        public EndpointConfigurator([NotNull] IEndpointAddress address,
            [NotNull] IEndpointFactoryDefaultSettings defaultSettings)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (defaultSettings == null)
                throw new ArgumentNullException("defaultSettings");

            _baseUri = new Uri(address.Uri.GetLeftPart(UriPartial.Path));

            _transportFactory = DefaultTransportFactory;
            _errorTransportFactory = DefaultErrorTransportFactory;

            _settings = defaultSettings.CreateEndpointSettings(address);
        }

        public IEndpointConfigurator UseSerializer(IMessageSerializer serializer)
        {
            _settings.Serializer = serializer;
            return this;
        }

        public IEndpointConfigurator SetErrorAddress(Uri uri)
        {
            _errorAddress = new EndpointAddress(uri);
            return this;
        }

        public IEndpointConfigurator PurgeExistingMessages()
        {
            _settings.PurgeExistingMessages = true;
            return this;
        }

        public IEndpointConfigurator SetInboundMessageTrackerFactory(MessageTrackerFactory messageTrackerFactory)
        {
            _settings.TrackerFactory = messageTrackerFactory;
            return this;
        }

        public IEndpointConfigurator CreateIfMissing()
        {
            _settings.CreateIfMissing = true;
            return this;
        }

        public IEndpointConfigurator SetMessageRetryLimit(int retryLimit)
        {
            _settings.RetryLimit = retryLimit;
            return this;
        }

        public IEndpointConfigurator SetTransportFactory(DuplexTransportFactory transportFactory)
        {
            _transportFactory = transportFactory;
            return this;
        }

        public IEndpointConfigurator SetErrorTransportFactory(OutboundTransportFactory errorTransportFactory)
        {
            _errorTransportFactory = errorTransportFactory;
            return this;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            if (_errorAddress != null)
            {
                if (string.Compare(_errorAddress.Uri.Scheme, _settings.Address.Uri.Scheme,
                    StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    yield return this.Failure("ErrorAddress", _errorAddress.ToString(),
                        "The error address ('{0}') must use the same scheme as the endpoint address ('{1}')"
                            .FormatWith(_errorAddress.Uri, _settings.Address.Uri.Scheme));
                }
                else
                    yield return this.Success("ErrorAddress", "Using specified error address: " + _errorAddress);
            }

            if (_transportFactory == null)
                yield return this.Failure("TransportFactory", "The transport factory method is null");

            if (_errorTransportFactory == null)
                yield return this.Failure("ErrorTransportFactory", "The error transport factory method is null");
        }

        public IEndpointFactoryBuilder Configure(IEndpointFactoryBuilder builder)
        {
            IEndpointBuilder endpointBuilder = CreateBuilder();

            builder.AddEndpointBuilder(_baseUri, endpointBuilder);

            return builder;
        }

        public IEndpointBuilder CreateBuilder()
        {
            ITransportSettings errorSettings = new TransportSettings(_errorAddress ?? _settings.ErrorAddress, _settings);

            var endpointBuilder = new EndpointBuilder(_settings.Address, _settings, errorSettings, _transportFactory,
                _errorTransportFactory, () => _settings.TrackerFactory(_settings.RetryLimit));

            return endpointBuilder;
        }

        static IDuplexTransport DefaultTransportFactory(ITransportFactory transportFactory, ITransportSettings settings)
        {
            return transportFactory.BuildLoopback(settings);
        }

        static IOutboundTransport DefaultErrorTransportFactory(ITransportFactory transportFactory,
            ITransportSettings settings)
        {
            return transportFactory.BuildError(settings);
        }
    }
}