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
namespace Burrows.Testing
{
    using System;
    using Context;

    public interface IReceivedMessage
    {
        IReceiveContext Context { get; }
        Exception Exception { get; }
    }

    public interface IReceivedMessage<T> :
        IReceivedMessage
    {
        T Message { get; }
    }

    public class ReceivedMessage :
		IReceivedMessage
	{
		readonly IReceiveContext _context;
		Exception _exception;

		public ReceivedMessage(IReceiveContext context)
		{
			_context = context;
		}

		public Exception Exception
		{
			get { return _exception; }
		}

		public IReceiveContext Context
		{
			get { return _context; }
		}

		public void SetException(Exception exception)
		{
			_exception = exception;
		}
	}

	public class ReceivedMessage<T> :
		IReceivedMessage<T>
		where T : class
	{
		readonly IConsumeContext<T> _context;
		Exception _exception;

		public ReceivedMessage(IConsumeContext<T> context)
		{
			_context = context;
		}

		public Exception Exception
		{
			get { return _exception; }
		}

		public IReceiveContext Context
		{
			get { return _context.BaseContext; }
		}

		public T Message
		{
			get { return _context.Message; }
		}

		public void SetException(Exception exception)
		{
			_exception = exception;
		}
	}
}