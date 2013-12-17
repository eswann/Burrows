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
    using System.Linq;
    using Diagnostics.Introspection;
    using Magnum.Extensions;
    using Messages;
    using Stact;

    public class SubscriptionRouterService : IBusService, ISubscriptionRouter, ISubscriptionObserver, IDiagnosticsSource
    {
        private readonly IList<BusSubscriptionEventListener> _listeners;
        private readonly string _network;
        private readonly IList<ISubscriptionObserver> _observers;
        private readonly ActorRef _peerCache;
        private readonly Guid _peerId;
        private readonly Uri _peerUri;
        bool _disposed;
        UnsubscribeAction _unregister;

        public SubscriptionRouterService(IServiceBus bus, string network)
        {
            _peerUri = bus.ControlBus.Endpoint.Address.Uri;
            _network = network;
            _peerId = NewId.NextGuid();

            _observers = new List<ISubscriptionObserver>();
            _listeners = new List<BusSubscriptionEventListener>();

            _unregister = () => true;

            _peerUri = bus.ControlBus.Endpoint.Address.Uri;

            var connector = new BusSubscriptionConnector(bus);

            _peerCache = ActorFactory.Create<PeerCache>(x =>
                {
                    x.ConstructedBy((fiber, scheduler, inbox) => new PeerCache(connector, _peerId, _peerUri));
                    x.UseSharedScheduler();
                    x.HandleOnPoolFiber();
                })
                .GetActor();
        }

        public void Inspect(IDiagnosticsProbe probe)
        {
            probe.Add("mt.network", _network);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Start(IServiceBus bus)
        {
            ListenToBus(bus);
        }

        public void Stop()
        {
            _unregister();
        }

        public void OnSubscriptionAdded(ISubscriptionAdded message)
        {
            lock (_observers)
                _observers.Each(x => x.OnSubscriptionAdded(message));
        }

        public void OnSubscriptionRemoved(ISubscriptionRemoved message)
        {
            lock (_observers)
                _observers.Each(x => x.OnSubscriptionRemoved(message));
        }

        public void OnComplete()
        {
        }

        public IEnumerable<ISubscription> LocalSubscriptions
        {
            get { return _listeners.SelectMany(x => x.Subscriptions); }
        }

        public void Send(AddPeerSubscription message)
        {
            if (_peerCache != null)
                _peerCache.Send(message);
        }

        public void Send(IRemovePeerSubscription message)
        {
            if (_peerCache != null)
                _peerCache.Send(message);
        }

        public void Send(AddPeer message)
        {
            if (_peerCache != null)
                _peerCache.Send(message);
        }

        public void Send(IRemovePeer message)
        {
            if (_peerCache != null)
                _peerCache.Send(message);
        }

        public string Network
        {
            get { return _network; }
        }

        public Guid PeerId
        {
            get { return _peerId; }
        }

        public Uri PeerUri
        {
            get { return _peerUri; }
        }

        public void AddObserver(ISubscriptionObserver observer)
        {
            lock (_observers)
                _observers.Add(observer);
        }

        void ListenToBus(IServiceBus bus)
        {
            var subscriptionEventListener = new BusSubscriptionEventListener(bus, this);

            _unregister += bus.Configure(x =>
                {
                    UnregisterAction unregisterAction = x.Register(subscriptionEventListener);

                    return () => unregisterAction();
                });

            _listeners.Add(subscriptionEventListener);

            IServiceBus controlBus = bus.ControlBus;
            if (controlBus != bus)
            {
                ListenToBus(controlBus);
            }
        }

        void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                lock (_observers)
                    _observers.Each(x => x.OnComplete());

                _peerCache.Send<StopSubscriptionRouterService>();
                _peerCache.SendRequestWaitForResponse<Exit>(new ExitImpl(), 30.Seconds());
            }

            _disposed = true;
        }

        class ExitImpl : Exit
        {
        }
    }
}