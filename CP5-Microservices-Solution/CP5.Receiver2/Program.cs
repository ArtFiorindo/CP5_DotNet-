// CP5.Receiver2/Program.cs
using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CP5.Common;
using CP5.Common.Models;

namespace CP5.Receiver2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== User Information Receiver ====");
            
            try
            {
                var factory = new ConnectionFactory() { 
                    HostName = Constants.HostName,
                    // Recommended additional connection settings:
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };
                
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                
                // Declare the exchange
                channel.ExchangeDeclare(
                    exchange: Constants.ValidationExchange, 
                    type: ExchangeType.Direct,
                    durable: true);
                    
                // Declare the queue with additional properties
                channel.QueueDeclare(
                    queue: Constants.ValidatedUserQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-queue-mode", "lazy" }  // Helps with large messages
                    });
                    
                // Bind the queue to the exchange with routing key
                channel.QueueBind(
                    queue: Constants.ValidatedUserQueue,
                    exchange: Constants.ValidationExchange,
                    routingKey: Constants.ValidatedUserRoutingKey);
                    
                // Configure quality of service
                channel.BasicQos(
                    prefetchSize: 0,
                    prefetchCount: 1,  // Process one message at a time
                    global: false);
                
                Console.WriteLine("RabbitMQ connection established. Waiting for validated user messages...");
                
                // Set up consumer
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var messageJson = Encoding.UTF8.GetString(body);
                        
                        var userMessage = JsonSerializer.Deserialize<UserMessage>(messageJson);
                        
                        Console.WriteLine("\n==================================");
                        Console.WriteLine($"[{DateTime.Now}] Received validated user information:");
                        Console.WriteLine($"Full Name: {userMessage.FullName}");
                        Console.WriteLine($"Address: {userMessage.Address}");
                        Console.WriteLine($"RG: {userMessage.RG}");
                        Console.WriteLine($"CPF: {userMessage.CPF}");
                        Console.WriteLine($"Registration Time: {userMessage.RegistrationTime}");
                        Console.WriteLine($"Validation Status: {(userMessage.IsValidated ? "Valid" : "Invalid")}");
                        Console.WriteLine($"Validation Message: {userMessage.ValidationMessage}");
                        Console.WriteLine("==================================\n");
                        
                        if (userMessage.IsValidated)
                        {
                            ProcessValidUser(userMessage);
                        }
                        else
                        {
                            ProcessInvalidUser(userMessage);
                        }

                        // Acknowledge message processing
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"ERROR: Failed to deserialize message - {jsonEx.Message}");
                        // Reject the malformed message (don't requeue)
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR: {ex.Message}");
                        // Reject the message but requeue it for retry
                        channel.BasicReject(ea.DeliveryTag, true);
                    }
                };

                // Start consuming
                channel.BasicConsume(
                    queue: Constants.ValidatedUserQueue,
                    autoAck: false,  // Manual acknowledgment
                    consumer: consumer);

                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static void ProcessValidUser(UserMessage user)
        {
            Console.WriteLine($"Processing VALID user registration for: {user.FullName}");
            
            // Calculate registration duration
            TimeSpan accountAge = DateTime.Now - user.RegistrationTime;
            Console.WriteLine($"Account age: {accountAge.TotalHours:F2} hours since registration");
            
            // Simulate database update
            Console.WriteLine($"Updating database for user {user.CPF}...");
            // In a real application, this would be a database call
            
            // Simulate sending welcome email
            Console.WriteLine($"Sending welcome email to {user.FullName}...");
            
            Console.WriteLine("User registration verified and processed successfully");
        }

        private static void ProcessInvalidUser(UserMessage user)
        {
            Console.WriteLine($"Processing INVALID user registration for: {user.FullName}");
            
            // Log the validation issues
            Console.WriteLine($"Validation failed: {user.ValidationMessage}");
            
            // Simulate sending notification to user
            Console.WriteLine($"Sending correction request to user {user.CPF}...");
            
            Console.WriteLine("User needs to correct information and resubmit");
        }
    }
}