# Environment Setup

## .NET Dev Certs

This project runs over https which can be configured locally
[using the .NET SDK](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs):

```powershell
dotnet dev-certs https --trust
```

### Troubleshooting

#### Long paths on Windows

When you clone the repository from Git and you are using Windows you might run
into a problem that Git cannot create files that have really long paths. This
would most likely happen in
`src/Kentico.Community.Portal.Web/App_Data/CIRepository` because some of the
sub-folder names are very long.

To fix this issue, enable long path support in Windows with
[Git configuration](https://github.com/desktop/desktop/issues/8023#issuecomment-515110259),
or
[with Group Policy or a registry key change](https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=registry#registry-setting-to-enable-long-paths).

#### ASP.NET Core Dev Certs

If you have issues with the Admin client Webpack dev server using the ASP.NET
Core dev cert, you might have a cert on the filesystem being used by the webpack
dev server that is no longer trusted. This can be caused by running
`dotnet dev-certs https --clean` and then `dotnet dev-certs https --trust`. This
will install a _new_ dev cert and the previous cert, which webpack exported, is
no longer valid.

To resolve this you can explicitly run the following commands to re-generate the
certificates.

On Windows:

```powershell
dotnet dev-certs https --format pem --no-password --export-path "$env:APPDATA/ASP.NET/https/kentico-community-portal-web-admin.pem"
```

On MacOS/Linux

```powershell
dotnet dev-certs https --format pem --no-password -ep $HOME/.aspnet/https/kentico-community-portal-web-admin.pem
```

These are the commands that are automatically executed for you if no local certs
exist when starting up the Admin local dev Node.js script (details in
`~\src\Kentico.Community.Portal.Admin\Client\webpack.config.js`).

## Restore Database

- Database backups are located in the `.\database` folder

  - `.bacpac` (database snapshot archive)
  - `.bak` (data + full transaction logs)

  - **Note**: The database backup listed on the first line of the
    `.\database\backups.txt` file is the most recent backup and should be used
    as a starting point for a newly set up project.

- PowerShell

  1. Use the `Restore-Database.ps1` script (`.bacpac` files only)

     ```powershell
     cd .\scripts

     .\Restore-Database.ps1 `
     -ServerName "<server name>" `
     -DatabaseName "<database name>" `
     -BacpacFile "<filename.bacpac>" `
     -WorkspaceFolder $PWD
     ```

- (**alternative**) Azure Data Studio

  1. [Restore a .bacpac file](https://learn.microsoft.com/en-us/sql/azure-data-studio/extensions/sql-server-dacpac-extension)
     (data + schema)
  1. [Restore a .bak file](https://learn.microsoft.com/en-us/sql/azure-data-studio/tutorial-backup-restore-sql-server?view=sql-server-ver16#restore-a-database-from-a-backup-file)

- (**alternative**) Add database in SQL Server Management Studio

  - [Restore a .bacpac file](https://learn.microsoft.com/en-us/sql/relational-databases/data-tier-applications/import-a-bacpac-file-to-create-a-new-user-database)

  - (**alternative**) Restore a .bak file

    1. Launch SQL SSMS

    1. Extract backup file might require from .zip

       - `.\database\<database-backup.zip>`

    1. Restore database

       - Right click on 'Databases'
       - 'Restore Database'
       - Select source 'Device' -> ...-> 'Add'
       - Select `.bak` from `.\database\<database-backup.bak>` -> OK

    1. Add User mapping

       - 'Security' -> 'Logins'
       - Right click on your account
       - 'Properties' -> 'User Mapping' -> tick `Kentico.Community` -> tick 'db
         owner' -> OK

- (**alternative**) Using MacOS/Linux

  1. Use [Docker Desktop](https://docs.docker.com/desktop/install/mac-install/)
     to setup a MS SQL Server docker container
     [to run the Xperience by Kentico database](https://community.kentico.com/blog/developing-with-xperience-by-kentico-on-macos)

     ```powershell
     docker run + `
      --name mssql2022 + `
      --hostname=localhost + `
      --user=mssql + `
      --env=ACCEPT_EULA=Y + `
      --env=MSSQL_SA_PASSWORD=Pass@12345 + `
      -p 1433:1433 + `
      -d mcr.microsoft.com/mssql/server:2022-latest
     ```

  1. Unzip the `.bak.zip` in `./database`

     ```powershell
     Expand-Archive -Path ./database/Kentico.Community-29.0.3-2024-05-14.bak.zip -DestinationPath ./database
     ```

  1. Copy the unzip'd `.bak` file into your docker container (using the correct
     file path)

     ```powershell
     docker cp ./database/Kentico.Community-29.0.3-2024-05-14.bak mssql2022:/var/opt/mssql/data
     ```

  1. Use
     [Azure Data Studio](https://learn.microsoft.com/en-us/azure-data-studio/download-azure-data-studio)
     to restore the `.bak` -> command click "Databases" node under SQL Server ->
     Restore

## Configure Application

1. Open `.\Kentico.Community.Portal.sln` OR open folder directly from VS Code.

   > If using VS Code, install all recommended extensions

1. Set your database Connection String in your local
   [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

1. Disable ReCaptcha validation in your local User Secrets (see settings for
   ReCaptcha in `appsettings.CI.json`) or supply a `localhost` ReCaptcha v3
   key/secret
