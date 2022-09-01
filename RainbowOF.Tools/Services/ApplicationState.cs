namespace RainbowOF.Tools.Services
{
    public class ApplicationState
    {
        private bool? localWooIsActive { get; set; } = null;
        public bool WooIsActive
        {
            get
            {
                return localWooIsActive ?? false;
            }
            set
            {
                localWooIsActive = value;
            }
        }
        public bool HaveCheckState
        {
            get { return localWooIsActive != null; }
        }
        //public event Action OnChange;
        public void SetWooIsActive(bool inputIsActive)
        {
            localWooIsActive = inputIsActive;
        }
    }
}
