using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Server;
using ModelContextProtocol.Utils.Json;

namespace AzureMCP.Commands.Server;

/// <summary>
/// From: https://github.com/modelcontextprotocol/csharp-sdk/blob/main/samples/AspNetCoreSseServer/McpEndpointRouteBuilderExtensions.cs
/// </summary>
public static class McpEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapMcpSse(this IEndpointRouteBuilder endpoints)
    {
        SseResponseStreamTransport? transport = null;
        var routeGroup = endpoints.MapGroup("");

        routeGroup.MapGet("/sse", async (HttpResponse response, CancellationToken requestAborted) =>
        {
            var mcpServer = endpoints.ServiceProvider.GetRequiredService<IMcpServer>();
            var wrapper = mcpServer as AzureMcpServer
                ?? throw new InvalidOperationException($"Expected mcpServer to be of type {typeof(AzureMcpServer)}."
                    + $"Instead it is {mcpServer.GetType()}");
            await using var localTransport = transport = new SseResponseStreamTransport(response.Body);

            await wrapper.SetTransportAndRunAsync(localTransport);

            response.Headers.ContentType = "text/event-stream";
            response.Headers.CacheControl = "no-cache";

            try
            {
                await transport.RunAsync(requestAborted);
            }
            catch (OperationCanceledException) when (requestAborted.IsCancellationRequested)
            {
                // RequestAborted always triggers when the client disconnects before a complete response body is written,
                // but this is how SSE connections are typically closed.
            }
        });

        routeGroup.MapPost("/message", async (HttpContext context) =>
        {
            if (transport is null)
            {
                await Results.BadRequest("Connect to the /sse endpoint before sending messages.").ExecuteAsync(context);
                return;
            }

            var message = await context.Request.ReadFromJsonAsync<IJsonRpcMessage>(McpJsonUtilities.DefaultOptions, context.RequestAborted);

            if (message is null)
            {
                await Results.BadRequest("No message in request body.").ExecuteAsync(context);
                return;
            }

            await transport.OnMessageReceivedAsync(message, context.RequestAborted);
            context.Response.StatusCode = StatusCodes.Status202Accepted;
            await context.Response.WriteAsync("Accepted");
        });

        return routeGroup;
    }
}
