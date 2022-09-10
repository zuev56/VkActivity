using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VkActivity.Data.Migrations
{
    public partial class AddStateFieldToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "vk",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                schema: "vk",
                table: "users");
        }
    }
}
