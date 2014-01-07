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
using Burrows.Configuration.BusConfigurators;
using Burrows.Configuration.Configurators;
using Burrows.Diagnostics;
using Burrows.Exceptions;
using Magnum;
using Burrows.Util;

namespace Burrows.Configuration
{
    /// <summary>
	/// The starting point to configure and create a service bus instance
	/// </summary>
	public static class ServiceBusFactory
	{
		static readonly ServiceBusDefaultSettings _defaultSettings = new ServiceBusDefaultSettings();

		[NotNull]
		public static IServiceBus New([NotNull] Action<IServiceBusConfigurator> configure)
		{
			Guard.AgainstNull(configure, "configure");

			var configurator = new ServiceBusConfigurator(_defaultSettings);

			configurator.EnableMessageTracing();

			configure(configurator);

			var result = ConfigurationResult.CompileResults(configurator.Validate());

			try
			{
				return configurator.CreateServiceBus();
			}
			catch (Exception ex)
			{
				throw new ConfigurationException(result, "An exception was thrown during service bus creation", ex);
			}
		}

		public static void ConfigureDefaultSettings([NotNull] Action<IServiceBusDefaultSettingsConfigurator> configure)
		{
			Guard.AgainstNull(configure);

			var configurator = new ServiceBusDefaultSettingsConfigurator(_defaultSettings);

			configure(configurator);
		}
	}
}