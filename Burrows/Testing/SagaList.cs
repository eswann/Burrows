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
namespace Burrows.Testing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Magnum.Extensions;
    using Saga;

    public interface ISagaList<T> :
    IEnumerable<ISagaInstance<T>>
    where T : class, ISaga
    {
        bool Any();

        bool Any(Func<T, bool> filter);

        T Contains(Guid sagaId);
    }

	public class SagaList<T> :
		ISagaList<T>,
		IDisposable
		where T : class, ISaga
	{
		readonly IDictionary<Guid, ISagaInstance<T>> _sagaIndex; 
		readonly HashSet<ISagaInstance<T>> _sagas;
		readonly AutoResetEvent _updated;
	    readonly TimeSpan _timeout = 12.Seconds();

		public SagaList()
		{
			_sagas = new HashSet<ISagaInstance<T>>(new SagaEqualityComparer());
			_sagaIndex = new Dictionary<Guid, ISagaInstance<T>>();
			_updated = new AutoResetEvent(false);
		}

		public void Dispose()
		{
			using (_updated)
			{
			}
		}

		public IEnumerator<ISagaInstance<T>> GetEnumerator()
		{
			lock (_sagas)
				return _sagas.ToList().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Any(Func<T, bool> filter)
		{
			bool any;

			Func<ISagaInstance<T>, bool> predicate = x => filter(x.Saga);

			lock (_sagas)
				any = _sagas.Any(predicate);

			while (any == false)
			{
				if (_updated.WaitOne(_timeout, true) == false)
					return _sagas.Any(predicate);

				lock (_sagas)
				{
					any = _sagas.Any(predicate);
				}
			}

			return true;
		}

		public T Contains(Guid sagaId)
		{
			ISagaInstance<T> instance;

			Func<ISagaInstance<T>, bool> predicate = x => x.Saga.CorrelationId == sagaId;

			lock (_sagas)
				instance = _sagas.FirstOrDefault(predicate);

			while (instance == null)
			{
				if (_updated.WaitOne(_timeout, true) == false)
					break;

				lock (_sagas)
				{
					instance = _sagas.FirstOrDefault(predicate);
				}
			}

			return instance == null ? null : instance.Saga;
		}

		public bool Any()
		{
			bool any;
			lock (_sagas)
				any = _sagas.Any();

			while (any == false)
			{
				if (_updated.WaitOne(_timeout, true) == false)
					return false;

				lock (_sagas)
					any = _sagas.Any();
			}

			return true;
		}

		public void Add(T saga)
		{
			lock (_sagas)
			{
				ISagaInstance<T> instance;
				if(!_sagaIndex.TryGetValue(saga.CorrelationId, out instance))
				{
					instance = new SagaInstance<T>(saga);
					_sagaIndex.Add(saga.CorrelationId, instance);
					_sagas.Add(instance);
				}
				else
				{
					if(!ReferenceEquals(instance.Saga, saga))
					{
						instance.Saga = saga;
					}
				}
			}

			_updated.Set();
		}

		class SagaEqualityComparer :
			IEqualityComparer<ISagaInstance<T>>
		{
			public bool Equals(ISagaInstance<T> x, ISagaInstance<T> y)
			{
				return Equals(x.Saga.CorrelationId, y.Saga.CorrelationId);
			}

			public int GetHashCode(ISagaInstance<T> message)
			{
				return message.Saga.CorrelationId.GetHashCode();
			}
		}
	}
}