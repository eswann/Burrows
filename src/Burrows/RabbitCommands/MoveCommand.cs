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

using Burrows.Context;
using Burrows.Logging;
using Burrows.Transports;
using Magnum.Extensions;

namespace Burrows.RabbitCommands
{
    public class MoveCommand : RabbitCommand
	{
		private static readonly ILog _log = Logger.Get(typeof (MoveCommand));
		private readonly int _count;
		private readonly string _fromUri;
		private readonly string _toUri;

		public MoveCommand(string fromUri, string toUri, int count)
		{
			_fromUri = fromUri;
			_toUri = toUri;
			_count = count;
		}

		public override bool Execute()
		{
            _log.InfoFormat("Moving messages from '{0}' to '{1}'", _fromUri, _toUri);

            IInboundTransport fromTransport = GetInboundTransport(_fromUri);
            IOutboundTransport toTransport = GetOutboundTransport(_toUri);
            
			int moveCount = 0;
			for (int i = 0; i < _count; i++)
			{
				fromTransport.Receive(receiveContext =>
					{
						return context =>
							{
								var moveContext = new MoveMessageSendContext(context);

								toTransport.Send(moveContext);

                                _log.DebugFormat("Moving Message Id: {0}", context.MessageId);

								moveCount++;
							};
                    }, 5.Seconds());
			}

			_log.InfoFormat("{0} message{1} moved from {2} to {3}", moveCount, moveCount == 1 ? "" : "s", _fromUri, _toUri);

			return true;
		}
	}
}