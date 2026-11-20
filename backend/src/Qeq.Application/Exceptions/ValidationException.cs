using FluentValidation.Results;

namespace Qeq.Application.Exceptions;

public class ValidationException : Exception
{
    public ValidationException() : base("Một hoặc nhiều lỗi xác thực đã xảy ra.")
    {
        Errors = new List<string>();
    }

    public List<string> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        foreach (var failure in failures)
        {
            Errors.Add(failure.ErrorMessage);
        }
    }
}