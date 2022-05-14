1. VkActivity.Data -> Open in Terminal

2. PS> dotnet ef migrations add InitialVkActivityContext --context VkActivityContext --output-dir "Migrations"

3. Append to generated MigrationBuilder.Up(...) method:
            migrationBuilder.Sql(VkActivityContext.GetOtherSqlScripts(@"..\VkActivity.Worker\appsettings.Development.json"));

4. VkActivity.Worker -> Open in Terminal

5. PS> dotnet ef database update --context VkActivityContext --project ..\VkActivity.Data\VkActivity.Data.csproj 


// ADD Microsoft.EntityFrameworkCore.Design
// THEN dotnet tool update --global dotnet-ef --version 6.0.3
//   OR dotnet tool install --global dotnet-ef
