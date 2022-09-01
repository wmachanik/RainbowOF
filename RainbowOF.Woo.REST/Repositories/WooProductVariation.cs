using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Woo.REST.Repositories
{
    public class WooProductVariation : IWooProductVariation
    {
        private readonly IWooBase _wooBase;

        public WooProductVariation(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            _wooBase = new WooBase(wooAPISettings, logger);
        }

        private WCObject _localWCObject = null;
        private WCObject getWCObject
        {
            get
            {
                if (_localWCObject == null)
                {
                    RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
                    try
                    {
                        _localWCObject = new WCObject(_RestAPI);
                    }
                    catch (Exception ex)
                    {
                        if (_wooBase.Logger != null)
                            _wooBase.Logger.LogError("Error setting up WOO REST API: " + ex.Message);
                    }
                }
                return _localWCObject;
            }
            set
            {
                _localWCObject = value;
            }
        }
        /// <summary>
        /// Get all the product variant by product id 
        /// </summary>
        /// <param name="parentProductId">The Parent Products Id </param>
        /// <returns>List of Product Variations found or null if not</returns>
        public async Task<List<Variation>> GetProductVariationsByProductIdAsync(uint parentProductId)
        {
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
            List<Variation> wooBaseVariations = null;
            try
            {
                WCObject _WC = new(_RestAPI);
                wooBaseVariations = await _WC.Product.Variations.GetAll(parentProductId);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError("Error calling WOO REST API: " + ex.Message);
            }
            return wooBaseVariations;
        }
        /// <summary>
        /// Get the Product Variation by variation id of the Product with parent Id
        /// </summary>
        /// <param name="sourceWooEntityId">The Id of the Woo Product's Variant</param>
        /// <param name="parentWooVariantId">The Id of the Parent Product</param>
        /// <returns>Product Variation found or null if not</returns>
        public async Task<Variation> GetProductVariationByIdAsync(uint sourceWooEntityId, uint parentWooVariantId)
        {
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
            Variation _Variation = null;
            try
            {
                WCObject _WC = new(_RestAPI);
                _Variation = await _WC.Product.Variations.Get((int)sourceWooEntityId, (int)parentWooVariantId);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return _Variation;
        }
        /// <summary>
        /// Add a variant to a product of id = parentWooProductId
        /// </summary>
        /// <param name="addWooProductVariant"></param>
        /// <param name="parentWooProductId"></param>
        /// <returns>Product Variation added or null if not</returns>
        public async Task<Variation> AddProductVariationAsync(Variation addWooProductVariant, uint parentWooProductId)
        {
            Variation _Variation = null;
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
            try
            {
                WCObject _WC = new(_RestAPI);
                _Variation = await _WC.Product.Variations.Add(addWooProductVariant, (int)parentWooProductId);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"Error calling Add ProductVariantAsync for product: {addWooProductVariant.description}. Error:  {ex.Message}");
            }
            return _Variation;
        }
        /// <summary>
        /// Delete the product Variant with parent product Id = parentWooProductId
        /// </summary>
        /// <param name="deleteWooVariantId">the </param>
        /// <param name="parentWooProductId"></param>
        /// <returns>Product Variation delete or null if not</returns>
        public async Task<string> DeleteProductVariationByIdAsync(uint deleteWooVariantId, uint parentWooProductId)
        {
            string _Variation = null;
            WCObject _WC = getWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _Variation = await _WC.Product.Variations.Delete((int)deleteWooVariantId, (int)parentWooProductId, true);   // force = true
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"Error calling deleting product category by id: {deleteWooVariantId} Async. Error:  {ex.Message}");
            }
            return _Variation;
        }
        /// <summary>
        /// Update a Product Variant of a parent product
        /// </summary>
        /// <param name="updateWooProductVariant">the Woo Product variant that has been updated, and needs to be saved</param>
        /// <param name="parentProductId">the Id of the parent product of the variant</param>
        /// <returns>Product Variation updated or null if not</returns>
        public async Task<Variation> UpdateProductVariationAsync(Variation updateWooProductVariant, uint parentProductId)
        {
            Variation _Variation = null;
            WCObject _WC = getWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _Variation = await _WC.Product.Variations.Update((int)updateWooProductVariant.id, updateWooProductVariant, (int)parentProductId);   // force = true
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"Error calling deleting product category by id: {updateWooProductVariant} Async. Error:  {ex.Message}");
            }
            return _Variation;
        }

    }
}
