using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class FixedSpeelingMistakeOnAffiliate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreAffliateProdcutsImported",
                table: "WooSettings",
                newName: "AreAffiliateProdcutsImported");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreAffiliateProdcutsImported",
                table: "WooSettings",
                newName: "AreAffliateProdcutsImported");
        }
    }
}
