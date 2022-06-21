﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RainbowOF.Data.SQL;

#nullable disable

namespace RainbowOF.Data.SQL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220212084501_MultipleVariantAttributes")]
    partial class MultipleVariantAttributes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("RainbowOF.Models.Items.Item", b =>
                {
                    b.Property<Guid>("ItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("BasePrice")
                        .HasColumnType("decimal(18,4)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("ItemAbbreviatedName")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ItemDetail")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("ItemType")
                        .HasColumnType("int");

                    b.Property<bool>("ManageStock")
                        .HasColumnType("bit");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PrimaryItemCategoryLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("QtyInStock")
                        .HasColumnType("int");

                    b.Property<Guid?>("ReplacementItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("SKU")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.HasKey("ItemId");

                    b.HasIndex("ItemName")
                        .IsUnique();

                    b.HasIndex("ReplacementItemId");

                    b.ToTable("Items", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemAttribute", b =>
                {
                    b.Property<Guid>("ItemAttributeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsUsedForItemVariety")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemAttributeLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ItemAttributeId");

                    b.HasIndex("ItemAttributeId")
                        .IsUnique();

                    b.HasIndex("ItemAttributeLookupId");

                    b.HasIndex("ItemId");

                    b.ToTable("ItemAttributes", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemAttributeVariety", b =>
                {
                    b.Property<Guid>("ItemAttributeVarietyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemAttributeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemAttributeVarietyLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ItemAttributeVarietyId");

                    b.HasIndex("ItemAttributeId");

                    b.HasIndex("ItemAttributeVarietyLookupId");

                    b.ToTable("ItemAttributeVarieties", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemCategory", b =>
                {
                    b.Property<Guid>("ItemCategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemCategoryLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UoMBaseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("UsedForPrediction")
                        .HasColumnType("bit");

                    b.HasKey("ItemCategoryId");

                    b.HasIndex("ItemCategoryLookupId");

                    b.HasIndex("ItemId");

                    b.HasIndex("UoMBaseId");

                    b.HasIndex("ItemCategoryId", "ItemId")
                        .IsUnique();

                    b.ToTable("ItemCategories", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemImage", b =>
                {
                    b.Property<Guid>("ItemImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Alt")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ImageURL")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("IsPrimary")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("ItemImageId");

                    b.HasIndex("ItemId");

                    b.ToTable("ItemImages");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemVariant", b =>
                {
                    b.Property<Guid>("ItemVariantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("BasePrice")
                        .HasColumnType("decimal(18,4)");

                    b.Property<string>("ImageURL")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ItemVariantAbbreviation")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ItemVariantName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("ManageStock")
                        .HasColumnType("bit");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("QtyInStock")
                        .HasColumnType("int");

                    b.Property<string>("SKU")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.HasKey("ItemVariantId");

                    b.HasIndex("ItemId");

                    b.HasIndex("ItemVariantId", "ItemId")
                        .IsUnique();

                    b.ToTable("ItemVariants", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemVariantAssociatedLookup", b =>
                {
                    b.Property<Guid>("ItemVariantAssociatedLookupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AssociatedAttributeLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("AssociatedAttributeVarietyLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemVariantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ItemVariantAssociatedLookupId");

                    b.HasIndex("ItemVariantId");

                    b.ToTable("ItemVariantAssociatedLookups", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Logs.WooSyncLog", b =>
                {
                    b.Property<int>("WooSyncLogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WooSyncLogId"), 1L, 1);

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Parameters")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Result")
                        .HasColumnType("int");

                    b.Property<int>("Section")
                        .HasColumnType("int");

                    b.Property<int>("SectionID")
                        .HasColumnType("int");

                    b.Property<DateTime>("WooSyncDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("WooSyncLogId");

                    b.ToTable("WooSyncLogs", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemAttributeLookup", b =>
                {
                    b.Property<Guid>("ItemAttributeLookupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AttributeName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OrderBy")
                        .HasColumnType("int");

                    b.HasKey("ItemAttributeLookupId");

                    b.HasIndex("AttributeName")
                        .IsUnique();

                    b.HasIndex("OrderBy", "AttributeName");

                    b.ToTable("ItemAttributesLookups", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemAttributeVarietyLookup", b =>
                {
                    b.Property<Guid>("ItemAttributeVarietyLookupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BGColour")
                        .HasMaxLength(11)
                        .HasColumnType("nvarchar(11)");

                    b.Property<string>("DefaultSKUSuffix")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("FGColour")
                        .HasMaxLength(11)
                        .HasColumnType("nvarchar(11)");

                    b.Property<Guid>("ItemAttributeLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.Property<string>("Symbol")
                        .HasMaxLength(2)
                        .HasColumnType("nvarchar(2)");

                    b.Property<Guid?>("UoMId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("UoMQtyPerItem")
                        .HasColumnType("float");

                    b.Property<string>("VarietyName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("ItemAttributeVarietyLookupId");

                    b.HasIndex("ItemAttributeLookupId");

                    b.HasIndex("VarietyName", "ItemAttributeLookupId")
                        .IsUnique();

                    b.HasIndex("VarietyName", "SortOrder");

                    b.ToTable("ItemAttributeVarietiesLookups", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemCategoryLookup", b =>
                {
                    b.Property<Guid>("ItemCategoryLookupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ParentCategoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid?>("UoMBaseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("UsedForPrediction")
                        .HasColumnType("bit");

                    b.HasKey("ItemCategoryLookupId");

                    b.HasIndex("CategoryName")
                        .IsUnique();

                    b.HasIndex("ParentCategoryId");

                    b.HasIndex("UoMBaseId");

                    b.ToTable("ItemCategoriesLookups", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemUoMLookup", b =>
                {
                    b.Property<Guid>("ItemUoMLookupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("BaseConversationFactor")
                        .HasColumnType("float");

                    b.Property<Guid?>("BaseUoMId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("RoundTo")
                        .HasColumnType("int");

                    b.Property<string>("UoMName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("UoMSymbol")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.HasKey("ItemUoMLookupId");

                    b.HasIndex("BaseUoMId")
                        .IsUnique()
                        .HasFilter("[BaseUoMId] IS NOT NULL");

                    b.HasIndex("UoMName")
                        .IsUnique();

                    b.ToTable("ItemUoMLookups", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.System.ClosureDate", b =>
                {
                    b.Property<int>("ClosureDateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ClosureDateId"), 1L, 1);

                    b.Property<DateTime>("DateClosed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateReopen")
                        .HasColumnType("datetime2");

                    b.Property<string>("EventName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("NextPrepDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ClosureDateId");

                    b.HasIndex("EventName")
                        .IsUnique()
                        .HasFilter("[EventName] IS NOT NULL");

                    b.ToTable("ClosureDates", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.System.SysPrefs", b =>
                {
                    b.Property<int>("SysPrefsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SysPrefsId"), 1L, 1);

                    b.Property<DateTime?>("DateLastPrepDateCalcd")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultDeliveryPersonId")
                        .HasColumnType("int");

                    b.Property<bool>("DoReccuringOrders")
                        .HasColumnType("bit");

                    b.Property<int?>("GroupItemTypeId")
                        .HasColumnType("int");

                    b.Property<string>("ImageFolderPath")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime?>("LastReccurringDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ReminderDaysNumber")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("SysPrefsId");

                    b.ToTable("SysPrefs", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.System.WooSettings", b =>
                {
                    b.Property<int>("WooSettingsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WooSettingsId"), 1L, 1);

                    b.Property<bool>("AreAffiliateProdcutsImported")
                        .HasColumnType("bit");

                    b.Property<bool>("AreAttributesImported")
                        .HasColumnType("bit");

                    b.Property<bool>("AreCategoriesImported")
                        .HasColumnType("bit");

                    b.Property<bool>("AreVarietiesMapped")
                        .HasColumnType("bit");

                    b.Property<string>("ConsumerKey")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("ConsumerSecret")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<bool>("IsSecureURL")
                        .HasColumnType("bit");

                    b.Property<string>("JSONAPIPostFix")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("OnlyInStockItemsImported")
                        .HasColumnType("bit");

                    b.Property<string>("QueryURL")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("RootAPIPostFix")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("WooSettingsId");

                    b.ToTable("WooSettings", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Woo.WooCategoryMap", b =>
                {
                    b.Property<int>("WooCategoryMapId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WooCategoryMapId"), 1L, 1);

                    b.Property<bool>("CanUpdate")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemCategoryLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("WooCategoryId")
                        .HasColumnType("bigint");

                    b.Property<string>("WooCategoryName")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<long?>("WooCategoryParentId")
                        .HasColumnType("bigint");

                    b.Property<string>("WooCategorySlug")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("WooCategoryMapId");

                    b.HasIndex("ItemCategoryLookupId");

                    b.ToTable("WooCategoryMaps", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Woo.WooProductAttributeMap", b =>
                {
                    b.Property<Guid>("WooProductAttributeMapId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("CanUpdate")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemAttributeLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("WooProductAttributeId")
                        .HasColumnType("int");

                    b.HasKey("WooProductAttributeMapId");

                    b.ToTable("WooProductAttributeMappings", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Woo.WooProductAttributeTermMap", b =>
                {
                    b.Property<Guid>("WooProductAttributeTermMapId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("CanUpdate")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemAttributeVarietyLookupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("WooProductAttributeTermId")
                        .HasColumnType("int");

                    b.HasKey("WooProductAttributeTermMapId");

                    b.ToTable("WooProductAttributeTermMappings", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Woo.WooProductMap", b =>
                {
                    b.Property<Guid>("WooProductMapId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("CanUpdate")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("WooProductId")
                        .HasColumnType("int");

                    b.HasKey("WooProductMapId");

                    b.ToTable("WooProductMaps", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Woo.WooProductVariantMap", b =>
                {
                    b.Property<Guid>("WooProductVariantMapId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("CanUpdate")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemVariantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("WooProductVariantId")
                        .HasColumnType("int");

                    b.HasKey("WooProductVariantMapId");

                    b.ToTable("WooProductVariantMaps", (string)null);
                });

            modelBuilder.Entity("RainbowOF.Models.Items.Item", b =>
                {
                    b.HasOne("RainbowOF.Models.Items.Item", "ReplacementItem")
                        .WithMany()
                        .HasForeignKey("ReplacementItemId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("ReplacementItem");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemAttribute", b =>
                {
                    b.HasOne("RainbowOF.Models.Lookups.ItemAttributeLookup", "ItemAttributeDetail")
                        .WithMany()
                        .HasForeignKey("ItemAttributeLookupId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("RainbowOF.Models.Items.Item", null)
                        .WithMany("ItemAttributes")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.Navigation("ItemAttributeDetail");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemAttributeVariety", b =>
                {
                    b.HasOne("RainbowOF.Models.Items.ItemAttribute", null)
                        .WithMany("ItemAttributeVarieties")
                        .HasForeignKey("ItemAttributeId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.HasOne("RainbowOF.Models.Lookups.ItemAttributeVarietyLookup", "ItemAttributeVarietyDetail")
                        .WithMany()
                        .HasForeignKey("ItemAttributeVarietyLookupId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("ItemAttributeVarietyDetail");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemCategory", b =>
                {
                    b.HasOne("RainbowOF.Models.Lookups.ItemCategoryLookup", "ItemCategoryDetail")
                        .WithMany()
                        .HasForeignKey("ItemCategoryLookupId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("RainbowOF.Models.Items.Item", null)
                        .WithMany("ItemCategories")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.HasOne("RainbowOF.Models.Lookups.ItemUoMLookup", "ItemUoMBase")
                        .WithMany()
                        .HasForeignKey("UoMBaseId");

                    b.Navigation("ItemCategoryDetail");

                    b.Navigation("ItemUoMBase");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemImage", b =>
                {
                    b.HasOne("RainbowOF.Models.Items.Item", null)
                        .WithMany("ItemImages")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemVariant", b =>
                {
                    b.HasOne("RainbowOF.Models.Items.Item", null)
                        .WithMany("ItemVariants")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemVariantAssociatedLookup", b =>
                {
                    b.HasOne("RainbowOF.Models.Items.ItemVariant", null)
                        .WithMany("ItemVariantAssociatedLookups")
                        .HasForeignKey("ItemVariantId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemAttributeVarietyLookup", b =>
                {
                    b.HasOne("RainbowOF.Models.Lookups.ItemAttributeLookup", null)
                        .WithMany("ItemAttributeVarietyLookups")
                        .HasForeignKey("ItemAttributeLookupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemCategoryLookup", b =>
                {
                    b.HasOne("RainbowOF.Models.Lookups.ItemCategoryLookup", "ParentCategory")
                        .WithMany()
                        .HasForeignKey("ParentCategoryId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("RainbowOF.Models.Lookups.ItemUoMLookup", "CategoryUoMBase")
                        .WithMany()
                        .HasForeignKey("UoMBaseId");

                    b.Navigation("CategoryUoMBase");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemUoMLookup", b =>
                {
                    b.HasOne("RainbowOF.Models.Lookups.ItemUoMLookup", "BaseUoM")
                        .WithOne()
                        .HasForeignKey("RainbowOF.Models.Lookups.ItemUoMLookup", "BaseUoMId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("BaseUoM");
                });

            modelBuilder.Entity("RainbowOF.Models.Woo.WooCategoryMap", b =>
                {
                    b.HasOne("RainbowOF.Models.Lookups.ItemCategoryLookup", "ItemCategoryLookup")
                        .WithMany()
                        .HasForeignKey("ItemCategoryLookupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ItemCategoryLookup");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.Item", b =>
                {
                    b.Navigation("ItemAttributes");

                    b.Navigation("ItemCategories");

                    b.Navigation("ItemImages");

                    b.Navigation("ItemVariants");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemAttribute", b =>
                {
                    b.Navigation("ItemAttributeVarieties");
                });

            modelBuilder.Entity("RainbowOF.Models.Items.ItemVariant", b =>
                {
                    b.Navigation("ItemVariantAssociatedLookups");
                });

            modelBuilder.Entity("RainbowOF.Models.Lookups.ItemAttributeLookup", b =>
                {
                    b.Navigation("ItemAttributeVarietyLookups");
                });
#pragma warning restore 612, 618
        }
    }
}
