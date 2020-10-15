using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST
{
    public interface IWooProducts
    {
        Task<List<Product>> GetProductsOfType(string pProductType);
        Task<List<Product>> GetAllProducts();
        Task<bool> CheckProductLink();
        Task<int> GetProductCount();
    }
}
