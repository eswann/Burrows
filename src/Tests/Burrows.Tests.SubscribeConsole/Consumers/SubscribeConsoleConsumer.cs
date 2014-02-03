using System;
using Burrows.Tests.Messages;

namespace Burrows.Tests.SubscribeConsole.Consumers
{
    public class SubscribeConsoleConsumer : Consumes<PublishFromConsumerMessage>.All
    {
        public void Consume(PublishFromConsumerMessage message)
        {
            Console.WriteLine("Just got a Publish From Consumer message");

           //throw new Exception("I failed!!!!!");
        }
    }
}