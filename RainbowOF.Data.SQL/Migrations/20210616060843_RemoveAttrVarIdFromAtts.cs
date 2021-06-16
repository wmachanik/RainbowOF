using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class RemoveAttrVarIdFromAtts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropColumn(
                name: "ItemAttributeVarietyId",
                table: "ItemAttributes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ItemAttributeVarietyId",
                table: "ItemAttributes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                columns: new[] { "ItemAttributeVarietyId", "ItemAttributeVarietyLookupId" },
                unique: true);
        }
    }
}
