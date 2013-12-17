// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using System.Text.RegularExpressions;
using System.Threading;
using Burrows.Exceptions;
using Magnum;
using Magnum.Extensions;
using RabbitMQ.Client;
using Burrows.Util;

namespace Burrows.Endpoints
{
    public interface IRabbitEndpointAddress : IEndpointAddress
    {
        ConnectionFactory ConnectionFactory { get; }
        string Name { get; }

        /// <summary>
        /// The prefetch count for consumers
        /// </summary>
        ushort PrefetchCount { get; }

        /// <summary>
        /// If bound to a queue, the queue should be exclusive
        /// </summary>
        bool Exclusive { get; }

        /// <summary>
        /// If bound to a queue, the queue should be durable
        /// </summary>
        bool Durable { get; }

        /// <summary>
        /// If bound to a queue, the queue should automatically be deleted when connection closed
        /// </summary>
        bool AutoDelete { get; }

        IRabbitEndpointAddress ForQueue(string name);

        IDictionary<string, object> QueueArguments();
    }

    public class RabbitEndpointAddress :
        IRabbitEndpointAddress
    {
        const string FormatErrorMsg =
            "The path can be empty, or a sequence of these characters: letters, digits, hyphen, underscore, period, or colon.";

        private static readonly string _localMachineName = Environment.MachineName.ToLowerInvariant();
        private static readonly Regex _regex = new Regex(@"^[A-Za-z0-9\-_\.:]+$");
        private readonly bool _autoDelete;
        private readonly ConnectionFactory _connectionFactory;
        private readonly bool _durable = true;
        private readonly bool _exclusive;
        private readonly bool _isHighAvailable;
        private readonly string _name;
        private readonly Uri _uri;
        Func<bool> _isLocal;
        ushort _prefetch;
        int _ttl;

        public RabbitEndpointAddress(Uri uri, ConnectionFactory connectionFactory, string name)
        {
            _uri = GetSanitizedUri(uri).Uri;

            _connectionFactory = connectionFactory;

            if (name == "*")
                name = NewId.Next().ToString("NS");

            _name = name;

            _isLocal = () => DetermineIfEndpointIsLocal(uri);

            _ttl = uri.Query.GetValueFromQueryString("ttl", 0);
            _prefetch = uri.Query.GetValueFromQueryString("prefetch", (ushort)Math.Max(Environment.ProcessorCount, 10));

            bool isTemporary = uri.Query.GetValueFromQueryString("temporary", false);

            _isHighAvailable = uri.Query.GetValueFromQueryString("ha", false);
            if (_isHighAvailable && isTemporary)
                throw new AddressException("A highly available queue cannot be temporary");

            _durable = uri.Query.GetValueFromQueryString("durable", !isTemporary);
            _exclusive = uri.Query.GetValueFromQueryString("exclusive", isTemporary);
            _autoDelete = uri.Query.GetValueFromQueryString("autodelete", isTemporary);
        }

        public bool Exclusive
        {
            get { return _exclusive; }
        }

        public ConnectionFactory ConnectionFactory
        {
            get { return _connectionFactory; }
        }

        public string Name
        {
            get { return _name; }
        }

        public ushort PrefetchCount
        {
            get { return _prefetch; }
        }

        public IRabbitEndpointAddress ForQueue(string name)
        {
            return ForQueue(_uri, name);
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public bool IsLocal
        {
            get { return _isLocal(); }
        }

        public bool Durable
        {
            get { return _durable; }
        }

        public bool AutoDelete
        {
            get { return _autoDelete; }
        }

        public IDictionary<string,object> QueueArguments()
        {
            var ht = new Dictionary<string, object>();

            if (_isHighAvailable)
                ht.Add("x-ha-policy", "all");
            if (_ttl > 0)
                ht.Add("x-message-ttl", _ttl);

            return ht.Keys.Count == 0
                       ? null
                       : ht;
        }

        public IRabbitEndpointAddress ForQueue(Uri originalUri, string name)
        {
            var uri = new Uri(originalUri.GetLeftPart(UriPartial.Path));
            if (uri.AbsolutePath.EndsWith(_name, StringComparison.InvariantCultureIgnoreCase))
            {
                var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port,
                    uri.AbsolutePath.Remove(uri.AbsolutePath.Length - _name.Length) + name);
                //builder.Query = uri.Query;

                return new RabbitEndpointAddress(builder.Uri, _connectionFactory, name);
            }

            throw new InvalidOperationException("Uri is not properly formed");
        }

        public void SetTtl(TimeSpan ttl)
        {
            _ttl = ttl.Milliseconds;
        }

        public void SetPrefetchCount(ushort count)
        {
            _prefetch = count;
        }

        static UriBuilder GetSanitizedUri(Uri uri)
        {
            var uriPath = new Uri(uri.GetLeftPart(UriPartial.Path));
            var builder = new UriBuilder(uriPath.Scheme, uriPath.Host, uriPath.Port, uriPath.PathAndQuery);
            builder.Query = string.IsNullOrEmpty(uri.Query) ? "" : uri.Query.Substring(1);
            return builder;
        }

        public override string ToString()
        {
            return _uri.ToString();
        }

        bool DetermineIfEndpointIsLocal(Uri uri)
        {
            string hostName = uri.Host;
            bool local = string.CompareOrdinal(hostName, ".") == 0 ||
                         string.Compare(hostName, "localhost", StringComparison.OrdinalIgnoreCase) == 0 ||
                         string.Compare(uri.Host, _localMachineName, StringComparison.OrdinalIgnoreCase) == 0;

            Interlocked.Exchange(ref _isLocal, () => local);

            return local;
        }

        public static RabbitEndpointAddress Parse(string address)
        {
            return Parse(new Uri(address));
        }

        public static RabbitEndpointAddress Parse(Uri address)
        {
            Guard.AgainstNull(address, "address");

            if (string.Compare("rabbitmq", address.Scheme, StringComparison.OrdinalIgnoreCase) != 0)
                throw new AddressException("The invalid scheme was specified: " + address.Scheme ?? "(null)");

            var connectionFactory = new ConnectionFactory
                {
                    HostName = address.Host,
                    UserName = "",
                    Password = "",
                };

            if (address.IsDefaultPort)
                connectionFactory.Port = 5672;
            else if (!address.IsDefaultPort)
                connectionFactory.Port = address.Port;

            if (!address.UserInfo.IsEmpty())
            {
                if (address.UserInfo.Contains(":"))
                {
                    string[] parts = address.UserInfo.Split(':');
                    connectionFactory.UserName = parts[0];
                    connectionFactory.Password = parts[1];
                }
                else
                    connectionFactory.UserName = address.UserInfo;
            }

            string name = address.AbsolutePath.Substring(1);
            string[] pathSegments = name.Split('/');
            if (pathSegments.Length == 2)
            {
                connectionFactory.VirtualHost = pathSegments[0];
                name = pathSegments[1];
            }

            ushort heartbeat = address.Query.GetValueFromQueryString("heartbeat", connectionFactory.RequestedHeartbeat);
            connectionFactory.RequestedHeartbeat = heartbeat;

            if (name == "*")
            {
                string uri = address.GetLeftPart(UriPartial.Path);
                if (uri.EndsWith("*"))
                {
                    name = NewId.Next().ToString("NS");
                    uri = uri.Remove(uri.Length - 1) + name;

                    var builder = new UriBuilder(uri);
                    builder.Query = string.IsNullOrEmpty(address.Query) ? "" : address.Query.Substring(1);

                    address = builder.Uri;
                }
                else
                    throw new InvalidOperationException("Uri is not properly formed");
            }
            else
                VerifyQueueOrExchangeNameIsLegal(name);

            return new RabbitEndpointAddress(address, connectionFactory, name);
        }

        static void VerifyQueueOrExchangeNameIsLegal(string path)
        {
            Match match = _regex.Match(path);

            if (!match.Success)
                throw new AddressException(FormatErrorMsg);
        }
    }
}