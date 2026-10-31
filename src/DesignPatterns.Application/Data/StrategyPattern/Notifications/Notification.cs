using System.ComponentModel.DataAnnotations;
using DesignPatterns.Application.Base.Enums;

namespace DesignPatterns.Application.Data.StrategyPattern.Notifications;

public record Notification(string Recipient, string Subject, string Message, NotificationChannel Channel) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Message))
        {
            yield return new ValidationResult("Message is required", [nameof(Message)]);
        }

        if (!Enum.IsDefined(Channel))
        {
            yield return new ValidationResult("Channel is required", [nameof(Channel)]);
        }

        if (string.IsNullOrWhiteSpace(Recipient))
        {
            yield return new ValidationResult("Recipient is required", [nameof(Recipient)]);
        }

        if (Channel == NotificationChannel.Email && !new EmailAddressAttribute().IsValid(Recipient))
        {
            yield return new ValidationResult("Recipient is not a valid email address", [nameof(Recipient)]);
        }

        if (Channel == NotificationChannel.Sms && !new PhoneAttribute().IsValid(Recipient))
        {
            yield return new ValidationResult("Recipient is not a valid phone number", [nameof(Recipient)]);
        }
    }
}
