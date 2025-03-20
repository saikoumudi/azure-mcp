using AzureMCP.Models;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands;

/// <summary>
/// Interface for all commands including argument chain functionality
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the command definition
    /// </summary>
    Command GetCommand();

    /// <summary>
    /// Executes the command
    /// </summary>
    Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions);

    /// <summary>
    /// Gets the current argument chain
    /// </summary>
    IEnumerable<ArgumentDefinition<string>>? GetArgumentChain();

    /// <summary>
    /// Clears the current argument chain
    /// </summary>
    void ClearArgumentChain();

    /// <summary>
    /// Adds an argument to the chain
    /// </summary>
    void AddArgumentToChain(ArgumentDefinition<string> argument);
}