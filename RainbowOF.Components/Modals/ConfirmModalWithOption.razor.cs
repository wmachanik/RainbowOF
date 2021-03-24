using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class ConfirmModalWithOption : ComponentBase
    {
        public enum ConfirmResults
        {
            cancel,
            confirm,
            confirmWithOption,
            none
        }

        public Modal modalRef;
        protected bool OptionIsConfirmed { get; set; } = false;
        
        [Parameter]
        public string ConfirmationTitle { get; set; } = "Confirm";
        [Parameter]
        public string ConfirmationMessage { get; set; } = "Are you sure?";
        [Parameter]
        public Blazorise.Color ConfirmColor { get; set; } = Blazorise.Color.Danger;
        [Parameter]
        public string ConfirmButtonText { get; set; } = "Delete";
        [Parameter]
        public bool ShowConfirmOption { get; set; } = false;
        [Parameter]
        public string ConfirmOptionCheckText { get; set; } = "Option";
        [Parameter]
        public string CancelButtonText { get; set; } = "Cancel";
        [Parameter]
        public EventCallback<ConfirmResults> ConfirmationClicked { get; set; }

        protected async Task OnConfirmationChange(bool confirmed)
        {
            modalRef.Hide();
            await ConfirmationClicked.InvokeAsync(
                confirmed ? (OptionIsConfirmed ? ConfirmResults.confirmWithOption : ConfirmResults.confirm)
                          : ConfirmResults.cancel);
        }

        internal void SetTitleAndMessage(string pTitle, string pMessage)
        {
            ConfirmationTitle = pTitle;
            ConfirmationMessage = pMessage;
        }
        /// <summary>
        /// ShowModal
        /// </summary>
        /// <param name="Title">Modal Title (optional)</param>
        /// <param name="Message">Modal Message (optional)</param>
        /// <param name="Color">Modal Colour</param>
        public void ShowModal()
        {
            modalRef.Show();
        }

        public void ShowModal(string modalTitle)
        {
            ConfirmationTitle = modalTitle;
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, bool modalShowConfirmation)
        {
            ShowConfirmOption = modalShowConfirmation;
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, Blazorise.Color modalColour)
        {
            ConfirmColor = modalColour;
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, 
            string modalConfirmationButtonText, string modalCancelButtonText)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            ConfirmButtonText = modalConfirmationButtonText;
            CancelButtonText = modalCancelButtonText;
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, 
            string modalConfirmationButtonText, string modalCancelButtonText, 
            Blazorise.Color modalColour)
        {
            ConfirmColor = modalColour;
            ShowModal(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage,
            string modalOptionText, string modalConfirmationButtonText, string modalCancelButtonText,
            Blazorise.Color modalColour)
        {
            ConfirmOptionCheckText = modalOptionText;
            ConfirmColor = modalColour;
            ShowModal(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage,
          string modalOptionText, string modalConfirmationButtonText, string modalCancelButtonText)
        {
            ConfirmOptionCheckText = modalOptionText;
            ShowModal(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
            ShowModal();
        }

        public void HideModal()
        {
            modalRef.Hide();
        }
    }
}
