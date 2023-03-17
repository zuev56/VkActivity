using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VkActivity.Data;

public sealed class VkActivityContextFactory : IDbContextFactory<VkActivityContext>, IDesignTimeDbContextFactory<VkActivityContext>
{
    private readonly DbContextOptions<VkActivityContext>? _options;

    public VkActivityContextFactory()
    {
    }

    public VkActivityContextFactory(DbContextOptions<VkActivityContext> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    // For repositories
    public VkActivityContext CreateDbContext() => new (_options!);

    // For migrations
    public VkActivityContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(System.IO.Path.GetFullPath(@"..\VkActivity.Worker\appsettings.Development.json"))
            .Build();
        var connectionString = configuration["ConnectionStrings:Default"];

        var optionsBuilder = new DbContextOptionsBuilder<VkActivityContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new VkActivityContext(optionsBuilder.Options);
    }
}