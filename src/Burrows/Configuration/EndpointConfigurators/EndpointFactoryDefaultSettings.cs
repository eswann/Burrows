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
using Burrows.Serialization;
using Burrows.Transports;
using Burrows.Util;

namespace Burrows.Configuration.EndpointConfigurators
{
    public class EndpointFactoryDefaultSettings : IEndpointFactoryDefaultSettings
    {
        public EndpointFactoryDefaultSettings()
        {
            CreateMissingQueues = true;
            PurgeOnStartup = false;
            Serializer = new JsonMessageSerializer();
            RetryLimit = 5;
            TrackerFactory = DefaultTrackerFactory;
        }

        static IInboundMessageTracker DefaultTrackerFactory(int retryLimit)
        {
            return new InMemoryInboundMessageTracker(retryLimit);
        }

        public EndpointFactoryDefaultSettings([NotNull] IEndpointFactoryDefaultSettings defaults)
        {
            if (defaults == null)
                throw new ArgumentNullException("defaults");

            CreateMissingQueues = defaults.CreateMissingQueues;
            PurgeOnStartup = defaults.PurgeOnStartup;
            Serializer = defaults.Serializer;
            RetryLimit = defaults.RetryLimit;
            TrackerFactory = defaults.TrackerFactory;
        }

        public bool CreateMissingQueues { get; set; }
        public int RetryLimit { get; set; }
        public MessageTrackerFactory TrackerFactory { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public bool PurgeOnStartup { get; set; }

        public EndpointSettings CreateEndpointSettings(IEndpointAddress address)
        {
            var settings = new EndpointSettings(address)
                {
                    Serializer = Serializer,
                    CreateIfMissing = CreateMissingQueues,
                    PurgeExistingMessages = PurgeOnStartup,
                    RetryLimit = RetryLimit,
                    TrackerFactory = TrackerFactory
                };

            return settings;
        }
    }
}