using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngularAuthAPI.Migrations
{
    public partial class updatedDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "Marca");

            migrationBuilder.AddColumn<string>(
                name: "Facultate",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nume",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Specializare",
                table: "users",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Facultate",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Nume",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Specializare",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "Marca",
                table: "users",
                newName: "Id");
        }
    }
}
