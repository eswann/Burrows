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
using System.Collections.Concurrent;
using Burrows.Endpoints;
using Burrows.Exceptions;
using Burrows.Transports;
using Burrows.Transports.Rabbit;
using Magnum.Extensions;

namespace Burrows.RabbitCommands
{
    public class TransportCache : IDisposable
	{
		readonly ITransportFactory _transportFactory = new RabbitTransportFactory();
        readonly ConcurrentDictionary<string, IInboundTransport> _inboundTransports = new ConcurrentDictionary<string, IInboundTransport>();
        readonly ConcurrentDictionary<string, IOutboundTransport> _outboundTransports = new ConcurrentDictionary<string, IOutboundTransport>();
		bool _disposed;

        public IInboundTransport GetInboundTransport(Uri uri)
        {
            string key = uri.ToString().ToLowerInvariant();
            IInboundTransport transport;
            if (_inboundTransports.TryGetValue(key, out transport))
                return transport;

            try
            {
                ITransportSettings settings = new TransportSettings(new EndpointAddress(uri));
                transport = _transportFactory.BuildInbound(settings);

                _inboundTransports.TryAdd(uri.ToString().ToLowerInvariant(), transport);

                return transport;
            }
            catch (Exception ex)
            {
                throw new TransportException(uri, "Failed to create inbound transport", ex);
            }
        }

        public IOutboundTransport GetOutboundTransport(Uri uri)
		{
			string key = uri.ToString().ToLowerInvariant();

            IOutboundTransport transport;
            if (_outboundTransports.TryGetValue(key, out transport))
				return transport;

			try
			{
				ITransportSettings settings = new TransportSettings(new EndpointAddress(uri));
				transport = _transportFactory.BuildOutbound(settings);

                _outboundTransports.TryAdd(uri.ToString().ToLowerInvariant(), transport);

				return transport;
			}
			catch (Exception ex)
			{
				throw new TransportException(uri, "Failed to create outbound transport", ex);
			}
		}


        public void Dispose()
        {
            Dispose(true);
        }

		private void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				_inboundTransports.Values.Each(x => x.Dispose());
				_outboundTransports.Values.Each(x => x.Dispose());
				_transportFactory.Dispose();
			}

			_disposed = true;
		}
	}
}