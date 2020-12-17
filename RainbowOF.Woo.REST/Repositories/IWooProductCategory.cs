using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST
{
    public interface IWooProductCategory
    {
        Task<List<ProductCategory>> GetProductCategoriesOfType(string pProductCategoryType);
        Task<List<ProductCategory>> GetAllProductCategories();
        Task<bool> CheckProductCategoryLink();
        Task<int> GetProductCategoryCount();
    }
}
