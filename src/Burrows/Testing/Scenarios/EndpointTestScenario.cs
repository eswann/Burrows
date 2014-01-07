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

using Burrows.Configuration;
using Burrows.Endpoints;

namespace Burrows.Testing.Scenarios
{
    using System;
    using System.Collections.Generic;
    using Diagnostics.Introspection;
    using Magnum.Extensions;
    using TestDecorators;
    using Transports;

    /// <summary>
    /// Adds the further feature onto <see cref="ITestScenario"/> of having 
    /// an endpoint cache and an endpoint factory. This is useful if you are 
    /// testing that you can send messages to what can be considered 'endpoints' rather than
    /// just 'subscribers'. It's more in line to the usage scenario of request-reply or fire-and-forget
    /// rather than publish-subscribe.
    /// </summary>
    public interface IEndpointTestScenario :
        ITestScenario
    {
        /// <summary>
        /// Gets the endpoint cache. Use this instance to find the <see cref="IEndpoint"/>s.
        /// </summary>
        IEndpointCache EndpointCache { get; }

        /// <summary>
        /// Gets the endpoint factory.
        /// </summary>
        IEndpointFactory EndpointFactory { get; }
    }

	public abstract class EndpointTestScenario : IEndpointTestScenario
	{
		readonly EndpointCache _endpointCache;
		readonly IDictionary<Uri, EndpointTestDecorator> _endpoints;
		readonly ReceivedMessageList _received;
		readonly PublishedMessageList _published;
		readonly SentMessageList _sent;
		readonly ReceivedMessageList _skipped;
		bool _disposed;

		protected EndpointTestScenario(IEndpointFactory endpointFactory)
		{
			_received = new ReceivedMessageList();
			_sent = new SentMessageList();
			_skipped = new ReceivedMessageList();
			_published = new PublishedMessageList();

			_endpoints = new Dictionary<Uri, EndpointTestDecorator>();

			EndpointFactory = new EndpointFactoryTestDecorator(endpointFactory, this);

			_endpointCache = new EndpointCache(EndpointFactory);

			EndpointCache = new EndpointCacheProxy(_endpointCache);

			ServiceBusFactory.ConfigureDefaultSettings(x =>
				{
					x.SetEndpointCache(EndpointCache);
					x.SetConcurrentConsumerLimit(4);
					x.SetConcurrentReceiverLimit(1);
					x.SetReceiveTimeout(50.Milliseconds());
					x.EnableAutoStart();
				});
		}

		public IEndpointCache EndpointCache { get; private set; }
		public IEndpointFactory EndpointFactory { get; private set; }

		public ISentMessageList Sent
		{
			get { return _sent; }
		}

		public IReceivedMessageList Skipped
		{
			get { return _skipped; }
		}

		public virtual IServiceBus InputBus
		{
			get { throw new NotImplementedException(); }
		}

	    public virtual IServiceBus OutputBus
	    {
            get { throw new NotImplementedException(); }
	    }

		public IPublishedMessageList Published
		{
			get { return _published; }
		}

		public IReceivedMessageList Received
		{
			get { return _received; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void AddEndpoint(EndpointTestDecorator endpoint)
		{
			_endpoints[endpoint.Address.Uri] = endpoint;
		}

		public void AddSent(ISentMessage message)
		{
			_sent.Add(message);
		}

		public void AddPublished(IPublishedMessage message)
		{
			_published.Add(message);
		}

		public void AddReceived(IReceivedMessage message)
		{
			_received.Add(message);

			_skipped.Remove(message);
		}

		public void AddSkipped(IReceivedMessage message)
		{
			_skipped.Add(message);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				_sent.Dispose();
				_received.Dispose();

				_endpointCache.Clear();

				if (EndpointCache != null)
				{
					EndpointCache.Dispose();
					EndpointCache = null;
				}

				ServiceBusFactory.ConfigureDefaultSettings(x => x.SetEndpointCache(null));
			}

			_disposed = true;
		}

		class EndpointCacheProxy :
			IEndpointCache
		{
			readonly IEndpointCache _endpointCache;

			public EndpointCacheProxy(IEndpointCache endpointCache)
			{
				_endpointCache = endpointCache;
			}

			public void Dispose()
			{
				// we don't dispose, since we're in testing
			}

			public IEndpoint GetEndpoint(Uri uri)
			{
				return _endpointCache.GetEndpoint(uri);
			}

		    public void Inspect(IDiagnosticsProbe probe)
		    {
		        _endpointCache.Inspect(probe);
		    }
		}

		public virtual IServiceBus GetDecoratedBus(IServiceBus bus)
		{
			return new ServiceBusTestDecorator(bus, this);
		}
	}
}