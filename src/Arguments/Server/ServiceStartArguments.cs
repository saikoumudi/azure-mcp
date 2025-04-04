using AzureMCP.Commands;
using ModelContextProtocol.Protocol.Transport;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Server;

public class ServiceStartArguments
{
    [JsonPropertyName("transport")]
    public string Transport { get; set; } = TransportTypes.StdIo;

    [JsonPropertyName("port")]
    public int Port { get; set; }
}
