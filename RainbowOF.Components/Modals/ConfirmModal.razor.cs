using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class ConfirmModal : ComponentBase
    {
       
        public Modal modalRef;
        protected bool ShowConfirmation { get; set; }

        [Parameter]
        public string ConfirmationTitle { get; set; } = "Confirm";
        [Parameter]
        public string ConfirmationMessage { get; set; } = "Are you sure?";
        [Parameter]
        public Blazorise.Color ConfirmColor { get; set; } = Blazorise.Color.Danger;
        [Parameter]
        public string ConfirmButtonText { get; set; } = "Delete";
        [Parameter]
        public string CancelButtonText { get; set; } = "Cancel";

        [Parameter]
        public EventCallback<bool> ConfirmationClicked { get; set; }

        protected async Task OnConfirmationChange(bool value)
        {
            modalRef.Hide();
            await ConfirmationClicked.InvokeAsync(value);
        }

        internal void SetTitleAndMessage(string modalTitle, string modalMessage)
        {
            ConfirmationTitle = modalTitle;
            ConfirmationMessage = modalMessage;
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
        public void ShowModal(string modalTitle, string modalMessage, Blazorise.Color modalColor)
        {
            ConfirmColor = modalColor;
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, string modalConfirmationButtonText, string modalCancelButtonText)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            ConfirmButtonText = modalConfirmationButtonText;
            CancelButtonText = modalCancelButtonText;
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, 
            string modalConfirmationButtonText, string modalCancelButtonText, 
            Blazorise.Color modalColor)
        {
            ConfirmColor = modalColor;
            ShowModal(modalTitle, modalMessage,modalConfirmationButtonText,modalCancelButtonText);
            ShowModal();
        }
        public void HideModal()
        {
            modalRef.Hide();
        }
    }
}
