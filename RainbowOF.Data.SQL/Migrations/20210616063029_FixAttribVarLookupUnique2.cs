using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class FixAttribVarLookupUnique2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyId",
                table: "ItemAttributeVarieties");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                columns: new[] { "ItemAttributeId", "ItemAttributeVarietyLookupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyId",
                unique: true);
        }
    }
}
