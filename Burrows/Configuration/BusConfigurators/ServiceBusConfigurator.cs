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

using Burrows.Configuration.Builders;
using Burrows.Configuration.Configurators;
using Burrows.Configuration.EndpointConfigurators;
using Burrows.Configuration.SubscriptionConfigurators;
using Burrows.Endpoints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Burrows.Logging;
using Magnum.Extensions;
using Burrows.Util;

namespace Burrows.Configuration.BusConfigurators
{
    /// <summary>
    /// <para>The configurator to call methods on, as well as extension methods on,
    /// in order to configure your service bus. The configuration
    /// goes a lot by convention, but this interface allows you to configure
    /// almost any aspect of the bus.</para>
    /// 
    /// <para>
    /// Documentation is at http://readthedocs.org/docs/masstransit/en/latest/configuration/index.html
    /// </para>
    /// </summary>
    public interface IServiceBusConfigurator : IEndpointFactoryConfigurator
    {
        /// <summary>
        /// Specifies the builder factory to use when the service is invoked
        /// </summary>
        /// <param name="builderFactory"></param>
        void UseBusBuilder(Func<ServiceBusSettings, IBusBuilder> builderFactory);

        /// <summary>
        /// Adds a configurator to the subscription coordinator builder
        /// </summary>
        /// <param name="configurator"></param>
        void AddSubscriptionRouterConfigurator(ISubscriptionRouterBuilderConfigurator configurator);

        /// <summary>
        /// Adds a configurator for the service bus builder to the configurator
        /// </summary>
        /// <param name="configurator"></param>
        void AddBusConfigurator(IBusBuilderConfigurator configurator);

        /// <summary>
        /// Specify the endpoint from which messages should be read
        /// </summary>
        /// <param name="uri">
        /// The uri of the endpoint that this bus should
        /// receive message from.
        /// </param>
        void ReceiveFrom(Uri uri);

        /// <summary>
        /// Sets the network key for subscriptions
        /// </summary>
        /// <param name="network"></param>
        void SetNetwork(string network);

        /// <summary>
        /// Disable the performance counters
        /// </summary>
        void DisablePerformanceCounters();

        /// <summary>
        /// Specifies an action to call before a message is consumed. Implementors
        /// should take care to not remove previously set actions so that multiple
        /// calls to this method generates calls to all those action parameters.
        /// </summary>
        /// <param name="beforeConsume">The action to run before consumption.</param>
        void BeforeConsumingMessage(Action beforeConsume);

        /// <summary>
        /// Specifies an action to call after a message is consumed. Implementors
        /// should take care to not remove previously set actions so that multiple
        /// calls to this method generates calls to all those action parameters.
        /// </summary>
        /// <param name="afterConsume">The action to run after consumption</param>
        void AfterConsumingMessage(Action afterConsume);
    }

    /// <summary>
    /// <see cref="IServiceBusConfigurator"/>. Core implementation of service bus
    /// configurator.
    /// </summary>
    public class ServiceBusConfigurator : IServiceBusConfigurator
    {
        private static readonly ILog _log = Logger.Get(typeof (ServiceBusConfigurator));

        private readonly IList<IBusBuilderConfigurator> _configurators;
        private readonly IEndpointFactoryConfigurator _endpointFactoryConfigurator;
        private readonly ServiceBusSettings _settings;

        private readonly SubscriptionRouterConfigurator _subscriptionRouterConfigurator;
        Func<ServiceBusSettings, IBusBuilder> _builderFactory;

        public ServiceBusConfigurator(ServiceBusDefaultSettings defaultSettings)
        {
            _settings = new ServiceBusSettings(defaultSettings);

            _builderFactory = DefaultBuilderFactory;
            _configurators = new List<IBusBuilderConfigurator>();

            _endpointFactoryConfigurator = new EndpointFactoryConfigurator(new EndpointFactoryDefaultSettings());

            _subscriptionRouterConfigurator = new SubscriptionRouterConfigurator(_settings.Network);
            _configurators.Add(_subscriptionRouterConfigurator);
        }

