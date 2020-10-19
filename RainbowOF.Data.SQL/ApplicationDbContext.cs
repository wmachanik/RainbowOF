using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL.EntityConfigs.Items;
using RainbowOF.Datsa.SQL.EntityConfigs.Items;
using RainbowOF.Models.Items;
using RainbowOF.Models.System;
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
        }
        #region system stuff
        //        public virtual DbSet<Party> Parties { get; set; }
        public virtual DbSet<SysPrefs> SysPrefs { get; set; }
        //        public virtual DbSet<UsageLog> UsageLog { get; set; }
        //        public virtual DbSet<WeekDay> WeekDays { get; set; }
        public virtual DbSet<WooSettings> WooSettings { get; set; }
        public virtual DbSet<ClosureDate> ClosureDates { get; set; }
        #endregion

        #region item stuff
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<ItemAttribute> ItemAttributes { get; set; }
        public DbSet<ItemAttributeVariety> ItemAttributeVarieties { get; set; }
        public DbSet<ActiveItemAttribute> ActiveItemAttributes { get; set; }
        public DbSet<ActiveItemAttributeVariety> ActiveItemAttributeVarieties { get; set; }
        public DbSet<ItemUoM> ItemUoMs { get; set; }

        //public DbSet<ItemUnit> ItemUnits { get; set; }
        //public DbSet<Packaging> Packagings { get; set; }
        //public DbSet<Variety> Varieties { get; set; }
        //public DbSet<ItemGroup> ItemGroups { get; set; }
        ////public DbSet<UsedItemGroup> UsedItemGroups { get; set; }
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
            modelBuilder.Entity<WooSettings>().ToTable(nameof(WooSettings));
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
            modelBuilder.ApplyConfiguration<ItemUoM>(new ItemUoMModelConfig());


            //modelBuilder.Entity<ItemGroup>().ToTable(nameof(ItemGroups));
            //modelBuilder.ApplyConfiguration<ItemGroup>(new ItemGroupModelConfig());
            ////modelBuilder.Entity<UsedItemGroup>().ToTable(nameof(UsedItemGroups));

            #endregion

        }
    }
}