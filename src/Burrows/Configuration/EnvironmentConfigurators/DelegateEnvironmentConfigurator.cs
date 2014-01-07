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
using Burrows.Configuration.Configuration;

namespace Burrows.Configuration.EnvironmentConfigurators
{
    public class DelegateEnvironmentConfigurator :
		IServiceBusEnvironment
	{
		readonly Action<IServiceBusConfigurator> _callback;

		public DelegateEnvironmentConfigurator(Action<IServiceBusConfigurator> callback)
		{
			_callback = callback;
		}

		public void Configure(IServiceBusConfigurator configurator)
		{
			_callback(configurator);
		}
	}
}