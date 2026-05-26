namespace Clovance.ApiService.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string resource, object key)
        : base(
            $"{resource} with ID '{key}' not found",
            StatusCodes.Status404NotFound,
            "RESOURCE_NOT_FOUND",
            new Dictionary<string, object?>
            {
                ["resource"] = resource,
                ["key"] = key
            })
    {
    }
}
