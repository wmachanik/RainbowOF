using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AttribNameChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssocatedAttributeLookupId",
                table: "ItemVariants",
                newName: "AssociatedAttributeLookupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssociatedAttributeLookupId",
                table: "ItemVariants",
                newName: "AssocatedAttributeLookupId");
        }
    }
}
