// CP5.Sender1/Program.cs
using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using CP5.Common;
using CP5.Common.Models;

namespace CP5.Sender1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Fruit Information Sender ====");
            
            var factory = new ConnectionFactory() { HostName = Constants.HostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            // Declare the exchange
            channel.ExchangeDeclare(
                exchange: Constants.FruitExchange, 
                type: ExchangeType.Direct);
                
            // Declare the queue
            channel.QueueDeclare(
                queue: Constants.FruitToValidationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
                
            // Bind the queue to the exchange with routing key
            channel.QueueBind(
                queue: Constants.FruitToValidationQueue,
                exchange: Constants.FruitExchange,
                routingKey: Constants.FruitToValidationRoutingKey);
                
            Console.WriteLine("RabbitMQ connection established. Ready to send fruit information.");
            Console.WriteLine("Press Enter to start sending fruit data (Ctrl+C to exit)");
            
            while (true)
            {
                Console.ReadLine();
                SendFruitMessage(channel);
            }
        }
        
        static void SendFruitMessage(IModel channel)
        {
            // Sample fruits with descriptions
            var fruits = new[]
            {
                new { Name = "Apple", Description = "In season during fall. Rich in fiber and vitamin C." },
                new { Name = "Strawberry", Description = "In season during spring. High in antioxidants." },
                new { Name = "Mango", Description = "In season during summer. Contains vitamins A and C." },
                new { Name = "Grape", Description = "In season during fall. Good source of resveratrol." },
                new { Name = "Banana", Description = "Available year-round. Great source of potassium." }
            };
            
            // Select a random fruit
            var random = new Random();
            var selectedFruit = fruits[random.Next(fruits.Length)];
            
            // Create message
            var fruitMessage = new FruitMessage
            {
                Name = selectedFruit.Name,
                Description = selectedFruit.Description,
                RequestTime = DateTime.Now,
                IsValidated = false
            };
            
            // Serialize and send
            string message = JsonSerializer.Serialize(fruitMessage);
            var body = Encoding.UTF8.GetBytes(message);
            
            channel.BasicPublish(
                exchange: Constants.FruitExchange,
                routingKey: Constants.FruitToValidationRoutingKey,
                basicProperties: null,
                body: body);
                
            Console.WriteLine($"[{DateTime.Now}] Sent fruit information:");
            Console.WriteLine($"Name: {fruitMessage.Name}");
            Console.WriteLine($"Description: {fruitMessage.Description}");
            Console.WriteLine($"Request Time: {fruitMessage.RequestTime}");
            Console.WriteLine("------------------------------");
        }
    }
}