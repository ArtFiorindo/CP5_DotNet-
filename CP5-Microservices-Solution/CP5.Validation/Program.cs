// CP5.Validation/Program.cs
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CP5.Common;
using CP5.Common.Models;

namespace CP5.Validation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Validation Service ====");
            
            var factory = new ConnectionFactory() { HostName = Constants.HostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            // Declare exchanges
            channel.ExchangeDeclare(exchange: Constants.FruitExchange, type: ExchangeType.Direct);
            channel.ExchangeDeclare(exchange: Constants.UserExchange, type: ExchangeType.Direct);
            channel.ExchangeDeclare(exchange: Constants.ValidationExchange, type: ExchangeType.Direct);
            
            // Declare queues
            channel.QueueDeclare(queue: Constants.FruitToValidationQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: Constants.UserToValidationQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: Constants.ValidatedFruitQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: Constants.ValidatedUserQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            
            // Bind queues
            channel.QueueBind(queue: Constants.FruitToValidationQueue, exchange: Constants.FruitExchange, routingKey: Constants.FruitToValidationRoutingKey);
            channel.QueueBind(queue: Constants.UserToValidationQueue, exchange: Constants.UserExchange, routingKey: Constants.UserToValidationRoutingKey);
            channel.QueueBind(queue: Constants.ValidatedFruitQueue, exchange: Constants.ValidationExchange, routingKey: Constants.ValidatedFruitRoutingKey);
            channel.QueueBind(queue: Constants.ValidatedUserQueue, exchange: Constants.ValidationExchange, routingKey: Constants.ValidatedUserRoutingKey);
            
            Console.WriteLine("RabbitMQ connection established. Ready to validate messages.");
            
            // Set up consumer for fruit information
            var fruitConsumer = new EventingBasicConsumer(channel);
            fruitConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                
                try
                {
                    var fruitMessage = JsonSerializer.Deserialize<FruitMessage>(messageJson);
                    Console.WriteLine($"[{DateTime.Now}] Received fruit message for validation:");
                    Console.WriteLine($"Name: {fruitMessage.Name}");
                    Console.WriteLine($"Description: {fruitMessage.Description}");
                    
                    // Validate fruit message
                    ValidateFruitMessage(fruitMessage);
                    
                    // Send validated message
                    var validatedJson = JsonSerializer.Serialize(fruitMessage);
                    var validatedBody = Encoding.UTF8.GetBytes(validatedJson);
                    
                    channel.BasicPublish(
                        exchange: Constants.ValidationExchange,
                        routingKey: Constants.ValidatedFruitRoutingKey,
                        basicProperties: null,
                        body: validatedBody);
                        
                    Console.WriteLine($"Validation result: {fruitMessage.ValidationMessage}");
                    Console.WriteLine("Fruit message forwarded to Receiver 1");
                    Console.WriteLine("------------------------------");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing fruit message: {ex.Message}");
                }
                
                // Acknowledge message
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            
            // Set up consumer for user information
            var userConsumer = new EventingBasicConsumer(channel);
            userConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                
                try
                {
                    var userMessage = JsonSerializer.Deserialize<UserMessage>(messageJson);
                    Console.WriteLine($"[{DateTime.Now}] Received user message for validation:");
                    Console.WriteLine($"Name: {userMessage.FullName}");
                    Console.WriteLine($"CPF: {userMessage.CPF}");
                    
                    // Validate user message
                    ValidateUserMessage(userMessage);
                    
                    // Send validated message
                    var validatedJson = JsonSerializer.Serialize(userMessage);
                    var validatedBody = Encoding.UTF8.GetBytes(validatedJson);
                    
                    channel.BasicPublish(
                        exchange: Constants.ValidationExchange,
                        routingKey: Constants.ValidatedUserRoutingKey,
                        basicProperties: null,
                        body: validatedBody);
                        
                    Console.WriteLine($"Validation result: {userMessage.ValidationMessage}");
                    Console.WriteLine("User message forwarded to Receiver 2");
                    Console.WriteLine("------------------------------");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing user message: {ex.Message}");
                }
                
                // Acknowledge message
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            
            // Start consuming messages
            channel.BasicConsume(queue: Constants.FruitToValidationQueue, autoAck: false, consumer: fruitConsumer);
            channel.BasicConsume(queue: Constants.UserToValidationQueue, autoAck: false, consumer: userConsumer);
            
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }
        
        static void ValidateFruitMessage(FruitMessage message)
        {
            // Simple validation rules for fruit message
            bool isValid = true;
            string validationMessage = "Valid";
            
            if (string.IsNullOrWhiteSpace(message.Name))
            {
                isValid = false;
                validationMessage = "Invalid: Fruit name cannot be empty";
            }
            else if (string.IsNullOrWhiteSpace(message.Description))
            {
                isValid = false;
                validationMessage = "Invalid: Fruit description cannot be empty";
            }
            else if (message.RequestTime > DateTime.Now)
            {
                isValid = false;
                validationMessage = "Invalid: Request time cannot be in the future";
            }
            
            message.IsValidated = isValid;
            message.ValidationMessage = validationMessage;
        }
        
        static void ValidateUserMessage(UserMessage message)
        {
            // Simple validation rules for user message
            bool isValid = true;
            string validationMessage = "Valid";
            
            if (string.IsNullOrWhiteSpace(message.FullName))
            {
                isValid = false;
                validationMessage = "Invalid: User name cannot be empty";
            }
            else if (string.IsNullOrWhiteSpace(message.Address))
            {
                isValid = false;
                validationMessage = "Invalid: User address cannot be empty";
            }
            else if (string.IsNullOrWhiteSpace(message.RG))
            {
                isValid = false;
                validationMessage = "Invalid: RG cannot be empty";
            }
            else if (string.IsNullOrWhiteSpace(message.CPF))
            {
                isValid = false;
                validationMessage = "Invalid: CPF cannot be empty";
            }
            else if (!IsValidCpf(message.CPF))
            {
                isValid = false;
                validationMessage = "Invalid: CPF format is invalid";
            }
            else if (message.RegistrationTime > DateTime.Now)
            {
                isValid = false;
                validationMessage = "Invalid: Registration time cannot be in the future";
            }
            
            message.IsValidated = isValid;
            message.ValidationMessage = validationMessage;
        }
        
        static bool IsValidCpf(string cpf)
        {
            // Simple CPF format validation (XXX.XXX.XXX-XX)
            var regex = new Regex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
            return regex.IsMatch(cpf);
        }
    }
}