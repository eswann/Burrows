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
namespace Burrows.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
	public class AddressException : ConfigurationException
	{
		const string DefaultHelpLink = "http://www.rabbitmq.com/specification.html";

		public AddressException()
		{
			HelpLink = DefaultHelpLink;
		}

		public AddressException(string message)
			: base(message)
		{
			HelpLink = DefaultHelpLink;
		}

		public AddressException(string message, Exception innerException)
			: base(message, innerException)
		{
			HelpLink = DefaultHelpLink;
		}

		protected AddressException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}