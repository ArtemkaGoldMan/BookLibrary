using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Validators;

public class DateLessThanCurrentAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime < DateTime.Now;
        }
        return false;
    }
}
