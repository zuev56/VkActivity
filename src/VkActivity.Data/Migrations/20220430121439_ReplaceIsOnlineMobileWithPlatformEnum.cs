using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VkActivity.Data.Migrations
{
    public partial class ReplaceIsOnlineMobileWithPlatformEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW vk.v_activity_log;");
            migrationBuilder.Sql("DROP VIEW vk.v_compare_with_prod");

            migrationBuilder.DropColumn(
                name: "is_online_mobile",
                schema: "vk",
                table: "activity_log");

            migrationBuilder.DropColumn(
                name: "online_app",
                schema: "vk",
                table: "activity_log");

            migrationBuilder.AddColumn<int>(
                name: "platform",
                schema: "vk",
                table: "activity_log",
                type: "int",
                nullable: false,
                defaultValue: 0);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "platform",
                schema: "vk",
                table: "activity_log");

            migrationBuilder.AddColumn<bool>(
                name: "is_online_mobile",
                schema: "vk",
                table: "activity_log",
                type: "bool",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "online_app",
                schema: "vk",
                table: "activity_log",
                type: "int",
                nullable: true);
        }
    }
}
