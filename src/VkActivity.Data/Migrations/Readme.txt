1. PS> dotnet ef migrations add InitialHomeContext --context HomeContext --output-dir "Migrations"

2.1 Add to MigrationBuilder.Up(...) method:
            migrationBuilder.Sql(PostgreSqlBotContext.GetOtherSqlScripts(@"..\Home.Bot\appsettings.json"));
            migrationBuilder.Sql(Data.HomeContext.GetOtherSqlScripts(@"..\Home.Bot\appsettings.json"));

3. PS> dotnet ef database update --context HomeContext


// ADD Microsoft.EntityFrameworkCore.Design
// THEN dotnet tool update --global dotnet-ef --version 6.0.3
//   OR dotnet tool install --global dotnet-ef