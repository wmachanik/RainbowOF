using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AddEdUsedPredToItemCate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UsedForPrediction",
                table: "ItemCategoriesLookup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookup_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup",
                column: "ItemAttributeLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarietiesLookup_ItemAttributesLookup_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup",
                column: "ItemAttributeLookupId",
                principalTable: "ItemAttributesLookup",
                principalColumn: "ItemAttributeLookupId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarietiesLookup_ItemAttributesLookup_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarietiesLookup_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup");

            migrationBuilder.DropColumn(
                name: "UsedForPrediction",
                table: "ItemCategoriesLookup");
        }
    }
}
