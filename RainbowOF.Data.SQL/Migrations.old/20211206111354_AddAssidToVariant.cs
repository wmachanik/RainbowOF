using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AddAssidToVariant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssocatedAttributeLookupId",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssocatedAttributeLookupId",
                table: "ItemVariants");

            migrationBuilder.DropColumn(
                name: "AssociatedAttributeVarietyLookupId",
                table: "ItemVariants");
        }
    }
}
