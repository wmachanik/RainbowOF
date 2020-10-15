using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public class ShowModalBase : ComponentBase
    {
        protected bool ShowModal { get; set; }

        [Parameter]
        public string ModalTitle { get; set; } = "Status Message";
        [Parameter]
        public string ModalMessage { get; set; } = "Something Happened";
        [Parameter]
        public EventCallback CloseModel { get; set; }

        public void Show()
        {
            ShowModal = true;
            StateHasChanged();
        }

        public void UpdateModalMessage(string pMessage)
        {
            ModalMessage = pMessage;
        }

        protected async Task OnCloseModal()
        {
            ShowModal = false;
            await CloseModel.InvokeAsync(null);
        }
    }
}
