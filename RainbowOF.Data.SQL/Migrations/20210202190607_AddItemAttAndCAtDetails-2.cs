using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AddItemAttAndCAtDetails2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookup_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "ItemCategories");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookup_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId",
                principalTable: "ItemAttributeVarietiesLookup",
                principalColumn: "ItemAttributeVarietyLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "ItemCategories",
                column: "ItemCategoryLookupId",
                principalTable: "ItemCategoriesLookup",
                principalColumn: "ItemCategoryLookupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookup_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "ItemCategories");

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
    }
}
