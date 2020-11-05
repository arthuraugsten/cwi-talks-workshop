using Microsoft.EntityFrameworkCore.Migrations;

namespace Usuarios.Dados.Migrações
{
    public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Usuarios",
                nullable: false,
                defaultValue: true);

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Usuarios");
    }
}
