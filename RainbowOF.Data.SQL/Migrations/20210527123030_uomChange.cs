using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class uomChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMs_ItemUoMId",
                table: "ItemAttributeVarieties");

            migrationBuilder.RenameColumn(
                name: "ItemUoMId",
                table: "ItemAttributeVarieties",
                newName: "UoMId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributeVarieties_ItemUoMId",
                table: "ItemAttributeVarieties",
                newName: "IX_ItemAttributeVarieties_UoMId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMs_UoMId",
                table: "ItemAttributeVarieties",
                column: "UoMId",
                principalTable: "ItemUoMs",
                principalColumn: "ItemUoMId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMs_UoMId",
                table: "ItemAttributeVarieties");

            migrationBuilder.RenameColumn(
                name: "UoMId",
                table: "ItemAttributeVarieties",
                newName: "ItemUoMId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributeVarieties_UoMId",
                table: "ItemAttributeVarieties",
                newName: "IX_ItemAttributeVarieties_ItemUoMId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMs_ItemUoMId",
                table: "ItemAttributeVarieties",
                column: "ItemUoMId",
                principalTable: "ItemUoMs",
                principalColumn: "ItemUoMId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
