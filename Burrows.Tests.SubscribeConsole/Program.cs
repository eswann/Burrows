using Autofac;
using Topshelf;

namespace Burrows.Tests.SubscribeConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var container = Bootstrapper.ServiceBootstrapper.Bootstrap();

            string serviceName = "SubscriberConsole";

            HostFactory
                .New(x => x.Service<MainService>(sc =>
                {
                    sc.ConstructUsing(() => new MainService(container.Resolve<IServiceBus>()));
                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                    x.RunAsLocalSystem();
                    x.SetDescription(serviceName);
                    x.SetDisplayName(serviceName);
                    x.SetServiceName(serviceName);
                    x.UseLog4Net();
                }))
                .Run();
        }
    }
}
