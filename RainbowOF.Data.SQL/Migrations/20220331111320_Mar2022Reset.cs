using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class Mar2022Reset : Migration
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
                name: "ItemAttributesLookups",
                columns: table => new
                {
                    ItemAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderBy = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributesLookups", x => x.ItemAttributeLookupId);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ItemDetail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryItemCategoryLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReplacementItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemAbbreviatedName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ManageStock = table.Column<bool>(type: "bit", nullable: false),
                    QtyInStock = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Items_ReplacementItemId",
                        column: x => x.ReplacementItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemUoMLookups",
                columns: table => new
                {
                    ItemUoMLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UoMName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UoMSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BaseUoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BaseConversationFactor = table.Column<double>(type: "float", nullable: false),
                    RoundTo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemUoMLookups", x => x.ItemUoMLookupId);
                    table.ForeignKey(
                        name: "FK_ItemUoMLookups_ItemUoMLookups_BaseUoMId",
                        column: x => x.BaseUoMId,
                        principalTable: "ItemUoMLookups",
                        principalColumn: "ItemUoMLookupId");
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
                });

            migrationBuilder.CreateTable(
                name: "WooProductMaps",
                columns: table => new
                {
                    WooProductMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WooProductId = table.Column<int>(type: "int", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooProductMaps", x => x.WooProductMapId);
                });

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
                    AreAffiliateProdcutsImported = table.Column<bool>(type: "bit", nullable: false)
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
                name: "ItemAttributeVarietiesLookups",
                columns: table => new
                {
                    ItemAttributeVarietyLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VarietyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UoMQtyPerItem = table.Column<double>(type: "float", nullable: false),
                    DefaultSKUSuffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    FGColour = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    BGColour = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributeVarietiesLookups", x => x.ItemAttributeVarietyLookupId);
                    table.ForeignKey(
                        name: "FK_ItemAttributeVarietiesLookups_ItemAttributesLookups_ItemAttributeLookupId",
                        column: x => x.ItemAttributeLookupId,
                        principalTable: "ItemAttributesLookups",
                        principalColumn: "ItemAttributeLookupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributes",
                columns: table => new
                {
                    ItemAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsUsedForItemVariety = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributes", x => x.ItemAttributeId);
                    table.ForeignKey(
                        name: "FK_ItemAttributes_ItemAttributesLookups_ItemAttributeLookupId",
                        column: x => x.ItemAttributeLookupId,
                        principalTable: "ItemAttributesLookups",
                        principalColumn: "ItemAttributeLookupId");
                    table.ForeignKey(
                        name: "FK_ItemAttributes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemImages",
                columns: table => new
                {
                    ItemImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Alt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemImages", x => x.ItemImageId);
                    table.ForeignKey(
                        name: "FK_ItemImages_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemVariants",
                columns: table => new
                {
                    ItemVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemVariantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ItemVariantAbbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ManageStock = table.Column<bool>(type: "bit", nullable: false),
                    QtyInStock = table.Column<int>(type: "int", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemVariants", x => x.ItemVariantId);
                    table.ForeignKey(
                        name: "FK_ItemVariants_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "ItemCategoriesLookups",
                columns: table => new
                {
                    ItemCategoryLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UsedForPrediction = table.Column<bool>(type: "bit", nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UoMBaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategoriesLookups", x => x.ItemCategoryLookupId);
                    table.ForeignKey(
                        name: "FK_ItemCategoriesLookups_ItemCategoriesLookups_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ItemCategoriesLookups",
                        principalColumn: "ItemCategoryLookupId");
                    table.ForeignKey(
                        name: "FK_ItemCategoriesLookups_ItemUoMLookups_UoMBaseId",
                        column: x => x.UoMBaseId,
                        principalTable: "ItemUoMLookups",
                        principalColumn: "ItemUoMLookupId");
                });

            migrationBuilder.CreateTable(
                name: "ItemAttributeVarieties",
                columns: table => new
                {
                    ItemAttributeVarietyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemAttributeVarietyLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttributeVarieties", x => x.ItemAttributeVarietyId);
                    table.ForeignKey(
                        name: "FK_ItemAttributeVarieties_ItemAttributes_ItemAttributeId",
                        column: x => x.ItemAttributeId,
                        principalTable: "ItemAttributes",
                        principalColumn: "ItemAttributeId");
                    table.ForeignKey(
                        name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookups_ItemAttributeVarietyLookupId",
                        column: x => x.ItemAttributeVarietyLookupId,
                        principalTable: "ItemAttributeVarietiesLookups",
                        principalColumn: "ItemAttributeVarietyLookupId");
                });

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
                        name: "FK_ItemVariantAssociatedLookups_ItemAttributesLookups_AssociatedAttributeLookupId",
                        column: x => x.AssociatedAttributeLookupId,
                        principalTable: "ItemAttributesLookups",
                        principalColumn: "ItemAttributeLookupId");
                    table.ForeignKey(
                        name: "FK_ItemVariantAssociatedLookups_ItemAttributeVarietiesLookups_AssociatedAttributeVarietyLookupId",
                        column: x => x.AssociatedAttributeVarietyLookupId,
                        principalTable: "ItemAttributeVarietiesLookups",
                        principalColumn: "ItemAttributeVarietyLookupId");
                    table.ForeignKey(
                        name: "FK_ItemVariantAssociatedLookups_ItemVariants_ItemVariantId",
                        column: x => x.ItemVariantId,
                        principalTable: "ItemVariants",
                        principalColumn: "ItemVariantId");
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    ItemCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemCategoryLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsedForPrediction = table.Column<bool>(type: "bit", nullable: false),
                    UoMBaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategories", x => x.ItemCategoryId);
                    table.ForeignKey(
                        name: "FK_ItemCategories_ItemCategoriesLookups_ItemCategoryLookupId",
                        column: x => x.ItemCategoryLookupId,
                        principalTable: "ItemCategoriesLookups",
                        principalColumn: "ItemCategoryLookupId");
                    table.ForeignKey(
                        name: "FK_ItemCategories_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ItemCategories_ItemUoMLookups_UoMBaseId",
                        column: x => x.UoMBaseId,
                        principalTable: "ItemUoMLookups",
                        principalColumn: "ItemUoMLookupId");
                });

            migrationBuilder.CreateTable(
                name: "WooCategoryMaps",
                columns: table => new
                {
                    WooCategoryMapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WooCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    WooCategoryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    WooCategorySlug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    WooCategoryParentId = table.Column<long>(type: "bigint", nullable: true),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false),
                    ItemCategoryLookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooCategoryMaps", x => x.WooCategoryMapId);
                    table.ForeignKey(
                        name: "FK_WooCategoryMaps_ItemCategoriesLookups_ItemCategoryLookupId",
                        column: x => x.ItemCategoryLookupId,
                        principalTable: "ItemCategoriesLookups",
                        principalColumn: "ItemCategoryLookupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClosureDates_EventName",
                table: "ClosureDates",
                column: "EventName",
                unique: true,
                filter: "[EventName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributes_ItemAttributeId",
                table: "ItemAttributes",
                column: "ItemAttributeId",
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
                name: "IX_ItemAttributesLookups_AttributeName",
                table: "ItemAttributesLookups",
                column: "AttributeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributesLookups_OrderBy_AttributeName",
                table: "ItemAttributesLookups",
                columns: new[] { "OrderBy", "AttributeName" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarieties_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookups_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookups",
                column: "ItemAttributeLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookups",
                columns: new[] { "VarietyName", "ItemAttributeLookupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_SortOrder",
                table: "ItemAttributeVarietiesLookups",
                columns: new[] { "VarietyName", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemCategoryId_ItemId",
                table: "ItemCategories",
                columns: new[] { "ItemCategoryId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemCategoryLookupId",
                table: "ItemCategories",
                column: "ItemCategoryLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemId",
                table: "ItemCategories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_UoMBaseId",
                table: "ItemCategories",
                column: "UoMBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoriesLookups_CategoryName",
                table: "ItemCategoriesLookups",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoriesLookups_ParentCategoryId",
                table: "ItemCategoriesLookups",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoriesLookups_UoMBaseId",
                table: "ItemCategoriesLookups",
                column: "UoMBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemImages_ItemId",
                table: "ItemImages",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemName",
                table: "Items",
                column: "ItemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ReplacementItemId",
                table: "Items",
                column: "ReplacementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUoMLookups_BaseUoMId",
                table: "ItemUoMLookups",
                column: "BaseUoMId",
                unique: true,
                filter: "[BaseUoMId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUoMLookups_UoMName",
                table: "ItemUoMLookups",
                column: "UoMName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAssociatedLookups_AssociatedAttributeLookupId",
                table: "ItemVariantAssociatedLookups",
                column: "AssociatedAttributeLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAssociatedLookups_AssociatedAttributeVarietyLookupId",
                table: "ItemVariantAssociatedLookups",
                column: "AssociatedAttributeVarietyLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAssociatedLookups_ItemVariantId",
                table: "ItemVariantAssociatedLookups",
                column: "ItemVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariants_ItemId",
                table: "ItemVariants",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariants_ItemVariantId_ItemId",
                table: "ItemVariants",
                columns: new[] { "ItemVariantId", "ItemId" },
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
                name: "ItemAttributeVarieties");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.DropTable(
                name: "ItemImages");

            migrationBuilder.DropTable(
                name: "ItemVariantAssociatedLookups");

            migrationBuilder.DropTable(
                name: "SysPrefs");

            migrationBuilder.DropTable(
                name: "WooCategoryMaps");

            migrationBuilder.DropTable(
                name: "WooProductAttributeMappings");

            migrationBuilder.DropTable(
                name: "WooProductAttributeTermMappings");

            migrationBuilder.DropTable(
                name: "WooProductMaps");

            migrationBuilder.DropTable(
                name: "WooProductVariantMaps");

            migrationBuilder.DropTable(
                name: "WooSettings");

            migrationBuilder.DropTable(
                name: "WooSyncLogs");

            migrationBuilder.DropTable(
                name: "ItemAttributes");

            migrationBuilder.DropTable(
                name: "ItemAttributeVarietiesLookups");

            migrationBuilder.DropTable(
                name: "ItemVariants");

            migrationBuilder.DropTable(
                name: "ItemCategoriesLookups");

            migrationBuilder.DropTable(
                name: "ItemAttributesLookups");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "ItemUoMLookups");
        }
    }
}
