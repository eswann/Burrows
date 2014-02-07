using System;
using Burrows.Publishing;
using Burrows.Tests.Messages;

namespace Burrows.Tests.PublishSubscribeConsole.Consumers
{
    public class SubscribeConsoleConsumer : Consumes<SimpleMessage>.All
    {
        private readonly IPublisher _publisher;

        public SubscribeConsoleConsumer(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public void Consume(SimpleMessage message)
        {
            Console.WriteLine("Just got a message");

            _publisher.Publish(new PublishFromConsumerMessage{Id = message.Id, Name = message.Name});

           //throw new Exception("I failed!!!!!");
        }
    }
}