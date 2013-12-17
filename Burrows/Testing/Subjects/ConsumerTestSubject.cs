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

using Burrows.Configuration;

namespace Burrows.Testing.Subjects
{
    using Scenarios;
    using TestDecorators;

    public interface IConsumerTestSubject<TConsumer> :
    ITestSubject<TConsumer>
    where TConsumer : class
    {
        /// <summary>
        /// The messages that were received by the handler
        /// </summary>
        IReceivedMessageList Received { get; }
    }

	public class ConsumerTestSubject<TScenario, TSubject> :
		IConsumerTestSubject<TSubject>
		where TSubject : class, IConsumer
	    where TScenario : ITestScenario
	{
		readonly IConsumerFactory<TSubject> _consumerFactory;
		readonly ReceivedMessageList _received;
		bool _disposed;
		UnsubscribeAction _unsubscribe;

		public ConsumerTestSubject(IConsumerFactory<TSubject> consumerFactory)
		{
			_consumerFactory = consumerFactory;

			_received = new ReceivedMessageList();
		}

		public IReceivedMessageList Received
		{
			get { return _received; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Prepare(TScenario scenario)
		{
			var decoratedConsumerFactory = new ConsumerFactoryTestDecorator<TSubject>(_consumerFactory, _received);

			_unsubscribe = scenario.InputBus.SubscribeConsumer(decoratedConsumerFactory);
		}

		void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				if (_unsubscribe != null)
				{
					_unsubscribe();
					_unsubscribe = null;
				}

				_received.Dispose();
			}

			_disposed = true;
		}
	}
}