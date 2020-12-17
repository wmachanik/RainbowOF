﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RainbowOF.Data.SQL;

namespace RainbowOF.Data.SQL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20200929120105_AppSys1")]
    partial class AppSys1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("RainbowOF.Models.System.ClosureDate", b =>
                {
                    b.Property<int>("ClosureDateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateClosed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateReopen")
                        .HasColumnType("datetime2");

                    b.Property<string>("EventName")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

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

                    b.ToTable("ClosureDates");
                });

            modelBuilder.Entity("RainbowOF.Models.System.SysPrefs", b =>
                {
                    b.Property<int>("SysPrefsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("DateLastPrepDateCalcd")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultDeliveryPersonId")
                        .HasColumnType("int");

                    b.Property<bool>("DoReccuringOrders")
                        .HasColumnType("bit");

                    b.Property<int?>("GroupItemTypeId")
                        .HasColumnType("int");

                    b.Property<string>("ImageFolderPath")
                        .HasColumnType("nvarchar(250)")
                        .HasMaxLength(250);

                    b.Property<DateTime?>("LastReccurringDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("QueryParamConsumerKey")
                        .HasColumnType("nvarchar(250)")
                        .HasMaxLength(250);

                    b.Property<string>("QueryParamConsumerSecret")
                        .HasColumnType("nvarchar(250)")
                        .HasMaxLength(250);

                    b.Property<int>("ReminderDaysNumber")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("SysPrefsId");

                    b.ToTable("SysPrefs");
                });
#pragma warning restore 612, 618
        }
    }
}
