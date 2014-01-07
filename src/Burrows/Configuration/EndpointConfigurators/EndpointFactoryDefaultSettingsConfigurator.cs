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

using Burrows.Serialization;

namespace Burrows.Configuration.EndpointConfigurators
{
    public interface IEndpointFactoryDefaultSettingsConfigurator
    {
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
    }

	public class EndpointFactoryDefaultSettingsConfigurator :
		IEndpointFactoryDefaultSettingsConfigurator
	{
		readonly EndpointFactoryDefaultSettings _defaults;

		public EndpointFactoryDefaultSettingsConfigurator(EndpointFactoryDefaultSettings defaults)
		{
			_defaults = defaults;
		}

		public void SetDefaultSerializer(IMessageSerializer defaultSerializer)
		{
			_defaults.Serializer = defaultSerializer;
		}

		public void SetCreateMissingQueues(bool createMissingQueues)
		{
			_defaults.CreateMissingQueues = createMissingQueues;
		}

		public void SetPurgeOnStartup(bool purgeOnStartup)
		{
			_defaults.PurgeOnStartup = purgeOnStartup;
		}
	}
}