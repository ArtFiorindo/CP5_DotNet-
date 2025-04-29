// Constants.cs
namespace CP5.Common
{
    public static class Constants
    {
        // RabbitMQ Connection
        public const string HostName = "localhost";
        
        // Exchanges
        public const string FruitExchange = "fruit_exchange";
        public const string UserExchange = "user_exchange";
        public const string ValidationExchange = "validation_exchange";
        
        // Queues
        public const string FruitToValidationQueue = "fruit_to_validation_queue";
        public const string UserToValidationQueue = "user_to_validation_queue";
        public const string ValidatedFruitQueue = "validated_fruit_queue";
        public const string ValidatedUserQueue = "validated_user_queue";
        
        // Routing Keys
        public const string FruitToValidationRoutingKey = "fruit.validation";
        public const string UserToValidationRoutingKey = "user.validation";
        public const string ValidatedFruitRoutingKey = "validated.fruit";
        public const string ValidatedUserRoutingKey = "validated.user";
    }
}

// Models/FruitMessage.cs
using System;

namespace CP5.Common.Models
{
    [Serializable]
    public class FruitMessage
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime RequestTime { get; set; }
        public bool IsValidated { get; set; }
        public string ValidationMessage { get; set; }

        public override string ToString()
        {
            return $"Fruit: {Name}\nDescription: {Description}\nRequest Time: {RequestTime}\nValidated: {IsValidated}\nValidation Message: {ValidationMessage}";
        }
    }
}

// Models/UserMessage.cs
using System;

namespace CP5.Common.Models
{
    [Serializable]
    public class UserMessage
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string RG { get; set; }
        public string CPF { get; set; }
        public DateTime RegistrationTime { get; set; }
        public bool IsValidated { get; set; }
        public string ValidationMessage { get; set; }

        public override string ToString()
        {
            return $"User: {FullName}\nAddress: {Address}\nRG: {RG}\nCPF: {CPF}\nRegistration Time: {RegistrationTime}\nValidated: {IsValidated}\nValidation Message: {ValidationMessage}";
        }
    }
}