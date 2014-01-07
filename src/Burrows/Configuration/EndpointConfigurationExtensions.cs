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

using Burrows.Configuration.EndpointConfigurators;
using Burrows.Endpoints;
using System;
using Burrows.Util;

namespace Burrows.Configuration
{
    public static class EndpointConfigurationExtensions
	{
		/// <summary>
		/// Returns a configurator for the specified endpoint URI
		/// </summary>
		/// <param name="configurator"></param>
		/// <param name="uriString"></param>
		/// <returns></returns>
		public static IEndpointConfigurator ConfigureEndpoint<T>(this T configurator, string uriString)
			where T : IEndpointFactoryConfigurator
		{
			return configurator.ConfigureEndpoint(uriString.ToUri("The configure endpoint URI is invalid"));
		}

		/// <summary>
		/// Returns a configurator for the specified endpoint URI
		/// </summary>
		/// <param name="configurator"></param>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static IEndpointConfigurator ConfigureEndpoint<T>(this T configurator, Uri uri)
			where T : IEndpointFactoryConfigurator
		{
			var endpointConfigurator = new EndpointConfigurator(new EndpointAddress(uri), configurator.Defaults);

			configurator.AddEndpointFactoryConfigurator(endpointConfigurator);

			return endpointConfigurator;
		}

		/// <summary>
		/// Configures the endpoint for the specified endpoint URI
		/// </summary>
		/// <param name="configurator"></param>
		/// <param name="uriString"></param>
		/// <param name="configureCallback"></param>
		/// <returns></returns>
		public static T ConfigureEndpoint<T>(this T configurator, string uriString,
		                                     Action<IEndpointConfigurator> configureCallback)
			where T : IEndpointFactoryConfigurator
		{
			IEndpointConfigurator endpointConfigurator =
				configurator.ConfigureEndpoint(uriString.ToUri("The configure endpoint URI is invalid"));

			configureCallback(endpointConfigurator);

			return configurator;
		}

		/// <summary>
		/// Configures the endpoint for the specified URI
		/// </summary>
		/// <param name="configurator"></param>
		/// <param name="uri"></param>
		/// <param name="configureCallback"></param>
		public static T ConfigureEndpoint<T>(this T configurator, Uri uri,
		                                     Action<IEndpointConfigurator> configureCallback)
			where T : IEndpointFactoryConfigurator
		{
			IEndpointConfigurator endpointConfigurator = configurator.ConfigureEndpoint(uri);

			configureCallback(endpointConfigurator);

			return configurator;
		}
	}
}