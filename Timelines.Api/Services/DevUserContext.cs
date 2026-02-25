using Microsoft.Azure.Functions.Worker.Http;

namespace Timelines.Api.Services;

/// <summary>
/// Temporary dev authentication context using X-Dev-UserId header.
/// This will be replaced with proper authentication in a later step.
/// </summary>
public class DevUserContext
{
    private const string DevUserIdHeader = "X-Dev-UserId";
    private static readonly Guid DefaultUserId = new("11111111-1111-1111-1111-111111111111");

    public Guid GetUserId(HttpRequestData req)
    {
        if (req.Headers.TryGetValues(DevUserIdHeader, out var values))
        {
            var headerValue = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerValue) && Guid.TryParse(headerValue, out var userId))
            {
                return userId;
            }
        }

        return DefaultUserId;
    }
}
