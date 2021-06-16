using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class RemoveItemIdFromAttribVars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "ItemAttributeVarieties");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "ItemAttributeVarieties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                columns: new[] { "ItemId", "ItemAttributeVarietyLookupId" },
                unique: true);
        }
    }
}
