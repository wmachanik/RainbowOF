using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using RainbowOF.Models.System;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace RainbowOF.Services
{
    public class ApplicationDbContext : DbContext // IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //// accounts
        //#region AccDBStuff
        //public DbSet<InvoiceType> InvoiceTypes { get; set; }
        //public DbSet<PaymentTerm> PaymentTerms { get; set; }
        //public DbSet<PaymentType> PaymentTypes { get; set; }
        //public DbSet<PriceGroup> PriceGroups { get; set; }
        //public DbSet<PriceLevel> PriceLevels { get; set; }
        //public DbSet<PriceListType> PriceListTypes { get; set; }
        //public DbSet<PriceTypeByPercent> PriceTypeByPercents { get; set; }
        //public DbSet<PriceTypeByValue> PriceTypeByValues { get; set; }
        //public DbSet<VATTaxType> VATTaxTypes { get; set; }
        //#endregion
        //// customers
        //#region CustomerDBStuff
        //public DbSet<Address> Addresses { get; set; }
        //public DbSet<ContactType> ContactTypes { get; set; }
        //public DbSet<Customer> Customers { get; set; }
        //public DbSet<CustomerContact> CustomerContacts { get; set; }
        //public DbSet<CustomerAccountingOptions> CustomerAccountingOptions { get; set; }
        //public DbSet<CustomerEquipment> CustomerEquipment { get; set; }
        //public DbSet<CustomerFulfilmentOptions> CustomerFulfilmentOptions { get; set; }
        //public DbSet<CustomerPostalAddress> CustomerPostalAddresses { get; set; }
        //public DbSet<CustomerShippingAddress> CustomerShippingAddresses { get; set; }
        //public DbSet<CustomerPrefPerType> CustomerPrefPerTypes { get; set; }
        //public DbSet<CustomerType> CustomerTypes { get; set; }
        //#endregion
        //// general
        //#region GeneralDBStuff
        //public DbSet<Area> Areas { get; set; }
        //public DbSet<AreaDaySetting> AreaDaySettings { get; set; }
        //public DbSet<City> Cities { get; set; }
        //public DbSet<DemoEquipment> DemoEquipment { get; set; }
        //public DbSet<DemoEquipmentUsage> DemoEquipmentUsage { get; set; }
        //public DbSet<EquipmentType> EquipmentTypes { get; set; }
        //public DbSet<PostCodeArea> PostCodeAreas { get; set; }
        //public DbSet<Models.General.TimeZone> TimeZones { get; set; }
        //#endregion
        ////Items
        //#region ItemsDBStuff
        //public DbSet<Item> Items { get; set; }
        //public DbSet<ItemPrice> ItemPrices { get; set; }
        //public DbSet<ItemType> ItemTypes { get; set; }
        //public DbSet<ItemUnit> ItemUnits { get; set; }
        //public DbSet<Packaging> Packagings { get; set; }
        //public DbSet<ServiceType> ServiceTypes { get; set; }
        //public DbSet<Variety> Varieties { get; set; }
        //public DbSet<ItemGroup> ItemGroups { get; set; }
        //public DbSet<UsedItemGroup> UsedItemGroups { get; set; }
        //#endregion
        //#region LookupsDBStuff
        //public DbSet<PreferedDeliveryTime> PreferedDeliveryTimes { get; set; }
        //public DbSet<Models.Lookups.WeekDay> WeekDays { get; set; }
        //#endregion
        //#region NotificationsDBStuff
        //public DbSet<NotificationEmailText> NotificationEmailTexts { get; set; }
        //public DbSet<NotificationsSentLog> NotificationsSentLogs { get; set; }
        //public DbSet<NotificationType> NotificationTypes { get; set; }
        //#endregion
        //#region OrdersDBStuff
        //public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderOptions> OrderOptions { get; set; }
        //public DbSet<OrderFulfillment> OrderFulfillments { get; set; }
        //public DbSet<OrderLine> OrderLines { get; set; }
        //public DbSet<OrderMethodType> OrderMethodTypes { get; set; }
        //public DbSet<OrderPaymentOptions> OrderPaymentOptions { get; set; }
        //public DbSet<RecurringOrder> RecurringOrders { get; set; }
        //public DbSet<RecurringOrderItem> RecurringOrderItem { get; set; }
        //public DbSet<RecurringType> RecurringTypes { get; set; }
        //#endregion
        //#region RepairsDBStuff
        //public DbSet<MachineCondition> MachineConditions { get; set; }
        //public DbSet<Repair> Repairs { get; set; }
        //public DbSet<RepairFault> RepairFaults { get; set; }
        //public DbSet<RepairStatus> RepairStatuses { get; set; }
        //#endregion
        #region SystemDBStuff
        public DbSet<ClosureDate> ClosureDates { get; set; }
        //public DbSet<Party> Parties { get; set; }
        public DbSet<SysPrefs> SystemPreferences { get; set; }
        public DbSet<TotalCountTracker> TotalCounterTracker { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema(nameof(iSele));
            base.OnModelCreating(modelBuilder);
            //Add all databases into the model
            //// account database stuff
            //#region AccDBStuff
            //modelBuilder.Entity<InvoiceType>().ToTable(nameof(InvoiceTypes));
            //modelBuilder.ApplyConfiguration<InvoiceType>(new InvoiceTypeModelConfig());
            //modelBuilder.Entity<PaymentTerm>().ToTable(nameof(PaymentTerms));
            //modelBuilder.ApplyConfiguration<PaymentTerm>(new PaymentTermModelConfig());
            //modelBuilder.Entity<PaymentType>().ToTable(nameof(PaymentTypes));
            //modelBuilder.ApplyConfiguration<PaymentType>(new PaymentTypeModelConfig());
            //modelBuilder.Entity<PriceGroup>().ToTable(nameof(PriceGroups));
            //modelBuilder.ApplyConfiguration<PriceGroup>(new PriceGroupModelConfig());
            //modelBuilder.Entity<PriceLevel>().ToTable(nameof(PriceLevels));
            //modelBuilder.ApplyConfiguration<PriceLevel>(new PriceLevelModelConfig());
            //modelBuilder.Entity<PriceListType>().ToTable(nameof(PriceListTypes));
            //modelBuilder.ApplyConfiguration<PriceListType>(new PriceListTypeModelConfig());
            //modelBuilder.Entity<PriceTypeByPercent>().ToTable(nameof(PriceTypeByPercents));
            //modelBuilder.ApplyConfiguration<PriceTypeByPercent>(new PriceTypeByPercentModelConfig());
            //modelBuilder.Entity<PriceTypeByValue>().ToTable(nameof(PriceTypeByValues));
            //modelBuilder.ApplyConfiguration<PriceTypeByValue>(new PriceTypeByValueModelConfig());
            //modelBuilder.Entity<VATTaxType>().ToTable(nameof(VATTaxTypes));
            //modelBuilder.ApplyConfiguration<VATTaxType>(new VATTaxTypeModelConfig());
            //#endregion
            //// customer database stuff
            //#region CustomerDBStuff
            //modelBuilder.Entity<Address>().ToTable(nameof(Addresses));
            //modelBuilder.ApplyConfiguration<Address>(new AddressModelConfig());
            //modelBuilder.Entity<ContactType>().ToTable(nameof(ContactTypes));
            //modelBuilder.ApplyConfiguration<ContactType>(new ContactTypeModelConfig());

            //modelBuilder.Entity<Customer>().ToTable(nameof(Customers));
            //modelBuilder.ApplyConfiguration<Customer>(new CustomerModelConfig());

            //modelBuilder.Entity<CustomerContact>().ToTable(nameof(CustomerContacts));
            //modelBuilder.ApplyConfiguration<CustomerContact>(new CustomerContactModelConfig());

            //modelBuilder.Entity<CustomerAccountingOptions>().ToTable(nameof(CustomerAccountingOptions));
            //modelBuilder.ApplyConfiguration<CustomerAccountingOptions>(new CustomerAccountingOptionsModelConfig());
            //modelBuilder.Entity<CustomerEquipment>().ToTable(nameof(CustomerEquipment));
            //modelBuilder.ApplyConfiguration<CustomerEquipment>(new CustomerEquipmentModelConfig());
            //modelBuilder.Entity<CustomerFulfilmentOptions>().ToTable(nameof(CustomerFulfilmentOptions));

            //modelBuilder.Entity<CustomerPostalAddress>().ToTable(nameof(CustomerPostalAddresses))
            //    .HasKey(cadr => new { cadr.CustomerID, cadr.AddressID });
            //modelBuilder.Entity<CustomerShippingAddress>().ToTable(nameof(CustomerShippingAddresses))
            //    .HasKey(cadr => new { cadr.CustomerID, cadr.AddressID });

            //modelBuilder.Entity<CustomerPrefPerType>().ToTable(nameof(CustomerPrefPerTypes));
            //modelBuilder.ApplyConfiguration<CustomerPrefPerType>(new CustomerPrefPerTypeModelConfig());
            //modelBuilder.Entity<CustomerType>().ToTable(nameof(CustomerTypes))
            //    .HasIndex(ct => ct.CustomerTypeName)
            //    .IsUnique();
            //#endregion

            //// general tables
            //#region GeneralDBStuff
            //modelBuilder.Entity<Area>().ToTable(nameof(Areas));
            //modelBuilder.ApplyConfiguration<Area>(new AreaModelConfig());
            //modelBuilder.Entity<AreaDaySetting>().ToTable(nameof(AreaDaySettings));
            //modelBuilder.ApplyConfiguration<AreaDaySetting>(new AreaDaySettingModelConfig());
            //modelBuilder.Entity<City>().ToTable(nameof(Cities));
            //modelBuilder.ApplyConfiguration<City>(new CityModelConfig());

            //modelBuilder.Entity<DemoEquipment>().ToTable(nameof(DemoEquipment));
            //modelBuilder.ApplyConfiguration<DemoEquipment>(new DemoEquipmentModelConfig());
            //modelBuilder.Entity<DemoEquipmentUsage>().ToTable(nameof(DemoEquipmentUsage));
            //modelBuilder.ApplyConfiguration<DemoEquipmentUsage>(new DemoEquipmentUsageModelConfig());

            //modelBuilder.Entity<EquipmentType>().ToTable(nameof(EquipmentTypes));
            //modelBuilder.ApplyConfiguration<EquipmentType>(new EquipmentTypeModelConfig());
            //modelBuilder.Entity<PostCodeArea>().ToTable(nameof(PostCodeAreas));
            //modelBuilder.ApplyConfiguration<PostCodeArea>(new PostCodeAreaModelConfig());
            //modelBuilder.Entity<Models.General.TimeZone>().ToTable(nameof(TimeZones))
            //    .HasIndex(tz => tz.TimeZoneName)
            //    .IsUnique();
            //#endregion

            //// general tables
            //#region ItemsDBStuff
            //modelBuilder.Entity<Item>().ToTable(nameof(Items));
            //modelBuilder.ApplyConfiguration<Item>(new ItemModelConfig());
            //modelBuilder.Entity<ItemPrice>().ToTable(nameof(ItemPrices));
            //modelBuilder.ApplyConfiguration<ItemPrice>(new ItemPriceModelConfig());
            //modelBuilder.Entity<ItemType>().ToTable(nameof(ItemTypes));
            //modelBuilder.ApplyConfiguration<ItemType>(new ItemTypeModelConfig());
            //modelBuilder.Entity<ItemUnit>().ToTable(nameof(ItemUnits));
            //modelBuilder.ApplyConfiguration<ItemUnit>(new ItemUnitModelConfig());
            //modelBuilder.Entity<Packaging>().ToTable(nameof(Packagings));
            //modelBuilder.ApplyConfiguration<Packaging>(new PackagingModelConfig());
            //modelBuilder.Entity<ServiceType>().ToTable(nameof(ServiceTypes));
            //modelBuilder.ApplyConfiguration<ServiceType>(new ServiceTypeModelConfig());
            //modelBuilder.Entity<Variety>().ToTable(nameof(Varieties));
            //modelBuilder.ApplyConfiguration<Variety>(new VarietyModelConfig());
            //modelBuilder.Entity<ItemGroup>().ToTable(nameof(ItemGroups));
            //modelBuilder.ApplyConfiguration<ItemGroup>(new ItemGroupModelConfig());
            //modelBuilder.Entity<UsedItemGroup>().ToTable(nameof(UsedItemGroups));
            //#endregion
            //#region NotificationsDBStuff
            //modelBuilder.Entity<NotificationEmailText>().ToTable(nameof(NotificationEmailTexts));
            //modelBuilder.Entity<NotificationsSentLog>().ToTable(nameof(NotificationsSentLogs));
            //modelBuilder.ApplyConfiguration<NotificationsSentLog>(new NotificationsSentLogModelConfig());
            //modelBuilder.Entity<NotificationType>().ToTable(nameof(NotificationTypes));
            //modelBuilder.ApplyConfiguration<NotificationType>(new NotificationTypeModelConfig());
            //#endregion
            //#region OrdersDBStuff
            //modelBuilder.Entity<Order>().ToTable(nameof(Orders));
            //modelBuilder.ApplyConfiguration<Order>(new OrderModelConfig());
            //modelBuilder.Entity<OrderOptions>().ToTable(nameof(OrderOptions));
            ////modelBuilder.ApplyConfiguration<OrderOptions>(new OrderOptionsModelConfig());
            //modelBuilder.Entity<OrderFulfillment>().ToTable(nameof(OrderFulfillments));
            //modelBuilder.ApplyConfiguration<OrderFulfillment>(new OrderFulfillmentModelConfig());
            //modelBuilder.Entity<OrderLine>().ToTable(nameof(OrderLines));
            //modelBuilder.ApplyConfiguration<OrderLine>(new OrderLineModelConfig());
            //modelBuilder.Entity<OrderMethodType>().ToTable(nameof(OrderMethodTypes));
            //modelBuilder.ApplyConfiguration<OrderMethodType>(new OrderMethodTypeModelConfig());
            //modelBuilder.Entity<OrderPaymentOptions>().ToTable(nameof(OrderPaymentOptions));
            //modelBuilder.ApplyConfiguration<OrderPaymentOptions>(new OrderPaymentOptionsModelConfig());
            //modelBuilder.Entity<RecurringOrder>().ToTable(nameof(RecurringOrders));
            //modelBuilder.ApplyConfiguration<RecurringOrder>(new RecurringOrderModelConfig());
            //modelBuilder.Entity<RecurringOrderItem>().ToTable(nameof(RecurringOrderItem));
            //modelBuilder.ApplyConfiguration<RecurringOrderItem>(new RecurringOrderItemModelConfig());
            //modelBuilder.Entity<RecurringType>().ToTable(nameof(RecurringTypes));
            //modelBuilder.ApplyConfiguration<RecurringType>(new RecurringTypeModelConfig());
            //#endregion
            //#region RepairsDBStuff
            //modelBuilder.Entity<MachineCondition>().ToTable(nameof(MachineConditions));
            //modelBuilder.ApplyConfiguration<MachineCondition>(new MachineConditionModelConfig());
            //modelBuilder.Entity<Repair>().ToTable(nameof(Repairs));
            //modelBuilder.ApplyConfiguration<Repair>(new RepairModelConfig());
            //modelBuilder.Entity<RepairFault>().ToTable(nameof(RepairFaults));
            //modelBuilder.ApplyConfiguration<RepairFault>(new RepairFaultModelConfig());
            //modelBuilder.Entity<RepairStatus>().ToTable(nameof(RepairStatuses));
            //modelBuilder.ApplyConfiguration<RepairStatus>(new RepairStatusModelConfig());
            //#endregion
            #region SystemDBStuff
            //modelBuilder.Entity<ClosureDate>().ToTable(nameof(ClosureDates))
            //    .HasIndex(cd => cd.EventName)
            //    .IsUnique();
            //modelBuilder.Entity<Party>().ToTable(nameof(Parties));
            //modelBuilder.ApplyConfiguration<Party>(new PartyModelConfig());
            //modelBuilder.Entity<SystemPreferences>().ToTable(nameof(SystemPreferences));
            //modelBuilder.Entity<TotalCountTracker>().ToTable(nameof(TotalCounterTracker));
            #endregion
        }

        //private bool disposed = false;

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!this.disposed)
        //    {
        //        this.Dispose();
        //    }
        //    this.disposed = true;
        //}

        //override public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
    }
}
