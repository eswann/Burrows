using System;
using System.Reflection;
using Autofac;

namespace Burrows.Tests.SubscribeConsole
{
    public class Bootstrapper
    {
        public class ServiceBootstrapper
        {
            private static bool _hasStarted;
            private static IContainer _container;

            public static IContainer Bootstrap(bool force = false)
            {
                if (_hasStarted && !force) return _container;

                InitDi();

                _hasStarted = true;

                return _container;
            }

            public static void InitDi()
            {
                //Auto registers any interface/implementations that follow the standard naming convention
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

                _container = containerBuilder.Build();
            }

            public static IContainer Container
            {
                get { return _container; }
                set { _container = value; }
            }
        } 
    }
}