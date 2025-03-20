using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace AzureMCP.Models;

/// <summary>
/// Provides context for command execution including service access and response management
/// </summary>
public class CommandContext
{
    /// <summary>
    /// The service provider for dependency injection
    /// </summary>
    private readonly IServiceProvider _serviceProvider;
    
    /// <summary>
    /// JSON serialization options for command output
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; }
    
    /// <summary>
    /// The response object that will be returned to the client
    /// </summary>
    public CommandResponse Response { get; }

    /// <summary>
    /// Creates a new command context
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    public CommandContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        JsonOptions = new JsonSerializerOptions { WriteIndented = true };
        Response = new CommandResponse
        {
            Status = 200,
            Message = "Success",
            Arguments = []
        };
    }

    /// <summary>
    /// Gets a required service from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve</typeparam>
    /// <returns>The requested service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not registered</exception>
    public T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}