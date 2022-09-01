using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class ConfirmModal : ComponentBase
    {
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
        public string CancelButtonText { get; set; } = "Cancel";
        [Parameter]
        public EventCallback<bool> ConfirmationClicked { get; set; }

        #region internal variables
        private string confirmationTitle { get; set; }
        private string confirmationMessage { get; set; }
        private Blazorise.Color confirmColor { get; set; }
        private string confirmButtonText { get; set; }
        private string cancelButtonText { get; set; }
        #endregion
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.confirmationTitle = ConfirmationTitle;
            this.confirmationMessage = ConfirmationMessage;
            this.confirmColor = ConfirmColor;
            this.confirmButtonText = ConfirmButtonText;
            this.cancelButtonText = CancelButtonText;
        }
        protected async Task OnConfirmationChangeAsync(bool value)
        {
            await modalRef.Hide();
            await ConfirmationClicked.InvokeAsync(value);
        }
        private void SetTitleAndMessage(string modalTitle, string modalMessage)
        {
            confirmationTitle = modalTitle;
            confirmationMessage = modalMessage;
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
        public async Task ShowModalAsync(string modalTitle, string modalMessage, Blazorise.Color modalColor)
        {
            confirmColor = modalColor;
            SetTitleAndMessage(modalTitle, modalMessage);
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage, string modalConfirmationButtonText, string modalCancelButtonText)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            confirmButtonText = modalConfirmationButtonText;
            cancelButtonText = modalCancelButtonText;
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage,
            string modalConfirmationButtonText, string modalCancelButtonText,
            Blazorise.Color modalColor)
        {
            confirmColor = modalColor;
            await ShowModalAsync(modalTitle, modalMessage, modalConfirmationButtonText, modalCancelButtonText);
        }
        public async Task HideModalAsync()
        {
            await modalRef.Hide();
        }
    }
}
