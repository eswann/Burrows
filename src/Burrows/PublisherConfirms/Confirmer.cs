// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, Eric Swann, et. al.
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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Burrows.PublisherConfirms
{
    public class Confirmer : IConfirmer
    {
        private readonly ConcurrentDictionary<string, ConfirmableMessage> _unconfirmedMessages =
            new ConcurrentDictionary<string, ConfirmableMessage>();

        public event Action<IEnumerable<ConfirmableMessage>> PublicationFailed;
        public event Action<IEnumerable<ConfirmableMessage>> PublicationSucceeded;

        public void RecordPublicationAttempt(ConfirmableMessage message)
        {
            _unconfirmedMessages.TryAdd(message.Id, message);
        }

        public void RecordPublicationSuccess(IEnumerable<string> messageIds)
        {
            var confirmableMessages = RemoveMessages(messageIds);

            if (PublicationSucceeded != null)
                PublicationSucceeded(confirmableMessages);
        }

        public void RecordPublicationFailure(IEnumerable<string> messageIds)
        {
            var messages = RemoveMessages(messageIds);

            if (PublicationFailed != null)
                PublicationFailed(messages);
        }

        public void ClearMessages()
        {
            _unconfirmedMessages.Clear();
        }

        public IEnumerable<ConfirmableMessage> RemoveMessages(IEnumerable<string> messageIds)
        {
            var removedMessages = new List<ConfirmableMessage>();

            foreach (var messageId in messageIds)
            {
                ConfirmableMessage message;
                _unconfirmedMessages.TryRemove(messageId, out message);
                if (message != null)
                {
                    removedMessages.Add(message);
                }
            }

            return removedMessages;
        }

        public ICollection<ConfirmableMessage> GetMessages()
        {
            return _unconfirmedMessages.Values;
        }
    }
}