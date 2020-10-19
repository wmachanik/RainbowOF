using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class ShowModal : ComponentBase
    {
        protected bool IsShowModal { get; set; }

        [Parameter]
        public string ModalTitle { get; set; } = "Status Message";
        [Parameter]
        public string ModalMessage { get; set; } = "Something Happened";
        [Parameter]
        public EventCallback CloseModel { get; set; }

        public void Show()
        {
            IsShowModal = true;
            StateHasChanged();
        }

        public void UpdateModalMessage(string pMessage)
        {
            ModalMessage = pMessage;
        }

        protected async Task OnCloseModal()
        {
            IsShowModal = false;
            await CloseModel.InvokeAsync(null);
        }
    }
}
