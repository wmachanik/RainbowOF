using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class LinkAssociatedTablesTioItemVariant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAssociatedLookups_AssociatedAttributeLookupId",
                table: "ItemVariantAssociatedLookups",
                column: "AssociatedAttributeLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAssociatedLookups_AssociatedAttributeVarietyLookupId",
                table: "ItemVariantAssociatedLookups",
                column: "AssociatedAttributeVarietyLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAssociatedLookups_ItemAttributesLookups_AssociatedAttributeLookupId",
                table: "ItemVariantAssociatedLookups",
                column: "AssociatedAttributeLookupId",
                principalTable: "ItemAttributesLookups",
                principalColumn: "ItemAttributeLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAssociatedLookups_ItemAttributeVarietiesLookups_AssociatedAttributeVarietyLookupId",
                table: "ItemVariantAssociatedLookups",
                column: "AssociatedAttributeVarietyLookupId",
                principalTable: "ItemAttributeVarietiesLookups",
                principalColumn: "ItemAttributeVarietyLookupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAssociatedLookups_ItemAttributesLookups_AssociatedAttributeLookupId",
                table: "ItemVariantAssociatedLookups");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAssociatedLookups_ItemAttributeVarietiesLookups_AssociatedAttributeVarietyLookupId",
                table: "ItemVariantAssociatedLookups");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAssociatedLookups_AssociatedAttributeLookupId",
                table: "ItemVariantAssociatedLookups");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAssociatedLookups_AssociatedAttributeVarietyLookupId",
                table: "ItemVariantAssociatedLookups");
        }
    }
}
