﻿// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Burrows.Subscriptions.Coordinator
{
    using System;
    using System.Collections.Generic;
    using Logging;
    using Messages;

    public class BusSubscription : ISubscription
    {
        private static readonly ILog _log = Logger.Get(typeof(BusSubscription));
        private readonly string _correlationId;
        private readonly HashSet<Guid> _ids;
        private readonly string _messageName;
        private readonly ISubscriptionObserver _observer;
        Uri _endpointUri;
        Guid _subscriptionId;

        public BusSubscription(string messageName, string correlationId, ISubscriptionObserver observer)
        {
            _messageName = messageName;
            _correlationId = correlationId;
            _observer = observer;

            _ids = new HashSet<Guid>();

            _subscriptionId = Guid.Empty;
        }

        public IEnumerable<ISubscription> Subscriptions
        {
            get
            {
                if (_ids.Count > 0)
                    yield return this;
            }
        }

        public Guid SubscriptionId
        {
            get { return _subscriptionId; }
        }

        public Uri EndpointUri
        {
            get { return _endpointUri; }
        }

        public string MessageName
        {
            get { return _messageName; }
        }

        public string CorrelationId
        {
            get { return _correlationId; }
        }

        public void OnSubscribeTo(ISubscribeTo added)
        {
            lock (_ids)
            {
                bool wasAdded = _ids.Add(added.SubscriptionId);

                if (!wasAdded || _ids.Count != 1)
                    return;
            }

            _subscriptionId = NewIds.NewId.NextGuid();
            _endpointUri = added.EndpointUri;

            var add = new SubscriptionAddedMessage
                {
                    SubscriptionId = _subscriptionId,
                    EndpointUri = _endpointUri,
                    MessageName = _messageName,
                    CorrelationId = _correlationId,
                };

            _log.DebugFormat("SubscribeTo: {0}, {1}", _messageName, _subscriptionId);

            _observer.OnSubscriptionAdded(add);
        }

        public void OnUnsubscribeFrom(IUnsubscribeFrom removed)
        {
            lock (_ids)
            {
                if (!_ids.Contains(removed.SubscriptionId))
                    return;

                _ids.Clear();
            }

            var remove = new SubscriptionRemovedMessage
                {
                    SubscriptionId = _subscriptionId,
                    EndpointUri = _endpointUri,
                    MessageName = _messageName,
                    CorrelationId = _correlationId,
                };

            _log.DebugFormat("UnsubscribeFrom: {0}, {1}", _messageName, _subscriptionId);

            _observer.OnSubscriptionRemoved(remove);
            _subscriptionId = Guid.Empty;
        }
    }
}