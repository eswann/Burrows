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
    using Context;
    using Logging;
    using Messages;
    using Services.Subscriptions.Messages;

    /// <summary>
    /// The Subscription Message Consumer handles subscription update messages send from the Subscription Service. 
    /// </summary>
    public class SubscriptionMessageConsumer :
        Consumes<AddSubscriptionClient>.Context,
        Consumes<RemoveSubscriptionClient>.Context,
        Consumes<SubscriptionRefresh>.Context,
        Consumes<AddSubscription>.Context,
        Consumes<RemoveSubscription>.Context,
        Consumes<AddPeerSubscription>.Context,
        Consumes<IRemovePeerSubscription>.Context,
        Consumes<AddPeer>.Context,
        Consumes<IRemovePeer>.Context
    {
        private static readonly ILog _log = Logger.Get(typeof (SubscriptionMessageConsumer));
        private readonly ISubscriptionRouter _router;
        private readonly HashSet<Uri> _ignoredSourceAddresses;
        private readonly string _network;
        private readonly Guid _peerId;
        private readonly Uri _peerUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionMessageConsumer"/> class.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="network">The network.</param>
        /// <param name="ignoredSourceAddresses">The ignored source addresses.</param>
        public SubscriptionMessageConsumer(ISubscriptionRouter router, string network, params Uri[] ignoredSourceAddresses)
        {
            _router = router;
            _peerId = router.PeerId;
            _peerUri = router.PeerUri;
            _network = network;
            _ignoredSourceAddresses = new HashSet<Uri>(ignoredSourceAddresses);
        }

        public void Consume(IConsumeContext<AddPeer> context)
        {
            if (DiscardMessage(context, context.Message.PeerId))
                return;

            _router.Send(context.Message);
        }

        public void Consume(IConsumeContext<AddPeerSubscription> context)
        {
            if (DiscardMessage(context, context.Message.PeerId))
                return;

            _router.Send(context.Message);
        }

        public void Consume(IConsumeContext<AddSubscription> context)
        {
            if (DiscardMessage(context, context.Message.Subscription.ClientId))
                return;

            _router.Send(new AddPeerSubscriptionMessage
                {
                    PeerId = context.Message.Subscription.ClientId,
                    EndpointUri = context.Message.Subscription.EndpointUri,
                    MessageName = context.Message.Subscription.MessageName,
                    MessageNumber = context.Message.Subscription.SequenceNumber,
                    SubscriptionId = context.Message.Subscription.SubscriptionId,
                    CorrelationId = context.Message.Subscription.CorrelationId,
                });
        }

        public void Consume(IConsumeContext<AddSubscriptionClient> context)
        {
            if (DiscardMessage(context, context.Message.CorrelationId))
                return;

            _router.Send(new AddPeerMessage
                {
                    PeerId = context.Message.CorrelationId,
                    PeerUri = context.Message.ControlUri,
                    Timestamp = DateTime.UtcNow.Ticks,
                });
        }

        public void Consume(IConsumeContext<IRemovePeer> context)
        {
            if (DiscardMessage(context, context.Message.PeerId))
                return;

            _router.Send(context.Message);
        }

        public void Consume(IConsumeContext<IRemovePeerSubscription> context)
        {
            if (DiscardMessage(context, context.Message.PeerId))
                return;

            _router.Send(context.Message);
        }

        public void Consume(IConsumeContext<RemoveSubscription> context)
        {
            if (DiscardMessage(context, context.Message.Subscription.ClientId))
                return;

            _router.Send(new RemovePeerSubscriptionMessage
                {
                    PeerId = context.Message.Subscription.ClientId,
                    EndpointUri = context.Message.Subscription.EndpointUri,
                    MessageName = context.Message.Subscription.MessageName,
                    MessageNumber = context.Message.Subscription.SequenceNumber,
                    SubscriptionId = context.Message.Subscription.SubscriptionId,
                    CorrelationId = context.Message.Subscription.CorrelationId,
                });
        }

        public void Consume(IConsumeContext<RemoveSubscriptionClient> context)
        {
            if (DiscardMessage(context, context.Message.CorrelationId))
                return;

            _router.Send(new RemovePeerMessage
                {
                    PeerId = context.Message.CorrelationId,
                    PeerUri = context.Message.ControlUri,
                    Timestamp = DateTime.UtcNow.Ticks,
                });
        }

        public void Consume(IConsumeContext<SubscriptionRefresh> context)
        {
            if (DiscardMessage(context, Guid.Empty))
                return;

            foreach (SubscriptionInformation subscription in context.Message.Subscriptions)
            {
                // TODO:
                // do we trust subscriptions that are third-party (sent to us from systems that are not the system containing the actual subscription)
                // maybe keep track of source address for the AddPeer and allow if it is from the subscription service but not others?

                _router.Send(new AddPeerSubscriptionMessage
                    {
                        PeerId = subscription.ClientId,
                        EndpointUri = subscription.EndpointUri,
                        MessageName = subscription.MessageName,
                        MessageNumber = subscription.SequenceNumber,
                        SubscriptionId = subscription.SubscriptionId,
                        CorrelationId = subscription.CorrelationId
                    });
            }
        }

        bool DiscardMessage(IMessageContext context, Guid clientId)
        {
            if (_peerId == clientId && clientId != Guid.Empty)
            {
                _log.DebugFormat("{0} Ignoring message from client: {1}", _peerUri, clientId);
                return true;
            }

            if (_ignoredSourceAddresses.Contains(context.SourceAddress))
            {
                _log.DebugFormat("{0} Ignoring subscription because source address is us", _peerUri);
                return true;
            }

            if (!string.Equals(context.Network, _network))
            {
                _log.DebugFormat("{0} Ignoring subscription because the network '{1}' != ours '{2}'", _peerUri, context.Network, _network);
                return true;
            }

            return false;
        }
    }
}