using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace minimal_api.Migrations
{
    /// <inheritdoc />
    public partial class AlterandoVeiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Veciulos",
                table: "Veciulos");

            migrationBuilder.RenameTable(
                name: "Veciulos",
                newName: "Veiculos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Veiculos",
                table: "Veiculos",
                column: "VeiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Veiculos",
                table: "Veiculos");

            migrationBuilder.RenameTable(
                name: "Veiculos",
                newName: "Veciulos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Veciulos",
                table: "Veciulos",
                column: "VeiculoId");
        }
    }
}
