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

using Burrows.Configuration.Advanced;

namespace Burrows.RequestResponse.Configurators
{
    using System;
    using System.Threading;
    using Context;

    public interface IInlineRequestConfigurator<TRequest> :
    IRequestConfigurator<TRequest>
    where TRequest : class
    {
        /// <summary>
        /// Sets the synchronization context for the response and timeout handlers to 
        /// the current synchronization context
        /// </summary>
        void UseCurrentSynchronizationContext();

        /// <summary>
        /// Sets the synchronization context to the specified synchronization context
        /// </summary>
        /// <param name="synchronizationContext"></param>
        void SetSynchronizationContext(SynchronizationContext synchronizationContext);

        /// <summary>
        /// Configures a handler to be called if a response of the specified type
        /// is received. Once received, the request completes by default unless
        /// overridden by calling Continue on the request.
        /// </summary>
        /// <typeparam name="TResponse">The message type of the response</typeparam>
        /// <param name="handler">The handler to call with the response message</param>
        void Handle<TResponse>(Action<TResponse> handler)
            where TResponse : class;

        /// <summary>
        /// Configures a handler to be called if a response of the specified type
        /// is received. Once received, the request completes by default unless
        /// overridden by calling Continue on the request.
        /// </summary>
        /// <typeparam name="TResponse">The message type of the response</typeparam>
        /// <param name="handler">The handler to call with the response message</param>
        void Handle<TResponse>(Action<IConsumeContext<TResponse>, TResponse> handler)
            where TResponse : class;

        /// <summary>
        /// Specifies a handler for a fault published by the request handler
        /// </summary>
        /// <param name="faultCallback"></param>
        void HandleFault(Action<Fault<TRequest>> faultCallback);

        /// <summary>
        /// Specifies a handler for a fault published by the request handler
        /// </summary>
        /// <param name="faultCallback"></param>
        void HandleFault(Action<IConsumeContext<Fault<TRequest>>, Fault<TRequest>> faultCallback);
    }


    public class InlineRequestConfigurator<TRequest> :
        RequestConfiguratorBase<TRequest>,
        IInlineRequestConfigurator<TRequest>
        where TRequest : class
    {
        private readonly Request<TRequest> _request;

        public InlineRequestConfigurator(TRequest message)
            : base(message)
        {
            _request = new Request<TRequest>(RequestId, Request);
        }

        public void Handle<T>(Action<T> handler)
            where T : class
        {
            AddHandler(typeof(T),
                () => new CompleteResponseHandler<T>(RequestId, _request, RequestSynchronizationContext, handler));
        }

        public void Handle<T>(Action<IConsumeContext<T>, T> handler)
            where T : class
        {
            AddHandler(typeof(T),
                () => new CompleteResponseHandler<T>(RequestId, _request, RequestSynchronizationContext, handler));
        }

        public void Watch<T>(Action<T> watcher)
            where T : class
        {
            AddHandler(typeof(T), () => new WatchResponseHandler<T>(RequestId, RequestSynchronizationContext, watcher));
        }

        public void Watch<T>(Action<IConsumeContext<T>, T> watcher)
            where T : class
        {
            AddHandler(typeof(T), () => new WatchResponseHandler<T>(RequestId, RequestSynchronizationContext, watcher));
        }

        public void HandleFault(Action<Fault<TRequest>> faultCallback)
        {
            AddHandler(typeof(Fault<TRequest>), () => new CompleteResponseHandler<Fault<TRequest>>(RequestId, 
                _request, RequestSynchronizationContext, faultCallback));
        }

        public void HandleFault(Action<IConsumeContext<Fault<TRequest>>, Fault<TRequest>> faultCallback)
        {
            AddHandler(typeof(Fault<TRequest>), () => new CompleteResponseHandler<Fault<TRequest>>(RequestId,
                _request, RequestSynchronizationContext, faultCallback));
        }

        public IAsyncRequest<TRequest> Build(IServiceBus bus)
        {
            _request.SetTimeout(Timeout);
            if (TimeoutHandler != null)
                _request.SetTimeoutHandler(TimeoutHandler);

            UnsubscribeAction unsubscribeAction = bus.Configure(x => Handlers.CombineSubscriptions(h => h.Connect(x)));

            _request.SetUnsubscribeAction(unsubscribeAction);

            return _request;
        }
    }
}