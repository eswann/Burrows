using System;
using Burrows.Transports;
using Burrows.Util;

namespace Burrows.RabbitCommands
{
    public interface IRabbitCommand
    {
        bool Execute();
    }

    public abstract class RabbitCommand : IRabbitCommand
    {
        private static readonly TransportCache _transportCache = new TransportCache();

        public abstract bool Execute();

        public IInboundTransport GetInboundTransport(string uri)
        {
            Uri fromUri = uri.ToUri("The inbound URI was invalid");
            return _transportCache.GetInboundTransport(fromUri);
        }

        public IOutboundTransport GetOutboundTransport(string uri)
        {
            Uri toUri = uri.ToUri("The outbound URI was invalid");
            return _transportCache.GetOutboundTransport(toUri);
        }

    }
}