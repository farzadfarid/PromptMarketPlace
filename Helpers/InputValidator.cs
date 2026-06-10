using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Helpers;

public static class InputValidator
{
    public static string? Validate(List<AppInputField> fields, Dictionary<string, string> inputs)
    {
        foreach (var field in fields)
        {
            inputs.TryGetValue(field.Name, out var value);
            var isEmpty = string.IsNullOrWhiteSpace(value);

            if (field.IsRequired && isEmpty)
                return $"فیلد «{field.Label}» الزامی است.";

            if (isEmpty || field.Type == FieldType.FileUpload) continue;

            if (field.MaxLength.HasValue && value!.Length > field.MaxLength.Value)
                return $"فیلد «{field.Label}» نباید بیشتر از {field.MaxLength.Value} کاراکتر باشد.";

            if (field.MinLength.HasValue && value!.Length < field.MinLength.Value)
                return $"فیلد «{field.Label}» باید حداقل {field.MinLength.Value} کاراکتر باشد.";

            if (field.Type == FieldType.Number && !decimal.TryParse(value, out _))
                return $"فیلد «{field.Label}» باید عدد باشد.";
        }

        return null;
    }

    public static string SubstituteVariables(string prompt, Dictionary<string, string> inputs)
    {
        foreach (var kv in inputs)
            prompt = prompt.Replace($"{{{kv.Key}}}", kv.Value, StringComparison.OrdinalIgnoreCase);

        return prompt;
    }
}
