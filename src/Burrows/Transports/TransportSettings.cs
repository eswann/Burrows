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

using Burrows.Endpoints;

namespace Burrows.Transports
{
    using Magnum;
    using Util;

    public class TransportSettings : ITransportSettings
	{
		public TransportSettings([NotNull] IEndpointAddress address)
		{
			Guard.AgainstNull(address, "address");

			Address = address;

			CreateIfMissing = true;
			PurgeExistingMessages = false;
		}

		public TransportSettings([NotNull] IEndpointAddress address, [NotNull] ITransportSettings source)
		{
			Guard.AgainstNull(address, "address");
			Guard.AgainstNull(source, "source");

			Address = address;

			CreateIfMissing = source.CreateIfMissing;
			PurgeExistingMessages = source.PurgeExistingMessages;
		}

		/// <summary>
		/// The address of the endpoint
		/// </summary>
		public IEndpointAddress Address { get; private set; }

	    /// <summary>
		/// The transport should be created if it was not found
		/// </summary>
		public bool CreateIfMissing { get; set; }

	    /// <summary>
		/// If the transport should purge any existing messages before reading from the queue
		/// </summary>
		public bool PurgeExistingMessages { get; set; }
	}
}