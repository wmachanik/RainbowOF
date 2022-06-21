using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class fixItemCateChildLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategoriesLookup_ItemCategoriesLookup_CategoryId",
                table: "ItemCategoriesLookup");

            migrationBuilder.DropIndex(
                name: "IX_ItemCategoriesLookup_CategoryId",
                table: "ItemCategoriesLookup");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ItemCategoriesLookup");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "ItemCategoriesLookup",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoriesLookup_CategoryId",
                table: "ItemCategoriesLookup",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategoriesLookup_ItemCategoriesLookup_CategoryId",
                table: "ItemCategoriesLookup",
                column: "CategoryId",
                principalTable: "ItemCategoriesLookup",
                principalColumn: "ItemCategoryLookupId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
