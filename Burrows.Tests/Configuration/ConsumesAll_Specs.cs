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

using Burrows.Configuration.Configuration;
using Burrows.Configuration.SubscriptionConnectors;

namespace Burrows.Tests.Configuration
{
    using System;
    using System.Linq;
    using Magnum.TestFramework;
    using Burrows.Configuration;

    [Scenario]
	public class When_a_consumer_with_consumes_all_interfaces_is_inspected
	{
		ConsumerConnector<Consumer> _factory;
	    DelegateConsumerFactory<Consumer> _consumerFactory;

	    [When]
		public void A_consumer_with_consumes_all_interfaces_is_inspected()
	    {
	        _consumerFactory = new DelegateConsumerFactory<Consumer>(() => new Consumer());

	        _factory = new ConsumerConnector<Consumer>();
	    }

	    [Then]
		public void Should_create_the_builder()
		{
			_factory.ShouldNotBeNull();
		}

		[Then]
		public void Should_have_four_subscription_types()
		{
			_factory.Connectors.Count().ShouldEqual(4);
		}

		[Then]
		public void Should_have_an_a()
		{
			_factory.Connectors.First().MessageType.ShouldEqual(typeof (A));
		}

		[Then]
		public void Should_have_a_b()
		{
			_factory.Connectors.Skip(1).First().MessageType.ShouldEqual(typeof (B));
		}

		[Then]
		public void Should_have_a_c()
		{
			_factory.Connectors.Skip(2).First().MessageType.ShouldEqual(typeof (IC));
		}

		[Then]
		public void Should_have_a_d()
		{
			_factory.Connectors.Skip(3).First().MessageType.ShouldEqual(typeof (D<A>));
		}


		class Consumer :
			Consumes<A>.All,
			Consumes<B>.All,
			Consumes<IC>.All,
			Consumes<D<A>>.All
		{
			public void Consume(A message)
			{
			}

			public void Consume(B message)
			{
			}

			public void Consume(D<A> message)
			{
			}

			public void Consume(IC message)
			{
			}
		}

		class A
		{
		}

		class B
		{
		}

		class D<T>
		{
		}

		interface IC
		{
		}
	}

	[Scenario]
	public class When_a_consumer_with_consumes_selected_interfaces_is_inspected
	{
		ConsumerConnector<SelectedConsumer> _factory;
	    DelegateConsumerFactory<SelectedConsumer> _consumerFactory;

	    [When]
		public void A_consumer_with_consumes_all_interfaces_is_inspected()
	    {
	        _consumerFactory = new DelegateConsumerFactory<SelectedConsumer>(() => new SelectedConsumer());
	        _factory = new ConsumerConnector<SelectedConsumer>();
	    }

	    [Then]
		public void Should_create_the_builder()
		{
			_factory.ShouldNotBeNull();
		}

		[Then]
		public void Should_have_four_subscription_types()
		{
			_factory.Connectors.Count().ShouldEqual(4);
		}

		[Then]
		public void Should_have_an_a()
		{
			IConsumerSubscriptionConnector connector = _factory.Connectors.First();
			connector.MessageType.ShouldEqual(typeof (A));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (SelectedConsumerSubscriptionConnector<,>));
		}

