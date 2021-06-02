using Blazorise;
using Microsoft.AspNetCore.Components;

namespace RainbowOF.Components.Modals
{
    public partial class ShowModalMessage : ComponentBase
    {
        [Parameter]
        public string ModalTitle { get; set; } = "Title";
        [Parameter]
        public string ModalMessage { get; set; } = "Message";

        public Modal modalRef;
        public void ShowModal()
        {
            modalRef.Show();
        }
        public void HideModal()
        {
            modalRef.Hide();
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

