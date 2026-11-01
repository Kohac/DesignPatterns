using System.ComponentModel.DataAnnotations;

namespace DesignPatterns.Application.Data.DecoratorPattern.Reports.Create;

public record ReportCreateRequest(string Name, byte[] Content) : IValidatableObject
{
    public static bool IsValidFileName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name is not "." and not ".." &&
        name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
        !name.Contains('/') && !name.Contains('\\') &&
        !name.EndsWith('.') && !name.EndsWith(' ');

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Content.Length == 0)
        {
            yield return new ValidationResult("Content is required", [nameof(Content)]);
        }

        if (!IsValidFileName(Name))
        {
            yield return new ValidationResult("A simple file name without a path is required", [nameof(Name)]);
        }

        if (!new LengthAttribute(1, 100).IsValid(Name))
        {
            yield return new ValidationResult("Name length has to be in range 1 - 100.", [nameof(Name)]);
        }
    }
}
