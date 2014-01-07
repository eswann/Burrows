using NUnit.Framework;
using RabbitMQ.Client;

namespace Burrows.Tests.RabbitMq.Assumptions
{
    [TestFixture]
    public class BasicExchangePerformanceDifferences
    {
        [Test]
        public void Run()
        {
            var cf = new ConnectionFactory();
            cf.UserName = "guest";
            cf.Password = "guest";
            cf.Port = 5672;
            cf.VirtualHost = "/";
            cf.HostName = "localhost";

            using(var conn = cf.CreateConnection())
            {
                using(var model = conn.CreateModel())
                {
                    model.QueueDeclare("testq", true, true, true, null);
                    model.ExchangeDeclare("testtopic", "topic", true, true, null);



                }
                
            }
        }
        
    }
}