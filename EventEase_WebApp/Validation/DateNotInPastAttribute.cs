using System.ComponentModel.DataAnnotations;

namespace EventEase_WebApp.Validation;

public class DateNotInPastAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime date)
            return ValidationResult.Success; // Let [Required] handle missing values

        if (date.Date < DateTime.Today)
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be in the past.");
        }

        return ValidationResult.Success;
    }
}