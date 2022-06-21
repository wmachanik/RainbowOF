using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class ItemEditChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UoMBaseId",
                table: "ItemCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsedForPrediction",
                table: "ItemCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_UoMBaseId",
                table: "ItemCategories",
                column: "UoMBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_ItemUoMLookups_UoMBaseId",
                table: "ItemCategories",
                column: "UoMBaseId",
                principalTable: "ItemUoMLookups",
                principalColumn: "ItemUoMLookupId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_ItemUoMLookups_UoMBaseId",
                table: "ItemCategories");

            migrationBuilder.DropIndex(
                name: "IX_ItemCategories_UoMBaseId",
                table: "ItemCategories");

            migrationBuilder.DropColumn(
                name: "UoMBaseId",
                table: "ItemCategories");

            migrationBuilder.DropColumn(
                name: "UsedForPrediction",
                table: "ItemCategories");
        }
    }
}
