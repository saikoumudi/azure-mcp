using Xunit;

public class LiveOnlyFactAttribute : FactAttribute
{
    public LiveOnlyFactAttribute()
    {
        var mcpPath = Environment.GetEnvironmentVariable("AZURE_MCP_PATH");

        if (string.IsNullOrWhiteSpace(mcpPath))
        {
            Skip = "AZURE_MCP_PATH is not set. Skipping client <-> server live test.";
        }
    }
}
