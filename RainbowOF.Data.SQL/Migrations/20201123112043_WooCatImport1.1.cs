using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class WooCatImport11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerKey",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "ConsumerSecret",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "IsSecureURL",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "JSONAPIPostFix",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "QueryURL",
                table: "SysPrefs");

            migrationBuilder.DropColumn(
                name: "RootAPIPostFix",
                table: "SysPrefs");

            migrationBuilder.CreateTable(
                name: "ActiveItemAttributes",
                columns: table => new
                {
                    ActiveItemAttributeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(nullable: false),
                    IsUsedForVariableType = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveItemAttributes", x => x.ActiveItemAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributes",
                columns: table => new
                {
                    ItemAttributeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeName = table.Column<string>(maxLength: 100, nullable: false),
                    OrderBy = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributes", x => x.ItemAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributeVarieties",
                columns: table => new
                {
                    ItemAttributeVarietyId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemAttributeId = table.Column<int>(nullable: false),
                    VarietyName = table.Column<string>(maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(maxLength: 2, nullable: true),
                    FGColour = table.Column<int>(maxLength: 11, nullable: false),
                    BGColour = table.Column<string>(maxLength: 11, nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributeVarieties", x => x.ItemAttributeVarietyId);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    ItemCategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCategoryName = table.Column<string>(maxLength: 255, nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategories", x => x.ItemCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ItemUoMs",
                columns: table => new
                {
                    ItemUoMId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UoMName = table.Column<string>(maxLength: 100, nullable: false),
                    UoMSymbol = table.Column<string>(maxLength: 10, nullable: false),
                    BaseUoMId = table.Column<int>(nullable: true),
                    BaseConversationFactor = table.Column<double>(nullable: false),
                    RoundTo = table.Column<int>(nullable: false)
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
                name: "WooProductAttributeMappings",
                columns: table => new
                {
                    WooProductAttributeMappingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WooProductAttributeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooProductAttributeMappings", x => x.WooProductAttributeMappingId);
                });

            migrationBuilder.CreateTable(
                name: "WooProductAttributeTermMappings",
                columns: table => new
                {
                    ItemAttributeVarietyId = table.Column<int>(nullable: false),
                    WooProductAttributeTermId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooProductAttributeTermMappings", x => new { x.ItemAttributeVarietyId, x.WooProductAttributeTermId });
                });

            migrationBuilder.CreateTable(
                name: "WooSettings",
                columns: table => new
                {
                    WooSettingsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryURL = table.Column<string>(maxLength: 500, nullable: true),
                    IsSecureURL = table.Column<bool>(nullable: false),
                    ConsumerKey = table.Column<string>(maxLength: 250, nullable: true),
                    ConsumerSecret = table.Column<string>(maxLength: 250, nullable: true),
                    RootAPIPostFix = table.Column<string>(maxLength: 100, nullable: true),
                    JSONAPIPostFix = table.Column<string>(maxLength: 100, nullable: true),
                    AreCategoriesImported = table.Column<bool>(nullable: false),
                    AreAttributesImported = table.Column<bool>(nullable: false),
                    AreVarietiesMapped = table.Column<bool>(nullable: false),
                    OnlyInStockItemsImported = table.Column<bool>(nullable: false),
                    AreItemQuantatiesImported = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooSettings", x => x.WooSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "WooSyncLogs",
                columns: table => new
                {
                    WooSyncLogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WooSyncDateTime = table.Column<DateTime>(nullable: false),
                    Section = table.Column<int>(nullable: false),
                    SectionID = table.Column<int>(nullable: false),
                    Result = table.Column<int>(nullable: false),
                    Parameters = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooSyncLogs", x => x.WooSyncLogId);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(maxLength: 100, nullable: false),
                    SKU = table.Column<string>(maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false),
                    ItemDetail = table.Column<string>(maxLength: 255, nullable: true),
                    ItemCategoryId = table.Column<int>(nullable: true),
                    ParentItemId = table.Column<int>(nullable: true),
                    ReplacementItemId = table.Column<int>(nullable: true),
                    ItemAbbreviatedName = table.Column<string>(maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_ItemCategories_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "ItemCategoryId",
                        onDelete: ReferentialAction.SetNull);
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
                name: "WooCategoryMaps",
                columns: table => new
                {
                    WooCategoryMapId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WooCategoryId = table.Column<int>(nullable: false),
                    WooCategoryName = table.Column<string>(maxLength: 255, nullable: true),
                    WooCategorySlug = table.Column<string>(maxLength: 255, nullable: true),
                    WooCategoryParentId = table.Column<int>(nullable: true),
                    ItemCategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooCategoryMaps", x => x.WooCategoryMapId);
                    table.ForeignKey(
                        name: "FK_WooCategoryMaps_ItemCategories_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "ItemCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveItemAttributeVarieties",
                columns: table => new
                {
                    ActiveItemAttributeVarietyId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(nullable: false),
                    ItemAttributeId = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false),
                    ItemUoMId = table.Column<int>(nullable: true),
                    UoMQtyPerItem = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveItemAttributeVarieties", x => x.ActiveItemAttributeVarietyId);
                    table.ForeignKey(
                        name: "FK_ActiveItemAttributeVarieties_ItemUoMs_ItemUoMId",
                        column: x => x.ItemUoMId,
                        principalTable: "ItemUoMs",
                        principalColumn: "ItemUoMId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveItemAttributes_ActiveItemAttributeId_ItemId",
                table: "ActiveItemAttributes",
                columns: new[] { "ActiveItemAttributeId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActiveItemAttributeVarieties_ItemUoMId",
                table: "ActiveItemAttributeVarieties",
                column: "ItemUoMId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveItemAttributeVarieties_ItemId_ActiveItemAttributeVarietyId",
                table: "ActiveItemAttributeVarieties",
                columns: new[] { "ItemId", "ActiveItemAttributeVarietyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_AttributeName",
                table: "ItemAttributes",
                column: "AttributeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_VarietyName_ItemAttributeId",
                table: "ItemAttributeVarieties",
                columns: new[] { "VarietyName", "ItemAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemCategoryName",
                table: "ItemCategories",
                column: "ItemCategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCategoryId",
                table: "Items",
                column: "ItemCategoryId");

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
                name: "IX_WooCategoryMaps_ItemCategoryId",
                table: "WooCategoryMaps",
                column: "ItemCategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveItemAttributes");

            migrationBuilder.DropTable(
                name: "ActiveItemAttributeVarieties");

            migrationBuilder.DropTable(
                name: "ItemAttributes");

            migrationBuilder.DropTable(
                name: "ItemAttributeVarieties");

            migrationBuilder.DropTable(
                name: "Items");

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
                name: "ItemUoMs");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.AddColumn<string>(
                name: "ConsumerKey",
                table: "SysPrefs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerSecret",
                table: "SysPrefs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecureURL",
                table: "SysPrefs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JSONAPIPostFix",
                table: "SysPrefs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryURL",
                table: "SysPrefs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootAPIPostFix",
                table: "SysPrefs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
