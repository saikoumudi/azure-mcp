using AzureMcp.Services.Azure.BicepSchema.ResourceProperties.Entities;

namespace AzureMcp.Services.Interfaces
{
    public interface IBicepSchemaService
    {
        TypesDefinitionResult GetResourceTypeDefinitions(
        IServiceProvider serviceProvider,
        string? resourceTypeName,
        string? apiVersion = null);
    }
}
