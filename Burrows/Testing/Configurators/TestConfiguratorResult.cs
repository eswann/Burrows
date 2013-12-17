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
namespace Burrows.Testing.Configurators
{
    using System;

    public interface ITestConfiguratorResult
    {
        /// <summary>
        /// The disposition of the result, any Failure items will prevent
        /// the configuration from completing.
        /// </summary>
        TestConfiguratorResultDisposition Disposition { get; }

        /// <summary>
        /// The key associated with the result (chained if configurators are nested)
        /// </summary>
        string Key { get; }

        /// <summary>
        /// The message associated with the result
        /// </summary>
        string Message { get; }

        /// <summary>
        /// The value associated with the result
        /// </summary>
        string Value { get; }
    }

    [Serializable]
    public class TestConfiguratorResult :
        ITestConfiguratorResult
    {
        public TestConfiguratorResult(TestConfiguratorResultDisposition disposition, string key, string value,
                                          string message)
        {
            Disposition = disposition;
            Key = key;
            Value = value;
            Message = message;
        }

        public TestConfiguratorResult(TestConfiguratorResultDisposition disposition, string key, string message)
        {
            Disposition = disposition;
            Key = key;
            Message = message;
        }

        public TestConfiguratorResult(TestConfiguratorResultDisposition disposition, string message)
        {
            Key = "";
            Disposition = disposition;
            Message = message;
        }

        public TestConfiguratorResultDisposition Disposition { get; private set; }
        public string Key { get; private set; }
        public string Message { get; private set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Disposition, string.IsNullOrEmpty(Key) ? Message : Key + " " + Message);
        }
    }
}