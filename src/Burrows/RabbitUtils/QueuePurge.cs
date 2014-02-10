using Burrows.Endpoints;
using Burrows.Transports.Configuration.Builders;
using RabbitMQ.Client;

namespace Burrows.RabbitUtils
{
    public class QueuePurge
    {
        public static void PurgeQueue(string uri)
        {
            RabbitEndpointAddress address = RabbitEndpointAddress.Parse(uri);

            var connectionFactory = new ConnectionFactoryBuilder(address).Build();

            using (var connection = connectionFactory.CreateConnection())
            {
                using (IModel model = connection.CreateModel())
                {
                    model.QueuePurge(address.Name);
                }
            }
        }
    }
}