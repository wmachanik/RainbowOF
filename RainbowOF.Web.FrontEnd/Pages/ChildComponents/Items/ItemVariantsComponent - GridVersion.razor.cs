using AutoMapper;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class ItemVariantsComponent : ComponentBase
    {
        #region Grid Variables
        // Interface Stuff
        List<ItemVariant> ModelItemVariants = null;
        ItemVariant SeletectedItemVariant;
        DataGrid<ItemVariant> _ItemVariantsDataGrid;
        // All there workings are here
        IItemVariantGridViewRepository _ItemVariantGridViewRepository = null;
        //string[] ValidationErrors = new string[] { };
        #endregion
        #region Injected Variables
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        [Inject]
        public IMapper _Mapper { get; set; }
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
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            IAppRepository<ItemVariant> _itemVariantRepository = _AppUnitOfWork.Repository<ItemVariant>();
            // Inits don't seem to like awaits -> so we need to call without an await
            //var _itemVariants = await _itemVariantRepository.GetByAsync(iv => iv.ItemId == ParentItemId);
            var _itemVariants = _itemVariantRepository.GetBy(iv => iv.ItemId == ParentItemId); //=> non async seems to be an issue calling more than one await in a total screen init
            if (_itemVariants != null)
            {
                ModelItemVariants = _itemVariants.OrderBy(iv => iv.SortOrder).ThenBy(iv => iv.ItemVariantName).ToList();
                // load lookup here, force reload as could be new item    - async order by failing again, not sure why
                if (ModelItemVariants.Any())
                {
                    //foreach (var itemvar in ModelItemVariants)
                    //{
                    //    _AppUnitOfWork.GetListOfAnItemsAttributes(ParentItemId);
                    //    _AppUnitOfWork.GetListOfAnItemsAttributeVarieties(ParentItemId, itemvar.AssociatedAttributeLookupId);
                    //}
                }
                else
                    ModelItemVariants = null; //-> there are no variants so tell the grid to display none
                _ItemVariantGridViewRepository = new ItemVariantGridViewRepository(_Logger, _AppUnitOfWork);
                _ItemVariantGridViewRepository._GridSettings.PopUpRef = PopUpRef;   // use the forms Pop Up ref
                await InvokeAsync(StateHasChanged);
            }
            _Logger.LogDebug("Item Variants Component initialised.");
        }
        #endregion
        #region BackEnd Routines
        public List<ItemAttribute> GetListOfPossibleAttributes(Guid? currentItemAttributeId = null, bool IsForceReload = false)
        {

            List<ItemAttribute> _itemAttributes = _AppUnitOfWork.GetListOfAnItemsAttributes(ParentItemId);
            if (_itemAttributes == null)
                return null; //-- this should not happen

            if (currentItemAttributeId == null)
                return _itemAttributes;  // no item selected 

            // 1.
            var _usedItems = ModelItemVariants.Where(miav => (miav.AssociatedAttributeLookupId != currentItemAttributeId));
            // 2. 
            var _unselectedItems = _itemAttributes.Where(iav => !_usedItems.Any(miav => (miav.AssociatedAttributeLookupId == iav.ItemAttributeLookupId)));
            return _unselectedItems.ToList();

        }
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
                                                            bool IsForceReload = false)
        {

            List<ItemAttributeVariety> _itemAttributeVarieties = _AppUnitOfWork.GetListOfAnItemsAttributeVarieties(sourceItemId, currentAssociatedAttributeLookupId);
            if (_itemAttributeVarieties == null)
                return null; //-- this should not happen

            if (currentItemAttributeVarietyId == null)
                return _itemAttributeVarieties;  // no item selected 
            // 1.
            var _usedItems = ModelItemVariants.Where(miav => (miav.AssociatedAttributeVarietyLookupId != currentItemAttributeVarietyId));
            // 2. 
            var _unselectedItems = _itemAttributeVarieties.Where(iav => !_usedItems.Any(miav => (miav.AssociatedAttributeVarietyLookupId == iav.ItemAttributeVarietyLookupId)));
            return _unselectedItems.ToList();

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
        //                                                                  || (((string)validationArgs.Value).Length > 10))
        //            {
        //                validationArgs.ErrorText = "Abbreviation must be between 2 and 10 letters in length.";
        //                validationArgs.Status = ValidationStatus.Error;
        //            }
        //        }
        #endregion
        #region Database linked stuff
        void OnNewItemVariantDefaultSetter(ItemVariant newItem)
        {
            newItem = _ItemVariantGridViewRepository.NewViewEntityDefaultSetter(newItem, ParentItemId);
        }
        /// <summary>
        /// Handle the grid insert 
        /// </summary>
        /// <param name="insertedItemVariant">what the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowInsertingAsync(SavedRowItem<ItemVariant, Dictionary<string, object>> insertedItemVariant)
        {
            var newItemVariant = insertedItemVariant.Item;
            if (newItemVariant.AssociatedAttributeVarietyLookupId == Guid.Empty)
            {
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's Variant cannot be blank!");
            }
            else
            {
                PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Adding Variant to item.");
                await _ItemVariantGridViewRepository.InsertViewRowAsync(newItemVariant, "Variant");
            }
            await _ItemVariantsDataGrid.Reload();
        }
        /// <summary>
        /// Handle the grid update
        /// </summary>
        /// <param name="updatedItem">What the grid passes us</param>
        /// <returns>void</returns>
        async Task OnRowUpdatedAsync(SavedRowItem<ItemVariant, Dictionary<string, object>> updatedItem)
        {
            var _updatedItemVariant = updatedItem.Item;
            if (_updatedItemVariant.ItemVariantName == String.Empty)
            {
                PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Item's Variant cannot be blank...");
            }
            else
            {
                // load item for database to see it exists, could later add a check using row version to see if it has changed.
                PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Updating Variant  in item.");
                var _currentItemVariant = await _ItemVariantGridViewRepository.GetEntityByIdAsync(_updatedItemVariant.ItemVariantId);
                if (_currentItemVariant == null)
                {
                    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, "Error updating Variant , it could not be found in the table.");
                }
                else
                {
                    _Mapper.Map(_updatedItemVariant, _currentItemVariant);
                    //var _result = 
                    await _ItemVariantGridViewRepository.UpdateViewRowAsync(_currentItemVariant, _currentItemVariant.ItemVariantName);
                    if (_AppUnitOfWork.IsInErrorState())
                    {
                        PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Error updating Variant {_AppUnitOfWork.GetErrorMessage()}, it could not be found in the table.");
                    }
                    // update the in memory children, but only after update just in case EF core tries to update the children (which should not be needed as they are added as views
                    //-> note needed as this we disabled the editing _updatedItemVariant = SetVariantDetail(_updatedItemVariant);
                }
            }
            //await _ItemVariantsDataGrid.Reload();
            StateHasChanged();
        }

        //private ItemVariant SetVariantDetail(ItemVariant updatedItemVariant)
        //{
        //    //=> used if there is a reference class, at the moment ItemVariants do not 
        //    //if (updatedItemVariant.AssociatedAttributeVarietyLookupId == Guid.Empty)
        //    //{
        //    //}
        //    //else
        //    //{
        //    //    var _ItemAtts = GetListOfVariants();
        //    //    updatedItemVariant.ItemVariantDetail = _ItemAtts.FirstOrDefault(ic => ic.ItemVariantLookupId == updatedItemVariant.ItemVariantLookupId);
        //    //}
        //    return updatedItemVariant;
        //}

        async Task OnRowRemovingAsync(CancellableRowChange<ItemVariant> modelItem)
        {
            // set the Selected Item Variant for use later
            SeletectedItemVariant = modelItem.Item;
            var deleteItem = modelItem;
            await _ItemVariantGridViewRepository._GridSettings.DeleteConfirmation.ShowModalAsync("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.ItemVariantName}?");  //,"Delete","Cancel"); - passed in on init
        }
        /// <summary>
        /// Confirm Delete Click is called when the user confirms they want to delete.
        /// </summary>
        /// <param name="confirmationOption">confirmation option by the user selection</param>
        /// <returns></returns>
        async Task ConfirmDelete_ClickAsync(bool confirmationOption)
        {
            if (confirmationOption)
            {
                await _ItemVariantGridViewRepository.DeleteViewRowByIdAsync(SeletectedItemVariant.ItemVariantId, SeletectedItemVariant.ItemVariantName);
            }
            //IsLoading = false;
            await _ItemVariantsDataGrid.Reload();
        }
        public async Task OnRowRemovedAsync(ItemVariant modelItem)
        {
            await _ItemVariantsDataGrid.Reload();  // reload the list so the latest item is displayed - not working here I think because of the awaits so move to confirm_clicks
            await InvokeAsync(StateHasChanged);
        }

        #endregion
        #region Button handling etc
//        EventCallback AddItemVariant
        #endregion

    }
}
