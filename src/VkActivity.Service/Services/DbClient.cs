using Npgsql;
using Zs.Common.Abstractions;

namespace VkActivity.Service.Services;

public sealed class DbClient : DbClientBase<NpgsqlConnection, NpgsqlCommand>
{
    public DbClient(string connectionString, ILogger<DbClient> logger = null)
        : base(connectionString, logger)
    {
    }
}
