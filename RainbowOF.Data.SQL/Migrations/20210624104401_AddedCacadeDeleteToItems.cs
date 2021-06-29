using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AddedCacadeDeleteToItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributes_Items_ItemId",
                table: "ItemAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_Items_ItemId",
                table: "ItemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemImages_Items_ItemId",
                table: "ItemImages");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributes_Items_ItemId",
                table: "ItemAttributes",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeId",
                principalTable: "ItemAttributes",
                principalColumn: "ItemAttributeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_Items_ItemId",
                table: "ItemCategories",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemImages_Items_ItemId",
                table: "ItemImages",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributes_Items_ItemId",
                table: "ItemAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_Items_ItemId",
                table: "ItemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemImages_Items_ItemId",
                table: "ItemImages");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributes_Items_ItemId",
                table: "ItemAttributes",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeId",
                principalTable: "ItemAttributes",
                principalColumn: "ItemAttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_Items_ItemId",
                table: "ItemCategories",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemImages_Items_ItemId",
                table: "ItemImages",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId");
        }
    }
}
