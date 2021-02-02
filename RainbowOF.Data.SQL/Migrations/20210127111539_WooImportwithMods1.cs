using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class WooImportwithMods1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemId_ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropColumn(
                name: "ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.RenameColumn(
                name: "ItemCategoryLookup",
                table: "ItemCategories",
                newName: "ItemCategoryLookupId");

            migrationBuilder.AddColumn<Guid>(
                name: "ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                columns: new[] { "ItemId", "ItemAttributeVarietyLookupId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropColumn(
                name: "ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.RenameColumn(
                name: "ItemCategoryLookupId",
                table: "ItemCategories",
                newName: "ItemCategoryLookup");

            migrationBuilder.AddColumn<int>(
                name: "ItemAttributeId",
                table: "ItemAttributeVarieties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemId_ItemAttributeId",
                table: "ItemAttributeVarieties",
                columns: new[] { "ItemId", "ItemAttributeId" },
                unique: true);
        }
    }
}
