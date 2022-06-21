using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class ShowModalMessage : ComponentBase
    {
        [Parameter]
        public string ModalTitle { get; set; } = "Title";
        [Parameter]
        public string ModalMessage { get; set; } = "Message";

        private Modal modalRef;

        public Task ShowModalAsync()
        {
            return modalRef.Show();
        }
        public Task HideModalAsync()
        {
            return modalRef.Hide();
        }

        public void UpdateModalTitle(string pTitle)
        {
            ModalTitle = pTitle;
        }

        public void UpdateModalMessage(string pMessage)
        {
            ModalMessage = pMessage;
        }
    }
}

