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
        private string confirmationTitle { get; set; }
        private string confirmationMessage { get; set; }
        private Blazorise.Color confirmColor { get; set; }
        private string confirmButtonText { get; set; }
        private bool showConfirmOption { get; set; }
        private string confirmOptionCheckText { get; set; }
        private string cancelButtonText { get; set; }
        #endregion

        protected override void OnInitialized()
        {
            confirmationTitle = ConfirmationTitle;
            confirmationMessage = ConfirmationMessage;
            confirmColor = ConfirmColor;
            confirmButtonText = ConfirmButtonText;
            showConfirmOption = ShowConfirmOption;
            confirmOptionCheckText = ConfirmOptionCheckText;
            cancelButtonText = CancelButtonText;
        }

        private bool _optionIsConfirmed = false;
        protected async Task OnConfirmationChange(bool confirmed)
        {
            await modalRef.Hide();
            await ConfirmationClicked.InvokeAsync(
                confirmed ? (_optionIsConfirmed ? ConfirmResults.confirmWithOption : ConfirmResults.confirm)
                          : ConfirmResults.cancel);
        }

        internal void SetTitleAndMessage(string pTitle, string pMessage)
        {
            confirmationTitle = pTitle;
            confirmationMessage = pMessage;
        }
        /// <summary>
        /// ShowModal
        /// </summary>
        /// <param name="Title">Modal Title (optional)</param>
        /// <param name="Message">Modal Message (optional)</param>
        /// <param name="Color">Modal Colour</param>
        public async Task ShowModalAsync()
        {
            StateHasChanged();
            await modalRef.Show();
        }

        public async Task ShowModalAsync(string modalTitle)
        {
            confirmationTitle = modalTitle;
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage, bool modalShowConfirmation)
        {
            showConfirmOption = modalShowConfirmation;
            SetTitleAndMessage(modalTitle, modalMessage);
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage, string modalOptionText, bool modalShowConfirmation)
        {
            showConfirmOption = modalShowConfirmation;
            confirmOptionCheckText = modalOptionText;
            SetTitleAndMessage(modalTitle, modalMessage);
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage, Blazorise.Color modalColour)
        {
            confirmColor = modalColour;
            SetTitleAndMessage(modalTitle, modalMessage);
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage,
            string modalConfirmationButtonText, string modalCancelButtonText)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            confirmButtonText = modalConfirmationButtonText;
            cancelButtonText = modalCancelButtonText;
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage,
            string modalConfirmationButtonText, string modalCancelButtonText,
            Blazorise.Color modalColour)
        {
            confirmColor = modalColour;
            await ShowModalAsync(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage,
            string modalOptionText, string modalConfirmationButtonText, string modalCancelButtonText,
            Blazorise.Color modalColour)
        {
            confirmOptionCheckText = modalOptionText;
            confirmColor = modalColour;
            await ShowModalAsync(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage,
          string modalOptionText, string modalConfirmationButtonText, string modalCancelButtonText)
        {
            confirmOptionCheckText = modalOptionText;
            await ShowModalAsync(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
        }
        public async Task HideModalAsync() => await modalRef.Hide();
    }
}
