using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class AddValorAtualToFundo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "imovelarrendado_banco_id_fkey",
                table: "imovelarrendado");

            migrationBuilder.AddColumn<decimal>(
                name: "valoratual",
                table: "fundoinvestimento",
                type: "numeric(15,2)",
                precision: 15,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "imovelarrendado_banco_id_fkey",
                table: "imovelarrendado",
                column: "banco_id",
                principalTable: "banco",
                principalColumn: "banco_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "imovelarrendado_banco_id_fkey",
                table: "imovelarrendado");

            migrationBuilder.DropColumn(
                name: "valoratual",
                table: "fundoinvestimento");

            migrationBuilder.AddForeignKey(
                name: "imovelarrendado_banco_id_fkey",
                table: "imovelarrendado",
                column: "banco_id",
                principalTable: "banco",
                principalColumn: "banco_id");
        }
    }
}
