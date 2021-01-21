using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Woo.REST.Repositories
{
    public class WooProductAttributeTerm : IWooProductAttributeTerm
    {
        private readonly IWooBase _Woo;

        public WooProductAttributeTerm(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            _Woo = new WooBase(wooAPISettings, logger);
        }
        public async Task<List<ProductAttributeTerm>> GetAttributeTermsByAtttribute(ProductAttribute pProductAttribute)
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;

            List<ProductAttributeTerm> _WooProductAttributeTerms = null;
            bool _GetMore = true;
            int _Page = 1;
            Dictionary<string, string> _ProductAttributeTermParams = new Dictionary<string, string>();
            _ProductAttributeTermParams.Add("per_page", "20");
            _ProductAttributeTermParams.Add("page", "0");
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                //Get all ProductAttributeTerms
                while (_GetMore)
                {
                    _ProductAttributeTermParams["page"] = _Page.ToString();
                    List<ProductAttributeTerm> TwentyProductAttributeTerms = await _WC.Attribute.Terms.GetAll(pProductAttribute.id, _ProductAttributeTermParams);

                    if (TwentyProductAttributeTerms.Count > 0)
                    {
                        if (_WooProductAttributeTerms == null)
                            _WooProductAttributeTerms = TwentyProductAttributeTerms;
                        else
                            _WooProductAttributeTerms.AddRange(TwentyProductAttributeTerms);
                        _Page++;
                    }
                    else
                        _GetMore = false;
                }
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST API: " + ex.Message);
            }
            return _WooProductAttributeTerms;
        }

    }
}
