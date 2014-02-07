using System;
using Burrows.Publishing;

namespace Burrows.Tests.PublishSubscribeConsole
{
    public class MainService
    {
        private readonly IPublisher _publisher;

        public MainService(IPublisher publisher)
        {
            _publisher = publisher;

            Console.WriteLine(new string('-', 20));
            Console.WriteLine("Constructing the Service");
        }

        public void Start()
        {
            Console.WriteLine(new string('-', 20));
            Console.WriteLine("Starting the Service");
        }

        public void Stop()
        {
            Console.WriteLine(new string('-', 20));
            Console.WriteLine("Stopping the Service");
        }

        public void Dispose()
        {
            _publisher.Dispose();
            Console.WriteLine(new string('-', 20));
            Console.WriteLine("Disposing the Service");
        }
    }
}