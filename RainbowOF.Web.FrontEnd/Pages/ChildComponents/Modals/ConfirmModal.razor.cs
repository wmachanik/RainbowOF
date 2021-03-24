using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Modals
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

        public void ShowModal(string pTitle)
        {
            ConfirmationTitle = pTitle;
            ShowModal();
        }
        public void ShowModal(string pTitle, string pMessage)
        {
            SetTitleAndMessage(pTitle, pMessage);
            ShowModal();
        }
        public void ShowModal(string pTitle, string pMessage, Blazorise.Color pColor)
        {
            SetTitleAndMessage(pTitle, pMessage);
            ShowModal();
        }
        public void ShowModal(string pTitle, string pMessage, string pConfirmationButtonText, string pCancelButtonText)
        {
            SetTitleAndMessage(pTitle, pMessage);
            ConfirmButtonText = pConfirmationButtonText;
            CancelButtonText = pCancelButtonText;
            ShowModal();
        }

        public void ShowModal(string pTitle, string pMessage, 
            string pConfirmationButtonText, string pCancelButtonText, 
            Blazorise.Color pColor)
        {
            ConfirmColor = pColor;
            ShowModal(pTitle, pMessage,pConfirmationButtonText,pCancelButtonText);
            ShowModal();
        }

        public void HideModal()
        {
            modalRef.Hide();
        }
    }
}
