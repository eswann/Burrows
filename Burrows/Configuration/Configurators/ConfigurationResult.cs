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
using System.Diagnostics;
using System.Linq;
using Burrows.Exceptions;
using Burrows.Util;

namespace Burrows.Configuration.Configurators
{
    public interface IConfigurationResult
    {
        IEnumerable<IValidationResult> Results { get; }
    }

	[Serializable, DebuggerDisplay("{DebuggerString()}")]
	public class ConfigurationResult :
		IConfigurationResult
	{
		readonly IList<IValidationResult> _results;

		ConfigurationResult(IEnumerable<IValidationResult> results)
		{
			_results = results.ToList();
		}

		public IEnumerable<IValidationResult> Results
		{
			get { return _results; }
		}

	    public bool ContainsFailure
	    {
	        get { return _results.Any(x => x.Disposition == ValidationResultDisposition.Failure); }
	    }

		[UsedImplicitly]
		protected string DebuggerString()
		{
			var debuggerString = string.Join(", ", _results);

			return string.IsNullOrWhiteSpace(debuggerString)
				? "No Obvious Problems says ConfigurationResult"
				: debuggerString;
		}

		public static IConfigurationResult CompileResults(IEnumerable<IValidationResult> results)
		{
			var result = new ConfigurationResult(results);

			if (result.ContainsFailure)
			{
				string message = "The service bus was not properly configured:" +
				                 Environment.NewLine +
				                 string.Join(Environment.NewLine, result.Results.Select(x => x.ToString()).ToArray());

				throw new ConfigurationException(result, message);
			}

			return result;
		}
	}
}