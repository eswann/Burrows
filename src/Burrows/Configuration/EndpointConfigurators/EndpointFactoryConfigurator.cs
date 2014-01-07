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
using Burrows.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Burrows.Configuration.EndpointConfigurators
{
    /// <summary>
	/// Allows for the configuration of the EndpointFactory through the use of an EndpointFactoryConfigurator
	/// </summary>
	public interface IEndpointFactoryConfigurator :
		IConfigurator
	{
		/// <summary>
		/// Gets the endpoint factory defaults.
		/// </summary>
		IEndpointFactoryDefaultSettings Defaults { get; }

		/// <summary>
		/// Creates the endpoint factory with the configuration
		/// </summary>
		/// <returns></returns>
		IEndpointFactory CreateEndpointFactory();

		/// <summary>
		/// Overrides the default EndpointResolver builder with another builder
		/// </summary>
		/// <param name="endpointFactoryBuilderFactory"></param>
		void UseEndpointFactoryBuilder(Func<IEndpointFactoryDefaultSettings, IEndpointFactoryBuilder> endpointFactoryBuilderFactory);

		/// <summary>
		/// Adds an endpoint configurator to the endpoint resolver builder
		/// </summary>
		/// <param name="configurator"></param>
		void AddEndpointFactoryConfigurator(IEndpointFactoryBuilderConfigurator configurator);
	}

	public class EndpointFactoryConfigurator :
		IEndpointFactoryConfigurator
	{
		readonly EndpointFactoryDefaultSettings _defaultSettings;
		readonly IList<IEndpointFactoryBuilderConfigurator> _endpointFactoryConfigurators;
		Func<IEndpointFactoryDefaultSettings, IEndpointFactoryBuilder> _endpointFactoryBuilderFactory;

		public EndpointFactoryConfigurator(EndpointFactoryDefaultSettings defaultSettings)
		{
			_defaultSettings = defaultSettings;
			_endpointFactoryBuilderFactory = DefaultEndpointResolverBuilderFactory;
			_endpointFactoryConfigurators = new List<IEndpointFactoryBuilderConfigurator>();
		}

		public IEnumerable<IValidationResult> Validate()
		{
			if (_endpointFactoryBuilderFactory == null)
				yield return this.Failure("BuilderFactory", "The builder factory was null. Since this came from a 'Default' this is spooky.");

			foreach (var result in _endpointFactoryConfigurators.SelectMany(configurator => configurator.Validate()))
				yield return result.WithParentKey("EndpointFactory");
		}

		public void UseEndpointFactoryBuilder(Func<IEndpointFactoryDefaultSettings, IEndpointFactoryBuilder> endpointFactoryBuilderFactory)
		{
			_endpointFactoryBuilderFactory = endpointFactoryBuilderFactory;
		}

		public void AddEndpointFactoryConfigurator(IEndpointFactoryBuilderConfigurator configurator)
		{
			_endpointFactoryConfigurators.Add(configurator);
		}

		public IEndpointFactoryDefaultSettings Defaults
		{
			get { return _defaultSettings; }
		}

		public IEndpointFactory CreateEndpointFactory()
		{
			IEndpointFactoryBuilder builder = _endpointFactoryBuilderFactory(_defaultSettings);

			foreach (IEndpointFactoryBuilderConfigurator configurator in _endpointFactoryConfigurators)
			{
				builder = configurator.Configure(builder);
			}

			return builder.Build();
		}

		static IEndpointFactoryBuilder DefaultEndpointResolverBuilderFactory(IEndpointFactoryDefaultSettings defaults)
		{
			return new EndpointFactoryBuilder(defaults);
		}
	}
}