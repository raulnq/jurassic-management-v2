param location string = resourceGroup().location

@description('Environment')
param environment string = 'qa' //prod

@description('SQL database name')
param databaseName string = 'jurassic'

@description('SQL database SKU')
param databaseSKU string = 'Basic' //Standard

@description('SQL database DTU capacity')
param databaseCapacity int = 5

@description('SQL database capacity')
param databaseSize int = 2147483648 //268435456000

@description('The administrator username of the SQL logical server.')
param administratorLogin string = 'jurassic'

@description('The administrator password of the SQL logical server.')
@secure()
param administratorLoginPassword string

param prefix string = 'jurassic'

var serverName = '${prefix}-sqlserver-${environment}'

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDataBase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: databaseSKU
    tier: databaseSKU
    capacity: databaseCapacity
  }
  properties:{
    maxSizeBytes: databaseSize
  }
}

resource symbolicname 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  name: 'all'
  parent: sqlServer
  properties: {
    endIpAddress: '255.255.255.255'
    startIpAddress: '0.0.0.0'
  }
}
