using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class FixUniqueFieldItemAttLkupNotSort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_SortOrder",
                table: "ItemAttributeVarietiesLookups");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_SortOrder",
                table: "ItemAttributeVarietiesLookups",
                columns: new[] { "VarietyName", "SortOrder" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_SortOrder",
                table: "ItemAttributeVarietiesLookups");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_SortOrder",
                table: "ItemAttributeVarietiesLookups",
                columns: new[] { "VarietyName", "SortOrder" },
                unique: true);
        }
    }
}
