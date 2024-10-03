# jurassic-management

## Running locally

Run the following command to prepare the development enviroment:

```
dotnet tool install Nuke.GlobalTool --global
nuke StartEnv
nuke RunMigrator
```

- **Azurite**: 
  - QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1
  - TableEndpoint=http://127.0.0.1:10002/devstoreaccount1
  - BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1
- **Seq**: http://localhost:5339/
- **RabbitMQ**: http://localhost:15671/
  - User: admin
  - Password: Rabbitmq123$
- **SQLServer**: localhost,1431
  - User: sa
  - Password: Sqlserver123$

Open the solution and run the `WebApi` project.

Note: You need to have [docker](https://docs.docker.com/desktop/install/windows-install/) up and running.

### IaC

```
az deployment group create --resource-group jurassic-qa --template-file .\iac\database.bicep --parameters administratorLoginPassword='<MY_PASSWORD>'

az deployment group create --resource-group jurassic-qa --template-file .\iac\application.bicep --parameters administratorLoginPassword='<MY_PASSWORD>'
```

### Deployment

```
nuke Deploy --web-app-password <MY_FTPS_PASSWORD> --web-app-name jurassic-qa --web-app-user '$jurassic-qa'
```