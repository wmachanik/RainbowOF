using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class UoMFromItemToLookup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMLookups_UoMId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarieties_UoMId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropColumn(
                name: "UoMId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropColumn(
                name: "UoMQtyPerItem",
                table: "ItemAttributeVarieties");

            migrationBuilder.AddColumn<string>(
                name: "DefaultSKUSuffix",
                table: "ItemAttributeVarietiesLookups",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UoMQtyPerItem",
                table: "ItemAttributeVarietiesLookups",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_SortOrder",
                table: "ItemAttributeVarietiesLookups",
                columns: new[] { "VarietyName", "SortOrder" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_SortOrder",
                table: "ItemAttributeVarietiesLookups");

            migrationBuilder.DropColumn(
                name: "DefaultSKUSuffix",
                table: "ItemAttributeVarietiesLookups");

            migrationBuilder.DropColumn(
                name: "UoMQtyPerItem",
                table: "ItemAttributeVarietiesLookups");

            migrationBuilder.AddColumn<Guid>(
                name: "UoMId",
                table: "ItemAttributeVarieties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UoMQtyPerItem",
                table: "ItemAttributeVarieties",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_UoMId",
                table: "ItemAttributeVarieties",
                column: "UoMId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMLookups_UoMId",
                table: "ItemAttributeVarieties",
                column: "UoMId",
                principalTable: "ItemUoMLookups",
                principalColumn: "ItemUoMLookupId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
