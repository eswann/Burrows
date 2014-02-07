using Autofac;
using Burrows.Publishing;
using Topshelf;

namespace Burrows.Tests.PublishSubscribeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = Bootstrapper.ServiceBootstrapper.Bootstrap();

            string serviceName = "PublishSubscribeConsole";

            HostFactory
                .New(x => x.Service<MainService>(sc =>
                {
                    sc.ConstructUsing(() => new MainService(container.Resolve<IPublisher>()));
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
