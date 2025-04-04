using AzureMCP.Models;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands;

public abstract class BaseCommandWithoutArgs : ICommand
{
    public abstract Command GetCommand();

    public abstract Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions);

    protected void HandleException(CommandResponse response, Exception ex)
    {
        response.Arguments = null;
        response.Status = GetStatusCode(ex);
        response.Message = GetErrorMessage(ex);
        response.Results = null;
    }

    protected virtual string GetErrorMessage(Exception ex) => ex.Message;

    protected virtual int GetStatusCode(Exception ex) => 500;

    public IEnumerable<ArgumentDefinition<string>>? GetArgumentChain() => null;

    public void ClearArgumentChain() { } // No-op since we don't have args

    public void AddArgumentToChain(ArgumentDefinition<string> argument) { } // No-op since we don't have args
}