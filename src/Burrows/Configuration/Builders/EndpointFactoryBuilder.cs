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

using Burrows.Configuration.EndpointConfigurators;
using Burrows.Endpoints;
using System;
using System.Collections.Generic;
using Burrows.Serialization;
using Burrows.Transports;
using Burrows.Transports.Loopback;
using Burrows.Util;

namespace Burrows.Configuration.Builders
{
    public interface IEndpointFactoryBuilder
    {
        /// <summary>
        /// Create the endpoint factory
        /// </summary>
        /// <returns></returns>
        IEndpointFactory Build();

        /// <summary>
        /// Sets the default serializer used for endpoints
        /// </summary>
        void SetDefaultSerializer(IMessageSerializer serializerFactory);

        /// <summary>
        /// Sets the flag indicating that missing queues should be created
        /// </summary>
        /// <param name="createMissingQueues"></param>
        void SetCreateMissingQueues(bool createMissingQueues);

        /// <summary>
        /// Specifies if the input queue should be purged on startup
        /// </summary>
        /// <param name="purgeOnStartup"></param>
        void SetPurgeOnStartup(bool purgeOnStartup);

        /// <summary>
        /// Provides a configured endpoint builder for the specified URI
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="endpointBuilder"></param>
        void AddEndpointBuilder(Uri uri, IEndpointBuilder endpointBuilder);

        /// <summary>
        /// Adds a transport factory to the builder
        /// </summary>
        void AddTransportFactory(ITransportFactory transportFactory);


        /// <summary>
        /// Sets the default retry limit for inbound messages
        /// </summary>
        void SetDefaultRetryLimit(int retryLimit);

        /// <summary>
        /// Sets the default message tracker factory for all endpoints
        /// </summary>
        void SetDefaultInboundMessageTrackerFactory(MessageTrackerFactory messageTrackerFactory);

    }

    public class EndpointFactoryBuilder :
        IEndpointFactoryBuilder
    {
        private readonly EndpointFactoryDefaultSettings _defaults;
        private readonly IDictionary<Uri, IEndpointBuilder> _endpointBuilders;
        private readonly IDictionary<string, ITransportFactory> _transportFactories;

        public EndpointFactoryBuilder([NotNull] IEndpointFactoryDefaultSettings defaults)
        {
            if (defaults == null)
                throw new ArgumentNullException("defaults");
            _endpointBuilders = new Dictionary<Uri, IEndpointBuilder>();
            _transportFactories = new Dictionary<string, ITransportFactory>();

            AddTransportFactory(new LoopbackTransportFactory());

            _defaults = new EndpointFactoryDefaultSettings(defaults);

        }

        public IEndpointFactory Build()
        {
            var endpointFactory = new EndpointFactory(_transportFactories, _endpointBuilders, _defaults);

            return endpointFactory;
        }

        public void SetDefaultSerializer(IMessageSerializer defaultSerializer)
        {
            _defaults.Serializer = defaultSerializer;
        }

        public void SetDefaultRetryLimit(int retryLimit)
        {
            _defaults.RetryLimit = retryLimit;
        }

        public void SetDefaultInboundMessageTrackerFactory(MessageTrackerFactory messageTrackerFactory)
        {
            _defaults.TrackerFactory = messageTrackerFactory;
        }

        public void SetCreateMissingQueues(bool createMissingQueues)
        {
            _defaults.CreateMissingQueues = createMissingQueues;
        }

        public void SetPurgeOnStartup(bool purgeOnStartup)
        {
            _defaults.PurgeOnStartup = purgeOnStartup;
        }

        public void AddEndpointBuilder(Uri uri, IEndpointBuilder endpointBuilder)
        {
            _endpointBuilders[uri] = endpointBuilder;
        }

        public void AddTransportFactory(ITransportFactory transportFactory)
        {
            string scheme = transportFactory.Scheme.ToLowerInvariant();

            _transportFactories[scheme] = transportFactory;
        }
    }
}