using System.Text.Json.Serialization;
using ModelContextProtocol.Protocol.Transport;

namespace AzureMCP.Arguments.Server;

public class ServerStartArguments
{
    [JsonPropertyName("transport")]
    public string Transport { get; set; } = TransportTypes.StdIo;
}
