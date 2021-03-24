using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProductAttributeTerm
    {
        Task<List<ProductAttributeTerm>> GetAttributeTermsByAtttribute(ProductAttribute pProductAttribute);
//        Task<List<ProductAttributeTerm>> GetAttributeTermsByProduct(Product pProduct);
    }
}
