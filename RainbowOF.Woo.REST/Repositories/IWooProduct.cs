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
        //// all UI related stuff here
        #region DataHandlingStuffForUIIntegration
        Task<Product> GetProductByIdAsync(int deleteWooEntityId);
        Task<Product> AddProductAsync(Product addWooProduct);
        Task<Product> DeleteProductByIdAsync(int deleteWooProductId);
        Task<Product> UpdateProductAsync(Product updateWooProduct);
        #endregion
    }
}
