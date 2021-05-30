using Blazorise;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class NewItemAttributeVarietyComponent : ComponentBase
    {
        [Inject]
        IAppUnitOfWork _appUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public bool AddItemToWoo { get; set; } = false;

        private Modal NewAttributeVarietieModalRef;

        public ItemAttributeVarietyLookupView _NewItemAttributeVarietyLookupView = new ItemAttributeVarietyLookupView();

        public void ShowModal(Guid parentItemAttributeId) // (ModalSize modalSize, int? maxHeight = null, bool centered = false)
        {
            _NewItemAttributeVarietyLookupView.ItemAttributeLookupId = parentItemAttributeId;

            _NewItemAttributeVarietyLookupView.VarietyName = "New";
            _NewItemAttributeVarietyLookupView.SortOrder = 0;
            _NewItemAttributeVarietyLookupView.UoM = null;
            _NewItemAttributeVarietyLookupView.Notes = $"Added: {DateTime.Now:f}";
            _NewItemAttributeVarietyLookupView.FGColour = String.Empty;
            _NewItemAttributeVarietyLookupView.BGColour = String.Empty;
            _NewItemAttributeVarietyLookupView.Symbol = String.Empty; 
            _NewItemAttributeVarietyLookupView.CanUpdateWooMap = AddItemToWoo ? true : null;  // if they want to add woo then do so here
            NewAttributeVarietieModalRef.Show();
        }

        private async Task HideModal(bool SaveClicked)
        {
            if (SaveClicked)
            {
                IAppRepository<ItemAttributeVarietyLookup> appRepository = _appUnitOfWork.Repository<ItemAttributeVarietyLookup>();
                if (appRepository != null)
                {
                    int _result = await appRepository.AddAsync(_NewItemAttributeVarietyLookupView);
                    if (_result == AppUnitOfWork.CONST_WASERROR)
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding new attribute variety: {_NewItemAttributeVarietyLookupView.VarietyName}");
                    else
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute variety: {_NewItemAttributeVarietyLookupView.VarietyName} added.");
                }
            }
            NewAttributeVarietieModalRef.Hide();
        }
    }
}
