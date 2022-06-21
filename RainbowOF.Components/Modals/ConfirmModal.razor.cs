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
        private string _ConfirmationTitle;
        private string _ConfirmationMessage;
        private Blazorise.Color _ConfirmColor;
        private string _ConfirmButtonText;
        private string _CancelButtonText;
        #endregion
        protected override void OnInitialized()
        {
            base.OnInitialized();
            _ConfirmationTitle = ConfirmationTitle;
            _ConfirmationMessage = ConfirmationMessage;
            _ConfirmColor = ConfirmColor;
            _ConfirmButtonText = ConfirmButtonText;
            _CancelButtonText = CancelButtonText;
        }
        protected async Task OnConfirmationChangeAsync(bool value)
        {
            await modalRef.Hide();
            await ConfirmationClicked.InvokeAsync(value);
        }
        internal void SetTitleAndMessage(string modalTitle, string modalMessage)
        {
            _ConfirmationTitle = modalTitle;
            _ConfirmationMessage = modalMessage;
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
            _ConfirmationTitle = modalTitle;
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage, Blazorise.Color modalColor)
        {
            _ConfirmColor = modalColor;
            SetTitleAndMessage(modalTitle, modalMessage);
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage, string modalConfirmationButtonText, string modalCancelButtonText)
        {
            SetTitleAndMessage(modalTitle, modalMessage);
            _ConfirmButtonText = modalConfirmationButtonText;
            _CancelButtonText = modalCancelButtonText;
            await ShowModalAsync();
        }
        public async Task ShowModalAsync(string modalTitle, string modalMessage, 
            string modalConfirmationButtonText, string modalCancelButtonText, 
            Blazorise.Color modalColor)
        {
            _ConfirmColor = modalColor;
            await ShowModalAsync(modalTitle, modalMessage,modalConfirmationButtonText,modalCancelButtonText);
        }
        public async Task HideModalAsync()
        {
            await modalRef.Hide();
        }
    }
}
