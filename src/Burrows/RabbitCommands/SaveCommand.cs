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

using System.IO;
using System.Text;
using Burrows.Context;
using Burrows.Logging;
using Burrows.Transports;
using Magnum.Extensions;

namespace Burrows.RabbitCommands
{
    public class SaveToFileCommand : RabbitCommand
	{
		static readonly ILog _log = Logger.Get(typeof (SaveToFileCommand));
		readonly int _count;
		readonly bool _remove;
		readonly string _uri;
        private readonly string _destinationPath;
        int _nextFileNumber;

        public SaveToFileCommand(string uri, string destinationPath, int count, bool remove)
		{
			_uri = uri;
            _destinationPath = destinationPath;
			_count = count;
			_remove = remove;
			_nextFileNumber = 0;
		}

		public override bool Execute()
		{
            if (!Directory.Exists(_destinationPath))
                Directory.CreateDirectory(_destinationPath);

			IInboundTransport fromTransport = GetInboundTransport(_uri);

            _log.InfoFormat("Saving messages from '{0}' to {1}", _uri, _destinationPath);

			int lastCount;
			int saveCount = 0;
			do{
				lastCount = saveCount;

				fromTransport.Receive(receiveContext =>
					{
						if (saveCount >= _count)
							return null;

						string body = Encoding.UTF8.GetString(receiveContext.BodyStream.ReadToEnd());

                        _log.DebugFormat("Message-Id: '{0}'", receiveContext.MessageId);

						WriteMessageToFile(_destinationPath, receiveContext, body);

						saveCount++;

						if (_remove)
							return context => { };

						return null;
                    }, 5.Seconds());
			} 
            while (_remove && saveCount < _count && saveCount != lastCount);

            _log.InfoFormat("Saving messages from '{0}' to {1}", _uri, _destinationPath);

			return true;
		}

		private void WriteMessageToFile(string pathName, IReceiveContext context, string body)
		{
			string fileName = GetNextFileName(pathName);

			using (StreamWriter stream = File.CreateText(fileName))
			{
				if (context.ContentType.IsNotEmpty())
					stream.WriteLine("Content-Type: {0}", context.ContentType);
				if (context.MessageId.IsNotEmpty())
					stream.WriteLine("Message-Id: {0}", context.MessageId);

				stream.WriteLine();
				stream.Write(body);
				stream.Close();
			}
		}

		private string GetNextFileName(string pathName)
		{
			string nextFileName;
			do{
				nextFileName = string.Format("{0}-{1:00000}.msg", pathName, _nextFileNumber++);
			} while (File.Exists(nextFileName));

			return nextFileName;
		}
	}
}