using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class Initialv13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClosureDates",
                columns: table => new
                {
                    ClosureDateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateClosed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReopen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextPrepDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosureDates", x => x.ClosureDateId);
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributesLookup",
                columns: table => new
                {
                    ItemAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderBy = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributesLookup", x => x.ItemAttributeLookupId);
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributeVarietiesLookup",
                columns: table => new
                {
                    ItemAttributeVarietyLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VarietyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    FGColour = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    BGColour = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributeVarietiesLookup", x => x.ItemAttributeVarietyLookupId);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategoriesLookup",
                columns: table => new
                {
                    ItemCategoryLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategoriesLookup", x => x.ItemCategoryLookupId);
                    table.ForeignKey(
                        name: "FK_ItemCategoriesLookup_ItemCategoriesLookup_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ItemCategoriesLookup",
                        principalColumn: "ItemCategoryLookupId");
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ItemDetail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PrimaryItemCategoryLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReplacementItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemAbbreviatedName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Items_ParentItemId",
                        column: x => x.ParentItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_Items_Items_ReplacementItemId",
                        column: x => x.ReplacementItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemUoMs",
                columns: table => new
                {
                    ItemUoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UoMName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UoMSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BaseUoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BaseConversationFactor = table.Column<double>(type: "float", nullable: false),
                    RoundTo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemUoMs", x => x.ItemUoMId);
                    table.ForeignKey(
                        name: "FK_ItemUoMs_ItemUoMs_BaseUoMId",
                        column: x => x.BaseUoMId,
                        principalTable: "ItemUoMs",
                        principalColumn: "ItemUoMId");
                });

            migrationBuilder.CreateTable(
                name: "SysPrefs",
                columns: table => new
                {
                    SysPrefsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastReccurringDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoReccuringOrders = table.Column<bool>(type: "bit", nullable: false),
                    DateLastPrepDateCalcd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReminderDaysNumber = table.Column<int>(type: "int", nullable: false),
                    GroupItemTypeId = table.Column<int>(type: "int", nullable: true),
                    DefaultDeliveryPersonId = table.Column<int>(type: "int", nullable: true),
                    ImageFolderPath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysPrefs", x => x.SysPrefsId);
                });

            migrationBuilder.CreateTable(
                name: "WooProductAttributeMappings",
                columns: table => new
                {
                    WooProductAttributeMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WooProductAttributeId = table.Column<int>(type: "int", nullable: false),
                    ItemAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooProductAttributeMappings", x => x.WooProductAttributeMapId);
                });

            migrationBuilder.CreateTable(
                name: "WooProductAttributeTermMappings",
                columns: table => new
                {
                    WooProductAttributeTermMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeVarietyLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WooProductAttributeTermId = table.Column<int>(type: "int", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooProductAttributeTermMappings", x => x.WooProductAttributeTermMapId);
                    table.UniqueConstraint("AK_WooProductAttributeTermMappings_ItemAttributeVarietyLookupId_WooProductAttributeTermId", x => new { x.ItemAttributeVarietyLookupId, x.WooProductAttributeTermId });
                });

            migrationBuilder.CreateTable(
                name: "WooSettings",
                columns: table => new
                {
                    WooSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSecureURL = table.Column<bool>(type: "bit", nullable: false),
                    ConsumerKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ConsumerSecret = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RootAPIPostFix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JSONAPIPostFix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AreCategoriesImported = table.Column<bool>(type: "bit", nullable: false),
                    AreAttributesImported = table.Column<bool>(type: "bit", nullable: false),
                    AreVarietiesMapped = table.Column<bool>(type: "bit", nullable: false),
                    OnlyInStockItemsImported = table.Column<bool>(type: "bit", nullable: false),
                    AreItemQuantatiesImported = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooSettings", x => x.WooSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "WooSyncLogs",
                columns: table => new
                {
                    WooSyncLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WooSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Section = table.Column<int>(type: "int", nullable: false),
                    SectionID = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooSyncLogs", x => x.WooSyncLogId);
                });

            migrationBuilder.CreateTable(
                name: "WooCategoryMaps",
                columns: table => new
                {
                    WooCategoryMapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WooCategoryId = table.Column<int>(type: "int", nullable: false),
                    WooCategoryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    WooCategorySlug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    WooCategoryParentId = table.Column<int>(type: "int", nullable: true),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false),
                    ItemCategoryLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooCategoryMaps", x => x.WooCategoryMapId);
                    table.ForeignKey(
                        name: "FK_WooCategoryMaps_ItemCategoriesLookup_ItemCategoryLookupId",
                        column: x => x.ItemCategoryLookupId,
                        principalTable: "ItemCategoriesLookup",
                        principalColumn: "ItemCategoryLookupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributes",
                columns: table => new
                {
                    ItemAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsUsedForVariableType = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributes", x => x.ItemAttributeId);
                    table.ForeignKey(
                        name: "FK_ItemAttributes_ItemAttributesLookup_ItemAttributeLookupId",
                        column: x => x.ItemAttributeLookupId,
                        principalTable: "ItemAttributesLookup",
                        principalColumn: "ItemAttributeLookupId");
                    table.ForeignKey(
                        name: "FK_ItemAttributes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    ItemCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemCategoryLookup = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategories", x => x.ItemCategoryId);
                    table.ForeignKey(
                        name: "FK_ItemCategories_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributeVarieties",
                columns: table => new
                {
                    ItemAttributeVarietyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    ItemUoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UoMQtyPerItem = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributeVarieties", x => x.ItemAttributeVarietyId);
                    table.ForeignKey(
                        name: "FK_ItemAttributeVarieties_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemAttributeVarieties_ItemUoMs_ItemUoMId",
                        column: x => x.ItemUoMId,
                        principalTable: "ItemUoMs",
                        principalColumn: "ItemUoMId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClosureDates_EventName",
                table: "ClosureDates",
                column: "EventName",
                unique: true,
                filter: "[EventName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_ItemAttributeId_ItemId",
                table: "ItemAttributes",
                columns: new[] { "ItemAttributeId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_ItemAttributeLookupId",
                table: "ItemAttributes",
                column: "ItemAttributeLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_ItemId",
                table: "ItemAttributes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributesLookup_AttributeName",
                table: "ItemAttributesLookup",
                column: "AttributeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributesLookup_OrderBy_AttributeName",
                table: "ItemAttributesLookup",
                columns: new[] { "OrderBy", "AttributeName" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemId_ItemAttributeId",
                table: "ItemAttributeVarieties",
                columns: new[] { "ItemId", "ItemAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemUoMId",
                table: "ItemAttributeVarieties",
                column: "ItemUoMId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookup_VarietyName_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup",
                columns: new[] { "VarietyName", "ItemAttributeLookupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemCategoryId_ItemId",
                table: "ItemCategories",
                columns: new[] { "ItemCategoryId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemId",
                table: "ItemCategories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoriesLookup_CategoryName",
                table: "ItemCategoriesLookup",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoriesLookup_ParentCategoryId",
                table: "ItemCategoriesLookup",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemName",
                table: "Items",
                column: "ItemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ParentItemId",
                table: "Items",
                column: "ParentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ReplacementItemId",
                table: "Items",
                column: "ReplacementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SKU",
                table: "Items",
                column: "SKU",
                unique: true,
                filter: "[SKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUoMs_BaseUoMId",
                table: "ItemUoMs",
                column: "BaseUoMId",
                unique: true,
                filter: "[BaseUoMId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUoMs_UoMName",
                table: "ItemUoMs",
                column: "UoMName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WooCategoryMaps_ItemCategoryLookupId",
                table: "WooCategoryMaps",
                column: "ItemCategoryLookupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClosureDates");

            migrationBuilder.DropTable(
                name: "ItemAttributes");

            migrationBuilder.DropTable(
                name: "ItemAttributeVarieties");

            migrationBuilder.DropTable(
                name: "ItemAttributeVarietiesLookup");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.DropTable(
                name: "SysPrefs");

            migrationBuilder.DropTable(
                name: "WooCategoryMaps");

            migrationBuilder.DropTable(
                name: "WooProductAttributeMappings");

            migrationBuilder.DropTable(
                name: "WooProductAttributeTermMappings");

            migrationBuilder.DropTable(
                name: "WooSettings");

            migrationBuilder.DropTable(
                name: "WooSyncLogs");

            migrationBuilder.DropTable(
                name: "ItemAttributesLookup");

            migrationBuilder.DropTable(
                name: "ItemUoMs");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "ItemCategoriesLookup");
        }
    }
}
