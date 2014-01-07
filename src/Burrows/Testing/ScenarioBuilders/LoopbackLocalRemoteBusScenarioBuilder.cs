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
using Burrows.Configuration.BusConfigurators;
using Burrows.Endpoints;

namespace Burrows.Testing.ScenarioBuilders
{
    using System;
    using Magnum.Extensions;
    using Scenarios;
    using Subscriptions.Coordinator;
    using Transports;

    public class LoopbackLocalRemoteBusScenarioBuilder :
		EndpointScenarioBuilder<ILocalRemoteTestScenario>,
		ILocalRemoteScenarioBuilder
	{
		const string DefaultLocalUri = "loopback://localhost/mt_client";
		const string DefaultRemoteUri = "loopback://localhost/mt_server";

		readonly ServiceBusConfigurator _localConfigurator;
		readonly ServiceBusConfigurator _remoteConfigurator;
		readonly ServiceBusDefaultSettings _settings;
		SubscriptionLoopback _localLoopback;
		SubscriptionLoopback _remoteLoopback;

		public LoopbackLocalRemoteBusScenarioBuilder()
		{
			_settings = new ServiceBusDefaultSettings();
			_settings.ConcurrentConsumerLimit = 4;
			_settings.ReceiveTimeout = 50.Milliseconds();

			_localConfigurator = new ServiceBusConfigurator(_settings);
			_localConfigurator.ReceiveFrom(DefaultLocalUri);

			_remoteConfigurator = new ServiceBusConfigurator(_settings);
			_remoteConfigurator.ReceiveFrom(DefaultRemoteUri);
		}

		public void ConfigureLocalBus(Action<IServiceBusConfigurator> configureCallback)
		{
			configureCallback(_localConfigurator);
		}

		public void ConfigureRemoteBus(Action<IServiceBusConfigurator> configureCallback)
		{
			configureCallback(_remoteConfigurator);
		}

		public override ILocalRemoteTestScenario Build()
		{
			IEndpointFactory endpointFactory = BuildEndpointFactory();

			var scenario = new LocalRemoteTestScenario(endpointFactory);

			BuildLocalBus(scenario);
			BuildRemoteBus(scenario);

			_localLoopback.SetTargetCoordinator(_remoteLoopback.Router);
			_remoteLoopback.SetTargetCoordinator(_localLoopback.Router);

			return scenario;
		}

		protected virtual void BuildLocalBus(LocalRemoteTestScenario scenario)
		{
			_localConfigurator.ChangeSettings(x => { x.EndpointCache = scenario.EndpointCache; });

			_localConfigurator.AddSubscriptionObserver((bus, coordinator) =>
				{
					_localLoopback = new SubscriptionLoopback(bus, coordinator);
					return _localLoopback;
				});

			scenario.LocalBus = _localConfigurator.CreateServiceBus();
		}

		protected virtual void BuildRemoteBus(LocalRemoteTestScenario scenario)
		{
			_remoteConfigurator.ChangeSettings(x => { x.EndpointCache = scenario.EndpointCache; });

			_remoteConfigurator.AddSubscriptionObserver((bus, coordinator) =>
				{
					_remoteLoopback = new SubscriptionLoopback(bus, coordinator);
					return _remoteLoopback;
				});

			scenario.RemoteBus = _remoteConfigurator.CreateServiceBus();
		}
	}
}