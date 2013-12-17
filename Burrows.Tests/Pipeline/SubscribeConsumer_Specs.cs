// Copyright 2007-2010 The Apache Software Foundation.
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

using Burrows.Tests.Framework;

namespace Burrows.Tests.Pipeline
{
    using System;
    using System.Diagnostics;
    using Magnum.Extensions;
    using Burrows.Pipeline;
    using Burrows.Pipeline.Configuration;
    using Burrows.Pipeline.Inspectors;
    using Messages;
    using NUnit.Framework;
    using Rhino.Mocks;
    using TestConsumers;

    [TestFixture]
	public class When_subscribing_a_consumer_to_the_pipeline
	{
		[SetUp]
		public void Setup()
		{
			_pipeline = InboundPipelineConfigurator.CreateDefault(null);
		}

		private IInboundMessagePipeline _pipeline;

		[Test]
		public void A_bunch_of_mixed_subscriber_types_should_work()
		{
			var consumer = new IndiscriminantConsumer<PingMessage>();
			var consumerYes = new ParticularConsumer(true);
			var consumerNo = new ParticularConsumer(false);

			Stopwatch firstTime = Stopwatch.StartNew();
			var unsubscribeToken = _pipeline.ConnectInstance(consumer);
			firstTime.Stop();

			Stopwatch secondTime = Stopwatch.StartNew();
			unsubscribeToken += _pipeline.ConnectInstance(consumerYes);
			secondTime.Stop();

			unsubscribeToken += _pipeline.ConnectInstance(consumerNo);

			Trace.WriteLine(string.Format("First time: {0}, Second Time: {1}", firstTime.Elapsed, secondTime.Elapsed));

			PipelineViewer.Trace(_pipeline);

			var message = new PingMessage();

			_pipeline.Dispatch(message);

			Assert.AreEqual(message, consumer.Consumed);
			Assert.AreEqual(message, consumerYes.Consumed);
			Assert.AreEqual(null, consumerNo.Consumed);

			unsubscribeToken();

			var nextMessage = new PingMessage();
			_pipeline.Dispatch(nextMessage);

			Assert.AreEqual(message, consumer.Consumed);
			Assert.AreEqual(message, consumerYes.Consumed);
		}

		[Test]
		public void A_component_should_be_subscribed_to_the_pipeline()
		{
			var consumer = new TestMessageConsumer<PingMessage>();

			_pipeline.ConnectConsumer<TestMessageConsumer<PingMessage>>(() => consumer);

			PipelineViewer.Trace(_pipeline);

			var message = new PingMessage();

			_pipeline.ShouldHaveSubscriptionFor<PingMessage>();

			_pipeline.Dispatch(message);

			TestMessageConsumer<PingMessage>.AnyShouldHaveReceivedMessage(message, 1.Seconds());
		}

		[Test]
		public void A_selective_component_should_properly_handle_the_love()
		{
			ParticularConsumer consumer = MockRepository.GenerateMock<ParticularConsumer>();

			_pipeline.ConnectConsumer<ParticularConsumer>(() => consumer);

			PipelineViewer.Trace(_pipeline);

			var message = new PingMessage();
			consumer.Expect(x => x.Accept(message)).Return(true);
			consumer.Expect(x => x.Consume(message));

			_pipeline.Dispatch(message);

			consumer.VerifyAllExpectations();
		}

		[Test]
		public void A_component_should_be_subscribed_to_multiple_messages_on_the_pipeline()
		{
			PingPongConsumer consumer = MockRepository.GenerateMock<PingPongConsumer>();

			_pipeline.ConnectConsumer<PingPongConsumer>(() => consumer);

			PipelineViewer.Trace(_pipeline);

			var ping = new PingMessage();
			consumer.Expect(x => x.Consume(ping));
			_pipeline.Dispatch(ping);

			var pong = new PongMessage(ping.CorrelationId);
			consumer.Expect(x => x.Consume(pong));
			_pipeline.Dispatch(pong);

			consumer.VerifyAllExpectations();
		}

		[Test]
		public void The_subscription_should_be_added()
		{
			var consumer = new IndiscriminantConsumer<PingMessage>();

			Stopwatch firstTime = Stopwatch.StartNew();
			_pipeline.ConnectInstance(consumer);
			firstTime.Stop();


			var message = new PingMessage();

			_pipeline.Dispatch(message);

			Assert.AreEqual(message, consumer.Consumed);
		}

		[Test]
		public void Correlated_subscriptions_should_make_happy_sounds()
		{
			var message = new PingMessage();

			var consumer = new TestCorrelatedConsumer<PingMessage, Guid>(message.CorrelationId);
			var negativeConsumer = new TestCorrelatedConsumer<PingMessage, Guid>(Guid.Empty);

			var token = _pipeline.ConnectInstance(consumer);
			token += _pipeline.ConnectInstance(negativeConsumer);

			PipelineViewer.Trace(_pipeline);

			_pipeline.Dispatch(message);

			consumer.ShouldHaveReceivedMessage(message, 0.Seconds());
			negativeConsumer.ShouldNotHaveReceivedMessage(message, 0.Seconds());

			token();

			PipelineViewer.Trace(_pipeline);
		}

		[Test, Explicit]
		public void Correlated_subscription_benchmark()
		{
			var consumer = new TestCorrelatedConsumer<PingMessage, Guid>(Guid.NewGuid());

			UnsubscribeAction token = _pipeline.ConnectInstance(consumer);
			token();

			Stopwatch overall = Stopwatch.StartNew();
			for (int i = 0; i < 10000; i++)
			{
				token = _pipeline.ConnectInstance(consumer);
				token();
			}
			overall.Stop();

			Trace.WriteLine("Elapsed Time: " + overall.Elapsed);
            }


		[Test]
		public void The_subscription_should_be_added_for_selective_consumers()
		{
			var consumer = new ParticularConsumer(false);

			_pipeline.ConnectInstance(consumer);

			var message = new PingMessage();

			_pipeline.Dispatch(message);

			Assert.AreEqual(null, consumer.Consumed);
		}

		[Test]
		public void The_subscription_should_be_added_for_selective_consumers_that_are_interested()
		{
			var consumer = new ParticularConsumer(true);

			_pipeline.ConnectInstance(consumer);

			var message = new PingMessage();

			_pipeline.Dispatch(message);

			Assert.AreEqual(message, consumer.Consumed);
		}

		[Test]
		public void The_wrong_type_of_message_should_not_blow_up_the_test()
		{
			var consumer = new IndiscriminantConsumer<PingMessage>();

			_pipeline.ConnectInstance(consumer);

			var message = new PongMessage();

			_pipeline.Dispatch(message);

			Assert.AreEqual(null, consumer.Consumed);
		}
	}
}