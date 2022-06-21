using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class MultipleVariantAttributes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssociatedAttributeLookupId",
                table: "ItemVariants");

            migrationBuilder.DropColumn(
                name: "AssociatedAttributeVarietyLookupId",
                table: "ItemVariants");

            migrationBuilder.CreateTable(
                name: "ItemVariantAssociatedLookups",
                columns: table => new
                {
                    ItemVariantAssociatedLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociatedAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociatedAttributeVarietyLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemVariantAssociatedLookups", x => x.ItemVariantAssociatedLookupId);
                    table.ForeignKey(
                        name: "FK_ItemVariantAssociatedLookups_ItemVariants_ItemVariantId",
                        column: x => x.ItemVariantId,
                        principalTable: "ItemVariants",
                        principalColumn: "ItemVariantId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAssociatedLookups_ItemVariantId",
                table: "ItemVariantAssociatedLookups",
                column: "ItemVariantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemVariantAssociatedLookups");

            migrationBuilder.AddColumn<Guid>(
                name: "AssociatedAttributeLookupId",
                table: "ItemVariants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AssociatedAttributeVarietyLookupId",
                table: "ItemVariants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
