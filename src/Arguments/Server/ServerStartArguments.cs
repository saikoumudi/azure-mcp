using ModelContextProtocol.Protocol.Transport;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Server;

public class ServerStartArguments
{
    [JsonPropertyName("transport")]
    public string Transport { get; set; } = TransportTypes.StdIo;
}
