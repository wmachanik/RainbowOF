using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL.EntityConfigs.Items;
using RainbowOF.Datsa.SQL.EntityConfigs.Items;
using RainbowOF.Models.Items;
using RainbowOF.Models.Logs;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowOF.Data.SQL
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
         //   Configuration.LazyLoadingEnabled = true;
        }
        #region system stuff
        //        public virtual DbSet<Party> Parties { get; set; }
        public virtual DbSet<SysPrefs> SysPrefs { get; set; }
        //        public virtual DbSet<UsageLog> UsageLog { get; set; }
        //        public virtual DbSet<WeekDay> WeekDays { get; set; }
        public virtual DbSet<ClosureDate> ClosureDates { get; set; }
        #endregion

        #region item stuff
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemCategory> ItemCategories { get; set; }
        public virtual DbSet<ItemAttribute> ItemAttributes { get; set; }
        public virtual DbSet<ItemAttributeVariety> ItemAttributeVarieties { get; set; }
        public virtual DbSet<ActiveItemAttribute> ActiveItemAttributes { get; set; }
        public virtual DbSet<ActiveItemAttributeVariety> ActiveItemAttributeVarieties { get; set; }
        public virtual DbSet<ItemUoM> ItemUoMs { get; set; }

        //public DbSet<ItemUnit> ItemUnits { get; set; }
        //public DbSet<Packaging> Packagings { get; set; }
        //public DbSet<Variety> Varieties { get; set; }
        //public DbSet<ItemGroup> ItemGroups { get; set; }
        ////public DbSet<UsedItemGroup> UsedItemGroups { get; set; }
        #endregion

        #region WooStuff
        public virtual DbSet<WooSettings> WooSettings { get; set; }
        public virtual DbSet<WooSyncLog> WooSyncLogs { get; set; }
        public virtual DbSet<WooCategoryMap> WooCategoryMaps { get; set; }
        public virtual DbSet<WooProductAttributeMap> WooProductAttributeMappings { get; set; }
        public virtual DbSet<WooProductAttributeTermMap> WooProductAttributeTermMappings { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region system stuff
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ClosureDate>().ToTable(nameof(ClosureDates))
                .HasIndex(cd => cd.EventName)
                .IsUnique();

            //modelBuilder.Entity<Party>().ToTable(nameof(Parties));
            //modelBuilder.ApplyConfiguration<Party>(new PartyModelConfig());

            modelBuilder.Entity<SysPrefs>().ToTable(nameof(SysPrefs));
            #endregion

            #region item stuff
            modelBuilder.Entity<Item>().ToTable(nameof(Items));
            modelBuilder.ApplyConfiguration<Item>(new ItemModelConfig());
            modelBuilder.Entity<ItemCategory>().ToTable(nameof(ItemCategories));
            modelBuilder.ApplyConfiguration<ItemCategory>(new ItemCategoryModelConfig());
            modelBuilder.Entity<ItemAttribute>().ToTable(nameof(ItemAttributes));
            modelBuilder.ApplyConfiguration<ItemAttribute>(new ItemAttributeModelConfig());
            modelBuilder.Entity<ItemAttributeVariety>().ToTable(nameof(ItemAttributeVarieties));
            modelBuilder.ApplyConfiguration<ItemAttributeVariety>(new ItemAttributeVarietyModelConfig());
            modelBuilder.Entity<ActiveItemAttribute>().ToTable(nameof(ActiveItemAttributes));
            modelBuilder.ApplyConfiguration<ActiveItemAttribute>(new ActiveItemAttributeModelConfig());
            modelBuilder.Entity<ActiveItemAttributeVariety>().ToTable(nameof(ActiveItemAttributeVarieties));
            modelBuilder.ApplyConfiguration<ActiveItemAttributeVariety>(new ActiveItemAttributeVarietyModelConfig());
            modelBuilder.Entity<ItemUoM>().ToTable(nameof(ItemUoMs));
            modelBuilder.ApplyConfiguration<ItemUoM>(new ItemUoMMappingConfig());
            //modelBuilder.Entity<ItemGroup>().ToTable(nameof(ItemGroups));
            //modelBuilder.ApplyConfiguration<ItemGroup>(new ItemGroupModelConfig());
            ////modelBuilder.Entity<UsedItemGroup>().ToTable(nameof(UsedItemGroups));
            #endregion

            #region WooStuff
            modelBuilder.Entity<WooSettings>().ToTable(nameof(WooSettings));
            modelBuilder.Entity<WooSyncLog>().ToTable(nameof(WooSyncLogs));

            modelBuilder.Entity<WooCategoryMap>().ToTable(nameof(WooCategoryMaps));
            modelBuilder.Entity<WooProductAttributeMap>().ToTable(nameof(WooProductAttributeMappings));
            modelBuilder.Entity<WooProductAttributeTermMap>().ToTable(nameof(WooProductAttributeTermMappings));
            modelBuilder.ApplyConfiguration<WooProductAttributeTermMap>(new WooProductAttributeTermMappingConfig());
            #endregion
        }
    }
}