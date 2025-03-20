using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class IdentityUserId_toUtilizador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_user_id",
                table: "utilizador",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identity_user_id",
                table: "utilizador");
        }
    }
}
