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

using Burrows.Configuration.BusConfigurators;
using Burrows.Configuration.BusServiceConfigurators;
using Burrows.Configuration.SubscriptionConfigurators;
using Burrows.Endpoints;
using System;
using System.Collections.Generic;
using Burrows.Exceptions;
using Burrows.Logging;
using Magnum;
using Magnum.Extensions;
using Burrows.Util;

namespace Burrows.Configuration.Builders
{
    public interface IControlBusBuilder :
    IBusBuilder
    {
    }

    public class ControlBusBuilder :
        IControlBusBuilder
    {
        private static readonly ILog _log = Logger.Get(typeof (ControlBusBuilder));

        private readonly IList<IBusServiceConfigurator> _busServiceConfigurators;
        private readonly IList<Action<ServiceBus>> _postCreateActions;
        private readonly ServiceBusSettings _settings;

        public ControlBusBuilder([NotNull] ServiceBusSettings settings)
        {
            Guard.AgainstNull(settings, "settings");

            _settings = settings;
            _postCreateActions = new List<Action<ServiceBus>>();
            _busServiceConfigurators = new List<IBusServiceConfigurator>();

            var subscriptionCoordinatorConfigurator = new SubscriptionRouterConfigurator(_settings.Network);
            subscriptionCoordinatorConfigurator.SetNetwork(settings.Network);
            subscriptionCoordinatorConfigurator.Configure(this);
        }

        public ServiceBusSettings Settings
        {
            get { return _settings; }
        }

        public IControlBus Build()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Creating ControlBus at {0}", _settings.InputAddress);

            ServiceBus bus = CreateServiceBus();

            ConfigureBusSettings(bus);

            RunPostCreateActions(bus);

            RunBusServiceConfigurators(bus);

            if (_settings.AutoStart)
            {
                bus.Start();
            }

            return bus;
        }

        public void Match<T>(Action<T> callback)
            where T : class, IBusBuilder
        {
            Guard.AgainstNull(callback);

            if (typeof (T).IsAssignableFrom(GetType()))
                callback(this as T);
        }

        public void AddPostCreateAction(Action<ServiceBus> postCreateAction)
        {
            _postCreateActions.Add(postCreateAction);
        }

        public void AddBusServiceConfigurator(IBusServiceConfigurator configurator)
        {
            _busServiceConfigurators.Add(configurator);
        }

        ServiceBus CreateServiceBus()
        {
            IEndpoint endpoint = _settings.EndpointCache.GetEndpoint(_settings.InputAddress);

            var serviceBus = new ServiceBus(endpoint, _settings.EndpointCache, _settings.EnablePerformanceCounters);

            return serviceBus;
        }

        void ConfigureBusSettings(ServiceBus bus)
        {
            if (_settings.ConcurrentConsumerLimit > 0)
                bus.MaximumConsumerThreads = _settings.ConcurrentConsumerLimit;

            if (_settings.ConcurrentReceiverLimit > 0)
                bus.ConcurrentReceiveThreads = _settings.ConcurrentReceiverLimit;

            bus.ReceiveTimeout = _settings.ReceiveTimeout;
		    bus.ShutdownTimeout = _settings.ShutdownTimeout;
        }

        void RunBusServiceConfigurators(ServiceBus bus)
        {
            foreach (IBusServiceConfigurator busServiceConfigurator in _busServiceConfigurators)
            {
                try
                {
                    IBusService busService = busServiceConfigurator.Create(bus);

                    bus.AddService(busServiceConfigurator.Layer, busService);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException("Failed to create the bus service: " +
                                                     busServiceConfigurator.ServiceType.ToShortTypeName(), ex);
                }
            }
        }

        void RunPostCreateActions(ServiceBus bus)
        {
            foreach (var postCreateAction in _postCreateActions)
            {
                try
                {
                    postCreateAction(bus);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException("An exception was thrown while running post-creation actions", ex);
                }
            }
        }
    }
}