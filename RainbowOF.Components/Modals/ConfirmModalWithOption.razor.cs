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
        protected bool _OptionIsConfirmed { get; set; } = false;
        
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

        #region internal variables
        private string _ConfirmationTitle;
        private string _ConfirmationMessage;
        private Blazorise.Color _ConfirmColor;
        private string _ConfirmButtonText;
        private bool _ShowConfirmOption;
        private string _ConfirmOptionCheckText;
        private string _CancelButtonText;
        #endregion

        protected override void OnInitialized()
        {
            _ConfirmationTitle = ConfirmationTitle;
            _ConfirmationMessage = ConfirmationMessage;
            _ConfirmColor = ConfirmColor;
            _ConfirmButtonText = ConfirmButtonText;
            _ShowConfirmOption = ShowConfirmOption;
            _ConfirmOptionCheckText = ConfirmOptionCheckText;
            _CancelButtonText = CancelButtonText;
        }
        protected async Task OnConfirmationChange(bool confirmed)
        {
            modalRef.Hide();
            await ConfirmationClicked.InvokeAsync(
                confirmed ? (_OptionIsConfirmed ? ConfirmResults.confirmWithOption : ConfirmResults.confirm)
                          : ConfirmResults.cancel);
        }

        internal void SetTitleAndMessage(string pTitle, string pMessage)
        {
            _ConfirmationTitle = pTitle;
            _ConfirmationMessage = pMessage;
        }
        /// <summary>
        /// ShowModal
        /// </summary>
        /// <param name="Title">Modal Title (optional)</param>
        /// <param name="Message">Modal Message (optional)</param>
        /// <param name="Color">Modal Colour</param>
        public void ShowModal()
        {
            StateHasChanged();
            modalRef.Show();
        }

        public void ShowModal(string modalTitle)
        {
            _ConfirmationTitle = modalTitle;
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, bool modalShowConfirmation)
        {
            _ShowConfirmOption = modalShowConfirmation;
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, string modalOptionText, bool modalShowConfirmation)
        {
            _ShowConfirmOption = modalShowConfirmation;
            _ConfirmOptionCheckText = modalOptionText;
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, Blazorise.Color modalColour)
        {
            _ConfirmColor = modalColour;
            SetTitleAndMessage(modalTitle, modalMessage);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, 
            string modalConfirmationButtonText, string modalCancelButtonText)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            _ConfirmButtonText = modalConfirmationButtonText;
            _CancelButtonText = modalCancelButtonText;
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage, 
            string modalConfirmationButtonText, string modalCancelButtonText, 
            Blazorise.Color modalColour)
        {
            _ConfirmColor = modalColour;
            ShowModal(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage,
            string modalOptionText, string modalConfirmationButtonText, string modalCancelButtonText,
            Blazorise.Color modalColour)
        {
            _ConfirmOptionCheckText = modalOptionText;
            _ConfirmColor = modalColour;
            ShowModal(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
            ShowModal();
        }
        public void ShowModal(string modalTitle, string modalMessage,
          string modalOptionText, string modalConfirmationButtonText, string modalCancelButtonText)
        {
            _ConfirmOptionCheckText = modalOptionText;
            ShowModal(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
            ShowModal();
        }
        public void HideModal()
        {
            modalRef.Hide();
        }
    }
}
