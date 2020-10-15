using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class SysVars2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSecure",
                table: "SysPrefs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QueryURL",
                table: "SysPrefs",
                maxLength: 250,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSecure",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "QueryURL",
                table: "SysPrefs");
        }
    }
}
