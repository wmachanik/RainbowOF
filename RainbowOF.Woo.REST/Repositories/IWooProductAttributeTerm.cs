using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProductAttributeTerm
    {
        Task<List<ProductAttributeTerm>> GetAttributeTermsByAtttribute(ProductAttribute sourceProductAttribute);
        Task<ProductAttributeTerm> AddProductAttributeTermAsync(ProductAttributeTerm addWooProductAttribute, int parentWooAttributeId);
        Task<ProductAttributeTerm> GetProductAttributeTermByIdAsync(int sourceEntityId, int parentWooAttributeId);
        Task<string> DeleteProductAttributeTermByIdAsync(int deleteWooAttributeTermId, int deleteWooAttributeId);
        //        Task<List<ProductAttributeTerm>> GetAttributeTermsByProduct(Product pProduct);
        Task<ProductAttributeTerm> UpdateProductAttributeTermAsync(ProductAttributeTerm updateWooProductAttribute, int parentWooAttributeId);
    }
}
