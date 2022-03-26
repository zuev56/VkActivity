using System.Data.Common;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VkActivity.Data.Models;
using Zs.Common.Extensions;

namespace VkActivity.Data;

public partial class VkActivityContext : DbContext
{
    public DbSet<ActivityLogItem>? VkActivityLog { get; set; }
    public DbSet<User>? VkUsers { get; set; }

    public VkActivityContext()
    {
    }

    public VkActivityContext(DbContextOptions<VkActivityContext> options)
       : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        ConfigureEntities(modelBuilder);
    }

    private void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLogItem>(b =>
        {
            b.Property<int>("Id")
            .HasColumnType("int")
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            b.Property<int>("UserId")
            .HasColumnType("int")
            .HasColumnName("user_id")
            .IsRequired();

            b.Property<bool?>("IsOnline")
            .HasColumnType("bool")
            .HasColumnName("is_online");

            b.Property<bool>("IsOnlineMobile")
            .HasColumnType("bool")
            .HasColumnName("is_online_mobile");

            b.Property<int?>("OnlineApp")
            .HasColumnType("int")
            .HasColumnName("online_app");

            b.Property<DateTime>("InsertDate")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("insert_date")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("now()");

            b.Property<int>("LastSeen")
            .HasColumnType("int")
            .HasColumnName("last_seen");

            b.HasKey("Id");

            b.HasIndex("UserId", "LastSeen", "InsertDate");

            b.ToTable("activity_log", "vk");

            b.HasComment("Vk users activity log item");
        });

        modelBuilder.Entity<User>(b =>
        {
            b.Property<int>("Id")
            .HasColumnType("int")
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            b.Property<string>("FirstName")
            .HasColumnType("character varying(50)")
            .HasColumnName("first_name")
            .HasMaxLength(50);

            b.Property<string>("LastName")
            .HasColumnType("character varying(50)")
            .HasColumnName("last_name")
            .HasMaxLength(50);

            b.Property<string>("RawData")
            .HasColumnType("json")
            .HasColumnName("raw_data")
            .HasMaxLength(50);

            b.Property<DateTime>("InsertDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("insert_date")
                .HasDefaultValueSql("now()");

            b.Property<DateTime>("UpdateDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("update_date")
                .HasDefaultValueSql("now()");

            b.HasKey("Id");

            b.ToTable("users", "vk");
        });
    }

    public static string GetOtherSqlScripts(string configPath)
    {
        var configuration = new ConfigurationBuilder()
               .AddJsonFile(System.IO.Path.GetFullPath(configPath))
               .Build();

        var connectionStringBuilder = new DbConnectionStringBuilder()
        {
            ConnectionString = configuration.GetSecretValue("ConnectionStrings:Default")
        };
        var dbName = connectionStringBuilder["Database"] as string;

        var resources = new[]
        {
            "Priveleges.sql",
            "ForeignTales.sql",
            "StoredFunctions.sql",
            "Views.sql"
        };

        var sb = new StringBuilder();
        foreach (var resourceName in resources)
        {
            var sqlScript = Assembly.GetExecutingAssembly().ReadResource(resourceName);
            sb.Append(sqlScript + Environment.NewLine);
        }

        if (!string.IsNullOrWhiteSpace(dbName))
            sb.Replace("DefaultDbName", dbName);

        return sb.ToString();
    }
}
