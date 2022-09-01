using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProduct
    {
        Task<List<Product>> GetProductsOfTypeAsync(string pProductType);
        Task<List<Product>> GetAllProductsAsync();
        Task<List<Product>> GetAllProductsInStockAsync();
        Task<bool> CheckProductLinkAsync();
        Task<int> GetProductCountAsync();
        //// all UI related stuff here
        #region DataHandlingStuffForUIIntegration
        Task<Product> GetProductByIdAsync(int deleteWooEntityId);
        Task<Product> AddProductAsync(Product addWooProduct);
        Task<Product> DeleteProductByIdAsync(int deleteWooProductId);
        Task<Product> UpdateProductAsync(Product updateWooProduct);
        #endregion
    }
}
