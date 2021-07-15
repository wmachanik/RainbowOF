using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels.Lookups
{
    public class ECommerceOption
    {
        public bool? CanUpdateECommerceMap { get; set; }
        public bool HasECommerceAttributeMap
        {
            get { return (CanUpdateECommerceMap != null); }
        }
    }
}
