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
using Burrows.Configuration;
using Burrows.Configuration.BusConfigurators;
using Burrows.Transports;
using NUnit.Framework;
using Burrows.Saga;
using Burrows.Services.Subscriptions.Configuration;
using Burrows.Services.Subscriptions.Server;

namespace Burrows.Tests.Framework.Fixtures
{
    [TestFixture, Ignore]
	public class SubscriptionServiceTestFixture<TTransportFactory> :
		EndpointTestFixture<TTransportFactory>
		where TTransportFactory : class, ITransportFactory, new()
	{
		[TestFixtureSetUp]
		public void LocalAndRemoteTestFixtureSetup()
		{
			SetupSubscriptionService();

			LocalBus = SetupServiceBus(LocalUri);
			RemoteBus = SetupServiceBus(RemoteUri);
		}

		void SetupSubscriptionService()
		{
			SubscriptionClientSagaRepository = SetupSagaRepository<SubscriptionClientSaga>();
			SubscriptionSagaRepository = SetupSagaRepository<SubscriptionSaga>();

			SubscriptionBus = SetupServiceBus(SubscriptionUri, x => x.SetConcurrentConsumerLimit(1));

			SubscriptionService = new SubscriptionService(SubscriptionBus,
				SubscriptionSagaRepository,
				SubscriptionClientSagaRepository);

			SubscriptionService.Start();
		}

		protected InMemorySagaRepository<SubscriptionSaga> SubscriptionSagaRepository { get; private set; }

		protected InMemorySagaRepository<SubscriptionClientSaga> SubscriptionClientSagaRepository { get; private set; }

		[TestFixtureTearDown]
		public void LocalAndRemoteTestFixtureTeardown()
		{
			LocalBus = null;
			RemoteBus = null;
			SubscriptionService = null;
		}

		protected Uri LocalUri { get; set; }
		protected Uri RemoteUri { get; set; }
		protected Uri SubscriptionUri { get; set; }

		protected IServiceBus LocalBus { get; private set; }
		protected IServiceBus RemoteBus { get; private set; }
		protected IServiceBus SubscriptionBus { get; private set; }

		protected SubscriptionService SubscriptionService { get; private set; }

		protected override void ConfigureServiceBus(Uri uri, IServiceBusConfigurator configurator)
		{
			base.ConfigureServiceBus(uri, configurator);

			configurator.UseControlBus();
			configurator.UseSubscriptionService(SubscriptionUri);
		}
	}
}