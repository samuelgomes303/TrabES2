using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:tipoativofinanceiro", "DepositoPrazo,ImovelArrendado,FundoInvestimento")
                .Annotation("Npgsql:Enum:tipoutilizador", "Cliente,Admin,UserManager");

            migrationBuilder.CreateTable(
                name: "banco",
                columns: table => new
                {
                    banco_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("banco_pkey", x => x.banco_id);
                });

            migrationBuilder.CreateTable(
                name: "utilizador",
                columns: table => new
                {
                    utilizador_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("utilizador_pkey", x => x.utilizador_id);
                });

            migrationBuilder.CreateTable(
                name: "carteira",
                columns: table => new
                {
                    carteira_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    utilizador_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("carteira_pkey", x => x.carteira_id);
                    table.ForeignKey(
                        name: "carteira_utilizador_id_fkey",
                        column: x => x.utilizador_id,
                        principalTable: "utilizador",
                        principalColumn: "utilizador_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ativofinanceiro",
                columns: table => new
                {
                    ativofinanceiro_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    percimposto = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    duracaomeses = table.Column<int>(type: "integer", nullable: true),
                    datainicio = table.Column<DateOnly>(type: "date", nullable: true),
                    carteira_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ativofinanceiro_pkey", x => x.ativofinanceiro_id);
                    table.ForeignKey(
                        name: "ativofinanceiro_carteira_id_fkey",
                        column: x => x.carteira_id,
                        principalTable: "carteira",
                        principalColumn: "carteira_id");
                });

            migrationBuilder.CreateTable(
                name: "depositoprazo",
                columns: table => new
                {
                    depositoprazo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valorinicial = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    banco_id = table.Column<int>(type: "integer", nullable: false),
                    nrconta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titular = table.Column<string>(type: "text", nullable: false),
                    taxajuroanual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    valoratual = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ativofinanceiro_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("depositoprazo_pkey", x => x.depositoprazo_id);
                    table.ForeignKey(
                        name: "depositoprazo_ativofinanceiro_id_fkey",
                        column: x => x.ativofinanceiro_id,
                        principalTable: "ativofinanceiro",
                        principalColumn: "ativofinanceiro_id");
                    table.ForeignKey(
                        name: "depositoprazo_banco_id_fkey",
                        column: x => x.banco_id,
                        principalTable: "banco",
                        principalColumn: "banco_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fundoinvestimento",
                columns: table => new
                {
                    fundoinvestimento_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    banco_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    montanteinvestido = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    taxajuropdefeito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ativofinanceiro_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("fundoinvestimento_pkey", x => x.fundoinvestimento_id);
                    table.ForeignKey(
                        name: "fundoinvestimento_ativofinanceiro_id_fkey",
                        column: x => x.ativofinanceiro_id,
                        principalTable: "ativofinanceiro",
                        principalColumn: "ativofinanceiro_id");
                    table.ForeignKey(
                        name: "fundoinvestimento_banco_id_fkey",
                        column: x => x.banco_id,
                        principalTable: "banco",
                        principalColumn: "banco_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imovelarrendado",
                columns: table => new
                {
                    imovelarrendado_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    designacao = table.Column<string>(type: "text", nullable: false),
                    localizacao = table.Column<string>(type: "text", nullable: false),
                    valorimovel = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    valorrenda = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    valormensalcondo = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    valoranualdespesas = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    banco_id = table.Column<int>(type: "integer", nullable: false),
                    ativofinanceiro_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("imovelarrendado_pkey", x => x.imovelarrendado_id);
                    table.ForeignKey(
                        name: "imovelarrendado_ativofinanceiro_id_fkey",
                        column: x => x.ativofinanceiro_id,
                        principalTable: "ativofinanceiro",
                        principalColumn: "ativofinanceiro_id");
                    table.ForeignKey(
                        name: "imovelarrendado_banco_id_fkey",
                        column: x => x.banco_id,
                        principalTable: "banco",
                        principalColumn: "banco_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ativofinanceiro_carteira_id",
                table: "ativofinanceiro",
                column: "carteira_id");

            migrationBuilder.CreateIndex(
                name: "IX_carteira_utilizador_id",
                table: "carteira",
                column: "utilizador_id");

            migrationBuilder.CreateIndex(
                name: "depositoprazo_ativofinanceiro_id_key",
                table: "depositoprazo",
                column: "ativofinanceiro_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_depositoprazo_banco_id",
                table: "depositoprazo",
                column: "banco_id");

            migrationBuilder.CreateIndex(
                name: "fundoinvestimento_ativofinanceiro_id_key",
                table: "fundoinvestimento",
                column: "ativofinanceiro_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fundoinvestimento_banco_id",
                table: "fundoinvestimento",
                column: "banco_id");

            migrationBuilder.CreateIndex(
                name: "imovelarrendado_ativofinanceiro_id_key",
                table: "imovelarrendado",
                column: "ativofinanceiro_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "imovelarrendado_banco_id_key",
                table: "imovelarrendado",
                column: "banco_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "utilizador_email_key",
                table: "utilizador",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "depositoprazo");

            migrationBuilder.DropTable(
                name: "fundoinvestimento");

            migrationBuilder.DropTable(
                name: "imovelarrendado");

            migrationBuilder.DropTable(
                name: "ativofinanceiro");

            migrationBuilder.DropTable(
                name: "banco");

            migrationBuilder.DropTable(
                name: "carteira");

            migrationBuilder.DropTable(
                name: "utilizador");
        }
    }
}
