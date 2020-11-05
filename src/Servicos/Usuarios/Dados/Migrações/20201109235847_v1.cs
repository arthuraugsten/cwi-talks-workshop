using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Usuarios.Dados.Migrações
{
    public partial class v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
                 name: "Usuarios",
                 columns: table => new
                 {
                     Id = table.Column<Guid>(nullable: false),
                     Nome = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                     Senha = table.Column<string>(unicode: false, maxLength: 64, nullable: false)
                 },
                 constraints: table =>
                 {
                     table.PrimaryKey("PK_Usuarios", x => x.Id);
                 });

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
                 name: "Usuarios");
    }
}
