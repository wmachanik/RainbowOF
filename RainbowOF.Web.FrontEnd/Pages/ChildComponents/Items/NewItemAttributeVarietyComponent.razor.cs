using Blazorise;
using Microsoft.AspNetCore.Components;
using RainbowOF.Components.Modals;
using RainbowOF.Models.Lookups;
using RainbowOF.Repositories.Common;
using RainbowOF.ViewModels.Lookups;
using System;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class NewItemAttributeVarietyComponent : ComponentBase
    {
        [Inject]
        public IUnitOfWork AppUnitOfWork { get; set; }
        [Parameter]
        public PopUpAndLogNotification PopUpRef { get; set; }

        private Modal newAttributeVarietyModalRef { get; set; }

        private ItemAttributeVarietyLookupView newItemAttributeVarietyLookupView { get; set; } = new();

        public async Task ShowModalAsync(Guid parentItemAttributeId)
        {
            newItemAttributeVarietyLookupView.ItemAttributeLookupId = parentItemAttributeId;

            newItemAttributeVarietyLookupView.VarietyName = "New";
            newItemAttributeVarietyLookupView.SortOrder = 0;
            newItemAttributeVarietyLookupView.UoM = null;
            newItemAttributeVarietyLookupView.Notes = $"Added: {DateTime.Now:f}";
            newItemAttributeVarietyLookupView.FGColour = String.Empty;
            newItemAttributeVarietyLookupView.BGColour = String.Empty;
            newItemAttributeVarietyLookupView.Symbol = String.Empty;
            await newAttributeVarietyModalRef.Show();
        }

        private async Task HideModal(bool SaveClicked)
        {
            if (SaveClicked)
            {
                IRepository<ItemAttributeVarietyLookup> _appRepository = AppUnitOfWork.Repository<ItemAttributeVarietyLookup>();
                if (_appRepository != null)
                {
                    var _result = await _appRepository.AddAsync(newItemAttributeVarietyLookupView);
                    if (_result == null)
                        await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Error, $"Error adding new attribute variety: {newItemAttributeVarietyLookupView.VarietyName}");
                    else
                        await PopUpRef.ShowQuickNotificationAsync(PopUpAndLogNotification.NotificationType.Success, $"Attribute variety: {newItemAttributeVarietyLookupView.VarietyName} added.");
                }
            }
            await newAttributeVarietyModalRef.Hide();
        }
    }
}
