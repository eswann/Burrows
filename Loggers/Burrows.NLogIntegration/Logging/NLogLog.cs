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

    /// <summary>
    /// A logger that wraps to NLog. See http://stackoverflow.com/questions/7412156/how-to-retain-callsite-information-when-wrapping-nlog
    /// </summary>
    public class NLogLog : ILog
    {
        private readonly NLog.Logger _log;

        /// <summary>
        /// Create a new NLog logger instance.
        /// </summary>
        /// <param name="name">Name of type to log as.</param>
        public NLogLog([NotNull] NLog.Logger log, [NotNull] string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            _log = log;
        }

        public bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _log.IsWarnEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _log.IsFatalEnabled; }
        }

        public void Log(Burrows.Logging.LogLevel level, object message)
        {
            _log.Log(GetNLogLevel(level), message);
        }

        public void Log(Burrows.Logging.LogLevel level, object message, Exception exception)
        {
            _log.LogException(GetNLogLevel(level), message == null ? "" : message.ToString(), exception);
        }

        public void Log(Burrows.Logging.LogLevel level, LogOutputProvider messageProvider)
        {
            _log.Log(GetNLogLevel(level), ToGenerator(messageProvider));
        }

        public void LogFormat(Burrows.Logging.LogLevel level, IFormatProvider formatProvider, string format,
                              params object[] args)
        {
            _log.Log(GetNLogLevel(level), formatProvider, format, args);
        }

        public void LogFormat(Burrows.Logging.LogLevel level, string format, params object[] args)
        {
            _log.Log(GetNLogLevel(level), format, args);
        }

        public void Debug(object message)
        {
            _log.Log(NLog.LogLevel.Debug, message);
        }

        public void Debug(object message, Exception exception)
        {
            _log.LogException(NLog.LogLevel.Debug, message == null ? "" : message.ToString(), exception);
        }

        public void Debug(LogOutputProvider messageProvider)
        {
            _log.Debug(ToGenerator(messageProvider));
        }

        public void Info(object message)
        {
            _log.Log(NLog.LogLevel.Info, message);
        }

        public void Info(object message, Exception exception)
        {
            _log.LogException(NLog.LogLevel.Info, message == null ? "" : message.ToString(), exception);
        }

        public void Info(LogOutputProvider messageProvider)
        {
            _log.Info(ToGenerator(messageProvider));
        }

        public void Warn(object message)
        {
            _log.Log(NLog.LogLevel.Warn, message);
        }

        public void Warn(object message, Exception exception)
        {
            _log.LogException(NLog.LogLevel.Warn, message == null ? "" : message.ToString(), exception);
        }

        public void Warn(LogOutputProvider messageProvider)
        {
            _log.Warn(ToGenerator(messageProvider));
        }

        public void Error(object message)
        {
            _log.Log(NLog.LogLevel.Error, message);
        }

        public void Error(object message, Exception exception)
        {
            _log.LogException(NLog.LogLevel.Error, message == null ? "" : message.ToString(), exception);
        }

        public void Error(LogOutputProvider messageProvider)
        {
            _log.Error(ToGenerator(messageProvider));
        }

        public void Fatal(object message)
        {
            _log.Log(NLog.LogLevel.Fatal, message);
        }

        public void Fatal(object message, Exception exception)
        {
            _log.LogException(NLog.LogLevel.Fatal, message == null ? "" : message.ToString(), exception);
        }

        public void Fatal(LogOutputProvider messageProvider)
        {
            _log.Fatal(ToGenerator(messageProvider));
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Debug, formatProvider, format, args);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Debug, format, args);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Info, formatProvider, format, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Info, format, args);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Warn, formatProvider, format, args);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Warn, format, args);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Error, formatProvider, format, args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Error, format, args);
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Fatal, formatProvider, format, args);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _log.Log(NLog.LogLevel.Fatal, format, args);
        }

        NLog.LogLevel GetNLogLevel(Burrows.Logging.LogLevel level)
        {
            if (level == Burrows.Logging.LogLevel.Fatal)
                return NLog.LogLevel.Fatal;
            if (level == Burrows.Logging.LogLevel.Error)
                return NLog.LogLevel.Error;
            if (level == Burrows.Logging.LogLevel.Warn)
                return NLog.LogLevel.Warn;
            if (level == Burrows.Logging.LogLevel.Info)
                return NLog.LogLevel.Info;
            if (level == Burrows.Logging.LogLevel.Debug)
                return NLog.LogLevel.Debug;
            if (level == Burrows.Logging.LogLevel.All)
                return NLog.LogLevel.Trace;

            return NLog.LogLevel.Off;
        }

        LogMessageGenerator ToGenerator(LogOutputProvider provider)
        {
            return () =>
                {
                    object obj = provider();
                    return obj == null ? "" : obj.ToString();
                };
        }
    }
}