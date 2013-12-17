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

    public interface IPublishedMessageList :
        IEnumerable<IPublishedMessage>
    {
        bool Any();
        bool Any(Func<IPublishedMessage, bool> filter);

        bool Any<T>()
            where T : class;
    }

    public class PublishedMessageList :
        IPublishedMessageList,
        IDisposable
    {
        private readonly HashSet<IPublishedMessage> _messages;
        private readonly AutoResetEvent _published;
        private readonly TimeSpan _timeout = 12.Seconds();

        public PublishedMessageList()
        {
            _messages = new HashSet<IPublishedMessage>(new MessageIdEqualityComparer());
            _published = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            using (_published)
            {
            }
        }

        public IEnumerator<IPublishedMessage> GetEnumerator()
        {
            lock (_messages)
                return _messages.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Any()
        {
            return Any(x => true);
        }

        public bool Any<T>()
            where T : class
        {
            return Any(x => typeof(T).IsAssignableFrom(x.MessageType));
        }

        public bool Any(Func<IPublishedMessage, bool> filter)
        {
            bool any;
            lock (_messages)
                any = _messages.Any(filter);

            while (any == false)
            {
                if (_published.WaitOne(_timeout, true) == false)
                    return false;

                lock (_messages)
                    any = _messages.Any(filter);
            }

            return true;
        }

        public void Add(IPublishedMessage message)
        {
            lock (_messages)
            {
                if (_messages.Add(message))
                    _published.Set();
            }
        }

        class MessageIdEqualityComparer :
            IEqualityComparer<IPublishedMessage>
        {
            public bool Equals(IPublishedMessage x, IPublishedMessage y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(IPublishedMessage message)
            {
                return message.Context.GetHashCode();
            }
        }
    }
}