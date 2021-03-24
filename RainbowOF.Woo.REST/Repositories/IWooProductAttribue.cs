using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProductAttribute
    {
        Task<List<ProductAttribute>> GetProductAttributesOfType(string pProductAttributeType);
        Task<List<ProductAttribute>> GetAllProductAttributes();
        Task<bool> CheckProductAttributeLink();
        Task<int> GetProductAttributeCount();
    }
}
