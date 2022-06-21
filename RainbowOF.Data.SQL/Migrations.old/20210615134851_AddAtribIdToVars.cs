using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AddAtribIdToVars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeVarietyId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributes_ItemAttributeId_ItemId",
                table: "ItemAttributes");

            migrationBuilder.AddColumn<Guid>(
                name: "ItemAttributeId",
                table: "ItemAttributeVarieties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                columns: new[] { "ItemAttributeVarietyId", "ItemAttributeVarietyLookupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_ItemAttributeId",
                table: "ItemAttributes",
                column: "ItemAttributeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeId",
                principalTable: "ItemAttributes",
                principalColumn: "ItemAttributeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyId_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributes_ItemAttributeId",
                table: "ItemAttributes");

            migrationBuilder.DropColumn(
                name: "ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_ItemAttributeId_ItemId",
                table: "ItemAttributes",
                columns: new[] { "ItemAttributeId", "ItemId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeVarietyId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyId",
                principalTable: "ItemAttributes",
                principalColumn: "ItemAttributeId");
        }
    }
}
