namespace AzureMCP.Models;

public class ArgumentInfo(string name, string description, string? value = "", string? command = "", string? defaultValue = default, List<ArgumentOption>? values = null, bool required = false) : ArgumentDefinition<string>(name, description, value, command, defaultValue, values, required)
{
}