		[Then]
		public void Should_have_a_b()
		{
			IConsumerSubscriptionConnector connector = _factory.Connectors.Skip(1).First();
			connector.MessageType.ShouldEqual(typeof (B));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (SelectedConsumerSubscriptionConnector<,>));
		}

		[Then]
		public void Should_have_a_c()
		{
			IConsumerSubscriptionConnector connector = _factory.Connectors.Skip(2).First();
			connector.MessageType.ShouldEqual(typeof (IC));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (SelectedConsumerSubscriptionConnector<,>));
		}

		[Then]
		public void Should_have_a_d()
		{
			IConsumerSubscriptionConnector connector = _factory.Connectors.Skip(3).First();
			connector.MessageType.ShouldEqual(typeof (D<A>));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (SelectedConsumerSubscriptionConnector<,>));
		}

		class SelectedConsumer :
			Consumes<A>.Selected,
			Consumes<B>.Selected,
			Consumes<IC>.Selected,
			Consumes<D<A>>.Selected
		{
			public void Consume(A message)
			{
			}

			public bool Accept(A message)
			{
				return true;
			}

			public void Consume(B message)
			{
			}

			public bool Accept(B message)
			{
				return true;
			}

			public void Consume(D<A> message)
			{
			}

			public bool Accept(D<A> message)
			{
				return true;
			}

			public void Consume(IC message)
			{
			}

			public bool Accept(IC message)
			{
				return true;
			}
		}


		class A
		{
		}

		class B
		{
		}

		class D<T>
		{
		}

		interface IC
		{
		}
	}

	[Scenario]
	public class When_a_instance_with_consumes_for_interfaces_is_inspected
	{
		InstanceConnector<CorrelatedConsumer> _factory;

		[When]
		public void A_consumer_with_consumes_all_interfaces_is_inspected()
		{
			_factory = new InstanceConnector<CorrelatedConsumer>();
		}

		[Then]
		public void Should_create_the_builder()
		{
			_factory.ShouldNotBeNull();
		}

		[Then]
		public void Should_have_four_subscription_types()
		{
			_factory.Connectors.Count().ShouldEqual(4);
		}

		[Then]
		public void Should_have_an_a()
		{
			IInstanceSubscriptionConnector connector = _factory.Connectors.First();
			connector.MessageType.ShouldEqual(typeof (A));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (CorrelatedInstanceSubscriptionConnector<,,>));
			connector.GetType().GetGenericArguments()[2].ShouldEqual(typeof (Guid));
		}

		[Then]
		public void Should_have_a_b()
		{
			IInstanceSubscriptionConnector connector = _factory.Connectors.Skip(1).First();
			connector.MessageType.ShouldEqual(typeof (B));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (CorrelatedInstanceSubscriptionConnector<,,>));
			connector.GetType().GetGenericArguments()[2].ShouldEqual(typeof (int));
		}

		[Then]
		public void Should_have_a_c()
		{
			IInstanceSubscriptionConnector connector = _factory.Connectors.Skip(2).First();
			connector.MessageType.ShouldEqual(typeof (IC));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (CorrelatedInstanceSubscriptionConnector<,,>));
			connector.GetType().GetGenericArguments()[2].ShouldEqual(typeof (long));
		}

		[Then]
		public void Should_have_a_d()
		{
			IInstanceSubscriptionConnector connector = _factory.Connectors.Skip(3).First();
			connector.MessageType.ShouldEqual(typeof (D<A>));
			connector.GetType().GetGenericTypeDefinition().ShouldEqual(typeof (CorrelatedInstanceSubscriptionConnector<,,>));
			connector.GetType().GetGenericArguments()[2].ShouldEqual(typeof (string));
		}

		class CorrelatedConsumer :
			Consumes<A>.For<Guid>,
			Consumes<B>.For<int>,
			Consumes<IC>.For<long>,
			Consumes<D<A>>.For<string>
		{
			public void Consume(A message)
			{
			}

			Guid ICorrelatedBy<Guid>.CorrelationId
			{
				get { return default(Guid); }
			}

			public void Consume(B message)
			{
			}

			int ICorrelatedBy<int>.CorrelationId
			{
				get { return default(int); }
			}

			public void Consume(D<A> message)
			{
			}

			string ICorrelatedBy<string>.CorrelationId
			{
				get { return default(string); }
			}

			public void Consume(IC message)
			{
			}

			long ICorrelatedBy<long>.CorrelationId
			{
				get { return default(long); }
			}
		}


		class A : ICorrelatedBy<Guid>
		{
			public Guid CorrelationId
			{
				get { return default(Guid); }
			}
		}

		class B : ICorrelatedBy<int>
		{
			public int CorrelationId
			{
				get { return default(int); }
			}
		}

		class D<T> : ICorrelatedBy<string>
		{
			public string CorrelationId
			{
				get { return default(string); }
			}
		}

		interface IC : ICorrelatedBy<long>
		{
		}
	}
}