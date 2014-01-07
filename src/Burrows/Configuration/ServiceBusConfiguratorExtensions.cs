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

using Burrows.Configuration.BusConfigurators;
using Burrows.Util;

namespace Burrows.Configuration
{
    public static class ServiceBusConfiguratorExtensions
	{
		/// <summary>
		/// Specify the endpoint from which messages should be read
		/// </summary>
		/// <param name="configurator"></param>
		/// <param name="uriString">The uri of the endpoint</param>
        public static IServiceBusConfigurator ReceiveFrom(this IServiceBusConfigurator configurator, string uriString)
		{
			configurator.ReceiveFrom(uriString.ToUri("The receive endpoint URI is invalid"));
		    return configurator;
		}
	}
}