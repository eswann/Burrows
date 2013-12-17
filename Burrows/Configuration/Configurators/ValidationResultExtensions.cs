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
namespace Burrows.Configuration.Configurators
{
	public static class ValidationResultExtensions
	{
		public static IValidationResult Failure(this IConfigurator configurator, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Failure, message);
		}

		public static IValidationResult Failure(this IConfigurator configurator, string key, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Failure, key, message);
		}

		public static IValidationResult Failure(this IConfigurator configurator, string key, string value, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Failure, key, value, message);
		}

		public static IValidationResult Warning(this IConfigurator configurator, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Warning, message);
		}

		public static IValidationResult Warning(this IConfigurator configurator, string key, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Warning, key, message);
		}

		public static IValidationResult Warning(this IConfigurator configurator, string key, string value, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Warning, key, value, message);
		}

		public static IValidationResult Success(this IConfigurator configurator, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Success, message);
		}

		public static IValidationResult Success(this IConfigurator configurator, string key, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Success, key, message);
		}

		public static IValidationResult Success(this IConfigurator configurator, string key, string value, string message)
		{
			return new ValidationResult(ValidationResultDisposition.Success, key, value, message);
		}

		public static IValidationResult WithParentKey(this IValidationResult result, string parentKey)
		{
			//string key = result.Key.Contains(".") ? result.Key.Substring(result.Key.IndexOf('.')) : "";

			string key = parentKey + "." + result.Key;

			return new ValidationResult(result.Disposition, key, result.Value, result.Message);
		}
	}
}