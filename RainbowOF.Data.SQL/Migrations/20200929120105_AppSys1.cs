using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class AppSys1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClosureDates",
                columns: table => new
                {
                    ClosureDateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(maxLength: 50, nullable: true),
                    DateClosed = table.Column<DateTime>(nullable: false),
                    DateReopen = table.Column<DateTime>(nullable: false),
                    NextPrepDate = table.Column<DateTime>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosureDates", x => x.ClosureDateId);
                });

            migrationBuilder.CreateTable(
                name: "SysPrefs",
                columns: table => new
                {
                    SysPrefsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastReccurringDate = table.Column<DateTime>(nullable: true),
                    DoReccuringOrders = table.Column<bool>(nullable: false),
                    DateLastPrepDateCalcd = table.Column<DateTime>(nullable: true),
                    ReminderDaysNumber = table.Column<int>(nullable: false),
                    GroupItemTypeID = table.Column<int>(nullable: true),
                    DefaultDeliveryPersonID = table.Column<int>(nullable: true),
                    ImageFolderPath = table.Column<string>(maxLength: 250, nullable: true),
                    QueryParamCustomerKey = table.Column<string>(maxLength: 250, nullable: true),
                    QueryParamCustomerSecret = table.Column<string>(maxLength: 250, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysPrefs", x => x.SysPrefsId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClosureDates_EventName",
                table: "ClosureDates",
                column: "EventName",
                unique: true,
                filter: "[EventName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClosureDates");

            migrationBuilder.DropTable(
                name: "SysPrefs");
        }
    }
}
