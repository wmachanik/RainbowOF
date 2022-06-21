using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AddItemAttAndCAtDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemCategoryLookupId",
                table: "ItemCategories",
                column: "ItemCategoryLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookup_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId",
                principalTable: "ItemAttributeVarietiesLookup",
                principalColumn: "ItemAttributeVarietyLookupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "ItemCategories",
                column: "ItemCategoryLookupId",
                principalTable: "ItemCategoriesLookup",
                principalColumn: "ItemCategoryLookupId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookup_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "ItemCategories");

            migrationBuilder.DropIndex(
                name: "IX_ItemCategories_ItemCategoryLookupId",
                table: "ItemCategories");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");
        }
    }
}
