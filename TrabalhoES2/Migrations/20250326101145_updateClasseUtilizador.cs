using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrabalhoES2.Migrations
{
    /// <inheritdoc />
    public partial class updateClasseUtilizador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_utilizador_identity_user_id",
                table: "utilizador",
                column: "identity_user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Utilizador_AspNetUsers",
                table: "utilizador",
                column: "identity_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilizador_AspNetUsers",
                table: "utilizador");

            migrationBuilder.DropIndex(
                name: "IX_utilizador_identity_user_id",
                table: "utilizador");
        }
    }
}
