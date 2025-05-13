using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class AddFundoCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FundoCompras",
                columns: table => new
                {
                    FundoCompraId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FundoinvestimentoId = table.Column<int>(type: "integer", nullable: false),
                    ValorPorUnidade = table.Column<decimal>(type: "numeric", nullable: false),
                    DataCompra = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundoCompras", x => x.FundoCompraId);
                    table.ForeignKey(
                        name: "FK_FundoCompras_fundoinvestimento_FundoinvestimentoId",
                        column: x => x.FundoinvestimentoId,
                        principalTable: "fundoinvestimento",
                        principalColumn: "fundoinvestimento_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundoCompras_FundoinvestimentoId",
                table: "FundoCompras",
                column: "FundoinvestimentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundoCompras");
        }
    }
}
