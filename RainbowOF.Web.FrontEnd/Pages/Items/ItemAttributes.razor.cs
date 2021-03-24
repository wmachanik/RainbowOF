using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using RainbowOF.ViewModels.Lookups;
using RainbowOF.Components.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainbowOF.ViewModels.Common;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemAttributes : ComponentBase
    {
        // Interface Stuff
        public GridSettings localGridSettings = new GridSettings();

        public ItemAttributeLookup SelectedItemAttributeLookup = null;
        public BulkAction SelectedBulkAction = BulkAction.none;

        // variables / Models
        public List<ItemAttributeLookupView> modelItemAttributeLookupViews;
        public ItemAttributeLookupView seletectedItem = null;

        public List<ItemAttributeLookupView> SelectedItemAttributeLookups;
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        //[Parameter]
        //public ILoggerManager Logger { get; set; }  /// not needed here

        protected override async Task OnInitializedAsync()
        {
            await LoadItemAttributeLookupList();
        }

        private async Task LoadItemAttributeLookupList()
        {

            StateHasChanged();
            // store old select items list
            List<ItemAttributeLookupView> oldSelectedItems = null;
            if (SelectedItemAttributeLookups != null)
            {
                oldSelectedItems = new List<ItemAttributeLookupView>(SelectedItemAttributeLookups);
                SelectedItemAttributeLookups.Clear(); // the refresh does this
            }
            /// get all the items and using those get all the attributes in of items
            List<ItemAttributeLookup> itemAttributeLookups = await GetAllItemAttributeLookups();

            modelItemAttributeLookupViews = new List<ItemAttributeLookupView>();
            WooProductAttributeMap wooAttributeMap;
            foreach (var ItemAttrib in itemAttributeLookups)
            {
                //  map all the items across to the view then allocate extra woo stuff if exists.
                wooAttributeMap = await GetWooAttributeMappedAsync(ItemAttrib.ItemAttributeLookupId);

                modelItemAttributeLookupViews.Add(new ItemAttributeLookupView
                {
                    ItemAttributeLookupId = ItemAttrib.ItemAttributeLookupId,
                    AttributeName = ItemAttrib.AttributeName,
                    ItemAttributeVarietyLookups = ItemAttrib.ItemAttributeVarietyLookups,
                    Notes = ItemAttrib.Notes,

                    CanUpdateWooMap = (wooAttributeMap == null) ? null : wooAttributeMap.CanUpdate
                });
            }
            //ShowPager = (modelItemAttributeLookupViews.Count > PageSize);
            if (oldSelectedItems != null)
                foreach (var item in oldSelectedItems)
                {
                    SelectedItemAttributeLookups.Add(modelItemAttributeLookupViews.Where(ial => ial.ItemAttributeLookupId == item.ItemAttributeLookupId).FirstOrDefault());
                }
            StateHasChanged();
        }

        //public void OnSelectePageChanged(ChangeEventArgs e)
        //{
        //    //(e.Value shoe be string)
        //    PageSize = Convert.ToInt32(e.Value);
        //    ShowPager = (modelItemAttributeLookupViews != null) && (modelItemAttributeLookupViews.Count > PageSize);

        //}
        private async Task<WooProductAttributeMap> GetWooAttributeMappedAsync(Guid WooAttributeMap)
        {
            IAppRepository<WooProductAttributeMap> wooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();

            return await wooAttributeMapRepository.FindFirstAsync(wcm => wcm.ItemAttributeLookupId == WooAttributeMap);
        }

        private async Task<List<ItemAttributeLookup>> GetAllItemAttributeLookups()
        {
            IAppRepository<ItemAttributeLookup> _ItemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            List<ItemAttributeLookup> _ItemAttributeLookups = (await _ItemAttributeLookupRepository.GetAllEagerAsync(ial => ial.ItemAttributeVarietyLookups))
                .OrderBy(ial => ial.OrderBy)
                .ThenBy(ial => ial.AttributeName).ToList();

            return _ItemAttributeLookups;
        }

        bool OnCustomFilter(ItemAttributeLookupView model)
        {
            if (string.IsNullOrEmpty(localGridSettings.customFilterValue))
                return true;

            return
                model.AttributeName?.Contains(localGridSettings.customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || (bool)model.ItemAttributeVarietyLookups?.Exists(iav => iav.VarietyName.Contains(localGridSettings.customFilterValue, StringComparison.OrdinalIgnoreCase));
        }
        ItemAttributeLookup GetItemAttributeLookupItemFromView(ItemAttributeLookupView pItem)
        {
            ItemAttributeLookup newItemAttributeLookup = new ItemAttributeLookup
            {
                ItemAttributeLookupId = pItem.ItemAttributeLookupId,
                AttributeName = pItem.AttributeName,
                ItemAttributeVarietyLookups = pItem.ItemAttributeVarietyLookups,
                Notes = pItem.Notes,
            };

            return newItemAttributeLookup;
        }
        async Task OnRowInserted(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> pInsertedItem)
        {
            var newItem = pInsertedItem.Item;
            IAppRepository<ItemAttributeLookup> _ItemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            // first check if we do not already have a Attribute like this.
            if (await _ItemAttributeLookupRepository.FindFirstAsync(ial => ial.AttributeName == newItem.AttributeName) == null)
            {
                int _recsAdded = await _ItemAttributeLookupRepository.AddAsync(GetItemAttributeLookupItemFromView(newItem));
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                    localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newItem.AttributeName} - {_AppUnitOfWork.GetErrorMessage()}", "Attribute Added");
                else
                    localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.AttributeName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute");
            }
            else
                localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.AttributeName} already exists, so could not be added.");
            await LoadItemAttributeLookupList();   // reload the list so the latest item is displayed
        }
        void OnItemAttributeLookupNewItemDefaultSetter(ItemAttributeLookup pNewAttr)
        {
            if (pNewAttr == null)
                pNewAttr = new ItemAttributeLookup();

            pNewAttr.AttributeName = "Attribute (must be unique)";
            pNewAttr.Notes = $"Added {DateTime.Now.Date}";
            pNewAttr.ItemAttributeVarietyLookups = new List<ItemAttributeVarietyLookup>();  // needs a blank one
        }
        async Task<int> UpdateItemAttributeLookup(ItemAttributeLookup pUpdatedAttr)
        {
            int _recsUpdted = 0;
            IAppRepository<ItemAttributeLookup> _ItemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();
            // first check it exists - it could have been deleted 
            ItemAttributeLookup pUpdatedLookup = await _ItemAttributeLookupRepository.GetByIdAsync(pUpdatedAttr.ItemAttributeLookupId);

            if (pUpdatedLookup == null)
            {
                localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute: {pUpdatedAttr.AttributeName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                pUpdatedLookup.AttributeName = pUpdatedAttr.AttributeName;
                pUpdatedLookup.ItemAttributeVarietyLookups = pUpdatedAttr.ItemAttributeVarietyLookups;
                pUpdatedLookup.Notes = pUpdatedAttr.Notes;
                _recsUpdted = await _ItemAttributeLookupRepository.UpdateAsync(pUpdatedLookup);
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{pUpdatedAttr.AttributeName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute");
            }
            return _recsUpdted;
        }

        async Task<int> UpdateWooAttributeMap(ItemAttributeLookupView pUpdatedItem)
        {
            int _recsUpdated = 0;

            WooProductAttributeMap updateWooAttributeMap = await GetWooAttributeMappedAsync(pUpdatedItem.ItemAttributeLookupId);
            if (updateWooAttributeMap != null)
            {
                if (updateWooAttributeMap.CanUpdate == pUpdatedItem.CanUpdateWooMap)
                {
                    // not necessary to display message.
                    //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Warning, $"Woo Attribute Map for Attribute: {pUpdatedItem.AttributeName} has not changed, so was not updated?");
                }
                else
                {
                    updateWooAttributeMap.CanUpdate = (bool)pUpdatedItem.CanUpdateWooMap;
                    IAppRepository<WooProductAttributeMap> wooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductAttributeMap>();
                    _recsUpdated = await wooAttributeMapRepository.UpdateAsync(updateWooAttributeMap);
                    localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute: {pUpdatedItem.AttributeName} was updated.");
                }
            }
            else
                localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Woo Attribute Map for Attribute: {pUpdatedItem.AttributeName} is no longer found, was it deleted?");

            return _recsUpdated;
        }
        private bool IsDuplicate(ItemAttributeLookupView pItem)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            var _exists = modelItemAttributeLookupViews.FindAll(ml => ml.AttributeName == pItem.AttributeName);
            return ((_exists != null) && (_exists.Count > 1));
        }
        private bool IsValid(ItemAttributeLookupView pItem)
        {
            // check that there is a loop back on PaerentId
            return true;  /// may need a check here
        }
        async Task OnRowUpdated(SavedRowItem<ItemAttributeLookupView, Dictionary<string, object>> pUpdatedItem)
        {
            ItemAttributeLookupView updatedItem = pUpdatedItem.Item;
            if (IsDuplicate(updatedItem))
                localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Name: {updatedItem.AttributeName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updatedItem))
                {
                    ItemAttributeLookup updatedItemAttributeLookup = GetItemAttributeLookupItemFromView(updatedItem);
                    // update and check for errors 
                    if (await UpdateItemAttributeLookup(updatedItemAttributeLookup) != AppUnitOfWork.CONST_WASERROR)
                    {
                        if ((updatedItem.HasWooAttributeMap) && (await UpdateWooAttributeMap(updatedItem) == AppUnitOfWork.CONST_WASERROR))
                            localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"WooAttribute map for Item: {updatedItem.AttributeName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating");
                        //else
                        //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute: {updatedItem.AttributeName} was updated.");
                    }
                }
                else
                    localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute Item {updatedItem.AttributeName} not valid", "Error updating");

            }
            await LoadItemAttributeLookupList();   // reload the list so the latest item is displayed
        }
        void OnRowRemoving(CancellableRowChange<ItemAttributeLookupView> modelItem)
        {
            // set the Selected Item Attribute for use later
            SelectedItemAttributeLookup = modelItem.Item;
            var deleteItem = modelItem;
            localGridSettings.DeleteConfirmation.ShowModal("Delete confirmation", $"Are you sure you want to delete: {deleteItem.Item.AttributeName}?");  //,"Delete","Cancel"); - passed in on init
        }

        //protected async Task
        async Task ConfirmDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed)
            {
                IAppRepository<ItemAttributeLookup> _ItemAttributeLookupRepository = _AppUnitOfWork.Repository<ItemAttributeLookup>();

                var _recsDelete = await _ItemAttributeLookupRepository.DeleteByIdAsync(SelectedItemAttributeLookup.ItemAttributeLookupId);

                if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                    localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute: {SelectedItemAttributeLookup.AttributeName} is no longer found, was it deleted?");
                else
                    localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute: {SelectedItemAttributeLookup.AttributeName} was it deleted?");
            }
            await LoadItemAttributeLookupList();   // reload the list so the latest item is displayed
        }
        public void OnRowRemoved(ItemAttributeLookupView modelItem)
        {
            var deleteItem = modelItem;
            //if ( dataModels.Contains( model ) )
            //{
            //    dataModels.Remove( model );
            //}
        }

        public List<string> GetListOfAttributeVarieties()
        {
            List<string> _ListOfVarieties = new List<string>();
            foreach (var model in modelItemAttributeLookupViews)
            {
                if ((model.ItemAttributeVarietyLookups != null) && (model.ItemAttributeVarietyLookups.Count > 0))
                {
                    foreach (var item in model.ItemAttributeVarietyLookups)
                    {
                        _ListOfVarieties.Add(item.VarietyName);

                    }
                }
            }

            return _ListOfVarieties;
        }


        async Task<int> DoActionOnItem(ItemAttributeLookupView pItem)
        {
            if (SelectedBulkAction == BulkAction.AllowWooSync)
                pItem.CanUpdateWooMap = true;
            else if (SelectedBulkAction == BulkAction.DisallowWooSync)
                pItem.CanUpdateWooMap = false;
            return await UpdateWooAttributeMap(pItem);
        }
        async Task DoGroupAction()
        {
            //if (SelectedBulkAction == BulkAction.none)
            //    return;   ----> button should be disabled 

            localGridSettings.PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Info, "Applying the bulk action as requested");

            int done = 0;
            int failed = 0;
            foreach (var item in SelectedItemAttributeLookups)
            {
                if (await DoActionOnItem(item) > 0) done++;
                else failed++;
            }
            localGridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Info, $"Bulk Action applied to {done} items and not applied to {failed} items.");
            ///SelectedItemAttributeLookups.Clear();  // need to do this since we are reloading
            await LoadItemAttributeLookupList();   // reload the list so the latest item is displayed
        }
//        #region VarietyGridStuff
        // ------------------------------------
        // All the Attribute Variety stuff
        // ------------------------------------
    }
}

