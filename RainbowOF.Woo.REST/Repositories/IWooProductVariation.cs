using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;


namespace RainbowOF.Woo.REST.Repositories
{
    public interface IWooProductVariation
    {
        Task<List<Variation>> GetProductVariationsByProductIdAsync(uint parentProductId);
        Task<Variation> AddProductVariationAsync(Variation addWooProductVariation, uint parentWooVariantId);
        Task<Variation> GetProductVariationByIdAsync(uint sourceEntityId, uint parentWooProductId);
        Task<string> DeleteProductVariationByIdAsync(uint deleteWooVariantId, uint parentWooProductId);
        Task<Variation> UpdateProductVariationAsync(Variation updateWooProductVariation, uint parentProductId);
    }
}
