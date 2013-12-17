// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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

using System;
using Burrows.BusConfigurators;
using Burrows.Context;
using Burrows.Exceptions;
using Magnum.Extensions;
using Magnum.TestFramework;
using NUnit.Framework;

namespace Burrows.Tests.RabbitMq
{
    [Scenario]
    public class When_sending_a_request_to_a_rabbitmq_endpoint :
        Given_a_rabbitmq_bus
    {
        protected override void ConfigureServiceBus(Uri uri, IServiceBusConfigurator configurator)
        {
            base.ConfigureServiceBus(uri, configurator);

            configurator.Subscribe(x => { x.Consumer<PingHandler>(); });
        }

        class PingHandler
            : Consumes<IConsumeContext<IPingMessage>>.All
        {
            public void Consume(IConsumeContext<IPingMessage> context)
            {
                context.Respond<IPongMessage>(new Pong());
            }
        }

        public interface IPingMessage
        {
        }

        public interface IPlinkMessage
        {
        }

        public interface IPongMessage
        {
        }

        class Ping :
            IPingMessage
        {
        }

        class PlinkMessage : 
            IPlinkMessage
        {
        }

        class Pong :
            IPongMessage
        {
        }

        [Test]
        public void Should_respond_properly()
        {
            bool result = LocalBus.GetEndpoint(LocalBus.Endpoint.Address.Uri)
                .SendRequest<IPingMessage>(new Ping(), LocalBus, req =>
                    {
                        req.Handle<IPongMessage>(x => { });
                        req.SetTimeout(10.Seconds());
                    });

            result.ShouldBeTrue("No response was received.");
        }

        [Test]
        public void Should_timeout_for_unhandled_request()
        {
            Assert.Throws<RequestTimeoutException>(() =>
                {
                    LocalBus.GetEndpoint(LocalBus.Endpoint.Address.Uri)
                        .SendRequest<IPlinkMessage>(new PlinkMessage(), LocalBus, req =>
                            {
                                req.Handle<IPongMessage>(x => { });
                                req.SetTimeout(8.Seconds());
                            });
                });
        }
    }
}