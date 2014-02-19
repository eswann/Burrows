using Burrows.Endpoints;
using Burrows.Transports.Configuration.Builders;
using RabbitMQ.Client;

namespace Burrows.RabbitCommands
{
    public class PurgeCommand : RabbitCommand
    {
        private readonly string _uri;

        public PurgeCommand(string uri)
        {
            _uri = uri;
        }

        public override bool Execute()
        {
            RabbitEndpointAddress address = RabbitEndpointAddress.Parse(_uri);

            var connectionFactory = new ConnectionFactoryBuilder(address).Build();

            using (var connection = connectionFactory.CreateConnection())
            {
                using (IModel model = connection.CreateModel())
                {
                    model.QueuePurge(address.Name);
                }
            }

            return true;
        }
    }
}