﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using System.Collections.Generic;
using Burrows.Configuration.Builders;
using Burrows.Configuration.Configurators;
using Burrows.Exceptions;
using Burrows.Transports;
using Burrows.Util;

namespace Burrows.Configuration.EndpointConfigurators
{
    public class TransportFactoryConfigurator<TTransport> :
		IEndpointFactoryBuilderConfigurator
		where TTransport : class, ITransportFactory
	{
		readonly Func<TTransport> _transportFactory;

		public TransportFactoryConfigurator([NotNull] Func<TTransport> transportFactory)
		{
			_transportFactory = transportFactory;
		}

		public IEnumerable<IValidationResult> Validate()
		{
			if (_transportFactory == null)
				yield return this.Failure("TransportFactory", "The transport factory was null. This should have been in the ctor.");
		}

		public IEndpointFactoryBuilder Configure(IEndpointFactoryBuilder builder)
		{
			TTransport transportFactory = _transportFactory();

			if (transportFactory == null)
				throw new ConfigurationException("A transport factory was not created");

			builder.AddTransportFactory(transportFactory);

			return builder;
		}
	}
}