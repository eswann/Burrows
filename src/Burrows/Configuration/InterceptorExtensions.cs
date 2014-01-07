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

using Burrows.Configuration.BusConfigurators;
using Burrows.Pipeline;
using Burrows.Pipeline.Configuration;

namespace Burrows.Configuration
{
    public static class InterceptorExtensions
    {
        /// <summary>
        /// Adds an inbound message interceptor to the bus configuration
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="interceptor"></param>
        public static IServiceBusConfigurator AddInboundInterceptor(this IServiceBusConfigurator configurator, IInboundMessageInterceptor interceptor)
        {
            var builderConfigurator = new PostCreateBusBuilderConfigurator(bus =>
                {
                    var interceptorConfigurator = new InboundMessageInterceptorConfigurator(bus.InboundPipeline);

                    interceptorConfigurator.Create(interceptor);
                });

            configurator.AddBusConfigurator(builderConfigurator);
            return configurator;
        }
    }
}