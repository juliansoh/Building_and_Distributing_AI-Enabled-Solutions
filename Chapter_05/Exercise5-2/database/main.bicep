targetScope = 'subscription'

@description('Name of the resource group to create')
param resourceGroupName string

@description('Location for the resource group and SQL resources')
param location string = 'westus2'

@description('Name of the SQL Server')
param sqlServerName string

@description('Object ID of the Entra ID admin (user or group)')
param aadAdminObjectId string

@description('Display name of the Entra ID admin')
param aadAdminDisplayName string

@description('Tenant ID for Entra ID')
param tenantId string = subscription().tenantId

@description('Client IP address for firewall rule')
param clientIp string

// Create the resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// SQL Server with Entra-only authentication and database
module sqlServerModule 'sql.bicep' = {
  name: 'sqlServerDeployment'
  scope: rg
  params: {
    sqlServerName: sqlServerName
    location: location
    aadAdminDisplayName: aadAdminDisplayName
    aadAdminObjectId: aadAdminObjectId
    tenantId: tenantId
    clientIp: clientIp
  }
}

