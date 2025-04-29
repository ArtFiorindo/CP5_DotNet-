// CP5.Receiver1/Program.cs
using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CP5.Common;
using CP5.Common.Models;

namespace CP5.Receiver1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Fruit Information Receiver ====");
            
            var factory = new ConnectionFactory() { HostName = Constants.HostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            // Declare the exchange
            channel.ExchangeDeclare(
                exchange: Constants.ValidationExchange, 
                type: ExchangeType.Direct);
                
            // Declare the queue
            channel.QueueDeclare(
                queue: Constants.ValidatedFruitQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
                
            // Bind the queue to the exchange with routing key
            channel.QueueBind(
                queue: Constants.ValidatedFruitQueue,
                exchange: Constants.ValidationExchange,
                routingKey: Constants.ValidatedFruitRoutingKey);
                
            Console.WriteLine("RabbitMQ connection established. Waiting for validated fruit messages...");
            
            // Set up consumer
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                
                try
                {
                    var fruitMessage = JsonSerializer.Deserialize<FruitMessage>(messageJson);
                    
                    Console.WriteLine("\n==================================");
                    Console.WriteLine($"[{DateTime.Now}] Received validated fruit information:");
                    Console.WriteLine($"Fruit Name: {fruitMessage.Name}");
                    Console.WriteLine($"Description: {fruitMessage.Description}");
                    Console.WriteLine($"Request Time: {fruitMessage.RequestTime}");
                    Console.WriteLine($"Validation Status: {(fruitMessage.IsValidated ? "Valid" : "Invalid")}");
                    Console.WriteLine($"Validation Message: {fruitMessage.ValidationMessage}");
                    Console.WriteLine("==================================\n");
                    
                    // Process fruit information (in a real application, this might update a database, etc.)
                    
                    if (fruitMessage.IsValidated)
                    {
                        Console.WriteLine($"Processed fruit information for: {fruitMessage.Name}");
                        
                        // Display seasonal information based on current month
                        DisplaySeasonalInfo(fruitMessage.Name);
                    }
                    else
                    {
                        Console.WriteLine($"Skipped invalid fruit information for: {fruitMessage.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
                
                // Acknowledge message
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            
            // Start consuming messages
            channel.BasicConsume(
                queue: Constants.ValidatedFruitQueue,
                autoAck: false,
                consumer: consumer);
                
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }
        
        static void DisplaySeasonalInfo(string fruitName)
        {
            // Dictionary of fruit seasons (simplified for demonstration)
            var fruitSeasons = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Apple", "Fall (September to November)" },
                { "Strawberry", "Spring (March to June)" },
                { "Mango", "Summer (May to September)" },
                { "Grape", "Fall (August to October)" },
                { "Banana", "Year-round" },
                { "Orange", "Winter (December to March)" },
                { "Watermelon", "Summer (June to August)" },
                { "Pineapple", "Spring to Summer (March to July)" }
            };
            
            if (fruitSeasons.TryGetValue(fruitName, out string season))
            {
                Console.WriteLine($"Seasonal Information: {fruitName} is in season during {season}");
                
                // Check if the fruit is currently in season
                var currentMonth = DateTime.Now.Month;
                bool isInSeason = false;
                
                if (season.Contains("Year-round"))
                {
                    isInSeason = true;
                }
                else if (season.Contains("Spring") && currentMonth >= 3 && currentMonth <= 6)
                {
                    isInSeason = true;
                }
                else if (season.Contains("Summer") && currentMonth >= 6 && currentMonth <= 9)
                {
                    isInSeason = true;
                }
                else if (season.Contains("Fall") && currentMonth >= 9 && currentMonth <= 11)
                {
                    isInSeason = true;
                }
                else if (season.Contains("Winter") && (currentMonth >= 12 || currentMonth <= 3))
                {
                    isInSeason = true;
                }
                
                Console.WriteLine($"Is currently in season: {isInSeason}");
            }
            else
            {
                Console.WriteLine($"No seasonal information available for {fruitName}");
            }
        }
    }
}