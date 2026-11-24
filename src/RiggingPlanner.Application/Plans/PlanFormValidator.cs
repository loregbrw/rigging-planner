using System.ComponentModel.DataAnnotations;
using RiggingPlanner.Application.Catalog;

namespace RiggingPlanner.Application.Plans;

public static class PlanFormValidator
{
    public static IReadOnlyList<string> Validate(PlanFormModel form)
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true))
        {
            return [];
        }

        return [.. results
            .Select(x => x.ErrorMessage)
            .Where(x => !string.IsNullOrWhiteSpace(x))!];
    }
}
