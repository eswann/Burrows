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
    using System;
    using System.Linq;
    using Context;
    using Exceptions;
    using Magnum.Extensions;
    using Scenarios;

    public interface IHandlerTestSubject<TMessage> :
    ITestSubject<TMessage>
    where TMessage : class
    {
        /// <summary>
        /// The messages that were received by the handler
        /// </summary>
        IReceivedMessageList<TMessage> Received { get; }
    }

	public class HandlerTestSubject<TScenario, TSubject> :
		IHandlerTestSubject<TSubject>
		where TSubject : class
		where TScenario : ITestScenario
	{
		readonly Action<IConsumeContext<TSubject>, TSubject> _handler;
		readonly ReceivedMessageList<TSubject> _received;
		bool _disposed;
		UnsubscribeAction _unsubscribe;

		public HandlerTestSubject(Action<IConsumeContext<TSubject>, TSubject> handler)
		{
			_handler = handler;
			_received = new ReceivedMessageList<TSubject>();
		}

		public IReceivedMessageList<TSubject> Received
		{
			get { return _received; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Prepare(TScenario scenario)
		{
			_unsubscribe = scenario.InputBus.SubscribeContextHandler<TSubject>(HandleMessage);

            if(!scenario.OutputBus.HasSubscription<TSubject>(8.Seconds()).Any())
                throw new ConfigurationException("The subscription was not received on the output bus");
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

		void HandleMessage(IConsumeContext<TSubject> context)
		{
			var received = new ReceivedMessage<TSubject>(context);

			try
			{
				using (context.CreateScope())
				{
					_handler(context, context.Message);
				}
			}
			catch (Exception ex)
			{
				received.SetException(ex);
			}
			finally
			{
				_received.Add(received);
			}
		}
	}
}