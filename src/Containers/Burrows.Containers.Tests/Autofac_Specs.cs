// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using Burrows.Configuration.SubscriptionConfigurators;

namespace Burrows.Containers.Tests
{
    using Autofac;
    using AutofacIntegration;
    using Magnum.TestFramework;
    using Saga;
    using Scenarios;

    [Scenario]
    public class Autofac_Consumer :
        When_registering_a_consumer
    {
        private readonly IContainer _container;

        public Autofac_Consumer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleConsumer>();
            builder.RegisterType<SimpleConsumerDependency>()
                   .As<ISimpleConsumerDependency>();
            builder.RegisterType<AnotherMessageConsumer>()
                   .As<IAnotherMessageConsumer>();

            _container = builder.Build();
        }

        [Finally]
        public void Close_container()
        {
            _container.Dispose();
        }

        protected override void SubscribeLocalBus(ISubscriptionBusServiceConfigurator subscriptionBusServiceConfigurator)
        {
            subscriptionBusServiceConfigurator.LoadFrom(_container);
        }
    }


    [Scenario]
    public class Autofac_Saga :
        When_registering_a_saga
    {
        private readonly IContainer _container;

        public Autofac_Saga()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(InMemorySagaRepository<>))
                   .As(typeof(ISagaRepository<>))
                   .SingleInstance();
            builder.RegisterType<SimpleSaga>();

            _container = builder.Build();
        }

        [Finally]
        public void Close_container()
        {
            _container.Dispose();
        }

        protected override void SubscribeLocalBus(ISubscriptionBusServiceConfigurator subscriptionBusServiceConfigurator)
        {
            subscriptionBusServiceConfigurator.LoadFrom(_container);
        }
    }
}