// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMCP.Models.Identity;

public class ManagedIdentityInfo
{
    public SystemAssignedIdentityInfo? SystemAssignedIdentity { get; set; }
    public UserAssignedIdentityInfo[]? UserAssignedIdentities { get; set; }
}