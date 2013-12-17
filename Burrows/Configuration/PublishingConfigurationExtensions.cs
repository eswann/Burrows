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

using Burrows.Configuration.BusConfigurators;
using Burrows.PublisherConfirms;
using Burrows.PublisherConfirms.BackingStores;
using Burrows.Transports.Configuration.Extensions;

namespace Burrows.Configuration
{
    public static class PublishingConfigurationExtensions
    {

        public static IServiceBusConfigurator UsePublisherConfirms(this IServiceBusConfigurator configurator, PublishSettings publishSettings)
        {
            var confirmer = publishSettings.Confirmer;

            configurator.UseRabbitMq(conf => conf.UsePublisherConfirms(confirmer.RecordPublicationSuccess, confirmer.RecordPublicationFailure, publishSettings.TestNacks));
            return configurator;
        }

        public static PublishSettings UsePublisherConfirms(this PublishSettings publishSettings, string publisherId)
        {
            publishSettings.PublisherId = publisherId;
            publishSettings.UsePublisherConfirms = true;

            if(publishSettings.Confirmer == null)
                publishSettings.Confirmer = new Confirmer();

            return publishSettings;
        }

        public static PublishSettings WithFileBackingStore(this PublishSettings publishSettings, string fileRepositoryPath = "MessageBackingStore")
        {
            publishSettings.BackingStoreMethod = BackingStoreMethod.FileSystem;
            publishSettings.FileRepositoryPath = fileRepositoryPath;

            return publishSettings;
        }

        public static PublishSettings WithMaxSuccessiveFailures(this PublishSettings publishSettings, int maxSuccessiveFailures)
        {
            publishSettings.MaxSuccessiveFailures = maxSuccessiveFailures;
            return publishSettings;
        }

        public static PublishSettings WithGetBatchSize(this PublishSettings publishSettings, int getBatchSize)
        {
            publishSettings.GetStoredMessagesBatchSize = getBatchSize;
            return publishSettings;
        }

        public static PublishSettings WithPublishRetryInterval(this PublishSettings publishSettings, int intevalMilliseconds)
        {
            publishSettings.PublishRetryInterval = intevalMilliseconds;
            return publishSettings;
        }

        public static PublishSettings WithProcessBufferedMessagesInterval(this PublishSettings publishSettings, int intevalMilliseconds)
        {
            publishSettings.ProcessBufferedMessagesInterval = intevalMilliseconds;
            return publishSettings;
        }

        public static PublishSettings WithTimerCheckInteval(this PublishSettings publishSettings, int intevalMilliseconds)
        {
            publishSettings.TimerCheckInterval = intevalMilliseconds;
            return publishSettings;
        }

        public static PublishSettings WithTestNacks(this PublishSettings publishSettings, int testNacks)
        {
            publishSettings.TestNacks = testNacks;
            return publishSettings;
        }

        
    }
}