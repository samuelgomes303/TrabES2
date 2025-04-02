using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class lastUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:tipoativofinanceiro", "DepositoPrazo,ImovelArrendado,FundoInvestimento")
                .Annotation("Npgsql:Enum:tipoutilizador", "Cliente,Admin,UserManager");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    TpUtilizador = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilizador", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_utilizador_UserId",
                        column: x => x.UserId,
                        principalTable: "utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_utilizador_UserId",
                        column: x => x.UserId,
                        principalTable: "utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_utilizador_UserId",
                        column: x => x.UserId,
                        principalTable: "utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_utilizador_UserId",
                        column: x => x.UserId,
                        principalTable: "utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        principalColumn: "Id",
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

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
                name: "EmailIndex",
                table: "utilizador",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_utilizador_Email",
                table: "utilizador",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "utilizador",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "depositoprazo");

            migrationBuilder.DropTable(
                name: "fundoinvestimento");

            migrationBuilder.DropTable(
                name: "imovelarrendado");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

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
