using System.ComponentModel.DataAnnotations;
using Kevin.ApiService.Services;

namespace Kevin.ApiService.Contracts.V1;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class AllowedTargetUrlAttribute() : ValidationAttribute("The {0} field must be a relative path or an http(s) url on the trusted domain.")
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // null is handled by [Required]
        if (value is not string url)
        {
            return ValidationResult.Success;
        }

        var validator = validationContext.GetRequiredService<ITargetUrlValidator>();
        return validator.IsAllowed(url)
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName),
                validationContext.MemberName is { } memberName ? [memberName] : null);
    }
}
