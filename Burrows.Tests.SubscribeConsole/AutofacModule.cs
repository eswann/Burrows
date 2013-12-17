using System;
using System.Reflection;
using Autofac;
using Burrows.AutofacIntegration;
using Burrows.Configuration;
using Burrows.Log4NetIntegration;
using Burrows.Transports.Configuration.Extensions;
using Magnum.Extensions;
using Module = Autofac.Module;

namespace Burrows.Tests.SubscribeConsole
{
    public class AutofacModule : Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                   .Where(t => t.Implements<IConsumer>())
                   .AsSelf();

            builder.Register(c => ServiceBusFactory.New(sbc =>
                {
                    sbc.ReceiveFrom(@"rabbitmq://localhost/SubscribeConsole");
                    sbc.UseRabbitMq();
                    sbc.UseControlBus();
                    sbc.Subscribe(subs => subs.LoadFrom(c.Resolve<ILifetimeScope>()));
                    sbc.UseLog4Net();
                })).SingleInstance();

        }
    }
}