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

        public Modal modalRef;
        public async Task ShowModalAsync()
          =>  await modalRef.Show();
        public async Task HideModalAsync()
          => await modalRef.Hide();
       
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

