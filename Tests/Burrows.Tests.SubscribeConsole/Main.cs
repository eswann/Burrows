using System;

namespace Burrows.Tests.SubscribeConsole
{
    public class MainService
    {
        private readonly IServiceBus _bus;

        public MainService(IServiceBus bus)
        {
            _bus = bus;

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
            _bus.Dispose();
            Console.WriteLine(new string('-', 20));
            Console.WriteLine("Disposing the Service");
        }
    }
}