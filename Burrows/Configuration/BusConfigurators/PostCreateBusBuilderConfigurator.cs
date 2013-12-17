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

using System;
using System.Collections.Generic;
using System.Linq;
using Burrows.Configuration.Builders;
using Burrows.Configuration.Configurators;

namespace Burrows.Configuration.BusConfigurators
{
    public class PostCreateBusBuilderConfigurator :
		IBusBuilderConfigurator
	{
		readonly IList<IBusBuilderConfigurator> _configurators;
		readonly Action<ServiceBus> _postCreateAction;

		public PostCreateBusBuilderConfigurator(Action<ServiceBus> postCreateAction)
		{
			_postCreateAction = postCreateAction;
			_configurators = new List<IBusBuilderConfigurator>();
		}

		public IBusBuilder Configure(IBusBuilder builder)
		{
			builder.AddPostCreateAction(_postCreateAction);

			return builder;
		}

		public IEnumerable<IValidationResult> Validate()
		{
			return from configurator in _configurators
			       from result in configurator.Validate()
			       select result.WithParentKey("PostCreateBus");
		}
	}
}