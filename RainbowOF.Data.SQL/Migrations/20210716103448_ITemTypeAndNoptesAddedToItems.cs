using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class ITemTypeAndNoptesAddedToItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemDetail",
                table: "Items",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemType",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Items");

            migrationBuilder.AlterColumn<string>(
                name: "ItemDetail",
                table: "Items",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
