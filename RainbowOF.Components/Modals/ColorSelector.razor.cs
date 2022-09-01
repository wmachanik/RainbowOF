using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class ColorSelector : ComponentBase
    {
        private string _value;
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


        Task OnClick(string value)
        {
            ColourValue = value;

            return Task.CompletedTask;
        }

        private Modal modalRef { get; set; }

        public async Task ShowModalAsync()  //ModalSize modalSize, int? maxHeight = null, bool centered = false)
        {
            //this.centered = centered;
            //this.modalSize = modalSize;
            //this.maxHeight = maxHeight;

            await modalRef.Show();
        }

        public async Task HideModalAsync()
            => await modalRef.Hide();

    }
}
