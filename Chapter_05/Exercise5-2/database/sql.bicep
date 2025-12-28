@description('Name of the SQL Server')
param sqlServerName string

@description('Object ID of the Entra ID admin (user or group)')
param aadAdminObjectId string

@description('Display name of the Entra ID admin')
param aadAdminDisplayName string

@description('Tenant ID for Entra ID')
param tenantId string = subscription().tenantId

@description('Location for all resources')
param location string = resourceGroup().location

@description('Client IP address to allow through firewall')
param clientIp string

// SQL Server with Entra-only authentication
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      login: aadAdminDisplayName
      sid: aadAdminObjectId
      tenantId: tenantId
      azureADOnlyAuthentication: true
    }
  }
}

// Allow Azure services to access the server
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  name: 'AllowAzureServices'
  parent: sqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Allow local client IP
resource clientFirewall 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  name: 'ClientIPAddress'
  parent: sqlServer
  properties: {
    startIpAddress: clientIp
    endIpAddress: clientIp
  }
}

// SQL Database (D1 tier with sample data)
resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  name: 'sampledb'
  parent: sqlServer
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5 // D1 = 5 DTUs
  }
  properties: {
    sampleName: 'AdventureWorksLT'
  }
}
