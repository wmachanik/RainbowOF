using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class DeletUniqueKLeyAttTerm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_WooProductAttributeTermMappings_ItemAttributeVarietyLookupId_WooProductAttributeTermId",
                table: "WooProductAttributeTermMappings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_WooProductAttributeTermMappings_ItemAttributeVarietyLookupId_WooProductAttributeTermId",
                table: "WooProductAttributeTermMappings",
                columns: new[] { "ItemAttributeVarietyLookupId", "WooProductAttributeTermId" });
        }
    }
}
