using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
{
    class Worker
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())

            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: "task_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Não permitir que o consumidor receba mais de 1 mensagem por vez
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine(" [x] Received {0}", message);

                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    Console.WriteLine(" [x] Done");

                    // Obs: é possível acessar o canal via ((EventingBasicConsumer)sender).Model here
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(
                    queue: "task_queue",
                    autoAck: false,
                    consumer: consumer
                );

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            };
        }
    }
}
