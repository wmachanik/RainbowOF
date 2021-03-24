using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class ColorSelector : ComponentBase
    {
        [Parameter]
        public string ColourValue
        {
            get => _value;
            set
            {
                if (value == _value)
                {
                    return;
                }
                _value = value;
                this.StateHasChanged();
                ColourValueChanged.InvokeAsync(value);
            }
        }

        string ClassNames(string value)
            => $"color-item{(value == ColourValue ? " selected" : "")}";

        [Parameter]
        public EventCallback<string> ColourValueChanged { get; set; }

        private string _value;

        Task OnClick(string value)
        {
            ColourValue = value;

            return Task.CompletedTask;
        }

        private Modal modalRef;

        public void ShowModal()  //ModalSize modalSize, int? maxHeight = null, bool centered = false)
        {
            //this.centered = centered;
            //this.modalSize = modalSize;
            //this.maxHeight = maxHeight;

            modalRef.Show();
        }

        public void HideModal()
        {
            modalRef.Hide();
        }
    }
}
