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
        public async Task<List<ProductAttribute>> GetAllProductAttributes()
        {
            return await GetAll(null);
        }

        public async Task<bool> CheckProductAttributeLink()
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;

            int count = 0;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
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

        public async Task<List<ProductAttribute>> GetProductAttributesOfType(string pProductAttributeType)
        {
            return await GetAll(new Dictionary<string, string>() { { "type", pProductAttributeType } });
        }

        public async Task<int> GetProductAttributeCount()
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
    }
}
