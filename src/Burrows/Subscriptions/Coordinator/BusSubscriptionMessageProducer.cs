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

using Burrows.Endpoints;

namespace Burrows.Subscriptions.Coordinator
{
    using System;
    using System.Threading;
    using Context;
    using Logging;
    using Messages;

    public class BusSubscriptionMessageProducer :
        ISubscriptionObserver
    {
        private static readonly ILog _log = Logger.Get(typeof (BusSubscriptionMessageProducer));
        private readonly IEndpoint _endpoint;
        private readonly Uri _endpointUri;
        private readonly string _network;

        private readonly Guid _peerId;
        private readonly Uri _peerUri;
        long _lastMessageNumber;
        readonly long _timestamp;

        public long Timestamp
        {
            get { return _timestamp; }
        }

        public BusSubscriptionMessageProducer(ISubscriptionRouter router, IEndpoint endpoint, Uri endpointUri)
        {
            _peerId = router.PeerId;
            _peerUri = router.PeerUri;
            _network = router.Network;
            _endpoint = endpoint;
            _endpointUri = endpointUri;
            _timestamp = DateTime.UtcNow.Ticks;

            SendAddPeerMessage();
        }

        public void OnSubscriptionAdded(ISubscriptionAdded message)
        {
            long messageNumber = Interlocked.Increment(ref _lastMessageNumber);

            var add = new AddPeerSubscriptionMessage
                {
                    PeerId = _peerId,
                    MessageNumber = messageNumber,
                    SubscriptionId = message.SubscriptionId,
                    EndpointUri = message.EndpointUri,
                    MessageName = message.MessageName,
                    CorrelationId = message.CorrelationId,
                };

            if (_log.IsDebugEnabled)
                _log.DebugFormat("AddSubscription: {0}, {1}", add.MessageName, add.SubscriptionId);

            _endpoint.Send(add, SetSendContext);
        }

        public void OnSubscriptionRemoved(ISubscriptionRemoved message)
        {
            long messageNumber = Interlocked.Increment(ref _lastMessageNumber);

            var remove = new RemovePeerSubscriptionMessage
                {
                    PeerId = _peerId,
                    MessageNumber = messageNumber,
                    SubscriptionId = message.SubscriptionId,
                    EndpointUri = message.EndpointUri,
                    MessageName = message.MessageName,
                    CorrelationId = message.CorrelationId,
                };

            if (_log.IsDebugEnabled)
                _log.DebugFormat("RemoveSubscription: {0}, {1}", remove.MessageName, remove.SubscriptionId);

            _endpoint.Send(remove, SetSendContext);
        }

        public void OnComplete()
        {
            _endpoint.Send(new RemovePeerMessage
                {
                    PeerId = _peerId,
                    PeerUri = _peerUri,
                    Timestamp = _timestamp,
                }, SetSendContext);
        }

        void SendAddPeerMessage()
        {
            _endpoint.Send(new AddPeerMessage
                {
                    PeerId = _peerId,
                    PeerUri = _peerUri,
                    Timestamp = _timestamp,
                }, context =>
                    {
                        SetSendContext(context);
                        context.SetResponseAddress(_endpointUri);
                    });
        }

        void SetSendContext<T>(ISendContext<T> context)
            where T : class
        {
            context.SetNetwork(_network);
            context.SetSourceAddress(_peerUri);
        }
    }
}