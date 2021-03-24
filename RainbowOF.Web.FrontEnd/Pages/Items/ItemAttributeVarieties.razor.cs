using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemAttributeVarieties : ComponentBase
    {
        // Interface Stuff
        public GridSettings _GridSettings = new GridSettings();
        public Guid ParentItemAttributeLookupId { get; set; }

        [Parameter]
        public List<ItemAttributeVarietyLookup> ItemAttributeVarietyLookups { get; set; }
        //[Parameter]

        RainbowOF.Components.Modals.ColorSelector colorFGSelector { get; set; }
        RainbowOF.Components.Modals.ColorSelector colorBGSelector { get; set; }
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }


        public List<ItemAttributeVarietyLookup> attribVarieties = null;  // this is set as the Attribute is selected
        ItemAttributeVarietyLookup SelectedItemAttributeVarietyLookup = null; // used for selected item
        public bool FilterVars = false;
        protected override async Task OnInitializedAsync()
        {
            attribVarieties = await LoadVarieties();
            if (attribVarieties != null)
                ParentItemAttributeLookupId = attribVarieties[0].ItemAttributeLookupId; // set this for future reference.
        }

        async Task<List<ItemAttributeVarietyLookup>> LoadVarieties()
        {
            // need to async this so that the Task is compatible with others
            return await Task.Run(() => ItemAttributeVarietyLookups);
        }

        List<string> _ListOfSymbols = null;

        List<string> GetListOSymbols()
        {
            if (_ListOfSymbols == null)
            {
                _ListOfSymbols = new List<string>();

                for (int i = 32; i < 255; i++)    /// start at 32 to exclude the most common
                {
                    if (Char.IsSymbol((char)i))
                        _ListOfSymbols.Add(char.ToString((char)i));
                }
            }
            return _ListOfSymbols;
        }
        async Task OnVarietyRowInserted(SavedRowItem<ItemAttributeVarietyLookup, Dictionary<string, object>> pInsertedItem)
        {
            var newItem = pInsertedItem.Item;
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // first check if we do not already have a Attribute like this.
            if (await _ItemAttributeVarietyLookupRepository.FindFirstAsync(iavl => iavl.VarietyName == newItem.VarietyName) == null)
            {
                int _recsAdded = await _ItemAttributeVarietyLookupRepository.AddAsync(newItem);
                if (_recsAdded != AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"{newItem.VarietyName} - {_AppUnitOfWork.GetErrorMessage()}", "Attribute variety Added");
                else
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.VarietyName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute variety");
            }
            else
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{newItem.VarietyName} already exists, so could not be added.");
            await LoadVarieties();   // reload the list so the latest item is displayed
        }
        void OnItemAttributeVarietyLookupNewItemDefaultSetter(ItemAttributeVarietyLookup pNewVariety)
        {
            // can figure out how to get the parent id so used the default
            if (pNewVariety == null)
                pNewVariety = new ItemAttributeVarietyLookup();

            pNewVariety.ItemAttributeLookupId = ParentItemAttributeLookupId;  // the selected one should be the parent
            pNewVariety.VarietyName = "Variety (must be unique)";
            pNewVariety.SortOrder = 0;
            pNewVariety.Notes = $"Added {DateTime.Now.Date}";
        }
        async Task<int> UpdateItemAttributeVarietyLookup(ItemAttributeVarietyLookup pUpdatedVariety)
        {
            int _recsUpdted = 0;
            IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
            // first check it exists - it could have been deleted 
            ItemAttributeVarietyLookup _UpdatedLookup = await _ItemAttributeVarietyLookupRepository.GetByIdAsync(pUpdatedVariety.ItemAttributeVarietyLookupId);

            if (_UpdatedLookup == null)
            {
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute variety: {pUpdatedVariety.VarietyName} is no longer found, was it deleted?");
                return AppUnitOfWork.CONST_WASERROR;
            }
            else
            {
                _UpdatedLookup.ItemAttributeLookupId = pUpdatedVariety.ItemAttributeLookupId;
                _UpdatedLookup.VarietyName = pUpdatedVariety.VarietyName;
                _UpdatedLookup.UoMId = (pUpdatedVariety.UoMId == Guid.Empty) ? null : pUpdatedVariety.UoMId;
                _UpdatedLookup.SortOrder = pUpdatedVariety.SortOrder;
                _UpdatedLookup.Symbol = pUpdatedVariety.Symbol;
                _UpdatedLookup.FGColour = (pUpdatedVariety.FGColour == ItemAttributeVarietyLookup.CONST_NULL_COLOUR) ? null : pUpdatedVariety.FGColour;
                _UpdatedLookup.BGColour = (pUpdatedVariety.BGColour == ItemAttributeVarietyLookup.CONST_NULL_COLOUR) ? null : pUpdatedVariety.BGColour;
                _UpdatedLookup.Notes = pUpdatedVariety.Notes;
                //                if (!_UpdatedLookup.Equals(pUpdatedVariety))
                _recsUpdted = await _ItemAttributeVarietyLookupRepository.UpdateAsync(_UpdatedLookup);   //_ItemAttributeVarietyLookupRepository.Update(_UpdatedLookup); // 
                if (_recsUpdted == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"{pUpdatedVariety.VarietyName} - {_AppUnitOfWork.GetErrorMessage()}", "Error adding Attribute variety");
            }
            return _recsUpdted;
        }
        private bool IsDuplicate(ItemAttributeVarietyLookup pItem)
        {
            // check if does not exist in the list already (they edited it and it is the same name as another. Only a max of one should exists
            var _exists = attribVarieties.FindAll(av => av.VarietyName == pItem.VarietyName);
            return ((_exists != null) && (_exists.Count > 1));
        }
        private bool IsValid(ItemAttributeVarietyLookup pItem)
        {
            // check that there is a loop back on PaerentId
            return true;  /// may need a check here
        }
        async Task OnVarietyRowUpdated(SavedRowItem<ItemAttributeVarietyLookup, Dictionary<string, object>> pUpdatedItem)
        {
            ItemAttributeVarietyLookup updatedItem = pUpdatedItem.Item;
            if (IsDuplicate(updatedItem))
                _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute variety Name: {updatedItem.VarietyName} - already exists, cannot be updated", "Exists already");
            else
            {
                if (IsValid(updatedItem))
                {
                    // update and check for errors 
                    if (await UpdateItemAttributeVarietyLookup(updatedItem) != AppUnitOfWork.CONST_WASERROR)
                    {
                        //if ((updatedItem.HasWooAttributeVarietyMap) && (await UpdateWooAttributeVarietyMap(updatedItem) == AppUnitOfWork.CONST_WASERROR))
                        //    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"WooAttributeVariety map for Item: {updatedItem.AttributeVarietyName} - {_AppUnitOfWork.GetErrorMessage()}", "Error updating");
                        ////else
                        ////    PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"AttributeVariety: {updatedItem.AttributeVarietyName} was updated.");
                    }
                }
                else
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"Attribute variety Item {updatedItem.VarietyName} not valid", "Error updating");

            }
            //await LoadItemAttributeVarietyLookupList();   // reload the list so the latest item is displayed
        }
        void OnVarietyRowRemoving(CancellableRowChange<ItemAttributeVarietyLookup> varItem)
        {
            // set the Selected Item AttributeVariety for use later
            SelectedItemAttributeVarietyLookup = varItem.Item;
            var deleteItem = varItem;
            _GridSettings.DeleteConfirmation.ShowModal("Variety delete confirmation", $"Are you sure you want to delete variety: {deleteItem.Item.VarietyName}?");  //,"Delete","Cancel"); - passed in on init
        }

        //protected async Task
        async Task ConfirmVarietyDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed)
            {
                IAppRepository<ItemAttributeVarietyLookup> _ItemAttributeVarietyLookupRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();

                var _recsDelete = await _ItemAttributeVarietyLookupRepository.DeleteByIdAsync(SelectedItemAttributeVarietyLookup.ItemAttributeVarietyLookupId);

                if (_recsDelete == AppUnitOfWork.CONST_WASERROR)
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Error, $"AttributeVariety: {SelectedItemAttributeVarietyLookup.VarietyName} is no longer found, was it deleted?");
                else
                    _GridSettings.PopUpRef.ShowNotification(PopUpAndLogNotification.NotificationType.Success, $"AttributeVariety: {SelectedItemAttributeVarietyLookup.VarietyName} was it deleted?");
            }
            //await LoadItemAttributeVarietyLookupList();   // reload the list so the latest item is displayed
        }

        //
        //  need to move this to an Interface it is reloading the whole time. need to use a list that is set to null on load, then only reload when = null
        //
        Dictionary<Guid, string> _ListOfUoMs = null;

        public Dictionary<Guid, string> GetListOfUoMs()
        {
            if (_ListOfUoMs == null)
            {
                _ListOfUoMs = new Dictionary<Guid, string>();
                IAppRepository<ItemUoM> _UoMRepository = _AppUnitOfWork.Repository<ItemUoM>();
                List<ItemUoM> _ListOfitemUoMs = _UoMRepository.GetAll().ToList();
                if (_ListOfitemUoMs != null)
                    foreach (var item in _ListOfitemUoMs)
                    {
                        _ListOfUoMs.Add(item.ItemUoMId, item.UoMSymbol);
                    }
            }
            return _ListOfUoMs;
        }

    }
}
