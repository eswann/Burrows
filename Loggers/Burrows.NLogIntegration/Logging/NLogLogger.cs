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
namespace Burrows.NLogIntegration.Logging
{
    using System;
    using Burrows.Logging;
    using NLog;
    using Util;

    public class NLogLogger : ILogger
    {
        private readonly Func<string, NLog.Logger> _logFactory;
 
        public NLogLogger([NotNull] LogFactory factory)
        {
            _logFactory = factory.GetLogger;
        }

        public NLogLogger()
        {
            _logFactory = LogManager.GetLogger;
        }

        public ILog Get(string name)
        {
            return new NLogLog(_logFactory(name), name);
        }

        public static void Use()
        {
            Burrows.Logging.Logger.UseLogger(new NLogLogger());
        }
    }
}