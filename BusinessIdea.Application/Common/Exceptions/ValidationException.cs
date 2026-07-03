using FluentValidation.Results;

namespace BusinessIdea.Application.Common.Exceptions;

/// <summary>
/// Aggregates FluentValidation failures raised by the validation pipeline
/// behaviour into a single exception. Mapped to HTTP 400 with a field->errors
/// dictionary.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
