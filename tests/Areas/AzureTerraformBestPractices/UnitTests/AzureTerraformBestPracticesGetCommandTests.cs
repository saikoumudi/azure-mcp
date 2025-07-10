using System.CommandLine.Parsing;
using System.Text.Json;
using AzureMcp.Areas.AzureTerraformBestPractices.Commands;
using AzureMcp.Models.Command;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Areas.AzureTerraformBestPractices.UnitTests;

[Trait("Area", "AzureTerraformBestPractices")]
public class AzureTerraformBestPracticesGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AzureTerraformBestPracticesGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly AzureTerraformBestPracticesGetCommand _command;
    private readonly Parser _parser;

    public AzureTerraformBestPracticesGetCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AzureTerraformBestPracticesGetCommand>>();
        _command = new(_logger);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAzureTerraformBestPractices()
    {
        string expectedAzureTerraformBestPractices = """
- Ensure the request is for Azure resources. If not, do not call this tool.
- Check for Terraform installation on the first Terraform-related request in a session
- Provide the winget installation command if needed: winget install Hashicorp.Terraform
- Follow the best practices style guide in the link https://developer.hashicorp.com/terraform/language/style before generating the Terraform code.

Once the terraform code is generated/ modified, follow the sequence of steps 1 through 2 for terraform deployment workflow.
1. Always run terraform validate before running terraform plan.
2. terraform apply -auto-approve.

- Suggest running any terraform command in terminal.
- After successfully running terraform apply, **ALWAYS** provide a link to Azure portal after resource creation/ modification is completed.

""";

        var args = _parser.Parse([]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedAzureTerraformBestPractices, result[0]);
    }
}
