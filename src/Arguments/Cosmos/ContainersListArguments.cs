using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Cosmos;

public class ContainersListArguments : BaseCosmosArguments
{
    public string? Database { get; set; }
}
