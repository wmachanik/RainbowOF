using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Woo.REST.Repositories
{
    public class WooProductAttribute : IWooProductAttribute
    {
        private readonly WooBase _Woo;
        public WooProductAttribute(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            _Woo = new WooBase(wooAPISettings, logger);
        }
        private async Task<List<ProductAttribute>> GetAll(Dictionary<string, string> ProductAttributeParams)
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            List<ProductAttribute> _WooProductAttributes = null;
            //----- Get all the attributes not x per page like the other calls
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                _WooProductAttributes = await _WC.Attribute.GetAll(ProductAttributeParams);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST API: " + ex.Message);
            }
            return _WooProductAttributes;
        }
        public async Task<List<ProductAttribute>> GetAllProductAttributesAsync()
        {
            return await GetAll(null);
        }

        public async Task<bool> CheckProductAttributeLinkAsync()
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            int count = 0;
            try
            {
                WCObject _WC = new(_RestAPI);
                List<ProductAttribute> TenProductAttributes = await _WC.Attribute.GetAll();
                count = TenProductAttributes.Count;
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return count > 0;
        }

        public async Task<List<ProductAttribute>> GetProductAttributesOfTypeAsync(string pProductAttributeType)
        {
            return await GetAll(new Dictionary<string, string>() { { "type", pProductAttributeType } });
        }

        public async Task<int> GetProductAttributeCountAsync()
        {
            int _count = 0;
            RestAPI _RestAPI = _Woo.GetRootRestAPI;
            try
            {
                string Result = await _RestAPI.GetRestful("attributes/count");

                if (Result.Contains("count"))
                {
                    int _from = Result.IndexOf(":");
                    int _to = Result.IndexOf("}");
                    _count = Convert.ToInt32(Result.Substring(_from + 1, _to - _from - 1));
                }
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST ROOT API: " + ex.Message);
            }
            return _count;
        }

        public async Task<ProductAttribute> GetProductAttributeByIdAsync(int sourceWooEntityId)
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            ProductAttribute _productAttribute = null;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                _productAttribute = await _WC.Attribute.Get(sourceWooEntityId);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return _productAttribute;
        }

        public async Task<ProductAttribute> DeleteProductAttributeById(int wooProductAttributeId)
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            ProductAttribute _productAttribute = null;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                _productAttribute = await _WC.Attribute.Delete(wooProductAttributeId);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return _productAttribute;
        }

        public async Task<ProductAttribute> DeleteProductAttributeByIdAsync(int deleteWooProductAttributeId)
        {
            ProductAttribute _ProductAttribute = null;
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            try
            {
                WCObject _WC = new(_RestAPI);
                _ProductAttribute = await _WC.Attribute.Delete(deleteWooProductAttributeId, true);   // force = true
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling deleting product Attribute by id: {deleteWooProductAttributeId} Async. Error:  {ex.Message}");
            }
            return _ProductAttribute;
        }

        public async Task<ProductAttribute> AddProductAttributeAsync(ProductAttribute addWooProductAttribute)
        {
            ProductAttribute _ProductAttribute = null;
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            try
            {
                WCObject _WC = new(_RestAPI);
                _ProductAttribute = await _WC.Attribute.Add(addWooProductAttribute);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling Add ProductAttributeAsync for product: {addWooProductAttribute.name}. Error:  {ex.Message}");
            }
            return _ProductAttribute;
        }


        public async Task<ProductAttribute> UpdateProductAttributeAsync(ProductAttribute updateWooProductAttribute)
        {
            ProductAttribute _ProductAttribute = null;
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            try
            {
                WCObject _WC = new(_RestAPI);
                _ProductAttribute = await _WC.Attribute.Update((int)updateWooProductAttribute.id, updateWooProductAttribute);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling Update ProductAttributeAsync for product: {updateWooProductAttribute.name}. Error: {ex.Message}");
            }
            return _ProductAttribute;
        }

    }
}
