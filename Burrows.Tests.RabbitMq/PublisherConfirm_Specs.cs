using Burrows.BusConfigurators;
using Burrows.Tests.Framework;
using Magnum.Extensions;
using Magnum.TestFramework;
using NUnit.Framework;
using Burrows.Transports.Configuration.Configurators;

namespace Burrows.Tests.RabbitMq
{
    [TestFixture]
    public class PublisherConfirm_Specs:
        Given_two_rabbitmq_buses_walk_into_a_bar
    {
        [Test]
        public void Should_call_the_ack_method_upon_delivery()
        {
            RemoteBus.Publish(new A
                {
                    StringA = "ValueA",
                });

            _received.WaitUntilCompleted(8.Seconds()).ShouldBeTrue();
        }

        Future<A> _received;

        protected override void ConfigureLocalBus(IServiceBusConfigurator configurator)
        {
            base.ConfigureLocalBus(configurator);


            _received = new Future<A>();

            configurator.Subscribe(s => s.Handler<A>(message => _received.Complete(message)));
        }

        protected override void ConfigureRabbitMq(ITransportFactoryConfigurator configurator)
        {
            base.ConfigureRabbitMq(configurator);
        }

        class A
        {
            public string StringA { get; set; }
        }

        class B
        {
            public string StringB { get; set; }
        }
    }
}
