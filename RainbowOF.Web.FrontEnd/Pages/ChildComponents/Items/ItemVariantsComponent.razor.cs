﻿using AutoMapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RainbowOF.Components.Modals;
using RainbowOF.Integration.Repositories.Woo;
using RainbowOF.Models.Items;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Items;
using RainbowOF.Web.FrontEnd.Models.Items;
using RainbowOF.Woo.REST.Models;
using RainbowOF.Woo.REST.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class ItemVariantsComponent : ComponentBase
    {
        #region UI Variables
        // Interface Stuff
        private List<ItemVariant> itemVariants { get; set; } = null;
        //        List<AttributeLookup> PossibleItemVariants = null;
        private ItemVariantView selectedItemVariantView { get; set; }
        private List<ItemVariantView> itemVariantViews { get; set; } = null;
        //private bool canAddVariant = false;
        private bool isItemVariantSaveBusy { get; set; } = false;
        private bool isItemVariantImportBusy { get; set; } = false;
        //        DataGrid<ItemVariant> _ItemVariantsDataGrid;
        // All there workings are here

        // ---> all workings should here need to move them here under a single repo to see if that is the issue with the dbset


        private IItemVariantFormRepository itemVariantViewRepository { get; set; } = null;
        public ConfirmModal ImportVariantsConfirmationModal { get; set; }
        //public ConfirmModal DeleteItemVariantConfirmationModal { get; set; }
        public ConfirmModal AddAllItemVariantsConfirmationModal { get; set; }
        public ConfirmModal DeleteAllItemVariantsConfirmationModal { get; set; }
        #endregion
        #region Injected Variables
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager AppLoggerManager { get; set; }
        [Inject]
        public IMapper AppMapper { get; set; }
        #endregion
        #region Parameters
        [Parameter]
        [EditorRequired]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        [EditorRequired]
        public Guid ParentItemId { get; set; }
        //[Parameter]
        //public bool CanUseAsync { get; set; } = true;  // issues with detail mean that if this is a detail grid we disable Async
        #endregion
        #region Initialisation
        /// <summary>
        /// This is a child component that displays an Items Variants. In the initialisation we need to determine if the Item:
        /// 1. Has attribute that are marked as variants, 
        /// 1.1. if so
        /// 1.1.2. Load the variants
        /// 1.1.3. If not variants to null
        /// 1.2 If no attributes are marked as variants we need to tell the UI that, so it can display item has no variants. Ideally the UI should not display the variant component if no attributes are variable
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("Item Variants Component initialising.");
            // Inits don't seem to like awaits -> so we need to call without an await
            //var _itemVariants = await _itemVariantRepository.GetByAsync(iv => iv.ItemId == ParentItemId);
            itemVariants = await LoadItemVariantAttributesAsync();
            if (itemVariants != null)
            {
                itemVariantViewRepository = new ItemVariantFormRepository(AppLoggerManager, AppUnitOfWork);
                itemVariantViewRepository.CurrFormSettings.PopUpRef = PopUpRef;   // use the forms Pop Up ref that is passed in.
                //await InvokeAsync(StateHasChanged);
            }
            await base.OnInitializedAsync();
            if (AppLoggerManager.IsDebugEnabled()) AppLoggerManager.LogDebug("Item Variants Component initialised.");
        }
        #endregion  
        #region Support Routines
        private async Task<List<ItemVariant>> LoadItemVariantAttributesAsync(bool IsForceReload = false)
        {
            if (itemVariants != null) itemVariants.Clear();
            itemVariants = await AppUnitOfWork.ItemRepository.GetAllItemVariantsEagerByItemIdAsync(ParentItemId); //=> non async seems to be an issue calling more than one await in a total screen init
            if (itemVariants == null)
                return null;
            CheckEachVariableAttributeExists(IsForceReload);
            //CheckWhichVariantsAreUnassigned();
            MapItemVariantToView();
            //canAddVariant = (itemVariants?.Count ?? 0) > 0;
            return itemVariants;
        }
        /// <summary>
        /// Since the database does not always store the attribute if it is Guid.Empty on import we need to see if there are any
        /// </summary>
        private void CheckEachVariableAttributeExists(bool IsForceReload = false)
        {
            List<ItemAttribute> itemsPossibleVariableAttributes = AppUnitOfWork.GetListOfAnItemsVariableAttributes(ParentItemId, IsForceReload);
            // look for the item variant that does not currently have an associated attribute that is linked to an attribute that is marked as a variable. Each Attribute marked as a variable attribute must either have an associated variety or have the associated variable must be null or Guid.empty
            // So we need to search to see if there is none and then add that attribute 
            foreach (ItemVariant itemVariant in itemVariants)
            {
                foreach (ItemAttribute itemAttribute in itemsPossibleVariableAttributes)
                {
                    if (!itemVariant.ItemVariantAssociatedLookups.Exists(ival => ival.AssociatedAttributeLookupId == itemAttribute.ItemAttributeLookupId))
                    {
                        itemVariant.ItemVariantAssociatedLookups.Add(new ItemVariantAssociatedLookup
                        {
                            AssociatedAttributeLookupId = itemAttribute.ItemAttributeLookupId,
                            AssociatedAttributeLookup = itemAttribute.ItemAttributeDetail,
                            AssociatedAttributeVarietyLookupId = Guid.Empty,  // - this should only be for when all varieties apply,
                            AssociatedAttributeVarietyLookup = null
                        });
                    }
                }
                itemVariant.ItemVariantAssociatedLookups = itemVariant
                    .ItemVariantAssociatedLookups
                    .OrderBy(ival => ival.AssociatedAttributeLookup.AttributeName)
                    .ToList();
            }
            //            ItemVariants = ItemVariants.OrderBy(iv => iv.ItemVariantAssociatedLookups.OrderBy(ival => ival.AssociatedAttributeLookupId)).ToList();  // make sure we reorder so all the variants are in the correct order
        }
        /// <summary>
        /// return true if any variant has changed
        /// </summary>
        public bool ItemVariantHasChanged
            => itemVariantViews.Exists(iv => iv.IsModified == true);

        /* not used as to complex, was trying to build a matrix of which attributes where available to which attributes variants
        private void CheckWhichVariantsAreUnassigned()
        {

            
            IItemRepository itemRepository = AppUnitOfWork.itemRepository();
            var allPossibleItemAttributes = itemRepository.GetEagerItemVariableAttributeByItemId(ParentItemId);
            if (PossibleItemVariants == null)
                PossibleItemVariants = new List<AttributeLookup>();
            else
                PossibleItemVariants.Clear();

            foreach (var itemAttribute in allPossibleItemAttributes)
            {
                var possibleItemVariant = new AttributeLookup
                {
                    AttributeLookupId = itemAttribute.ItemAttributeLookupId,
                    AttrbiuteName = itemAttribute.ItemAttributeDetail.AttributeName,
                    AttributeVaraibleLookups = new List<AttributeVariableLookup>()
                };

                foreach (var itemAttibuteVariable in itemAttribute.ItemAttributeVarieties)
                {
                    //if (!ItemVariants.Exists(iv =>
                    //      (iv.ItemVariantAssociatedLookups
                    //          .Exists(iva => iva.AssociatedAttributeLookupId == itemAttribute.ItemAttributeLookupId))
                    //   && (iv.ItemVariantAssociatedLookups
                    //          .Exists(iva => iva.AssociatedAttributeVarietyLookupId == itemAttibuteVariable.ItemAttributeVarietyLookupId))))
                    //{
                    possibleItemVariant.AttributeVaraibleLookups.Add(new AttributeVariableLookup
                    {
                        AttributeVariantLookupId = itemAttibuteVariable.ItemAttributeVarietyLookupId,
                        AttributeVariantName = itemAttibuteVariable.ItemAttributeVarietyDetail.VarietyName
                    });

                }
                PossibleItemVariants.Add(possibleItemVariant);
            }
        }
        */
        /// <summary>
        ///  Return a list of Attribute Varieties that are available for this attribute
        /// </summary>
        /// <param name="parentAttributeLookupId">The parent Attribute for these variants</param>
        /// <param name="currentAttributeVarityLookupId">The one that is currently selected</param>
        /// <param name="mustForce">must the list be reloaded (used for refresh etc.)</param>
        /// <returns>List of item attribute varieties to be used.</returns>
        public List<ItemAttributeVariety> GetListOfAttributeAvailableVarieties(Guid parentAttributeLookupId,
                                                                               /*Guid currentAttributeVarityLookupId,*/
                                                                               bool mustForce = false)
        {
            return AppUnitOfWork.GetListOfAnItemsAttributeVarieties(ParentItemId, parentAttributeLookupId, mustForce);
        }
        /// <summary>
        /// Using the current Item variant data move that date into the view model
        /// </summary>
        private void MapItemVariantToView()
        {
            if (itemVariantViews == null)
                itemVariantViews = new List<ItemVariantView>();
            else
                itemVariantViews.Clear();

            foreach (ItemVariant _itemVariant in itemVariants)
            {
                itemVariantViews.Add(new ItemVariantView
                {
                    TabIsExpanded = false,
                    IsModified = false,
                    ItemVariant = _itemVariant
                }
                );
            }
        }
        #endregion
        #region BackEnd Routines
        public List<ItemAttribute> GetListOfPossibleAttributes(Guid? currentItemAttributeId = null, bool IsForceReload = false)
        {
            List<ItemAttribute> _itemAttributes = AppUnitOfWork.GetListOfAnItemsVariableAttributes(ParentItemId, IsForceReload);
            if (_itemAttributes == null)
                return null; //-- this should not happen
            if (currentItemAttributeId == null)
                return _itemAttributes;  // no item selected 
            // 1.finds all item that are not currently used
            var _usedItems = itemVariants.Where(miav => (miav.ItemVariantAssociatedLookups.Any(ival => ival.AssociatedAttributeLookupId != currentItemAttributeId)));
            // 2. Return the unused ones.
            var _unselectedItems = _itemAttributes.Where(iav => !_usedItems.Any(miav => (miav.ItemVariantAssociatedLookups.Any(ival => ival.AssociatedAttributeLookupId == iav.ItemAttributeLookupId))));
            return _unselectedItems.ToList();
        }
        /// <summary>
        /// Add a blank attribute to the ItemVariants record of each attribute
        /// </summary>
        //async Task AddItemVariant_CLick()
        //{
        //    // this should look for the numer Attributes at are marked at variable attributes and then add a list per item
        //    var possibleVariables = AppUnitOfWork.GetListOfAnItemsVariableAttributes(ParentItemId);
        //    ItemVariantView newItemVariantView = new ItemVariantView
        //    {
        //        IsModified = true,
        //        TabIsExpanded = true,
        //        ItemVariant = new ItemVariant
        //        {
        //            ItemId = ParentItemId,
        //            ItemVariantName = "new variant",
        //            IsEnabled = true,
        //            ItemVariantAbbreviation = "Abrv0",
        //            ManageStock = false,
        //            QtyInStock = 0,
        //            SKU = "SKAnother",
        //            ItemVariantAssociatedLookups = new List<ItemVariantAssociatedLookup>()
        //        }
        //    };

        //    foreach (var variable in possibleVariables)
        //    {
        //        newItemVariantView.ItemVariant.ItemVariantAssociatedLookups.Add(new ItemVariantAssociatedLookup
        //        {
        //            AssociatedAttributeLookupId = variable.ItemAttributeLookupId,
        //            AssociatedAttributeLookup = variable.ItemAttributeDetail,
        //            AssociatedAttributeVarietyLookupId = null
        //        });
        //    }
        //    ItemVariantViews.Add(newItemVariantView);
        //    await InvokeAsync(StateHasChanged);
        //    await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Success, "A new variant has been created.");
        //}
        //public List<ItemAttributeLookup> GetListOfAttributes(Guid currentAttributeLookupId,
        //                                                     bool IsForceReload = false)
        //{
        //    return AppUnitOfWork.GetListOf<ItemAttributeLookup>(IsForceReload, ial => ial.AttributeName);
        //}
        /// <summary>
        ///  Return a list of Attribute Varieties that are available for selection
        ///  Logic:
        ///     1. Get a list of all ids that are already allocated except the current item
        ///     2. set list to be the current item and any options that not already allocated
        /// </summary>
        /// <param name="currentAssociatedAttributeLookupId">The current Attribute Lookup for these variants</param>
        /// <param name="currentAttributeVarityLookupId">The one that is currently selected</param>
        /// <param name="mustForce">must the list be reloaded (used for refresh etc.)</param>
        /// <returns>List of item attribute varieties to be used.</returns>
        public List<ItemAttributeVariety> GetListOfPossibleVariants(Guid sourceItemId,
                                                            Guid currentAssociatedAttributeLookupId,
                                                            Guid? currentItemAttributeVarietyId = null,
                                                            bool isForceReload = false)
        {

            List<ItemAttributeVariety> _itemAttributeVarieties = AppUnitOfWork.GetListOfAnItemsAttributeVarieties(sourceItemId, currentAssociatedAttributeLookupId, isForceReload);
            if (_itemAttributeVarieties == null)
                return null; //-- this should not happen

            if (currentItemAttributeVarietyId == null)
                return _itemAttributeVarieties;  // no item selected 
                                                 // 1.
            var _usedItems = itemVariants.Where(miav => (miav.ItemVariantAssociatedLookups.Any(ival => ival.AssociatedAttributeVarietyLookupId != currentItemAttributeVarietyId)));
            // 2. 
            var _unselectedItems = _itemAttributeVarieties.Where(iav => !_usedItems.Any(miav => (miav.ItemVariantAssociatedLookups.Any(ival => ival.AssociatedAttributeVarietyLookupId == iav.ItemAttributeVarietyLookupId))));
            return _unselectedItems.ToList();
        }

        public async Task<bool> SaveItemVariants()
        {
            if (!ItemVariantHasChanged)  // no change ignore
                return true;
            isItemVariantSaveBusy = true;
            bool allSaved = true;
            foreach (var itemVariantView in itemVariantViews)
            {
                if (itemVariantView.IsModified)
                {
                    //if (itemVariantAssoLookup.ItemVariantAssociatedLookupId == Guid.Empty)
                    //{
                    //    itemVariantAssoLookup.ItemVariantId = itemVariantView.ItemVariant.ItemVariantId;  // make suer it is associated to this ItemVariant
                    //}                                                                                                          // make sure the ItemVariantId is set
                    if (itemVariantView.ItemVariant.ItemVariantId == Guid.Empty)
                    {
                        allSaved &= await itemVariantViewRepository.InsertViewRowAsync(itemVariantView.ItemVariant, itemVariantView.ItemVariant.ItemVariantName) != null;
                    }
                    else
                    {
                        // The variant did exist however it may have been set to null if an ItemVariantAssociatedLookups = Guid.Empty so add that item.
                        
                        foreach (var itemVariantAssoLookup in itemVariantView.ItemVariant.ItemVariantAssociatedLookups)
                        {
                            if (itemVariantAssoLookup.ItemVariantAssociatedLookupId == Guid.Empty)
                            {
                                itemVariantAssoLookup.ItemVariantId = itemVariantView.ItemVariant.ItemVariantId;  // make suer it is associated to this ItemVariant
                                allSaved &= await AddAnItemVariantAssociatedLookup(itemVariantAssoLookup);
                            }
                        }
                        allSaved &= await itemVariantViewRepository.UpdateViewRowAsync(itemVariantView.ItemVariant, itemVariantView.ItemVariant.ItemVariantName) != UnitOfWork.CONST_WASERROR;
                    }
                    itemVariantView.IsModified = !allSaved;  // set it to the opposite of success
                }
            }
            if (allSaved)
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, "Item variants saved.");
            else
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "An error occurred saving one or more variants, check log.");
            isItemVariantSaveBusy = false;
            return allSaved;
        }
        /// <summary>
        /// Add An Associated Lookup to an item
        /// </summary>
        /// <param name="itemVariantAssoLookup">the associated lookup</param>
        /// <returns></returns>
        private async Task<bool> AddAnItemVariantAssociatedLookup(ItemVariantAssociatedLookup itemVariantAssoLookup)
        {
            IRepository<ItemVariantAssociatedLookup> itemVariantAssoLookupRepository = AppUnitOfWork.Repository<ItemVariantAssociatedLookup>();
            var _result = await itemVariantAssoLookupRepository.AddAsync(itemVariantAssoLookup);
            return _result != null;
        }

        // Interface Stuff
        //        public void CheckAbbreviation(ValidatorEventArgs validationArgs)
        //        {
        //            // we are working with abbreviation so us the restraints
        ////            var attrType = typeof(T);
        ////            var property = instance.GetType().GetProperty(propertyName);
        ////            return (T)property.GetCustomAttributes(attrType, false).First();

        //            //PropertyInfo property = SeletectedItemVariant.GetType().GetProperty(nameof(ItemVariant.ItemVariantAbbreviation));
        //            //var MinLength = typeof() 

        //            ValidationRule.IsNotEmpty(validationArgs);
        //            // does not through error even though model has error so force error.
        //            if ((validationArgs.Status == ValidationStatus.Error) || (((string)validationArgs.Value).Length < 2)
        //                                                                  || (((string)validationArgs.Value).Length> 10))
        //            {
        //                validationArgs.ErrorText = "Abbreviation must be between 2 and 10 letters in length.";
        //                validationArgs.Status = ValidationStatus.Error;
        //            }
        //        }
        #endregion
        #region Database linked stuff
        ///// ---> All this is done in Insert as it is no longer a grid
        /// <summary>
        /// Handle the grid insert 
        /// </summary>
        /// <param name="insertedItemVariant">what the grid passes us</param>
        /// <returns>void</returns>
        //async Task OnRowInsertingAsync(SavedRowItem<ItemVariant, Dictionary<string, object>> insertedItemVariant)
        //{
        //    var newItemVariant = insertedItemVariant.Item;
        //    if (newItemVariant.ItemVariantAssociatedLookups.Any(ival => ival.AssociatedAttributeVarietyLookupId == Guid.Empty))
        //    {
        //        await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Item's Variant cannot be blank!");
        //    }
        //    else
        //    {
        //        await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Adding Variant to item.");
        //        await itemVariantViewRepository.InsertViewRowAsync(newItemVariant, "Variant");
        //    }
        //    //            await _ItemVariantsDataGrid.Reload();
        //}
        ///// ---> All this is done in save as it is no longer a grid
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        //async Task OnRowUpdatedAsync(SavedRowItem<ItemVariant, Dictionary<string, object>> updatedItem)
        //{
        //    var _updatedItemVariant = updatedItem.Item;
        //    if (_updatedItemVariant.ItemVariantName == String.Empty)
        //    {
        //        await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Item's Variant cannot be blank...");
        //    }
        //    else
        //    {
        //        // load item for database to see it exists, could later add a check using row version to see if it has changed.
        //        await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Info, "Updating Variant  in item.");
        //        var _currentItemVariant = await itemVariantViewRepository.GetEntityByIdAsync(_updatedItemVariant.ItemVariantId);
        //        if (_currentItemVariant == null)
        //        {
        //            await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "Error updating Variant , it could not be found in the table.");
        //        }
        //        else
        //        {
        //            AppMapper.Map(_updatedItemVariant, _currentItemVariant);
        //            //var _result = 
        //            await itemVariantViewRepository.UpdateViewRowAsync(_currentItemVariant, _currentItemVariant.ItemVariantName);
        //            if (AppUnitOfWork.IsInErrorState())
        //            {
        //                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error updating Variant {AppUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
        //            }
        //            // update the in memory children, but only after update just in case EF core tries to update the children (which should not be needed as they are added as views
        //            //-> note needed as this we disabled the editing _updatedItemVariant = SetVariantDetail(_updatedItemVariant);
        //        }
        //    }
        //    //await _ItemVariantsDataGrid.Reload();
        //    StateHasChanged();
        //}
        async Task<bool> DeleteAnItemsVariant(ItemVariantView sourceItemVariantView)
        {
            bool success = true;
            // delete the woo Mapping
            var wooProductVariantMapping = AppUnitOfWork.Repository<WooProductVariantMap>();
            if (sourceItemVariantView.ItemVariant.ItemVariantId != Guid.Empty)  // only if this is not a new variant that has been created while in this screen.
                await wooProductVariantMapping.DeleteByAsync(wpv => wpv.ItemVariantId == sourceItemVariantView.ItemVariant.ItemVariantId);

            // if the ItemVariant has not been saved to the db then the Id will be Guid.Empty 
            itemVariantViews.Remove(sourceItemVariantView);
            if (sourceItemVariantView.ItemVariant.ItemVariantId != Guid.Empty)
            {
                success = await AppUnitOfWork.ItemRepository.DeleteItemVariantAndAssociatedData(sourceItemVariantView.ItemVariant);
                // success = (result > 0) && (!AppUnitOfWork.IsInErrorState());
            }
            return success;
        }
        /// <summary>
        /// Confirm Delete Click is called when the user confirms they want to delete.
        /// </summary>
        /// <param name="confirmationOption">confirmation option by the user selection</param>
        /// <returns></returns>
        async Task DeleteItemVariantAsync(bool confirmationOption)
        {
            if (confirmationOption)
            {
                bool success = await DeleteAnItemsVariant(selectedItemVariantView);
                if (success)
                    await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Variant {selectedItemVariantView.ItemVariant.ItemVariantName} has been deleted.");
                else
                    await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error deleting Variant {selectedItemVariantView.ItemVariant.ItemVariantName} - check log.");
            }
            await InvokeAsync(StateHasChanged);
        }
        /// <summary>
        /// Usses the same logic from a single variant delete above, delete all of an Items Variants
        /// </summary>
        /// <param name="confirmationOption">passed in by the confirmation dialogue</param>
        /// <returns></returns>
        async Task DeleteAllItemVariantsAsync(bool confirmationOption)
        {
            if (confirmationOption)
            {
                bool success = true;
                // delete the wooMappings per item - perhaps we should be doing this elsewhere, but cannot think where now. Also what if there is no woo table?
                var wooProductVariantMapping = AppUnitOfWork.Repository<WooProductVariantMap>();
                foreach (var thisItemVariantView in itemVariantViews)
                {
                    if (thisItemVariantView.ItemVariant.ItemVariantId != Guid.Empty)  // only if this is not a new variant that has been created while in this screen.
                        await wooProductVariantMapping.DeleteByAsync(wpv => wpv.ItemVariantId == thisItemVariantView.ItemVariant.ItemVariantId);
                }
                itemVariantViews.Clear();  // clear the current list on the screen
                success = await AppUnitOfWork.ItemRepository.DeleteAllItemsVariantsAsync(ParentItemId);
                if (success)
                    await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Variants from Item id: {ParentItemId} have been deleted.");
                else
                    await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error deleting Variants from Item id: {ParentItemId} - check log.");
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Button handling etc.

        static Guid? SetChangedAttribute(Guid newGuid, ItemVariantView currentItemVariantView)
        {
            currentItemVariantView.IsModified = true;
            return newGuid;
        }
        /// <summary>
        /// Expand all the Variant Tabs or shrink them
        /// </summary>
        /// <param name="DoExpand">Expand or Shrink</param>
        void ExpandAll_Click(bool DoExpand)
        {
            foreach (var itemVar in itemVariantViews)
            {
                itemVar.TabIsExpanded = DoExpand;
            }
            StateHasChanged();
        }
        /// <summary>
        /// Add a Variant to the Item variant list
        /// </summary>
        /// <returns>void</returns>
        async Task AddItemVariant_CLick()
        {
            ItemVariantView newItemVariantView = new()
            {
                IsModified = true,
                TabIsExpanded = true,
                ItemVariant = itemVariantViewRepository.NewViewEntityDefaultSetter(new ItemVariant(), ParentItemId)
            };
            itemVariantViews.Add(newItemVariantView);
            await InvokeAsync(StateHasChanged);
            await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Success, "A new variant has been created.");
        }
        static List<AttributeLookupIdGroup> GetLookupIdCollection(List<ItemVariantAssociatedLookup> source)
        {
            List<AttributeLookupIdGroup> lookupIdCollection = new();

            foreach (var sourceItem in source)
            {
                lookupIdCollection.Add(new AttributeLookupIdGroup
                {
                    AssociatedAttributeLookupId = sourceItem.AssociatedAttributeLookupId,
                    AssociatedAttributeVarietyLookupId = sourceItem.AssociatedAttributeVarietyLookupId
                });
            }
            return lookupIdCollection;
        }

        /// <summary>
        /// Return true if this variety already exists
        /// </summary>
        /// <param name="sourceItemVariety"></param>
        /// <returns></returns>
        private bool VarietyDoesNotExistsAlready(ItemVariant sourceItemVariant)
        {
            bool foundIt = false;
            List<AttributeLookupIdGroup> newSourceIds = GetLookupIdCollection(sourceItemVariant.ItemVariantAssociatedLookups);
            // loop through each option and see if we have one that matches, if we do break out and return.
            foreach (var thisItemVariantView in itemVariantViews)
            {
                var currentSourceIds = GetLookupIdCollection(thisItemVariantView.ItemVariant.ItemVariantAssociatedLookups);
                foundIt = currentSourceIds.SequenceEqual(newSourceIds);
                if (foundIt)
                {
                    return true;
                }
            }
            return foundIt;  // -> should always be false if we get here.
        }
        /// <summary>
        /// Add a variant if it does not already exist
        /// </summary>
        /// <param name="thereWereNone"></param>
        /// <param name="attributeIndexs"></param>
        /// <param name="possibleItemVariables"></param>
        private void AddVariantIfNotThere(/*bool thereWereNone, */ int[] attributeIndexs, List<ItemAttribute> possibleItemVariables)
        {
            ItemVariantView newItemVariantView = new()
            {
                IsModified = true,
                TabIsExpanded = false,
                ItemVariant = itemVariantViewRepository.NewBasicViewEntityDefaultSetter(new ItemVariant(), ParentItemId)
            };
            // now add the varieties
            for (int i = 0; i < attributeIndexs.Length; i++)
            {
                newItemVariantView.ItemVariant.ItemVariantAssociatedLookups.Add(new ItemVariantAssociatedLookup
                {
                    AssociatedAttributeLookupId = possibleItemVariables[i].ItemAttributeLookupId,
                    AssociatedAttributeLookup = possibleItemVariables[i].ItemAttributeDetail,
                    AssociatedAttributeVarietyLookupId = possibleItemVariables[i].ItemAttributeVarieties[attributeIndexs[i]].ItemAttributeVarietyLookupId,
                    AssociatedAttributeVarietyLookup = possibleItemVariables[i].ItemAttributeVarieties[attributeIndexs[i]].ItemAttributeVarietyDetail
                });
            }
            if (!VarietyDoesNotExistsAlready(newItemVariantView.ItemVariant))
            {
                // add any extra info here now that we have confirmed it does not exist
                newItemVariantView.ItemVariant.SortOrder = itemVariants.Count + 1;  // add it to the end
                newItemVariantView.ItemVariant.ItemVariantName = String.Empty;
                foreach (var itemAttribute in newItemVariantView.ItemVariant.ItemVariantAssociatedLookups)
                {
                    newItemVariantView.ItemVariant.ItemVariantName += itemAttribute.AssociatedAttributeVarietyLookup.VarietyName + " ";
                }
                newItemVariantView.ItemVariant.ItemVariantName = newItemVariantView.ItemVariant.ItemVariantName.Trim();
                newItemVariantView.ItemVariant.ItemVariantAbbreviation = newItemVariantView.ItemVariant.ItemVariantAbbreviation + $"{newItemVariantView.ItemVariant.SortOrder}";
                itemVariantViews.Add(newItemVariantView);
            }
        }
        /// <summary>
        /// Add all variants that are not already added, as blanks
        /// </summary>
        async Task AddAllItemsVariants_CLick()
        {
            await AddAllItemVariantsConfirmationModal.ShowModalAsync("Add confirmation", $"Are you sure you want to add all variants of this item?");  //,"Delete","Cancel"); - passed in on init
        }
        /// <summary>
        /// Add all the items possible Variant that do not already exist
        /// </summary>
        /// <param name="confirmClicked">Did the user confirm that they wanted to go ahead?</param>
        /// <returns>void</returns>
        async Task AddAllItemsVariantsAsync(bool confirmClicked)
        {
            /// 1. If user cancelled then do nothing.
            if (confirmClicked)
            {

                // flag if there were none so we do not have to check if one exists.
                //bool thereWereNone = (itemVariantViews == null) || (itemVariantViews.Count == 0);
                var possibleItemVariableAttributes = AppUnitOfWork.GetListOfAnItemsVariableAttributes(ParentItemId);
                // now we have all the possible attribute we can loop through each attribute's variables and add an option if it does not exist
                // so using an array of numbers we loop through first the one list, then next then the next.
                bool IsAdding = true;
                int[] attributeIndexs = new int[possibleItemVariableAttributes.Count];
                while (IsAdding)
                {
                    AddVariantIfNotThere(/*thereWereNone,*/ attributeIndexs, possibleItemVariableAttributes);
                    // increment the indexes so each one is done, or we are finished
                    int activeIndex = 0;
                    bool isDone = false;
                    while ((!isDone) && (activeIndex < possibleItemVariableAttributes.Count))
                    {
                        if (attributeIndexs[activeIndex] >= (possibleItemVariableAttributes[activeIndex].ItemAttributeVarieties.Count - 1))
                        {
                            // we have reached the end so increment the next row, or exit
                            if (activeIndex < possibleItemVariableAttributes.Count)
                            {
                                // all of the items from this attribute have been added move to the next one
                                attributeIndexs[activeIndex] = 0;
                                activeIndex++;
                            }
                            else
                            {
                                IsAdding = false;   // we are at the end
                                isDone = true;  ///break;
                            }
                        }
                        else
                        {
                            attributeIndexs[activeIndex]++; // increment the index
                            isDone = true;  ///break;
                        }
                    }
                    IsAdding = activeIndex < possibleItemVariableAttributes.Count;
                }
                await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Success, "All variants (that were not there) added.");
            }
            await InvokeAsync(StateHasChanged);
        }
        /// <summary>
        /// Ask for confirmation to remove the passed in item variant view for the screen and the database
        /// </summary>
        /// <param name="sourceItemVariantView">Which ?Item to Delete</param>
        /// <returns>void and the dialogue box takes over</returns>
        async Task RemoveItemVariant_CLick(ItemVariantView sourceItemVariantView)
        {
            // set the Selected Item Variant for use later
            selectedItemVariantView = sourceItemVariantView;
            await itemVariantViewRepository.CurrFormSettings.DeleteConfirmation.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {selectedItemVariantView.ItemVariant.ItemVariantName}?");  //,"Delete","Cancel"); - passed in on init
                                                                                                                                                                                                                    // On confirmation calls - ConfirmDeleteItemVariant_ClickAsync
        }
        /// <summary>
        /// Ask for confirmation to remove all an item variant views for the screen and the database
        /// </summary>
        /// <returns>void and the dialogue box takes over</returns>
        async Task RemoveAllItemsVariants_CLick()
        {
            await DeleteAllItemVariantsConfirmationModal.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete all variant of item ID: {ParentItemId}?");  //,"Delete","Cancel"); - passed in on init
                                                                                                                                                                             // On confirmation calls - ConfirmDeleteAllItemVariants_ClickAsync
        }
        /// <summary>
        /// Handle the use clicking the Save in the Item Variant section
        /// </summary>
        /// <returns></returns>
        public async Task SaveItemVariants_Click()
        {
            await SaveItemVariants();
        }
        /// <summary>
        /// handle the click of import variant, show confirmation async then when the user confirms (or not) run the ImportVariant
        /// </summary>
        public async Task ImportItemVariants_Click()
        {
            await ImportVariantsConfirmationModal.ShowModalAsync("This is a confirmation", "Are you sure you import this item, data will de over written?", "Please Confirm", "Cancel");
        }
        /// <summary>
        /// Do the actual import of the variants for this item
        /// </summary>
        /// <param name="confirmClicked">was it confirmed that we must import this</param>
        /// <returns>void</returns>
        async Task ImportVariantAsync(bool confirmClicked)
        {
            /// 1. If user cancelled then do nothing.
            if (confirmClicked)
            {
                await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Info, $"Importing Item form Woo...");
                isItemVariantImportBusy = true;
                /// 2. Get the app woo settings.
                IRepository<WooSettings> wooPrefsAppRepository = AppUnitOfWork.Repository<WooSettings>();
                WooSettings wooSettings = await wooPrefsAppRepository.FindFirstAsync();
                if (wooSettings == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, "No woo settings retrieved. Please check your settings.");
                    isItemVariantImportBusy = false;
                    return;
                }
                /// 2. Use the ItemId to see if the item is linked via the WooMapping to a Woo product, then continue otherwise show message there is no mapping
                IRepository<WooProductMap> wooProductMapRepository = AppUnitOfWork.Repository<WooProductMap>();
                WooProductMap wooProductMap = await wooProductMapRepository.GetByIdAsync(wpm => wpm.ItemId == ParentItemId);
                if (wooProductMap == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"No woo product is mapped to item id: {ParentItemId}.");
                    isItemVariantImportBusy = false;
                    return;
                }
                /// 3. Using the returned mapping, get the woo product that it is mapped to using ID
                WooAPISettings wooAPISettings = new(wooSettings);
                WooProduct wooProduct = new(wooAPISettings, AppLoggerManager);
                if (wooProduct == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error creating Woo Product engine.");
                    isItemVariantImportBusy = false;
                    return;
                }
                Product _product = await wooProduct.GetProductByIdAsync(wooProductMap.WooProductId);
                if (_product == null)
                {
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"No product variants with product id {wooProductMap.WooProductId} found on Woo.");
                    isItemVariantImportBusy = false;
                    return;
                }
                WooImportVariation wooImportVariation = new(AppUnitOfWork, AppLoggerManager, wooSettings, AppMapper);
                var importCounters = await wooImportVariation.ImportProductVariantsAsync((uint)_product.id, ParentItemId);
                if (AppUnitOfWork.IsInErrorState())
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error importing product variants with product id {wooProductMap.WooProductId} found on Woo.");
                else
                    await PopUpRef.ShowNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Product variants with product id {wooProductMap.WooProductId} found on Woo and imported.");
                // Reload item.
                await LoadItemVariantAttributesAsync(true);
                await InvokeAsync(StateHasChanged);
            }
            isItemVariantImportBusy = false;
        }
        #endregion

    }
}
