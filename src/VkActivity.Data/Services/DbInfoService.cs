using System.Text.Json;
using Npgsql;
using VkActivity.Data.Models;

namespace VkActivity.Data.Services
{
    public static class DbInfoService
    {
        public static async Task<DbInfo[]> GetInfoAsync(string connectionString)
        {
            const string _tablesInfoQuery = @"
                select (with userQuery as (
                    select
                    	ist.table_name as table,
                    	sut.n_live_tup as rows,
                    	pg_relation_size('""'||ist.table_schema||'"".""'||ist.table_name||'""')/1024 || ' kB' as size
                    from information_schema.tables ist
                    left join pg_stat_user_tables sut on sut.relname = ist.table_name
                    where ist.table_schema = 'vk' and ist.table_type = 'BASE TABLE' 
                    order by 3
                ) select json_agg(q) from userQuery q)";

            using (var dbConnection = new NpgsqlConnection(connectionString))
            {
                dbConnection.Open();
                using (var command = new NpgsqlCommand(_tablesInfoQuery, dbConnection))
                {
                    var result = await command.ExecuteScalarAsync();
                    if (result is string value)
                    {
                        return JsonSerializer.Deserialize<DbInfo[]>(value)!;
                    }

                    return Array.Empty<DbInfo>();
                }
            }
        }
    }
}
