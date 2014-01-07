// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Burrows.Configuration.BusConfigurators;
using Burrows.Configuration.EndpointConfigurators;
using Magnum.Reflection;
using Newtonsoft.Json;
using Burrows.Serialization;

namespace Burrows.Configuration
{
    public static class SerializerConfigurationExtensions
    {
        public static T UseJsonSerializer<T>(this T configurator)
            where T : IEndpointFactoryConfigurator
        {
            configurator.SetDefaultSerializer<JsonMessageSerializer>();

            return configurator;
        }

        public static T ConfigureJsonSerializer<T>(this T configurator,
            Func<JsonSerializerSettings, JsonSerializerSettings> configure)
            where T : IEndpointFactoryConfigurator
        {
            JsonMessageSerializer.SerializerSettings = configure(JsonMessageSerializer.SerializerSettings);

            return configurator;
        }

        public static T ConfigureJsonDeserializer<T>(this T configurator,
            Func<JsonSerializerSettings, JsonSerializerSettings> configure)
            where T : IEndpointFactoryConfigurator
        {
            JsonMessageSerializer.DeserializerSettings = configure(JsonMessageSerializer.DeserializerSettings);

            return configurator;
        }


        static T SetDefaultSerializer<T>(this T configurator, Func<IMessageSerializer> serializerFactory)
            where T : IEndpointFactoryConfigurator
        {
            var serializerConfigurator = new DefaultSerializerEndpointFactoryConfigurator(serializerFactory);

            configurator.AddEndpointFactoryConfigurator(serializerConfigurator);

            return configurator;
        }


        /// <summary>
        /// Sets the default message serializer for endpoints
        /// </summary>
        /// <typeparam name="TSerializer"></typeparam>
        /// <param name="configurator"></param>
        /// <returns></returns>
        public static IEndpointFactoryConfigurator SetDefaultSerializer<TSerializer>(
            this IEndpointFactoryConfigurator configurator)
            where TSerializer : IMessageSerializer, new()
        {
            return SetDefaultSerializer(configurator, () => new TSerializer());
        }

        /// <summary>
        /// Sets the default message serializer for endpoints
        /// </summary>
        /// <typeparam name="TSerializer"></typeparam>
        /// <param name="configurator"></param>
        /// <returns></returns>
        public static IServiceBusConfigurator SetDefaultSerializer<TSerializer>(this IServiceBusConfigurator configurator)
            where TSerializer : IMessageSerializer, new()
        {
            return SetDefaultSerializer(configurator, () => new TSerializer());
        }

        /// <summary>
        /// Sets the default message serializer for endpoints
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public static T SetDefaultSerializer<T>(this T configurator,
            Type serializerType)
            where T : IEndpointFactoryConfigurator
        {
            return SetDefaultSerializer(configurator, () => (IMessageSerializer)FastActivator.Create(serializerType));
        }

        /// <summary>
        /// Sets the default message serializer for endpoints
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static T SetDefaultSerializer<T>(this T configurator,
            IMessageSerializer serializer)
            where T : IEndpointFactoryConfigurator
        {
            return SetDefaultSerializer(configurator, () => serializer);
        }
      
    }
}