namespace Clovance.ApiService.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(Dictionary<string, string[]> errors)
        : base(
            "One or more validation errors occurred",
            StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR",
            new Dictionary<string, object?>
            {
                ["errors"] = errors
            })
    {
    }

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string[]>
        {
            [field] = new[] { message }
        })
    {
    }
}
