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

using Burrows.Configuration;
using Burrows.NewIds;

namespace BusDriver.Commands
{
    using System;
    using System.Threading;
    using Formatting;
    using Magnum.Extensions;
    using Burrows;
    using Burrows.Context;
    using Burrows.Diagnostics.Introspection;
    using Burrows.Diagnostics.Introspection.Messages;
    using Burrows.Logging;
    using Burrows.Endpoints;

    public class StatusCommand :
        Consumes<IBusStatus>.Context,
        Command,
        IPendingCommand
    {
        static readonly ILog _log = Logger.Get(typeof (StatusCommand));

        readonly ManualResetEvent _complete;
        readonly string _uriString;
        string _requestId;
        UnsubscribeAction _unsubscribe;

        public StatusCommand(string uriString)
        {
            _uriString = uriString;
            _complete = new ManualResetEvent(false);
        }

        public bool Execute()
        {
            Uri uri = _uriString.ToUri("The status URI was invalid");

            IServiceBus bus = Program.GetBus(uri.Scheme);

            IEndpoint endpoint = bus.GetEndpoint(uri);

            _log.DebugFormat("Sending status request to '{0}'", uri);

            _unsubscribe = bus.SubscribeInstance(this);
            _requestId = NewId.Next().ToString("N");

            endpoint.Send<IGetBusStatus>(new GetBusStatus(), x =>
                {
                    x.SendResponseTo(bus);
                    x.SetRequestId(_requestId);
                });

            Program.AddPendingCommand(this);

            return true;
        }


        public void Consume(IConsumeContext<IBusStatus> context)
        {
            if (!_requestId.Equals(context.RequestId))
                return;

            if (_unsubscribe != null)
                _unsubscribe();
            _unsubscribe = null;

            ITextBlock text = new TextBlock()
                .BeginBlock("Status URI:", _uriString)
                .EndBlock();

            foreach (IBusStatusEntry entry in context.Message.Entries)
            {
                text.BodyFormat("{0}:{1}", entry.Key, entry.Value);
            }
            text.EndBlock();

            _log.Info(text.ToString());
            _complete.Set();
        }


        public string Description
        {
            get { return "status on '{0}'".FormatWith(_uriString); }
        }

        public WaitHandle WaitHandle
        {
            get { return _complete; }
        }
    }
}