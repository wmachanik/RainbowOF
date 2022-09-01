namespace RainbowOF.ViewModels.Common
{
    public class ClassHasChanged<TEntity> where TEntity : class
    {
        public bool HasChanged { get; set; } = false;
        public TEntity Entity { get; set; }
    }
}
