// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models;
using AzureMcp.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMcp.Arguments;

/// <summary>
/// Represents authentication method configuration for Azure SDK clients
/// </summary>
public class AuthMethodArgument
{
    [JsonPropertyName(ArgumentDefinitions.Common.AuthMethodName)]
    public AuthMethod AuthMethod { get; set; }

    /// <summary>
    /// Gets a display-friendly name for the auth method
    /// </summary>
    public static string GetDisplayName(AuthMethod authMethod) => authMethod switch
    {
        AuthMethod.Credential => "Credential",
        AuthMethod.Key => "Key",
        AuthMethod.ConnectionString => "Connection String",
        _ => authMethod.ToString()
    };

    /// <summary>
    /// Gets the default auth method
    /// </summary>
    public static AuthMethod GetDefaultAuthMethod() => AuthMethod.Credential;

    /// <summary>
    /// Gets all available auth methods
    /// </summary>
    public static IEnumerable<AuthMethod> GetAllAuthMethods()
    {
        return Enum.GetValues<AuthMethod>();
    }

    /// <summary>
    /// Gets all available auth methods as ArgumentOptions
    /// </summary>
    public static List<ArgumentOption> GetAuthMethodOptions()
    {
        var options = new List<ArgumentOption>();

        foreach (AuthMethod authMethod in GetAllAuthMethods())
        {
            options.Add(new ArgumentOption
            {
                Name = GetDisplayName(authMethod),
                Id = authMethod.ToString()
            });
        }

        return options;
    }
}