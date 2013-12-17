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

using Burrows.Endpoints;
using System;
using Burrows.Exceptions;
using Burrows.Transports;
using Burrows.Util;

namespace Burrows.Configuration.Builders
{
    public interface IEndpointBuilder
    {
        IEndpoint CreateEndpoint(ITransportFactory transportFactory);
    }

    public class EndpointBuilder :
        IEndpointBuilder
    {
        private readonly IEndpointAddress _address;
        private readonly ITransportSettings _errorSettings;
        private readonly OutboundTransportFactory _errorTransportFactory;
        private readonly Func<IInboundMessageTracker> _messageTrackerFactory;
        private readonly EndpointSettings _settings;
        private readonly DuplexTransportFactory _transportFactory;

        public EndpointBuilder([NotNull] IEndpointAddress address, [NotNull] EndpointSettings settings,
            [NotNull] ITransportSettings errorSettings, [NotNull] DuplexTransportFactory transportFactory,
            [NotNull] OutboundTransportFactory errorTransportFactory,
            [NotNull] Func<IInboundMessageTracker> messageTrackerFactory)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _address = address;
            _settings = settings;
            _errorSettings = errorSettings;
            _transportFactory = transportFactory;
            _errorTransportFactory = errorTransportFactory;
            _messageTrackerFactory = messageTrackerFactory;
        }

        public IEndpoint CreateEndpoint(ITransportFactory transportFactory)
        {
            try
            {
                IDuplexTransport transport = _transportFactory(transportFactory, _settings);
                IOutboundTransport errorTransport = _errorTransportFactory(transportFactory, _errorSettings);
                IInboundMessageTracker tracker = _messageTrackerFactory();

                var endpoint = new Endpoint(transport.Address, _settings.Serializer, transport, errorTransport, tracker);

                return endpoint;
            }
            catch (Exception ex)
            {
                throw new EndpointException(_address.Uri, "Failed to create endpoint", ex);
            }
        }
    }
}