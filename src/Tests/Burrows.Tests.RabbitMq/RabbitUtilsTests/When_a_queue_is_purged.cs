using System;
using Burrows.Configuration;
using Burrows.Configuration.BusConfigurators;
using Burrows.RabbitCommands;
using Burrows.Tests.Framework;
using NUnit.Framework;

namespace Burrows.Tests.RabbitMq.RabbitUtilsTests
{
    [TestFixture]
    public class When_a_queue_is_purged : Given_a_rabbitmq_bus
    {
        Future<TestMessage> _received;

        protected override void ConfigureServiceBus(Uri uri, IServiceBusConfigurator configurator)
        {
            base.ConfigureServiceBus(uri, configurator);
            _received = new Future<TestMessage>();

            configurator.Subscribe(s => s.Handler<TestMessage>(message => _received.Complete(message)));
        }
        
        [Test]
        public void CanPurgeQueue()
        {
            //publish a few messages to set up the subscriber queue
            LocalBus.Publish(new TestMessage { Message = "ValueA" });

            var purgeCommand = new PurgeCommand(LocalUri.ToString());

            purgeCommand.Execute();
        }

        class TestMessage
        {
            public string Message { get; set; }
        }
    }
}