﻿using System;
using Burrows.Configuration;
using Burrows.Log4Net;
using Burrows.Publishing;
using Burrows.Tests.Messages;
using Burrows.Transports.Configuration.Extensions;

namespace Burrows.Tests.PublishConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var publisher = new Publisher(
                sbc => sbc.Configure(@"rabbitmq://localhost/PublishConsole").UseRabbitMq().UseLog4Net(),
                ps => ps.UsePublisherConfirms("PublishConsole").WithFileBackingStore());

            publisher.RepublishStoredMessages();

            //var publisher = new Publisher(
            //    sbc => sbc.ReceiveFrom(@"rabbitmq://localhost/PublishConsole").UseControlBus().UseLog4Net(),
            //    ps => ps.UsePublisherConfirms("PublishConsole").WithFileBackingStore());

            Console.WriteLine("To publish a message, type a message count and hit enter");
            Console.WriteLine();

            string input;
            while (!string.IsNullOrEmpty(input = Console.ReadLine()))
            {
                int iterations;
                if(int.TryParse(input, out iterations))
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        var msg = new SimpleMessage { Id = "testId", Name = "TestName" };
                        publisher.Publish(msg);
                    }
                }
            }
        }
        
    }
}
