using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngularAuthAPI.Migrations
{
    public partial class updatedTemplatesWithFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fields",
                table: "templates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fields",
                table: "templates");
        }
    }
}
