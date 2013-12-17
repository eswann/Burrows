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
namespace Burrows.Subscriptions.Coordinator
{
    using System;
    using System.Collections.Generic;
    using Logging;
    using Messages;
    using Services.Subscriptions.Messages;

    public class SubscriptionLoopback : ISubscriptionObserver
    {
        private static readonly ILog _log = Logger.Get(typeof (SubscriptionLoopback));
        private readonly HashSet<string> _ignoredMessageTypes;

        private readonly Guid _peerId;
        private readonly ISubscriptionRouter _router;
        private readonly List<Action<ISubscriptionRouter>> _waiting;
        long _messageNumber;
        ISubscriptionRouter _targetRouter;

        public SubscriptionLoopback(IServiceBus bus, ISubscriptionRouter router)
        {
            _router = router;
            _peerId = NewId.NextGuid();

            _waiting = new List<Action<ISubscriptionRouter>>();

            _ignoredMessageTypes = IgnoredMessageTypes();

            WithTarget(x =>
                {
                    if (_log.IsDebugEnabled)
                        _log.DebugFormat("Send AddPeer: {0}, {1}", _peerId, bus.ControlBus.Endpoint.Address.Uri);

                    x.Send(new AddPeerMessage
                        {
                            PeerId = _peerId,
                            PeerUri = bus.ControlBus.Endpoint.Address.Uri,
                            Timestamp = DateTime.UtcNow.Ticks,
                        });
                });
        }

        public ISubscriptionRouter Router
        {
            get { return _router; }
        }

        public void OnSubscriptionAdded(ISubscriptionAdded message)
        {
            if (_ignoredMessageTypes.Contains(message.MessageName))
                return;

            WithTarget(x =>
                {
                    if (_log.IsDebugEnabled)
                        _log.DebugFormat("Send AddPeerSubscription: {0}, {1}", _peerId, message.MessageName);

                    x.Send(new AddPeerSubscriptionMessage
                        {
                            PeerId = _peerId,
                            MessageNumber = ++_messageNumber,
                            EndpointUri = message.EndpointUri,
                            MessageName = message.MessageName,
                            SubscriptionId = message.SubscriptionId,
                            CorrelationId = message.CorrelationId,
                        });
                });
        }

        public void OnSubscriptionRemoved(ISubscriptionRemoved message)
        {
            if (_ignoredMessageTypes.Contains(message.MessageName))
                return;

            WithTarget(x => x.Send(new RemovePeerSubscriptionMessage
                {
                    PeerId = _peerId,
                    MessageNumber = ++_messageNumber,
                    EndpointUri = message.EndpointUri,
                    MessageName = message.MessageName,
                    SubscriptionId = message.SubscriptionId,
                    CorrelationId = message.CorrelationId,
                }));
        }

        public void OnComplete()
        {
        }

        public void SetTargetCoordinator(ISubscriptionRouter targetRouter)
        {
            lock (this)
            {
                _targetRouter = targetRouter;
                _waiting.ForEach(x => x(_targetRouter));
                _waiting.Clear();
            }
        }

        void WithTarget(Action<ISubscriptionRouter> callback)
        {
            lock (this)
            {
                if (_targetRouter == null)
                {
                    _waiting.Add(callback);
                    return;
                }

                callback(_targetRouter);
            }
        }

        HashSet<string> IgnoredMessageTypes()
        {
            var ignoredMessageTypes = new HashSet<string>
                {
                    typeof (AddSubscription).ToMessageName(),
                    typeof (RemoveSubscription).ToMessageName(),
                    typeof (AddSubscriptionClient).ToMessageName(),
                    typeof (RemoveSubscriptionClient).ToMessageName(),
                    typeof (SubscriptionRefresh).ToMessageName()
                };

            return ignoredMessageTypes;
        }
    }
}