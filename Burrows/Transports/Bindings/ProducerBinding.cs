// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, Eric Swann et. al.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Burrows.Endpoints;
using Burrows.Logging;
using Burrows.Transports.PublisherConfirm;
using Burrows.Transports.Rabbit;
using Magnum.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrows.Transports.Bindings
{
    public class ProducerBinding : IConnectionBinding
    {
        private static readonly ILog _log = Logger.Get<ProducerBinding>();
        private readonly IEndpointAddress _address;
        private readonly PublisherConfirmSettings _publisherConfirmSettings;
        private readonly object _lock = new object();
        private int _testNackCount;

        private readonly ConcurrentDictionary<ulong, string> _confirms;
            
        IModel _channel;

        public ProducerBinding(IEndpointAddress address, PublisherConfirmSettings publisherConfirmSettings)
        {
            _address = address;
            _publisherConfirmSettings = publisherConfirmSettings;

            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                _confirms = new ConcurrentDictionary<ulong, string>();
            }
        }

        public void Bind(TransportConnection connection)
        {
            lock (_lock)
            {
                IModel channel = null;
                try
                {
                    channel = connection.Connection.CreateModel();

                    BindEvents(channel);

                    if (_publisherConfirmSettings.UsePublisherConfirms)
                    {
                        channel.ConfirmSelect();
                    }

                    _channel = channel;
                }
                catch (Exception ex)
                {
                    if (channel != null)
                    {
                        channel.Cleanup(500, ex.Message);
                    }

                    throw new InvalidConnectionException(_address.Uri, "Invalid connection to host", ex);
                }
            }
        }

        void BindEvents(IModel channel)
        {
            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                channel.BasicAcks += HandleAck;
                channel.BasicNacks += HandleNack;
                channel.ModelShutdown += HandleModelShutdown;
            }
            //channel.BasicReturn += HandleReturn;
            //channel.FlowControl += HandleFlowControl;
        }

        public void Unbind(TransportConnection connection)
        {
            lock (_lock)
            {
                try
                {
                    if (_channel != null)
                    {
                        if (_publisherConfirmSettings.UsePublisherConfirms)
                        {
                            WaitForPendingConfirms();
                        }

                        UnbindEvents(_channel);
                        _channel.Cleanup(200, "Producer Unbind");
                    }
                }
                finally
                {
                    if (_channel != null)
                        _channel.Dispose();
                    _channel = null;

                    FailPendingConfirms();
                }
            }
        }

        private void UnbindEvents(IModel channel)
        {
            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                channel.BasicAcks -= HandleAck;
                channel.BasicNacks -= HandleNack;
                channel.ModelShutdown -= HandleModelShutdown;
            }
            //channel.BasicReturn -= HandleReturn;
            //channel.FlowControl -= HandleFlowControl;
        }

        public IBasicProperties CreateProperties()
        {
            lock (_lock)
            {
                if (_channel == null)
                    throw new InvalidConnectionException(_address.Uri, "Channel should not be null");

                return _channel.CreateBasicProperties();
            }
        }

        public void Publish(string exchangeName, IBasicProperties properties, byte[] body)
        {
            lock (_lock)
            {
                if (_channel == null)
                    throw new InvalidConnectionException(_address.Uri, "No connection to RabbitMQ Host");

                if (_publisherConfirmSettings.UsePublisherConfirms)
                {
                    _confirms.TryAdd(_channel.NextPublishSeqNo,
                                        (string) properties.Headers[PublisherConfirmSettings.ClientMessageId]);
                }

                _channel.BasicPublish(exchangeName, "", properties, body);
            }
        }

        private void HandleAck(IModel model, BasicAckEventArgs args)
        {
            var confirmIds = GetConfirmIds(args.DeliveryTag, args.Multiple);

            if (confirmIds.Count > 0)
            {
                if (InTestNackMode)
                {
                    _testNackCount += confirmIds.Count;
                    _publisherConfirmSettings.Nacktion(confirmIds);
                }
                else
                {
                    _publisherConfirmSettings.Acktion(confirmIds);
                }
            }
        }


        private void HandleNack(IModel model, BasicNackEventArgs args)
        {
            var confirmIds = GetConfirmIds(args.DeliveryTag, args.Multiple);

            if (confirmIds.Count > 0)
                _publisherConfirmSettings.Nacktion(confirmIds);
        }

        private List<string> GetConfirmIds(ulong deliveryTag, bool multiple)
        {
            var confirmIds = new List<string>();
            string clientMessageId;
            if (multiple)
            {
                IEnumerable<ulong> confirmKeysToRemove = _confirms.Keys.Where(x => x <= deliveryTag);

                foreach (var confirmKey in confirmKeysToRemove)
                {
                    _confirms.TryRemove(confirmKey, out clientMessageId);
                    confirmIds.Add(clientMessageId);
                }
            }
            else
            {
                _confirms.TryRemove(deliveryTag, out clientMessageId);
                confirmIds.Add(clientMessageId);
            }

            return confirmIds;
        }

        private void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            try
            {
                FailPendingConfirms();
            }
            catch (Exception ex)
            {
                _log.Error("Fail pending confirms failed during model shutdown", ex);
            }
        }

        //void HandleFlowControl(IModel sender, FlowControlEventArgs args)
        //{
        //}

        //void HandleReturn(IModel model, BasicReturnEventArgs args)
        //{
        //}

        private void WaitForPendingConfirms()
        {
            try
            {
                bool timedOut;
                _channel.WaitForConfirms(60.Seconds(), out timedOut);
                if (timedOut)
                    _log.WarnFormat("Timeout waiting for all pending confirms on {0}", _address.Uri);
            }
            catch (Exception ex)
            {
                _log.Error("Waiting for pending confirms threw an exception", ex);
            }
        }

        private void FailPendingConfirms()
        {
            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                var confirmIds = _confirms.Values.ToList();
                _confirms.Clear();

                _publisherConfirmSettings.Nacktion(confirmIds);
            }
        }

        private bool InTestNackMode
        {
            get { return _publisherConfirmSettings.TestNacks > _testNackCount; }
        }
    }
}