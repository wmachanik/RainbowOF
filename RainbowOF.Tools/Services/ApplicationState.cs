using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Tools.Services
{
    public class ApplicationState
    {
        public  bool WooIsActive 
        { get 
            {
                return _WooIsActive ?? false;
            }
            set
            {
                _WooIsActive = value;
            }
        }
        public bool HaveCheckState
        {
            get { return _WooIsActive != null; }
        }
        //public event Action OnChange;

        private bool? _WooIsActive { get; set; } = null;

        public void SetWooIsActive(bool inputIsActive)
        {
            _WooIsActive = inputIsActive;
        }
    }
}
