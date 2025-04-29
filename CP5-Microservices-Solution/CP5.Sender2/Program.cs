// CP5.Sender2/Program.cs
using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using CP5.Common;
using CP5.Common.Models;

namespace CP5.Sender2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== User Information Sender ====");
            
            var factory = new ConnectionFactory() { HostName = Constants.HostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            // Declare the exchange
            channel.ExchangeDeclare(
                exchange: Constants.UserExchange, 
                type: ExchangeType.Direct);
                
            // Declare the queue
            channel.QueueDeclare(
                queue: Constants.UserToValidationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
                
            // Bind the queue to the exchange with routing key
            channel.QueueBind(
                queue: Constants.UserToValidationQueue,
                exchange: Constants.UserExchange,
                routingKey: Constants.UserToValidationRoutingKey);
                
            Console.WriteLine("RabbitMQ connection established. Ready to send user information.");
            Console.WriteLine("Press Enter to start sending user data (Ctrl+C to exit)");
            
            while (true)
            {
                Console.ReadLine();
                SendUserMessage(channel);
            }
        }
        
        static void SendUserMessage(IModel channel)
        {
            // Sample users with fictional data
            var users = new[]
            {
                new { Name = "João da Silva", Address = "Rua das Flores, 123 - São Paulo", RG = "12.345.678-9", CPF = "123.456.789-00" },
                new { Name = "Maria Oliveira", Address = "Av. Paulista, 1000 - São Paulo", RG = "23.456.789-0", CPF = "234.567.890-11" },
                new { Name = "Pedro Santos", Address = "Rua Augusta, 500 - São Paulo", RG = "34.567.890-1", CPF = "345.678.901-22" },
                new { Name = "Ana Costa", Address = "Av. Ibirapuera, 200 - São Paulo", RG = "45.678.901-2", CPF = "456.789.012-33" },
                new { Name = "Carlos Ferreira", Address = "Rua Oscar Freire, 300 - São Paulo", RG = "56.789.012-3", CPF = "567.890.123-44" }
            };
            
            // Select a random user
            var random = new Random();
            var selectedUser = users[random.Next(users.Length)];
            
            // Create message
            var userMessage = new UserMessage
            {
                FullName = selectedUser.Name,
                Address = selectedUser.Address,
                RG = selectedUser.RG,
                CPF = selectedUser.CPF,
                RegistrationTime = DateTime.Now,
                IsValidated = false
            };
            
            // Serialize and send
            string message = JsonSerializer.Serialize(userMessage);
            var body = Encoding.UTF8.GetBytes(message);
            
            channel.BasicPublish(
                exchange: Constants.UserExchange,
                routingKey: Constants.UserToValidationRoutingKey,
                basicProperties: null,
                body: body);
                
            Console.WriteLine($"[{DateTime.Now}] Sent user information:");
            Console.WriteLine($"Name: {userMessage.FullName}");
            Console.WriteLine($"Address: {userMessage.Address}");
            Console.WriteLine($"RG: {userMessage.RG}");
            Console.WriteLine($"CPF: {userMessage.CPF}");
            Console.WriteLine($"Registration Time: {userMessage.RegistrationTime}");
            Console.WriteLine("------------------------------");
        }
    }
}