using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class MoveItemAttrVarsUnderItemAttributesList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_Items_ItemId",
                table: "ItemAttributeVarieties");

            migrationBuilder.AddColumn<bool>(
                name: "ManageStock",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QtyInStock",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemAttributeVarietyId",
                table: "ItemAttributes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeVarietyId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyId",
                principalTable: "ItemAttributes",
                principalColumn: "ItemAttributeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeVarietyId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropColumn(
                name: "ManageStock",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "QtyInStock",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemAttributeVarietyId",
                table: "ItemAttributes");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_Items_ItemId",
                table: "ItemAttributeVarieties",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
