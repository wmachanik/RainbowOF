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



        //    protected bool IsShowModal { get; set; }
        //    private Modal modalRef; 

        //    [Parameter]
        //    public string ModalTitle { get; set; } = "Status Message";
        //    [Parameter]
        //    public string ModalMessage { get; set; } = "Something Happened";
        //    public void ShowModal()
        //    {
        //        //IsShowModal = true;
        //        modalRef.Show();
        //        StateHasChanged();
        //    }

        //    public void UpdateModalMessage(string pMessage)
        //    {
        //        ModalMessage = pMessage;
        //        StateHasChanged();
        //    }
        //    private void HideModal()
        //    {
        //        modalRef.Hide();
        //        StateHasChanged();
        //    }
        //    protected void OnModalClosing(CancelEventArgs e)
        //    {
        //        e.Cancel = true;
        //        HideModal(); 
        //    }
        //}
    }
}


//1. not saving pref when no records exists
//2. modal dialog fo saved on closing is OnClosed being called