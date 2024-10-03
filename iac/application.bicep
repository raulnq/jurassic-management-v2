param location string = resourceGroup().location

@description('Environment')
param environment string = 'qa' //prod

param prefix string = 'jurassic'

@description('App service plan SKU')
param appServicePlanSku string = 'B1'  //B3

@description('Storage account SKU')
param storageAccountSku string = 'Standard_LRS' //Standard_GRS

@description('ASPNETCORE_ENVIRONMENT')
param aspNetCoreEnvironment string = 'Testing'

@description('SQL database name')
param databaseName string = 'jurassic'

@description('The administrator username of the SQL logical server.')
param administratorLogin string = 'jurassic'

@description('The administrator password of the SQL logical server.')
@secure()
param administratorLoginPassword string

param azureEndpointSuffix string = 'core.windows.net'

var appServicePlanName = '${prefix}-app-${environment}'

var webSiteName = '${prefix}-${environment}'

var serviceBusNamespaceName = '${prefix}servicebus${environment}'

var storageAccountName ='${prefix}storage${environment}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: appServicePlanSku
    capacity: 1
  }
  kind: 'linux'
}

resource appService 'Microsoft.Web/sites@2020-06-01' = {
  name: webSiteName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
    }
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {}
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountSku
  }
  kind: 'StorageV2'
   properties: {
     accessTier: 'Hot'
     allowBlobPublicAccess: true
   }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

var serviceBusSharedAccessKey= listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryKey
var serviceBusConnectionString = 'Endpoint=sb://${serviceBusNamespaceName}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${serviceBusSharedAccessKey}'
var blobStorageAccountKey = listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value
var blobStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${azureEndpointSuffix};AccountKey=${blobStorageAccountKey}'
var dbConnectionString = 'Server=tcp:${prefix}-sqlserver-${environment}.database.windows.net,1433;Initial Catalog=${databaseName};Persist Security Info=False;User ID=${administratorLogin};Password=${administratorLoginPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
module appSettings 'settings.bicep' = {
  name: '${webSiteName}-appsettings'
  params: {
    appServiceName: appService.name
    currentAppSettings: list(resourceId('Microsoft.Web/sites/config', appService.name, 'appsettings'), appService.apiVersion).properties
    appSettings: {
      ASPNETCORE_ENVIRONMENT: aspNetCoreEnvironment
      AzureStorageConnectionString: blobStorageConnectionString
      DbConnectionString: dbConnectionString
      AzureServiceBus__ConnectionString: serviceBusConnectionString
      WEBSITE_TIME_ZONE: 'America/Lima'
    }
  }
}
