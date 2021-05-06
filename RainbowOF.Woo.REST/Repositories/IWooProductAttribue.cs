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
        Task<ProductAttribute> GetProductAttributeByIdAsync(int deleteWooEntityId);
        Task<ProductAttribute> DeleteProductAttributeByIdAsync(int deleteWooProductAttributeId);
        Task<ProductAttribute> AddProductAttributeAsync(ProductAttribute addWooProductAttribute);
        //        Task<ProductAttribute> FindProductAttributeByNameAsync(string findAttributeName); -> woocommerce does not support a search by string
        Task<ProductAttribute> UpdateProductAttributeAsync(ProductAttribute updateWooProductAttribute);

    }
}
