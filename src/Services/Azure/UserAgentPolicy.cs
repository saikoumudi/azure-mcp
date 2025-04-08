using Azure.Core;
using Azure.Core.Pipeline;

namespace AzureMCP.Services.Azure;

public class UserAgentPolicy : HttpPipelineSynchronousPolicy
{
    public const string UserAgentHeader = "User-Agent";

    private readonly string userAgent;

    public UserAgentPolicy(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            throw new ArgumentException(nameof(userAgent) + " cannot be empty.");
        }

        this.userAgent = userAgent;
    }

    public override void OnSendingRequest(HttpMessage message)
    {
        message.Request.Headers.SetValue(UserAgentHeader, userAgent);

        base.OnSendingRequest(message);
    }
}
