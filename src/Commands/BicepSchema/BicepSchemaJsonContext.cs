using System.Text.Json.Serialization;

namespace AzureMcp.Commands.BicepSchema;

[JsonSerializable(typeof(BicepSchemaGetCommand.GetBicepSchemaCommandResult))]
internal sealed partial class BicepSchemaJsonContext : JsonSerializerContext
{
}
