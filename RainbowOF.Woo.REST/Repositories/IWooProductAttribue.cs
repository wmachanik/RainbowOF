using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProductAttribute
    {
        Task<List<ProductAttribute>> GetProductAttributesOfTypeAsync(string pProductAttributeType);
        Task<List<ProductAttribute>> GetAllProductAttributesAsync();
        Task<bool> CheckProductAttributeLinkAsync();
        Task<int> GetProductAttributeCountAsync();
        Task<ProductAttribute> GetProductAttributeByIdAsync(int sourceWooEntityId);
        Task<ProductAttribute> DeleteProductAttributeByIdAsync(int deleteWooProductAttributeId);
        Task<ProductAttribute> AddProductAttributeAsync(ProductAttribute addWooProductAttribute);
        //        Task<ProductAttribute> FindProductAttributeByNameAsync(string findAttributeName); -> woocommerce does not support a search by string
        Task<ProductAttribute> UpdateProductAttributeAsync(ProductAttribute updateWooProductAttribute);

    }
}
