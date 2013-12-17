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

using Burrows.Configuration;
using Burrows.Configuration.BusConfigurators;

namespace Burrows.Tests
{
    using System.Linq;
    using Context;
    using Magnum.Extensions;
    using Magnum.TestFramework;
    using Burrows.Testing;
    using NUnit.Framework;
    using TextFixtures;

    [TestFixture]
    public class Putting_two_bodies_into_one_message :
        LoopbackLocalAndRemoteTestFixture
    {
        [Test]
        public void Should_be_possible()
        {
            RemoteBus.HasSubscription<ISecureCommand>(8.Seconds()).Any().ShouldBeTrue();

            RemoteBus.Publish(new CommandAndCredentials
                {
                    SqlText = "DROP TABLE [Users]",
                    Username = "sa",
                    Password = "god",
                });

            CommandHandler.CredentialsReceived.IsAvailable(8.Seconds()).ShouldBeTrue();
        }

        protected override void ConfigureLocalBus(IServiceBusConfigurator configurator)
        {
            base.ConfigureLocalBus(configurator);

            configurator.Subscribe(x => { x.Consumer<CommandHandler>(); });
        }


        public class CommandHandler :
            Consumes<ISecureCommand>.Context
        {
            public static FutureMessage<IUserCredentials> CredentialsReceived = new FutureMessage<IUserCredentials>();

            public void Consume(IConsumeContext<ISecureCommand> context)
            {
                IConsumeContext<IUserCredentials> credentials;
                if (context.TryGetContext(out credentials))
                {
                    CredentialsReceived.Set(credentials.Message);
                }
            }
        }


        class CommandAndCredentials :
            ISecureCommand,
            IUserCredentials
        {
            public string SqlText { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public interface ISecureCommand
        {
            string SqlText { get; }
        }

        public interface IUserCredentials
        {
            string Username { get; }
            string Password { get; }
        }
    }

    [TestFixture]
    public class Wrapping_a_message_in_a_generic_wrapper :
        LoopbackLocalAndRemoteTestFixture
    {
        [Test]
        public void Should_be_possible()
        {
            LocalBus.HasSubscription<IBusinessCommand>(8.Seconds()).Any().ShouldBeTrue();
            RemoteBus.HasSubscription<ISecureCommand<IBusinessCommand>>(8.Seconds()).Any().ShouldBeTrue();

            RemoteBus.Publish(new BusinessCommand
                {
                    SqlText = "DROP TABLE [Users]",
                });

            CommandHandler.CommandReceived.IsAvailable(8.Seconds()).ShouldBeTrue();
        }

        protected override void ConfigureLocalBus(IServiceBusConfigurator configurator)
        {
            base.ConfigureLocalBus(configurator);

            configurator.Subscribe(x => { x.Consumer<CommandHandler>(); });
        }

        protected override void ConfigureRemoteBus(IServiceBusConfigurator configurator)
        {
            base.ConfigureRemoteBus(configurator);

            configurator.Subscribe(x => x.Consumer<CommandSecurityMaker<IBusinessCommand>>());
        }


        public class CommandHandler :
            Consumes<ISecureCommand<IBusinessCommand>>.Context
        {
            public static FutureMessage<IUserCredentials> CommandReceived = new FutureMessage<IUserCredentials>();

            public void Consume(IConsumeContext<ISecureCommand<IBusinessCommand>> context)
            {
                CommandReceived.Set(context.Message.Credentials);
            }
        }


        class SecureCommand<T> :
            ISecureCommand<T>
            where T : class
        {
            public T Command { get;  set; }
            public IUserCredentials Credentials { get;  set; }
        }

        public interface ISecureCommand<T>
            where T : class
        {
            T Command { get; }
            IUserCredentials Credentials { get; }
        }

        public interface IBusinessCommand
        {
            string SqlText { get; }
        }

        class BusinessCommand : 
            IBusinessCommand
        {
            public string SqlText { get; set; }
        }

        public interface IUserCredentials
        {
            string Username { get; }
            string Password { get; }
        }

        class UserCredentials : IUserCredentials
        {
            public string Username { get;  set; }
            public string Password { get;  set; }
        }

        public class CommandSecurityMaker<T> :
            Consumes<T>.Context
            where T : class
        {
            public void Consume(IConsumeContext<T> message)
            {
                var output = new SecureCommand<T>()
                    {
                        Command = message.Message,
                        Credentials = new UserCredentials {Username = "sa", Password = "god"},
                    };

                message.Bus.Publish(output, x => x.ForwardUsingOriginalContext(message));
            }
        }
    }
}