using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Items;
using RainbowOF.Models.Logs;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Tools;
using RainbowOF.Web.FrontEnd.Pages.ChildComponents.Modals;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using RanbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooImportCategoriesComponent : ComponentBase
    {
        [Parameter]
        public WooSettings WooSettingsModel { get; set; }
        [Parameter]
        public ILoggerManager Logger { get; set; }

        [Inject]
        public IAppUnitOfWork _AppUnitOfWork { get; set; }

        // common area for variables used in class
        public bool _IsWaiting = false;
        public int _recsImported = 0;
        public int _maxRecs = 0;
        protected ShowModalMessage ShowModalStatus { get; set; }


        /* Objective:

            We create a list of item categories. From that list we need to create a list of primary categories. These are used in the items mapping. Only those that are imported can be used
            Mapping:

            The Table WooCategoryMaps stores the category information.

            Field	                                UsedFor
            WooCategoryID [int]	                    To store the CategoryID  that Woo returns
            WooCategoryName [string (size:2-255)]	To store Categories.name that Woo returns
            WooCategorySlug [string (size:2-255)]	To store Categories.slug that Woo returns
            WooCategoryParentID	                    Can be null if not will point to a WooCategoryID
            ItemCategoryID	                        This links to the Category in the system. Allows us to transfer the data.

            Item categories Should be linked to tracking so items in similar categories are tracked similarly.
        */

        // Retrieve data from Woo
        async Task<List<ProductCategory>> GetWooCategoryData()
        {
            WooAPISettings _WooAPISettings = new WooAPISettings
            {
                ConsumerKey = WooSettingsModel.ConsumerKey,
                ConsumerSecret = WooSettingsModel.ConsumerSecret,
                QueryURL = WooSettingsModel.QueryURL,
                IsSecureURL = WooSettingsModel.IsSecureURL,
                JSONAPIPostFix = WooSettingsModel.JSONAPIPostFix,
                RootAPIPostFix = WooSettingsModel.RootAPIPostFix
            };

            WooProductCategory _WooProductCategory = new WooProductCategory(_WooAPISettings, Logger);
            List<ProductCategory> wooProductCategories = await _WooProductCategory.GetAllProductCategories();
            return wooProductCategories;
        }
        async Task<int> UpdateItemCategory(ProductCategory pPC, ItemCategory pItemCategory)
        {
            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();
            bool _success = false;
            pItemCategory.ItemCategoryName = pPC.name;
            pItemCategory.Notes = $"Updated Woo Category ID {pPC.id}";
            _success = await _ItemCategoryRepository.UpdateAsync(pItemCategory) > 0;

            return pItemCategory.ItemCategoryId;
        }
        async Task<int> AddItemCategory(ProductCategory pPC)
        {
            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();

            ItemCategory _newItemCategory = new ItemCategory
            {
                ItemCategoryName = pPC.name,
                Notes = $"Imported Woo Category ID {pPC.id}"
            };

            await _ItemCategoryRepository.AddAsync(_newItemCategory);

            return _newItemCategory.ItemCategoryId;
        }

        async Task<int> AddOrUpdateItemCategory(ProductCategory pPC, int pWooCategoryMapCategoryId)
        {
            int _ItemCategoryId = 0;
            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();
            // check if the category existgs
            ItemCategory _ItemCategory = await _ItemCategoryRepository.FindFirstAsync(ic => ic.ItemCategoryId == pWooCategoryMapCategoryId);
            if (_ItemCategory != null)
            {
                _ItemCategoryId = await UpdateItemCategory(pPC, _ItemCategory);
            }
            else
            {
                _ItemCategoryId = await AddItemCategory(pPC);
            }
            return _ItemCategoryId;
        }
        async Task<int> UpdateProductCategory(ProductCategory pPC, WooCategoryMap pWooCategoryMap)
        {
            int _ItemCategoryId = 0;
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // copy data across
            pWooCategoryMap.WooCategoryName = pPC.name;
            pWooCategoryMap.WooCategorySlug = pPC.slug;
            pWooCategoryMap.WooCategoryParentId = pPC.parent;
            _ItemCategoryId = await AddOrUpdateItemCategory(pPC, pWooCategoryMap.ItemCategoryId);

            ///////
            /// Now update the woo categorY using the _ItemCategoryId returned.
            if (await _WooCategoryMapRepository.UpdateAsync(pWooCategoryMap) == 0)
            {
                // did not updated so set _ItemCategoryId to ItemCategoryID to 0 = error
                _ItemCategoryId = 0;
            }

            return _ItemCategoryId;
        }
        async Task<int> AddProductCategory(ProductCategory pPC, WooCategoryMap pWooCategoryMap)
        {
            int _ItemCategoryId = 0;
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            //// need to write this
            _ItemCategoryId = await AddItemCategory(pPC);
            pWooCategoryMap.WooCategoryName = pPC.name;
            pWooCategoryMap.WooCategorySlug = pPC.slug;
            pWooCategoryMap.WooCategoryParentId = pPC.parent;
            pWooCategoryMap.ItemCategoryId = _ItemCategoryId;

            if (await _WooCategoryMapRepository.AddAsync(pWooCategoryMap) == 0)
            {
                // did not add so set _ItemCategoryId to ItemCategoryID to 0 = error
                _ItemCategoryId = 0;   
            }

            return _ItemCategoryId;
        }
        async Task<int> ImportAndMapCategoryData(ProductCategory pPC)
        {
            int _ItemCategoryId = 0;
            // Get repostiory for each database we are accessing. ItemCategory. WooProductCategoryMap & WooSyncLog
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Import the category and set sync data
            ///first check if it exists in the mapping, just incase there has been a name change
            WooCategoryMap _WooCategoryMap = await _WooCategoryMapRepository.FindFirstAsync(wpc => wpc.WooCategoryId == pPC.id);
            if (_WooCategoryMap != null)
            {
                // the ide exists so update
                _ItemCategoryId = await UpdateProductCategory(pPC, _WooCategoryMap);
            }
            else
            {
                // the ide does not exists so add
                _ItemCategoryId = await AddProductCategory(pPC, _WooCategoryMap);
            }

            return _ItemCategoryId;
        }
        // Log the status of the import
        async Task LogImport(int pIdImported, ProductCategory pPC)
        {
            IAppRepository<WooSyncLog> _WooSyncLogRepository = _AppUnitOfWork.Repository<WooSyncLog>();

            await _WooSyncLogRepository.AddAsync(new WooSyncLog
            {
                // add the parameters
                WooSyncDateTime = DateTime.Now,
                Result = (pIdImported == 0) ? Models.WooResults.Error : Models.WooResults.Success,
                Parameters = $"Product Category {pPC.description}, imported and id is {pIdImported}",
                Section = Models.WooSections.ProductCategories,
                Notes = $"Imported {DateTime.Now:d}"

            });
        }
        // cycle through catagories and add to database if they do not exists
        // Store a WooReultsDate so we can filter the results later
        // log each category and what we do with t in the log and in the WooResults
        async Task<int> ImportCategoryData(List<ProductCategory> pWooProductCategories)
        {
            IAppRepository<ItemCategory> ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();

            // Load the current itemCategoriers
            List<ItemCategory> itemCategories = (List<ItemCategory>)await ItemCategoryRepository.GetAllAsync();

            // cycle through catagories and add to database if they do not exists
            foreach (var pc in pWooProductCategories)
            {
                // Import the categories that have count  > 0
                // set the values as per
                if (pc.count > 0)
                {
                    int _IdImported = await ImportAndMapCategoryData(pc);
                    await LogImport(_IdImported, pc);
                }
            }
            return _recsImported;
        }

        public async Task ImportCategories_Click()
        {
            // Retrieve data from Woo
            // cycle through catagories and add to database if they do not exists
            _IsWaiting = true;
            StateHasChanged();
            List<ProductCategory> _WooProductCategories = await GetWooCategoryData();
            _maxRecs = _WooProductCategories.Count;
            _recsImported = await ImportCategoryData(_WooProductCategories);
            StateHasChanged();
            ShowModalStatus.UpdateModalMessage($"Woo categories imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {_recsImported} of {_WooProductCategories.Count}.");
            ShowModalStatus.ShowModal();
        }

    }
}


