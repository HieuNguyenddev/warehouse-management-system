using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserManagementAPI.Common.Dtos;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NotBlankAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class OptionalPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        if (text.Length is >= 8 and <= 100)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class OptionalPhoneAttribute : ValidationAttribute
{
    private static readonly Regex Pattern = new(@"^\+?[0-9][0-9\-\s]{7,18}$", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        var phone = text.Trim();
        if (phone.Length <= 20 && Pattern.IsMatch(phone))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }
}
