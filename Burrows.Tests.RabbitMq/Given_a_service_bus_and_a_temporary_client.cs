using System;
using Burrows.BusConfigurators;
using Magnum.TestFramework;
using NUnit.Framework;
using Burrows.Transports.Configuration.Extensions;

namespace Burrows.Tests.RabbitMq
{
    [Scenario]
    public abstract class Given_a_service_bus_and_a_temporary_client
    {
        protected Uri LocalUri { get; set; }
        protected Uri RemoteUri { get; set; }

        protected IServiceBus LocalBus { get; private set; }
        protected IServiceBus RemoteBus { get; private set; }
        protected Uri LocalErrorUri { get; private set; }

        [TestFixtureSetUp]
        public void LocalAndRemoteTestFixtureSetup()
        {
            LocalUri = new Uri("rabbitmq://localhost:5672/*?temporary=true&prefetch=1");

            RemoteUri = new Uri("rabbitmq://localhost:5672/test_remote_queue");

            LocalBus = SetupServiceBus(LocalUri, ConfigureLocalBus);
            RemoteBus = SetupServiceBus(RemoteUri, ConfigureRemoteBus);
        }

        [TestFixtureTearDown]
        public void LocalAndRemoteTestFixtureTeardown()
        {
            LocalBus.Dispose();
            LocalBus = null;

            RemoteBus.Dispose();
            RemoteBus = null;
        }

        protected virtual void ConfigureLocalBus(IServiceBusConfigurator configurator)
        {
        }

        protected virtual void ConfigureRemoteBus(IServiceBusConfigurator configurator)
        {
        }

        protected IServiceBus SetupServiceBus(Uri uri, Action<IServiceBusConfigurator> configure)
        {
            IServiceBus bus = ServiceBusFactory.New(x =>
                {
                    x.ReceiveFrom(uri);
                    x.UseRabbitMq();

                    configure(x);
                });

            return bus;
        }
    }
}