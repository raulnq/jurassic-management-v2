param appServiceName string
param appSettings object
param currentAppSettings object

resource appService 'Microsoft.Web/sites@2022-09-01' existing = {
  name: appServiceName
}

resource siteconfig 'Microsoft.Web/sites/config@2022-09-01' = {
  parent: appService
  name: 'appsettings'
  //https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/bicep-functions-object#union
  properties: union(currentAppSettings, appSettings)
}
