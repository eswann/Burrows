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
namespace Burrows.Testing.Subjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Saga;
    using Saga.Configuration;
    using Scenarios;
    using TestDecorators;

    public interface ISagaTestSubject<TSaga> :
    ITestSubject<TSaga>,
    ISagaList<TSaga>
    where TSaga : class, ISaga
    {
        /// <summary>
        /// The messages that were received by the handler
        /// </summary>
        IReceivedMessageList Received { get; }

        /// <summary>
        /// The sagas that were created during the test
        /// </summary>
        ISagaList<TSaga> Created { get; }
    }

	public class SagaTestSubject<TScenario, TSaga> :
		ISagaTestSubject<TSaga>
		where TSaga : class, ISaga
		where TScenario : ITestScenario
	{
		readonly SagaList<TSaga> _created;
		readonly ReceivedMessageList _received;
		readonly ISagaRepository<TSaga> _sagaRepository;
		bool _disposed;
	    readonly SagaList<TSaga> _sagas;
		UnsubscribeAction _unsubscribe;

		public SagaTestSubject(ISagaRepository<TSaga> sagaRepository)
		{
			_sagaRepository = sagaRepository;

			_received = new ReceivedMessageList();
			_created = new SagaList<TSaga>();
			_sagas = new SagaList<TSaga>();
		}

		public IReceivedMessageList Received
		{
			get { return _received; }
		}

		public ISagaList<TSaga> Created
		{
			get { return _created; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public IEnumerator<ISagaInstance<TSaga>> GetEnumerator()
		{
			return _sagas.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Any()
		{
			return _sagas.Any();
		}

		public bool Any(Func<TSaga, bool> filter)
		{
			return _sagas.Any(filter);
		}

		public TSaga Contains(Guid sagaId)
		{
			return _sagas.Contains(sagaId);
		}

		public void Prepare(TScenario scenario)
		{
			var decoratedSagaRepository = new SagaRepositoryTestDecorator<TSaga>(_sagaRepository, _received, _created, _sagas);

			_unsubscribe = scenario.InputBus.SubscribeSaga(decoratedSagaRepository);
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