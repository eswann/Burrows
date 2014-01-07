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

namespace Burrows.Testing.TestDecorators
{
    using System;
    using Diagnostics.Introspection;
    using Scenarios;
    using Transports;

    public class EndpointFactoryTestDecorator :
		IEndpointFactory
	{
		readonly IEndpointFactory _endpointFactory;
		readonly EndpointTestScenario _testContext;

		public EndpointFactoryTestDecorator(IEndpointFactory endpointFactory, EndpointTestScenario testContext)
		{
			_endpointFactory = endpointFactory;
			_testContext = testContext;
		}

		public void Dispose()
		{
			_endpointFactory.Dispose();
		}

		public IEndpoint CreateEndpoint(Uri uri)
		{
			IEndpoint endpoint = _endpointFactory.CreateEndpoint(uri);

			var endpointTestDecorator = new EndpointTestDecorator(endpoint, _testContext);

			_testContext.AddEndpoint(endpointTestDecorator);

			return endpointTestDecorator;
		}

	    public void Inspect(IDiagnosticsProbe probe)
	    {
	        _endpointFactory.Inspect(probe);
	    }

	    public void AddTransportFactory(ITransportFactory factory)
		{
			_endpointFactory.AddTransportFactory(factory);
		}
	}
}