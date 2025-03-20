using System.CommandLine;

namespace AzureMCP.Commands;

public class CommandGroup
{
    public string Name { get; }
    public string Description { get; }
    public List<CommandGroup> SubGroups { get; } = [];
    public Dictionary<string, ICommand> Commands { get; } = [];
    public Command Command { get; }

    public CommandGroup(string name, string description)
    {
        Name = name;
        Description = description;
        Command = new Command(name, description);
    }

    public void AddCommand(string path, ICommand command)
    {
        // Split on first dot to get group and remaining path
        var parts = path.Split(new[] { '.' }, 2);
        
        if (parts.Length == 1)
        {
            // This is a direct command for this group
            Commands[path] = command;
        }
        else
        {
            // Find or create the subgroup
            var subGroup = SubGroups.FirstOrDefault(g => g.Name == parts[0]);
            if (subGroup == null)
            {
                throw new InvalidOperationException($"Subgroup {parts[0]} not found. Groups must be registered before commands.");
            }
            
            // Recursively add command to subgroup
            subGroup.AddCommand(parts[1], command);
        }
    }

    public void AddSubGroup(CommandGroup subGroup)
    {
        SubGroups.Add(subGroup);
        Command.Add(subGroup.Command);
    }

    public ICommand GetCommand(string path)
    {
        // Split on first dot to get group and remaining path
        var parts = path.Split(new[] { '.' }, 2);
        
        if (parts.Length == 1)
        {
            // This is a direct command for this group
            return Commands[parts[0]];
        }
        else
        {
            // Find the subgroup and recursively get the command
            var subGroup = SubGroups.FirstOrDefault(g => g.Name == parts[0]);
            if (subGroup == null)
            {
                throw new InvalidOperationException($"Subgroup {parts[0]} not found.");
            }
            
            return subGroup.GetCommand(parts[1]);
        }
    }
}
