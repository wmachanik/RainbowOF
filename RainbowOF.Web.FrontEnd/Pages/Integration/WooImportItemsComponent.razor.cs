using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models.Classes;
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
    public partial class WooImportItemsComponent : ComponentBase
    {
        [Parameter]
        public WooSettings WooSettingsModel { get; set; }
        [Parameter]
        public ILoggerManager Logger { get; set; }

        [Inject]
        public IAppUnitOfWork _AppUnitOfWork { get; set; }

        // common area for variables used in class
        public bool _IsCatWaiting = false;
        public bool _IsAttribWaiting = false;
        public bool _IsAttribTermWaiting = false;
        public int _totalRecsImported = 0;
        public int _PercentOfRecsImported = 0;  // progress bar uses percentage not tota rec imports so need to convert
        public int _maxRecs = 0;
        public string ImportingThis = String.Empty;
        protected ShowModalMessage ShowModalStatus { get; set; }
        // Private vars
        private DateTime _LogDate = DateTime.Now;

        #region GenericRoutines

        // Dispay Import Status if annof the imports are happening
        public bool IsImporting()
        {
            return (_IsCatWaiting || _IsAttribWaiting || _IsAttribTermWaiting);
        }
        // Log the status of the import
        async Task LogImport(int pIdImported, string pParameter, Models.WooSections pSection)
        {
            IAppRepository<WooSyncLog> _WooSyncLogRepository = _AppUnitOfWork.Repository<WooSyncLog>();
            await _WooSyncLogRepository.AddAsync(new WooSyncLog
            {
                // add the parameters
                WooSyncDateTime = _LogDate,
                Result = (pIdImported > 0) ? Models.WooResults.Success : Models.WooResults.Error,
                Parameters = pParameter,
                Section = pSection,
                Notes = $"Imported id: {pIdImported}, dt: {DateTime.Now:d}"
            });
        }

        private void InitWooImport(ref bool pIsWiaiting, string pImportThis)
        {
            // do the same as with categories but now with product AttributeTerm? 
            // Shhoud each one be a class. The issues with that is that, to update the screen is  problem
            pIsWiaiting = true;
            _totalRecsImported = 0;
            _PercentOfRecsImported = 0;
            _maxRecs = 10;   // so it displays - will change later
            _LogDate = DateTime.Now; // set here so all records for this import are the same DateTime.
            ImportingThis = pImportThis; 
            StateHasChanged();
        }
        #endregion
        /// <summary>
        /// All the category import stuff. Could we have generalised this?
        /// </summary>
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
        #region ItemCategoryImportStuff
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
        async Task<Guid> UpdateItemCategory(ProductCategory pPC, ItemCategory pItemCategory, List<WooItemWithParent> pAttribsWithParents)
        {
            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();

            pItemCategory.ItemCategoryName = pPC.name;
            // do not set parent Id here since it could cause datbase problems - if it is already null then it will be updated later.
            // pItemCategory.ParentCategoryId = Guid.Empty;   /// need to find the  parentid if it exists - or need to say it does not exists so that we can look later?
            if (pPC.parent > 0)
            {
                pAttribsWithParents.Add(new WooItemWithParent
                {
                    ChildId = (int)pPC.id,
                    ParentId = (int)pPC.parent
                });
            }
            pItemCategory.Notes = $"Updated Woo Category ID {pPC.id}";
            bool _success = await _ItemCategoryRepository.UpdateAsync(pItemCategory) != AppUnitOfWork.CONST_WASERROR ;
            return (_success) ? pItemCategory.ItemCategoryId: Guid.Empty;
        }
        async Task<Guid> AddOrGetIDItemCategory(ProductCategory pPC, List<WooItemWithParent> pAttribsWithParents)
        {
            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();

            ItemCategory _ItemCategory = await _ItemCategoryRepository.FindFirstAsync(ic => ic.ItemCategoryName == pPC.name);
            if (_ItemCategory == null)
            {
                ItemCategory _newItemCategory = new ItemCategory
                {
                    ItemCategoryName = pPC.name,
                    //  Null;? ParentCategoryId = Guid.Empty,
                    Notes = $"Imported Woo Category ID {pPC.id}"
                };
                if (pPC.parent > 0)
                {
                    pAttribsWithParents.Add(new WooItemWithParent
                    {
                        ChildId = (int)pPC.id,
                        ParentId = (int)pPC.parent
                    });
                }

                await _ItemCategoryRepository.AddAsync(_newItemCategory);
                return _newItemCategory.ItemCategoryId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        async Task<Guid> AddOrUpdateItemCategory(ProductCategory pPC, Guid pWooMappedItemCategoryId, List<WooItemWithParent> pAttribsWithParents)
        {
            Guid _ItemCategoryId = Guid.Empty;
            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();
            // check if the category existgs
            ItemCategory _ItemCategory = await _ItemCategoryRepository.FindFirstAsync(ic => ic.ItemCategoryId == pWooMappedItemCategoryId);
            if (_ItemCategory != null)
            {
                _ItemCategoryId = await UpdateItemCategory(pPC, _ItemCategory, pAttribsWithParents);
            }
            else
            {
                _ItemCategoryId = await AddOrGetIDItemCategory(pPC, pAttribsWithParents);
            }
            return _ItemCategoryId;
        }
        async Task<Guid> UpdateProductCategory(ProductCategory pPC, WooCategoryMap pWooCategoryMap, List<WooItemWithParent> pAttribsWithParents)
        {
            Guid _ItemCategoryId = Guid.Empty;
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // copy data across
            pWooCategoryMap.WooCategoryName = pPC.name;
            pWooCategoryMap.WooCategorySlug = pPC.slug;
            pWooCategoryMap.WooCategoryParentId = pPC.parent;
            _ItemCategoryId = await AddOrUpdateItemCategory(pPC, pWooCategoryMap.ItemCategoryId, pAttribsWithParents);
            if (_ItemCategoryId != Guid.Empty)
            {
                /// Now update the woo categorY using the _ItemCategoryId returned.
                if (await _WooCategoryMapRepository.UpdateAsync(pWooCategoryMap) == AppUnitOfWork.CONST_WASERROR)
                {
                    // did not updated so set _ItemCategoryId to ItemCategoryID to Guid.Empty = error
                    _ItemCategoryId = Guid.Empty;
                }
            }
            return _ItemCategoryId;
        }
        async Task<Guid> AddProductCategory(ProductCategory pPC, WooCategoryMap pWooCategoryMap, List<WooItemWithParent> pAttribsWithParents)
        {
            Guid _ItemCategoryId = Guid.Empty;
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Add Item Category if it does not exist
            _ItemCategoryId = await AddOrGetIDItemCategory(pPC, pAttribsWithParents);
            if (pWooCategoryMap == null)
            {
                pWooCategoryMap = new WooCategoryMap
                {
                    WooCategoryName = pPC.name,
                    WooCategorySlug = pPC.slug,
                    WooCategoryParentId = pPC.parent,
                    ItemCategoryId = _ItemCategoryId,
                    WooCategoryId = (int)pPC.id
                };
            }
            else
            {
                pWooCategoryMap.WooCategoryName = pPC.name;
                pWooCategoryMap.WooCategorySlug = pPC.slug;
                pWooCategoryMap.WooCategoryParentId = pPC.parent;
                pWooCategoryMap.ItemCategoryId = _ItemCategoryId;
                pWooCategoryMap.WooCategoryId = (int)pPC.id;
            }
            if (await _WooCategoryMapRepository.AddAsync(pWooCategoryMap) == AppUnitOfWork.CONST_WASERROR)
            {
                // did not add so set _ItemCategoryId to ItemCategoryID to Guid.Empty = error
                _ItemCategoryId = Guid.Empty;
            }

            return _ItemCategoryId;
        }
        async Task<Guid> ImportAndMapCategoryData(ProductCategory pPC, List<WooItemWithParent> pAttribsWithParents)
        {
            Guid _ItemCategoryId = Guid.Empty;
            // Get repostiory for each database we are accessing. ItemCategory. WooProductCategoryMap & WooSyncLog
            IAppRepository<WooCategoryMap> _WooCategoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            // Import the category and set sync data
            ///first check if it exists in the mapping, just incase there has been a name change
            WooCategoryMap _WooCategoryMap = await _WooCategoryMapRepository.FindFirstAsync(wpc => wpc.WooCategoryId == pPC.id);
            if (_WooCategoryMap != null)                  // the id exists so update
            {
                _ItemCategoryId = await UpdateProductCategory(pPC, _WooCategoryMap, pAttribsWithParents);
            }
            else                  // the id does not exists so add
            {
                _ItemCategoryId = await AddProductCategory(pPC, _WooCategoryMap, pAttribsWithParents);
            }

            return _ItemCategoryId;
        }
        // string
        string ProductCatToString(ProductCategory pPC, Guid pImportedId)
        {
            return $"Product Category {pPC.name}, id: {pPC.id}, imported and Categore Id is {pImportedId}";
        }

        async Task<Guid> GetCategoryId(int pAttribId)
        {
            // using hte category woo category mapping find the assocated ID
            IAppRepository<WooCategoryMap> _WooCatrgoryMapRepository = _AppUnitOfWork.Repository<WooCategoryMap>();

            WooCategoryMap _WooCategoryMap = await _WooCatrgoryMapRepository.FindFirstAsync(wc => wc.WooCategoryId == pAttribId);
            return  (_WooCategoryMap == null) ? Guid.Empty : _WooCategoryMap.ItemCategoryId;

        }
        async Task<bool> SetParentCategory(Guid pChildId, Guid pParentId)
        {
            // using the guids of the category id update the parent of that record
            bool _IsUpdated = false;

            if ((pChildId != Guid.Empty) && (pParentId != Guid.Empty))
            {
                IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();

                ItemCategory _ItemCategory = await _ItemCategoryRepository.FindFirstAsync(ic => ic.ItemCategoryId == pChildId);
                if (_ItemCategory == null)
                    _IsUpdated = false;
                else
                {     // found so update
                    _ItemCategory.ParentCategoryId = pParentId;
                    _IsUpdated = (await _ItemCategoryRepository.UpdateAsync(_ItemCategory)) != AppUnitOfWork.CONST_WASERROR;
                }
            }
            return _IsUpdated;
        }

        async Task<bool> FindAndSetParentCategory(WooItemWithParent pAttribWithParent)
        {
            ///Logic using the ids passed look for the linked attribute to the id then look for the parentid  get the guids of each and update thed atabase
            // Get pAttributeWithAParent.ID GUID from ItemAttribute Table = ParentID
            // Get pAttributeWithAParent.AttrID GUID from ItemAttribute Table = ChildID
            // Set the  ItemAttribute.ParentID = ParentID for ItemsAttrib.ID = ChildID
            Guid _AttribId = await GetCategoryId(pAttribWithParent.ChildId);
            Guid _ParentAttribId = await GetCategoryId(pAttribWithParent.ParentId);

            bool _IsDone = await SetParentCategory(_AttribId, _ParentAttribId);

            await LogImport(pAttribWithParent.ChildId, $"Setting of Parent of Child Category id: {pAttribWithParent.ChildId} to Parent Id {pAttribWithParent.ParentId} status: {_IsDone}", Models.WooSections.ProductCategories);
            return _IsDone;
        }

        // cycle through catagories and add to database if they do not exists
        // Store a WooReultsDate so we can filter the results later
        // log each category and what we do with t in the log and in the WooResults
        async Task<int> ImportCategoryData(List<ProductCategory> pWooProductCategories)
        {
            int _Imported = 0;
            Guid _IdImported;
            //            IAppRepository<ItemCategory> _ItemCategoryRepository = _AppUnitOfWork.Repository<ItemCategory>();
            List<WooItemWithParent> AttribsWithParents = new List<WooItemWithParent>();

            //// Load the current itemCategoriers
            // cycle through catagories and add to database if they do not exists
            foreach (var pc in pWooProductCategories)
            {
                ImportingThis = $"Importing Category ({_totalRecsImported}/{_maxRecs}): {pc.name}";
                // Import the categories that have count  > 0
                if (pc.count > 0)
                {
                    // set the values as per
                    _IdImported = await ImportAndMapCategoryData(pc, AttribsWithParents);
                    _Imported++;
                    await LogImport((int)pc.id, ProductCatToString(pc, _IdImported), Models.WooSections.ProductCategories);
                }
                if (_AppUnitOfWork.IsInErrorState())
                    return 0;
                _totalRecsImported++;
                _PercentOfRecsImported = Convert.ToInt32(Math.Round((_totalRecsImported / (double)_maxRecs) * 100, 0));
                StateHasChanged();
            }
            // Now we loop through all the Attribues that have parents and find them
            foreach (var AttributeWithAParent in AttribsWithParents)
            {
                if (!await FindAndSetParentCategory(AttributeWithAParent))
                {
                    if (_AppUnitOfWork.IsInErrorState())   // was there an error that was database related?
                        return 0;
                }
            }
            return _Imported;
        }
        #endregion
        public async Task ImportCategories_Click()
        {
            // Retrieve data from Woo
            // cycle through catagories and add to database if they do not exists
            InitWooImport(ref _IsCatWaiting,"Categories: Checking Woo");
            List<ProductCategory> _WooProductCategories = await GetWooCategoryData();
            if (_WooProductCategories == null)
            {
                ShowModalStatus.UpdateModalMessage($"Error retrieving Woo Categories. Please checkWoo API settings, or if there are any Woo Categories. View log for details.");
            }
            else
            {
                _maxRecs = _WooProductCategories.Count;
                StateHasChanged();
                int _recsImported = await ImportCategoryData(_WooProductCategories);
                if ((_recsImported == AppUnitOfWork.CONST_WASERROR) && (_AppUnitOfWork.IsInErrorState()))     // returned in there was an error importing
                    ShowModalStatus.UpdateModalMessage($"Error Importing Categories: {_AppUnitOfWork.GetErrorMessage()}");
                else
                    ShowModalStatus.UpdateModalMessage($"Woo categories imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {_totalRecsImported} of {_WooProductCategories.Count} of which {_recsImported} have products.");
            }
            StateHasChanged();
            ShowModalStatus.ShowModal();
            _IsCatWaiting = false;
        }

        /// <summary>
        /// All the atribute import stuff. Could we have generalised this?
        /// </summary>
        #region AttrbiuteStuff

        // Retrieve data from Woo
        async Task<List<ProductAttribute>> GetWooAttributeData()
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

            WooProductAttribute _WooProductAttribute = new WooProductAttribute(_WooAPISettings, Logger);
            List<ProductAttribute> wooProductAttributes = await _WooProductAttribute.GetAllProductAttributes();
            return wooProductAttributes;
        }
        async Task<Guid> UpdateItemAttribute(ProductAttribute pPA, ItemAttribute pItemAttribute)
        {
            IAppRepository<ItemAttribute> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttribute>();
            pItemAttribute.AttributeName = pPA.name;
            pItemAttribute.Notes = $"Updated Woo Attribute ID {pPA.id}";
            return (await _ItemAttributeRepository.UpdateAsync(pItemAttribute) != AppUnitOfWork.CONST_WASERROR) ?  pItemAttribute.ItemAttributeId : Guid.Empty;  // there was an error updating

        }
        async Task<Guid> AddOrGetIDItemAttribute(ProductAttribute pPA)
        {
            IAppRepository<ItemAttribute> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttribute>();

            ItemAttribute _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.AttributeName == pPA.name);
            if (_ItemAttribute == null)
            {
                ItemAttribute _newItemAttribute = new ItemAttribute
                {
                    AttributeName = pPA.name,
                    Notes = $"Imported Woo Attribute ID {pPA.id}"
                };

                return (await _ItemAttributeRepository.AddAsync(_newItemAttribute) != AppUnitOfWork.CONST_WASERROR) ? _newItemAttribute.ItemAttributeId : Guid.Empty;
            }
            else
            {
                return _ItemAttribute.ItemAttributeId;   // we found one with the same name so assume this is the correct one.
            }
        }

        async Task<Guid> AddOrUpdateItemAttribute(ProductAttribute pPA, Guid pWooMappedItemAttributeId)
        {
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<ItemAttribute> _ItemAttributeRepository = _AppUnitOfWork.Repository<ItemAttribute>();
            // check if the Attribute existgs
            ItemAttribute _ItemAttribute = await _ItemAttributeRepository.FindFirstAsync(ic => ic.ItemAttributeId == pWooMappedItemAttributeId);
            if (_ItemAttribute != null)
            {
                _ItemAttributeId = await UpdateItemAttribute(pPA, _ItemAttribute);
            }
            else
            {
                _ItemAttributeId = await AddOrGetIDItemAttribute(pPA);
            }
            return _ItemAttributeId;
        }
        async Task<Guid> UpdateProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        {
            // we have found a mapping between the woo Product Attribute and our Attribute id so update the Attrbiute table just incase.
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<WooProductAttributeMap> _WooProductAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            _ItemAttributeId = await AddOrUpdateItemAttribute(pPA, pWooAttributeMap.ItemAttributeId);
            /// Now update the woo Attribute using the _ItemAttributeId returned.
            return (await _WooProductAttributeMapRepository.UpdateAsync(pWooAttributeMap) != AppUnitOfWork.CONST_WASERROR) ? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error
        }
        async Task<Guid> AddProductAttribute(ProductAttribute pPA, WooProductAttributeMap pWooAttributeMap)
        {
            Guid _ItemAttributeId = Guid.Empty;
            IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            // Add Item Attribute if it does not exist
            _ItemAttributeId = await AddOrGetIDItemAttribute(pPA);
            if (pWooAttributeMap == null)
            {
                pWooAttributeMap = new WooProductAttributeMap
                {
                    WooProductAttributeId = (int)pPA.id,
                    ItemAttributeId = _ItemAttributeId
                };
            }
            else
            {
                pWooAttributeMap.WooProductAttributeId = (int)pPA.id;
                pWooAttributeMap.ItemAttributeId = _ItemAttributeId;
            }
            // return Id if we update okay
            return (await _WooAttributeMapRepository.AddAsync(pWooAttributeMap) != AppUnitOfWork.CONST_WASERROR)? _ItemAttributeId : Guid.Empty;      // did not updated so set _ItemAttributeId to ItemAttributeID to Guid.Empty = error== 0)
        }
        async Task<Guid> ImportAndMapAttributeData(ProductAttribute pPC)
        {
            Guid _ItemAttributeId = Guid.Empty;
            // Get repostiory for each database we are accessing. ItemAttribute. WooProductAttributeMap & WooSyncLog
            IAppRepository<WooProductAttributeMap> _WooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            // Import the Attribute and set sync data
            ///first check if it exists in the mapping, just incase there has been a name change
            WooProductAttributeMap _WooAttributeMap = await _WooAttributeMapRepository.FindFirstAsync(wa => wa.WooProductAttributeId == pPC.id);
            if (_WooAttributeMap != null)   // the id exists so update
            {
                _ItemAttributeId = await UpdateProductAttribute(pPC, _WooAttributeMap);
            }
            else      // the id does not exists so add
            {
                _ItemAttributeId = await AddProductAttribute(pPC, _WooAttributeMap);
            }

            return _ItemAttributeId;
        }
        // string
        string ProductAttributeToString(ProductAttribute pPA, Guid pImportedId)
        {
            return $"Product Attribute {pPA.name}, id: {pPA.id}, imported and Attribute Id is {pImportedId}";
        }
        // 1. Cycle through catagories and add to database if they do not exists - storing a WooReultsDate so we can filter the results later - ?
        // 3. Log each Attribute and what we do with t in the log and in the WooResults
        async Task<bool> ImportAttributeData(List<ProductAttribute> pWooProductAttributes)
        {
            _totalRecsImported = 0;
            Guid _IdImported = Guid.Empty;
            // cycle through catagories and add to database if they do not exists
            foreach (var pa in pWooProductAttributes)
            {
                ImportingThis = $"Importing Attribute ({_totalRecsImported}/{_maxRecs}): {pa.name}";
                // Import all Attributes since Woo does not signal if they are used we need to import all.
                _IdImported = await ImportAndMapAttributeData(pa);
                if (_IdImported == Guid.Empty)
                {
                    return false;
                }
                _totalRecsImported++;
                _PercentOfRecsImported = Convert.ToInt32(Math.Round((_totalRecsImported / (double)_maxRecs) * 100, 0));
                StateHasChanged();
                await LogImport((int)pa.id, ProductAttributeToString(pa, _IdImported), Models.WooSections.ProductAttributes);
            }
            return true; // if we get here no errors occured
        }

        #endregion
        public async Task ImportAttrib_Click()
        {
            InitWooImport(ref _IsAttribWaiting,"Attributes: Checking Woo");
            List<ProductAttribute> _WooProductAttributes = await GetWooAttributeData();
            if (_WooProductAttributes == null)
            {
                ShowModalStatus.UpdateModalMessage($"Woo Attributes retrieval error. Please checkWoo API settings. View log for details.");
            }
            else
            {
                _maxRecs = _WooProductAttributes.Count;
                StateHasChanged();
                if (!await ImportAttributeData(_WooProductAttributes) && _AppUnitOfWork.IsInErrorState())   // we had a problem importing
                {
                    ShowModalStatus.UpdateModalMessage($"Error importing Woo Attributes: {_AppUnitOfWork.GetErrorMessage()}");
                }
                else
                    ShowModalStatus.UpdateModalMessage($"Woo Attributes imported: {Environment.NewLine}{Environment.NewLine} Total Records imported: {_totalRecsImported} of {_WooProductAttributes.Count}.");
            }
            StateHasChanged();
            ShowModalStatus.ShowModal();
            _IsAttribWaiting = false;
        }
        /// <summary>
        /// All the atribute term import stuff. Attributes Terms in Woo are Attributed Varieties to us. Could we have generalised this for ezch item import with an object?
        /// </summary>
        #region AttrbiuteStuff

        // Retrieve data from Woo
        async Task<List<ProductAttributeTerm>> GetWooAttributeTermData(ProductAttribute pProductAttribute)
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

            WooProductAttributeTerm _WooProductAttributeTerm = new WooProductAttributeTerm(_WooAPISettings, Logger);
            List<ProductAttributeTerm> wooProductAttributeTerms = await _WooProductAttributeTerm.GetAttributeTermsByAtttribute(pProductAttribute);
            return wooProductAttributeTerms;
        }
        async Task<Guid> UpdateItemAttributeVariety(ProductAttributeTerm pPAT, Guid pParentAttributeId, ItemAttributeVariety pItemAttributeVariety)
        {
            IAppRepository<ItemAttributeVariety> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVariety>();
            bool _success = false;
            pItemAttributeVariety.ItemAttributeId = pParentAttributeId;
            pItemAttributeVariety.VarietyName = pPAT.name;
            pItemAttributeVariety.Notes = $"Updated Woo AttributeTerm ID {pPAT.id}";
            _success = await _ItemAttributeVarietyRepository.UpdateAsync(pItemAttributeVariety) != AppUnitOfWork.CONST_WASERROR;
            return (_success ? pItemAttributeVariety.ItemAttributeVarietyId : Guid.Empty);
        }
        async Task<Guid> AddOrGetIDItemAttributeVariety(ProductAttributeTerm pPAT, Guid pParentAttributeId)
        {
            IAppRepository<ItemAttributeVariety> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVariety>();

            ItemAttributeVariety _ItemAttributeVariety = await _ItemAttributeVarietyRepository.FindFirstAsync(ic => ic.VarietyName == pPAT.name);
            if (_ItemAttributeVariety == null)
            {
                ItemAttributeVariety _newItemAttributeVariety = new ItemAttributeVariety
                {
                    ItemAttributeId = pParentAttributeId,
                    VarietyName = pPAT.name,
                    Notes = $"Imported Woo Attribute Term ID {pPAT.id}"
                };

                int _recsAdded = await _ItemAttributeVarietyRepository.AddAsync(_newItemAttributeVariety);
                return (_recsAdded != AppUnitOfWork.CONST_WASERROR) ? _newItemAttributeVariety.ItemAttributeVarietyId : Guid.Empty;
            }
            else
            {
                return _ItemAttributeVariety.ItemAttributeVarietyId;   // we found one with the same name so assume this is the correct one.
            }
        }

        async Task<Guid> AddOrUpdateItemAttributeVariety(ProductAttributeTerm pPAT, Guid pParentAttributeId, Guid pWooMappedItemAttributeTermId)
        {
            Guid _ItemAttributeVarietyId = Guid.Empty;
            IAppRepository<ItemAttributeVariety> _ItemAttributeVarietyRepository = _AppUnitOfWork.Repository<ItemAttributeVariety>();
            // check if the AttributeTerm existgs
            ItemAttributeVariety _ItemAttributeVariety = await _ItemAttributeVarietyRepository.FindFirstAsync(ic => ic.ItemAttributeVarietyId == pWooMappedItemAttributeTermId);
            if (_ItemAttributeVariety != null)
            {
                _ItemAttributeVarietyId = await UpdateItemAttributeVariety(pPAT, pParentAttributeId, _ItemAttributeVariety);
            }
            else
            {
                _ItemAttributeVarietyId = await AddOrGetIDItemAttributeVariety(pPAT, pParentAttributeId);
            }
            return _ItemAttributeVarietyId;
        }
        async Task<Guid> UpdateProductAttributeTerm(ProductAttributeTerm pPAT, Guid pParentAttributeId, WooProductAttributeTermMap pWooAttributeTermMap)
        {
            // we have found a mapping between the woo Product AttributeTerm and our AttributeTerm id so update the Attrbiute table just incase.
            Guid _ItemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _WooProductAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            _ItemAttributeTermId = await AddOrUpdateItemAttributeVariety(pPAT, pParentAttributeId, pWooAttributeTermMap.ItemAttributeVarietyId);
            /// Now update the woo AttributeTerm using the _ItemAttributeTermId returned.
            if (await _WooProductAttributeTermMapRepository.UpdateAsync(pWooAttributeTermMap) == AppUnitOfWork.CONST_WASERROR)
            {   // did not updated so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _ItemAttributeTermId = Guid.Empty;
            }

            return _ItemAttributeTermId;
        }
        async Task<Guid> AddProductAttributeTerm(ProductAttributeTerm pPAT, Guid pParentAttributeId, WooProductAttributeTermMap pWooAttributeTermMap)
        {
            Guid _ItemAttributeTermId = Guid.Empty;
            IAppRepository<WooProductAttributeTermMap> _WooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            // Add Item AttributeTerm if it does not exist
            _ItemAttributeTermId = await AddOrGetIDItemAttributeVariety(pPAT, pParentAttributeId);
            if (pWooAttributeTermMap == null)
            {
                pWooAttributeTermMap = new WooProductAttributeTermMap
                {
                    WooProductAttributeTermId = (int)pPAT.id,
                    ItemAttributeVarietyId = _ItemAttributeTermId
                };
            }
            else
            {
                pWooAttributeTermMap.WooProductAttributeTermId = (int)pPAT.id;
                pWooAttributeTermMap.ItemAttributeVarietyId = _ItemAttributeTermId;
            }
            if (await _WooAttributeTermMapRepository.AddAsync(pWooAttributeTermMap) == AppUnitOfWork.CONST_WASERROR)
            {
                // did not add so set _ItemAttributeTermId to ItemAttributeTermID to Guid.Empty = error
                _ItemAttributeTermId = Guid.Empty;
            }

            return _ItemAttributeTermId;
        }
        async Task<Guid> ImportAndMapAttributeTermData(ProductAttributeTerm pPAT, Guid pParentAttributeId)
        {
            Guid _ItemAttributeTermId = Guid.Empty;
            // Get repostiory for each database we are accessing. ItemAttributeTerm. WooProductAttributeTermMap & WooSyncLog
            IAppRepository<WooProductAttributeTermMap> _WooAttributeTermMapRepository = _AppUnitOfWork.Repository<WooProductAttributeTermMap>();

            // Import the AttributeTerm and set sync data
            ///first check if it exists in the mapping, just incase there has been a name change
            WooProductAttributeTermMap _WooAttributeTermMap = await _WooAttributeTermMapRepository.FindFirstAsync(wa => wa.WooProductAttributeTermId == pPAT.id);
            if (_WooAttributeTermMap != null)   // the id exists so update
            {
                _ItemAttributeTermId = await UpdateProductAttributeTerm(pPAT, pParentAttributeId, _WooAttributeTermMap);
            }
            else      // the id does not exists so add
            {
                _ItemAttributeTermId = await AddProductAttributeTerm(pPAT, pParentAttributeId, _WooAttributeTermMap);
            }

            return _ItemAttributeTermId;
        }
        // Get the Variety's Parent id using the WooMapping, if is not found then return Empty
        private Guid GetVarietysParentAttributeID(int? pParentId)
        {
            IAppRepository<WooProductAttributeMap> _WooProductAttributeMapRepo = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            WooProductAttributeMap _WooProductAttributeMap = _WooProductAttributeMapRepo.FindFirst(wpa => wpa.WooProductAttributeId == pParentId);
            return (_WooProductAttributeMap == null) ? Guid.Empty : _WooProductAttributeMap.ItemAttributeId;

        }
        string ProductAttributeTermToString(ProductAttributeTerm pPAT, Guid pImportedId)
        {
            return $"Product Attribute Term {pPAT.name}, id: {pPAT.id}, imported and Categore Id is {pImportedId}";
        }
        // 1. Cycle through catagories and add to database if they do not exists - storing a WooReultsDate so we can filter the results later - ?
        // 3. Log each AttributeTerm and what we do with t in the log and in the WooResults

        async Task<bool> ImportAttributeTermData(ProductAttribute pWooProductAttribute)
        {
            Guid _IdImported = Guid.Empty;
            // cycle through catagories and add to database if they do not exists
            List<ProductAttributeTerm> _WooProductAttributeTerms = await GetWooAttributeTermData(pWooProductAttribute);
            if (_WooProductAttributeTerms == null)
            {
                await LogImport(0, $"Attribute {(int)pWooProductAttribute.id}- has no attribute terms, so none imported", Models.WooSections.ProductAttributeTerms);
            }
            else
            {
                ImportingThis = $"Importing Terms for Attribute ({_totalRecsImported}/{_maxRecs}): {pWooProductAttribute.name} with {_WooProductAttributeTerms.Count} terms ";
                Guid _VarietysParentAttributeID = GetVarietysParentAttributeID(pWooProductAttribute.id);
                if (_VarietysParentAttributeID == Guid.Empty)
                    await LogImport(0, $"Product Attribute of it {(int)pWooProductAttribute.id} appears not to have been imported, so cannot import terms. Please import or check log.", Models.WooSections.ProductAttributeTerms);
                else
                {
                    int _TermsDone = 0;
                    foreach (var PAT in _WooProductAttributeTerms)
                    {
                        // Import all Attribute Terms since Woo does not signal if they are used we need to import all.
                        _IdImported = await ImportAndMapAttributeTermData(PAT, _VarietysParentAttributeID);
                        if (_IdImported == Guid.Empty)
                            return false;
                        await LogImport((int)PAT.id, ProductAttributeTermToString(PAT, _IdImported), Models.WooSections.ProductAttributeTerms);
                        _TermsDone++;
                        _PercentOfRecsImported = Convert.ToInt32(Math.Round(((_totalRecsImported + (_TermsDone / (double)_WooProductAttributeTerms.Count)) / (double)_maxRecs) * 100, 0));
                        StateHasChanged();
                    }
                }
            }
            return true;
        }
        #endregion
        public async Task ImportAttribTerm_Click()
        {
            InitWooImport(ref _IsAttribTermWaiting,"Attribute Terms: checking Woo");
            /// Logic a little different here, since we have to get all the imported attributes, then find all its terms, and then import those terms into the attribute values (terms)
            List<ProductAttribute> _WooProductAttributes = await GetWooAttributeData();
            if (_WooProductAttributes == null)
            {
                ShowModalStatus.UpdateModalMessage($"Woo Attribute retrieval error. To retrieve the terms we need the Attributes. Please check Woo API settings. View log for details.");
            }
            else
            {
                _maxRecs = _WooProductAttributes.Count;
                foreach (ProductAttribute pa in _WooProductAttributes)
                {
                    StateHasChanged();
                    if (!await ImportAttributeTermData(pa))
                        break;
                    _totalRecsImported++;
                    _PercentOfRecsImported = Convert.ToInt32(Math.Round((_totalRecsImported / (double)_maxRecs) * 100, 0));
                    StateHasChanged();
                }
                if (_AppUnitOfWork.IsInErrorState())
                    ShowModalStatus.UpdateModalMessage($"ERROR Importing Woo Attribute Terms: {_AppUnitOfWork.GetErrorMessage()}");
                else
                    ShowModalStatus.UpdateModalMessage($"Woo Attribute Terms imported: {Environment.NewLine}{Environment.NewLine} Total Attributes imported: {_totalRecsImported} of {_WooProductAttributes.Count}.");
            }
            StateHasChanged();
            ShowModalStatus.ShowModal();
            _IsAttribTermWaiting = false;
        }
    }
}


