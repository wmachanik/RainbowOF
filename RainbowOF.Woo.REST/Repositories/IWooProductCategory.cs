using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProductCategory
    {
        Task<List<ProductCategory>> GetProductCategoriesOfTypeAsync(string pProductCategoryType);
        Task<List<ProductCategory>> GetAllProductCategoriesAsync();
        Task<bool> CheckProductCategoryLinkAsync();
        Task<int> GetProductCategoryCountAsync();
        Task<ProductCategory> GetProductCategoryByIdAsync(uint deleteWooEntityId);
        Task<ProductCategory> DeleteProductCategoryByIdAsync(uint deleteWooProductCategoryId);
        Task<ProductCategory> AddProductCategoryAsync(ProductCategory addWooProductCategory);
//        Task<ProductCategory> FindProductCategoryByNameAsync(string findCategoryName); -> woocommerce does not support a search by string
        Task<ProductCategory> UpdateProductCategoryAsync(ProductCategory updateWooProductCategory);
    }
}
