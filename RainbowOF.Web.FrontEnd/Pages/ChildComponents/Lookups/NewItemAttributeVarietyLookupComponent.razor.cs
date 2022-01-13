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

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Lookups
{
    public partial class NewItemAttributeVarietyLookupComponent : ComponentBase
    {
        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }
        [Parameter]
        public bool AddItemToWoo { get; set; } = false;

        private Modal NewAttributeVarietyLookupModalRef;

        public ItemAttributeVarietyLookupView _NewItemAttributeVarietyLookupView = new ();

        public async Task ShowModalAsync(Guid parentItemAttributeId)
        {
            _NewItemAttributeVarietyLookupView.ItemAttributeLookupId = parentItemAttributeId;

            _NewItemAttributeVarietyLookupView.VarietyName = "New";
            _NewItemAttributeVarietyLookupView.SortOrder = 0;
            _NewItemAttributeVarietyLookupView.UoM = null;
            _NewItemAttributeVarietyLookupView.Notes = $"Added: {DateTime.Now:f}";
            _NewItemAttributeVarietyLookupView.FGColour = String.Empty;
            _NewItemAttributeVarietyLookupView.BGColour = String.Empty;
            _NewItemAttributeVarietyLookupView.Symbol = String.Empty; 
            _NewItemAttributeVarietyLookupView.CanUpdateECommerceMap = AddItemToWoo ? true : null;  // if they want to add woo then do so here
            await NewAttributeVarietyLookupModalRef.Show();
        }

        private async Task HideModal(bool SaveClicked)
        {
            if (SaveClicked)
            {
                IAppRepository<ItemAttributeVarietyLookup> _appRepository = _AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
                if (_appRepository != null)
                {
                    var _result = await _appRepository.AddAsync(_NewItemAttributeVarietyLookupView);
                    if (_result == null)
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Error, $"Error adding new attribute variety: {_NewItemAttributeVarietyLookupView.VarietyName}");
                    else
                        PopUpRef.ShowQuickNotification(PopUpAndLogNotification.NotificationType.Success, $"Attribute variety: {_NewItemAttributeVarietyLookupView.VarietyName} added.");
                }
            }
            await NewAttributeVarietyLookupModalRef.Hide();
        }
    }
}
