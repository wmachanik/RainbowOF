using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class WooVariantsLinkTableAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WooProductVariantMaps",
                columns: table => new
                {
                    WooProductVariantMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WooProductVariantId = table.Column<int>(type: "int", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooProductVariantMaps", x => x.WooProductVariantMapId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WooProductVariantMaps");
        }
    }
}
