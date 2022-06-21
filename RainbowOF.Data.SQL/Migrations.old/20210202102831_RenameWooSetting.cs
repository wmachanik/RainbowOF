using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class RenameWooSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreItemQuantatiesImported",
                table: "WooSettings",
                newName: "AreAffliateProdcutsImported");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreAffliateProdcutsImported",
                table: "WooSettings",
                newName: "AreItemQuantatiesImported");
        }
    }
}
