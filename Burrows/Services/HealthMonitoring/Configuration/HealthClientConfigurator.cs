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
using Burrows.Configuration.BusServiceConfigurators;

namespace Burrows.Services.HealthMonitoring.Configuration
{
    using System;

    public class HealthClientConfigurator :
		IBusServiceConfigurator
	{
		int _intervalInSeconds;

		public Type ServiceType
		{
			get { return typeof (HealthClient); }
		}

		public IBusServiceLayer Layer
		{
			get { return IBusServiceLayer.Session; }
		}

		public IBusService Create(IServiceBus bus)
		{
			var service = new HealthClient(_intervalInSeconds);

			return service;
		}

		public void SetHeartbeatInterval(int seconds)
		{
			_intervalInSeconds = seconds;
		}
	}
}