using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantidadeToFundos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "quantidade",
                table: "fundoinvestimento",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 1m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quantidade",
                table: "fundoinvestimento");
        }
    }
}
