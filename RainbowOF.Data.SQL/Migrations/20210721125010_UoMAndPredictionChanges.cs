using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RainbowOF.Data.SQL.Migrations
{
    public partial class UoMAndPredictionChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributes_ItemAttributesLookup_ItemAttributeLookupId",
                table: "ItemAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookup_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            // did this manually due to the constraint
            //migrationBuilder.DropForeignKey(
            //    name: "FK_ItemAttributeVarieties_ItemUoMs_UoMId",
            //    table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarietiesLookup_ItemAttributesLookup_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "ItemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategoriesLookup_ItemCategoriesLookup_ParentCategoryId",
                table: "ItemCategoriesLookup");

            migrationBuilder.DropForeignKey(
                name: "FK_WooCategoryMaps_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "WooCategoryMaps");

            // did this manually due to the constraint
            //migrationBuilder.DropTable(
            //    name: "ItemUoMs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemCategoriesLookup",
                table: "ItemCategoriesLookup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemAttributeVarietiesLookup",
                table: "ItemAttributeVarietiesLookup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemAttributesLookup",
                table: "ItemAttributesLookup");

            migrationBuilder.RenameTable(
                name: "ItemCategoriesLookup",
                newName: "ItemCategoriesLookups");

            migrationBuilder.RenameTable(
                name: "ItemAttributeVarietiesLookup",
                newName: "ItemAttributeVarietiesLookups");

            migrationBuilder.RenameTable(
                name: "ItemAttributesLookup",
                newName: "ItemAttributesLookups");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategoriesLookup_ParentCategoryId",
                table: "ItemCategoriesLookups",
                newName: "IX_ItemCategoriesLookups_ParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategoriesLookup_CategoryName",
                table: "ItemCategoriesLookups",
                newName: "IX_ItemCategoriesLookups_CategoryName");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributeVarietiesLookup_VarietyName_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookups",
                newName: "IX_ItemAttributeVarietiesLookups_VarietyName_ItemAttributeLookupId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributeVarietiesLookup_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookups",
                newName: "IX_ItemAttributeVarietiesLookups_ItemAttributeLookupId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributesLookup_OrderBy_AttributeName",
                table: "ItemAttributesLookups",
                newName: "IX_ItemAttributesLookups_OrderBy_AttributeName");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributesLookup_AttributeName",
                table: "ItemAttributesLookups",
                newName: "IX_ItemAttributesLookups_AttributeName");

            migrationBuilder.AddColumn<Guid>(
                name: "UoMBaseId",
                table: "ItemCategoriesLookups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemCategoriesLookups",
                table: "ItemCategoriesLookups",
                column: "ItemCategoryLookupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemAttributeVarietiesLookups",
                table: "ItemAttributeVarietiesLookups",
                column: "ItemAttributeVarietyLookupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemAttributesLookups",
                table: "ItemAttributesLookups",
                column: "ItemAttributeLookupId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoriesLookups_UoMBaseId",
                table: "ItemCategoriesLookups",
                column: "UoMBaseId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributes_ItemAttributesLookups_ItemAttributeLookupId",
                table: "ItemAttributes",
                column: "ItemAttributeLookupId",
                principalTable: "ItemAttributesLookups",
                principalColumn: "ItemAttributeLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookups_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId",
                principalTable: "ItemAttributeVarietiesLookups",
                principalColumn: "ItemAttributeVarietyLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMLookups_UoMId",
                table: "ItemAttributeVarieties",
                column: "UoMId",
                principalTable: "ItemUoMLookups",
                principalColumn: "ItemUoMLookupId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarietiesLookups_ItemAttributesLookups_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookups",
                column: "ItemAttributeLookupId",
                principalTable: "ItemAttributesLookups",
                principalColumn: "ItemAttributeLookupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookups_ItemCategoryLookupId",
                table: "ItemCategories",
                column: "ItemCategoryLookupId",
                principalTable: "ItemCategoriesLookups",
                principalColumn: "ItemCategoryLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategoriesLookups_ItemCategoriesLookups_ParentCategoryId",
                table: "ItemCategoriesLookups",
                column: "ParentCategoryId",
                principalTable: "ItemCategoriesLookups",
                principalColumn: "ItemCategoryLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategoriesLookups_ItemUoMLookups_UoMBaseId",
                table: "ItemCategoriesLookups",
                column: "UoMBaseId",
                principalTable: "ItemUoMLookups",
                principalColumn: "ItemUoMLookupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WooCategoryMaps_ItemCategoriesLookups_ItemCategoryLookupId",
                table: "WooCategoryMaps",
                column: "ItemCategoryLookupId",
                principalTable: "ItemCategoriesLookups",
                principalColumn: "ItemCategoryLookupId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributes_ItemAttributesLookups_ItemAttributeLookupId",
                table: "ItemAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookups_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMLookups_UoMId",
                table: "ItemAttributeVarieties");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttributeVarietiesLookups_ItemAttributesLookups_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookups");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookups_ItemCategoryLookupId",
                table: "ItemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategoriesLookups_ItemCategoriesLookups_ParentCategoryId",
                table: "ItemCategoriesLookups");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategoriesLookups_ItemUoMLookups_UoMBaseId",
                table: "ItemCategoriesLookups");

            migrationBuilder.DropForeignKey(
                name: "FK_WooCategoryMaps_ItemCategoriesLookups_ItemCategoryLookupId",
                table: "WooCategoryMaps");

            migrationBuilder.DropTable(
                name: "ItemUoMLookups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemCategoriesLookups",
                table: "ItemCategoriesLookups");

            migrationBuilder.DropIndex(
                name: "IX_ItemCategoriesLookups_UoMBaseId",
                table: "ItemCategoriesLookups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemAttributeVarietiesLookups",
                table: "ItemAttributeVarietiesLookups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemAttributesLookups",
                table: "ItemAttributesLookups");

            migrationBuilder.DropColumn(
                name: "UoMBaseId",
                table: "ItemCategoriesLookups");

            migrationBuilder.RenameTable(
                name: "ItemCategoriesLookups",
                newName: "ItemCategoriesLookup");

            migrationBuilder.RenameTable(
                name: "ItemAttributeVarietiesLookups",
                newName: "ItemAttributeVarietiesLookup");

            migrationBuilder.RenameTable(
                name: "ItemAttributesLookups",
                newName: "ItemAttributesLookup");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategoriesLookups_ParentCategoryId",
                table: "ItemCategoriesLookup",
                newName: "IX_ItemCategoriesLookup_ParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategoriesLookups_CategoryName",
                table: "ItemCategoriesLookup",
                newName: "IX_ItemCategoriesLookup_CategoryName");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributeVarietiesLookups_VarietyName_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup",
                newName: "IX_ItemAttributeVarietiesLookup_VarietyName_ItemAttributeLookupId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributeVarietiesLookups_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup",
                newName: "IX_ItemAttributeVarietiesLookup_ItemAttributeLookupId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributesLookups_OrderBy_AttributeName",
                table: "ItemAttributesLookup",
                newName: "IX_ItemAttributesLookup_OrderBy_AttributeName");

            migrationBuilder.RenameIndex(
                name: "IX_ItemAttributesLookups_AttributeName",
                table: "ItemAttributesLookup",
                newName: "IX_ItemAttributesLookup_AttributeName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemCategoriesLookup",
                table: "ItemCategoriesLookup",
                column: "ItemCategoryLookupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemAttributeVarietiesLookup",
                table: "ItemAttributeVarietiesLookup",
                column: "ItemAttributeVarietyLookupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemAttributesLookup",
                table: "ItemAttributesLookup",
                column: "ItemAttributeLookupId");

            migrationBuilder.CreateTable(
                name: "ItemUoMs",
                columns: table => new
                {
                    ItemUoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseConversationFactor = table.Column<double>(type: "float", nullable: false),
                    BaseUoMId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoundTo = table.Column<int>(type: "int", nullable: false),
                    UoMName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UoMSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributes_ItemAttributesLookup_ItemAttributeLookupId",
                table: "ItemAttributes",
                column: "ItemAttributeLookupId",
                principalTable: "ItemAttributesLookup",
                principalColumn: "ItemAttributeLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemAttributeVarietiesLookup_ItemAttributeVarietyLookupId",
                table: "ItemAttributeVarieties",
                column: "ItemAttributeVarietyLookupId",
                principalTable: "ItemAttributeVarietiesLookup",
                principalColumn: "ItemAttributeVarietyLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarieties_ItemUoMs_UoMId",
                table: "ItemAttributeVarieties",
                column: "UoMId",
                principalTable: "ItemUoMs",
                principalColumn: "ItemUoMId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttributeVarietiesLookup_ItemAttributesLookup_ItemAttributeLookupId",
                table: "ItemAttributeVarietiesLookup",
                column: "ItemAttributeLookupId",
                principalTable: "ItemAttributesLookup",
                principalColumn: "ItemAttributeLookupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "ItemCategories",
                column: "ItemCategoryLookupId",
                principalTable: "ItemCategoriesLookup",
                principalColumn: "ItemCategoryLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategoriesLookup_ItemCategoriesLookup_ParentCategoryId",
                table: "ItemCategoriesLookup",
                column: "ParentCategoryId",
                principalTable: "ItemCategoriesLookup",
                principalColumn: "ItemCategoryLookupId");

            migrationBuilder.AddForeignKey(
                name: "FK_WooCategoryMaps_ItemCategoriesLookup_ItemCategoryLookupId",
                table: "WooCategoryMaps",
                column: "ItemCategoryLookupId",
                principalTable: "ItemCategoriesLookup",
                principalColumn: "ItemCategoryLookupId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
