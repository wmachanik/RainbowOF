using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProduct
    {
        Task<List<Product>> GetProductsOfType(string pProductType);
        Task<List<Product>> GetAllProducts();
        Task<List<Product>> GetAllProductsInStock();
        Task<bool> CheckProductLink();
        Task<int> GetProductCount();
    }
}
