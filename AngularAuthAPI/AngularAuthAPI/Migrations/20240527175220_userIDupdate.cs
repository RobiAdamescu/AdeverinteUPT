using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngularAuthAPI.Migrations
{
    public partial class userIDupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Marca",
                table: "users",
                newName: "userID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userID",
                table: "users",
                newName: "Marca");
        }
    }
}
