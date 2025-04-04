namespace AzureMCP.Models;

public class ManagedIdentityInfo
{
    public SystemAssignedIdentityInfo? SystemAssignedIdentity { get; set; }
    public UserAssignedIdentityInfo[]? UserAssignedIdentities { get; set; }
}