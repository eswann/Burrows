using Burrows.Endpoints;
using RabbitMQ.Client;

namespace Burrows.RabbitUtils
{
    public class QueuePurge
    {
        public static void PurgeQueue(string uri)
        {
            RabbitEndpointAddress address = RabbitEndpointAddress.Parse(uri);

            using (var connection = address.ConnectionFactory.CreateConnection())
            {
                using (IModel model = connection.CreateModel())
                {
                    model.QueuePurge(address.Name);
                }
            }
        }
    }
}