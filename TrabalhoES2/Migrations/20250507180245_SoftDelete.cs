using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "blocked_at",
                table: "utilizador",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "utilizador",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                table: "utilizador",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "utilizador",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "unblocked_at",
                table: "utilizador",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "blocked_at",
                table: "utilizador");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "utilizador");

            migrationBuilder.DropColumn(
                name: "is_blocked",
                table: "utilizador");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "utilizador");

            migrationBuilder.DropColumn(
                name: "unblocked_at",
                table: "utilizador");
        }
    }
}
