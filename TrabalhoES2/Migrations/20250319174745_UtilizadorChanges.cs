using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class UtilizadorChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TpUtilizador",
                table: "utilizador",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TpUtilizador",
                table: "utilizador");
        }
    }
}
