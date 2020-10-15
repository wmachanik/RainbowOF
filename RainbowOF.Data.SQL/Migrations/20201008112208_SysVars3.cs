using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class SysVars3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSecure",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "QueryParamCustomerKey",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "QueryParamCustomerSecret",
                table: "SysPrefs");

            migrationBuilder.AlterColumn<string>(
                name: "QueryURL",
                table: "SysPrefs",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerKey",
                table: "SysPrefs",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerSecret",
                table: "SysPrefs",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecureURL",
                table: "SysPrefs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JSONAPIPostFix",
                table: "SysPrefs",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootAPIPostFix",
                table: "SysPrefs",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerKey",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "CustomerSecret",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "IsSecureURL",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "JSONAPIPostFix",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "RootAPIPostFix",
                table: "SysPrefs");

            migrationBuilder.AlterColumn<string>(
                name: "QueryURL",
                table: "SysPrefs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecure",
                table: "SysPrefs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QueryParamCustomerKey",
                table: "SysPrefs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryParamCustomerSecret",
                table: "SysPrefs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
