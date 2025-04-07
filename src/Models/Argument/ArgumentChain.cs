using AzureMCP.Arguments;
using AzureMCP.Models.Command;

namespace AzureMCP.Models.Argument;

/// <summary>
/// Typed argument definition for a specific argument class
/// </summary>
/// <typeparam name="TArgs">The type of the arguments class</typeparam>
public class ArgumentChain<TArgs> : ArgumentDefinition<string> where TArgs : BaseArguments
{
    /// <summary>
    /// Function to access the current value of this argument from the arguments object
    /// </summary>
    public Func<TArgs, string> ValueAccessor { get; set; } = _ => string.Empty;

    /// <summary>
    /// Function to load suggested values for this argument
    /// </summary>
    public Func<CommandContext, TArgs, Task<List<ArgumentOption>>> ValueLoader { get; set; } = (_, __) => Task.FromResult(new List<ArgumentOption>());

    /// <summary>
    /// Creates a new instance of ArgumentChain with the specified name and description
    /// </summary>
    public static ArgumentChain<TArgs> Create(string name, string description)
    {
        return new ArgumentChain<TArgs>(name, description);
    }

    private ArgumentChain(string name, string description) : base(name, description)
    {
    }

    /// <summary>
    /// Sets the command example for this argument
    /// </summary>
    public ArgumentChain<TArgs> WithCommandExample(string command)
    {
        Command = command;
        return this;
    }

    /// <summary>
    /// Generates a command example for this argument using the specified command path
    /// </summary>
    public ArgumentChain<TArgs> WithCommandExample(string commandPath, string? placeholderValue = null)
    {
        string placeholder = placeholderValue ?? $"<{Name}>";
        Command = $"azmcp {commandPath} --{Name} {placeholder}";
        return this;
    }

    /// <summary>
    /// Sets whether this argument is required
    /// </summary>
    public ArgumentChain<TArgs> WithIsRequired(bool required)
    {
        Required = required;
        return this;
    }

    /// <summary>
    /// Sets the value accessor for this argument
    /// </summary>
    public ArgumentChain<TArgs> WithValueAccessor(Func<TArgs, string> valueAccessor)
    {
        ValueAccessor = valueAccessor;
        return this;
    }

    /// <summary>
    /// Sets the value loader for this argument
    /// </summary>
    public ArgumentChain<TArgs> WithValueLoader(Func<CommandContext, TArgs, Task<List<ArgumentOption>>> valueLoader)
    {
        ValueLoader = valueLoader;
        return this;
    }

    /// <summary>
    /// Sets the default value for this argument
    /// </summary>
    public ArgumentChain<TArgs> WithDefaultValue(string defaultValue)
    {
        DefaultValue = defaultValue;
        return this;
    }
}