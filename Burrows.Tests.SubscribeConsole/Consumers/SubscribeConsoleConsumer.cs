using System;
using Burrows.Tests.Messages;

namespace Burrows.Tests.SubscribeConsole.Consumers
{
    public class SubscribeConsoleConsumer : Consumes<SimpleMessage>.All
    {
        public void Consume(SimpleMessage message)
        {
            Console.WriteLine("Just got a message");
        }
    }
}