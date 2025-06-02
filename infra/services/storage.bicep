targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string

@description('The location of the resource. By default, this is the same as the resource group.')
param location string

@description('The client OID to grant access to test resources.')
param testApplicationOid string

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: baseName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: false
  }

  resource blobServices 'blobServices' = {
    name: 'default'
    resource fooContainer 'containers' = { name: 'foo' }
    resource barContainer 'containers' = { name: 'bar' }
    resource bazContainer 'containers' = { name: 'baz' }
  }

  resource tableServices 'tableServices' = {
    name: 'default'
    resource fooTable 'tables' = { name: 'foo' }
    resource barTable 'tables' = { name: 'bar' }
    resource bazTable 'tables' = { name: 'baz' }
  }
}

resource blobContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage Blob Data Contributor role.
  // Read, write, and delete Azure Storage containers and blobs
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource appBlobRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(blobContributorRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: blobContributorRoleDefinition.id
    description: 'Blob Contributor for testApplicationOid'
  }
}