        public IEnumerable<IValidationResult> Validate()
        {
            if (_builderFactory == null)
                yield return this.Failure("BuilderFactory", "The builder factory cannot be null.");

            if (_settings.InputAddress == null)
            {
                string msg =
                    "The 'InputAddress' is null. #sadpanda I was expecting an address to be set like 'msmq://localhost/queue'";
                msg += "or 'rabbitmq://localhost/queue'. The InputAddress is a 'Uri' by the way.";

                yield return this.Failure("InputAddress", msg);
            }

            foreach (IValidationResult result in _endpointFactoryConfigurator.Validate())
                yield return result.WithParentKey("EndpointFactory");

            foreach (IValidationResult result in _configurators.SelectMany(configurator => configurator.Validate()))
                yield return result;

            foreach (IValidationResult result in _subscriptionRouterConfigurator.Validate())
                yield return result;
        }

        public void UseBusBuilder(Func<ServiceBusSettings, IBusBuilder> builderFactory)
        {
            _builderFactory = builderFactory;
        }

        public void AddSubscriptionRouterConfigurator(ISubscriptionRouterBuilderConfigurator configurator)
        {
            _subscriptionRouterConfigurator.AddConfigurator(configurator);
        }

        public void AddBusConfigurator(IBusBuilderConfigurator configurator)
        {
            _configurators.Add(configurator);
        }

        public void ReceiveFrom(Uri uri)
        {
            _settings.InputAddress = uri;
        }

        public void SetNetwork(string network)
        {
            _settings.Network = network.IsEmpty() ? null : network;
        }

        public void DisablePerformanceCounters()
        {
            _settings.EnablePerformanceCounters = false;
        }

        public void BeforeConsumingMessage(Action beforeConsume)
        {
            if (_settings.BeforeConsume == null)
                _settings.BeforeConsume = beforeConsume;
            else
                _settings.BeforeConsume += beforeConsume;
        }

        public void AfterConsumingMessage(Action afterConsume)
        {
            if (_settings.AfterConsume == null)
                _settings.AfterConsume = afterConsume;
            else
                _settings.AfterConsume += afterConsume;
        }

        public void UseEndpointFactoryBuilder(
            Func<IEndpointFactoryDefaultSettings, IEndpointFactoryBuilder> endpointFactoryBuilderFactory)
        {
            _endpointFactoryConfigurator.UseEndpointFactoryBuilder(endpointFactoryBuilderFactory);
        }

        public void AddEndpointFactoryConfigurator(IEndpointFactoryBuilderConfigurator configurator)
        {
            _endpointFactoryConfigurator.AddEndpointFactoryConfigurator(configurator);
        }


        public IEndpointFactoryDefaultSettings Defaults
        {
            get { return _endpointFactoryConfigurator.Defaults; }
        }

        public IEndpointFactory CreateEndpointFactory()
        {
            return _endpointFactoryConfigurator.CreateEndpointFactory();
        }

        public IServiceBus CreateServiceBus()
        {
            LogAssemblyVersionInformation();

            IEndpointCache endpointCache = CreateEndpointCache();
            _settings.EndpointCache = endpointCache;

            IBusBuilder builder = _builderFactory(_settings);

            _subscriptionRouterConfigurator.SetNetwork(_settings.Network);

            // run through all configurators that have been set and let
            // them do their magic
            foreach (IBusBuilderConfigurator configurator in _configurators)
            {
                builder = configurator.Configure(builder);
            }

            IServiceBus bus = builder.Build();

            return bus;
        }

        static void LogAssemblyVersionInformation()
        {
            if (_log.IsInfoEnabled)
            {
                var assembly = typeof(ServiceBusFactory).Assembly;

                var assemblyVersion = assembly.GetName().Version;
                FileVersionInfo assemblyFileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                string assemblyFileVersion = assemblyFileVersionInfo.FileVersion;

                _log.InfoFormat("Burrows v{0}/v{1}, .NET Framework v{2}",
                    assemblyFileVersion,
                    assemblyVersion,
                    Environment.Version);
            }
        }

        /// <summary>
        /// This lets you change the bus settings without
        /// having to implement a <see cref="IBusBuilderConfigurator"/>
        /// first. Use with caution.
        /// </summary>
        /// <param name="callback">The callback that changes the settings.</param>
        public void ChangeSettings([NotNull] Action<ServiceBusSettings> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            callback(_settings);
        }

        IEndpointCache CreateEndpointCache()
        {
            if (_settings.EndpointCache != null)
                return _settings.EndpointCache;

            IEndpointFactory endpointFactory = CreateEndpointFactory();

            IEndpointCache endpointCache = new EndpointCache(endpointFactory);

            return endpointCache;
        }

        static IBusBuilder DefaultBuilderFactory(ServiceBusSettings settings)
        {
            return new ServiceBusBuilder(settings);
        }
    }
}