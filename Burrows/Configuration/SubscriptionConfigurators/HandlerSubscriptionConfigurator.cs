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
using Burrows.Configuration.Configurators;
using Burrows.Configuration.SubscriptionBuilders;
using Burrows.Context;
using Burrows.Pipeline;

namespace Burrows.Configuration.SubscriptionConfigurators
{
    public interface IHandlerSubscriptionConfigurator<TMessage> :
    ISubscriptionConfigurator<IHandlerSubscriptionConfigurator<TMessage>>
    where TMessage : class
    {
        IHandlerSubscriptionConfigurator<TMessage> Where(Predicate<TMessage> condition);
    }

	public class HandlerSubscriptionConfigurator<TMessage> :
		SubscriptionConfigurator<IHandlerSubscriptionConfigurator<TMessage>>,
		IHandlerSubscriptionConfigurator<TMessage>,
		ISubscriptionBuilderConfigurator
		where TMessage : class
	{
		HandlerSelector<TMessage> _handler;

		public HandlerSubscriptionConfigurator(Action<TMessage> handler)
		{
			_handler = HandlerSelector.ForHandler(handler);
		}

		public HandlerSubscriptionConfigurator(Action<IConsumeContext<TMessage>, TMessage> handler)
		{
			_handler = x => context => handler(context, context.Message);
		}

		public IHandlerSubscriptionConfigurator<TMessage> Where(Predicate<TMessage> condition)
		{
			_handler = HandlerSelector.ForCondition(_handler, condition);

			return this;
		}

		public IEnumerable<IValidationResult> Validate()
		{
			if (_handler == null)
				yield return this.Failure("The handler cannot be null. This should have come from the ctor.");
		}

		public ISubscriptionBuilder Configure()
		{
			return new HandlerSubscriptionBuilder<TMessage>(_handler, ReferenceFactory);
		}
	}
}