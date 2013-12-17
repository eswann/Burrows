using System;
using System.Threading;
using Magnum.Extensions;
using NUnit.Framework;
using Burrows.Transports.Configuration.Extensions;

namespace Burrows.Tests.RabbitMq
{
    [TestFixture]
    public class When_unable_to_connect_to_RabbitMQ
    {
        [Test, Explicit]
        public void Should_not_destroy_the_box()
        {
            using (IServiceBus bus = ServiceBusFactory.New(c =>
            {
                c.ReceiveFrom(new Uri("rabbitmq://localhost/restricted/no_legs"));
                c.UseRabbitMq();
            }))
            {
                bus.Publish(new A());

                Thread.Sleep(30.Seconds());
            }
        }


        class A
        {
        }
    }
}
