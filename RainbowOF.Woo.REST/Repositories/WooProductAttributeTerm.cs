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

        private WCObject _wcObject = null;
        private WCObject GetWCObject
        {
            get
            {
                if (_wcObject == null)
                {
                    RestAPI _RestAPI = _Woo.GetJSONRestAPI;
                    try
                    {
                        _wcObject = new WCObject(_RestAPI);
                    }
                    catch (Exception ex)
                    {
                        if (_Woo.Logger != null)
                            _Woo.Logger.LogError("Error setting up WOO REST API: " + ex.Message);
                    }
                }
                return _wcObject;
            }
            set
            {
                _wcObject = value;
            }
        }
        public async Task<List<ProductAttributeTerm>> GetAttributeTermsByAtttributeAsync(uint parentProductAttributeId)
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
                    List<ProductAttributeTerm> TwentyProductAttributeTerms = await _WC.Attribute.Terms.GetAll(parentProductAttributeId, _ProductAttributeTermParams);

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
        public async Task<ProductAttributeTerm> GetProductAttributeTermByIdAsync(int sourceWooEntityId, int parentWooAttributeId)
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            ProductAttributeTerm _productAttributeTerm = null;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                _productAttributeTerm = await _WC.Attribute.Terms.Get(sourceWooEntityId,parentWooAttributeId);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return _productAttributeTerm;
        }

        public async Task<ProductAttributeTerm> AddProductAttributeTermAsync(ProductAttributeTerm addWooProductAttribute, int parentWooAttributeId)
        {
            ProductAttributeTerm _ProductAttributeTerm = null;
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            try
            {
                WCObject _WC = new(_RestAPI);
                _ProductAttributeTerm = await _WC.Attribute.Terms.Add(addWooProductAttribute, parentWooAttributeId);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling Add ProductAttributeAsync for product: {addWooProductAttribute.name}. Error:  {ex.Message}");
            }
            return _ProductAttributeTerm;
        }
        public async Task<string> DeleteProductAttributeTermByIdAsync(int deleteWooAttributeTermId, int parentWooAttributeId)
        {
            string _ProductAttributeTerm = null;
            WCObject _WC = GetWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _ProductAttributeTerm = await _WC.Attribute.Terms.Delete(deleteWooAttributeTermId, parentWooAttributeId, true);   // force = true
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling deleting product category by id: {deleteWooAttributeTermId} Async. Error:  {ex.Message}");
            }
            return _ProductAttributeTerm;
        }
        public async Task<ProductAttributeTerm> UpdateProductAttributeTermAsync(ProductAttributeTerm updateWooProductAttribute, int parentWooAttributeId)
        {
            ProductAttributeTerm _ProductAttributeTerm = null;
            WCObject _WC = GetWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _ProductAttributeTerm = await _WC.Attribute.Terms.Update((int)updateWooProductAttribute.id, updateWooProductAttribute, parentWooAttributeId);   // force = true
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling deleting product category by id: {updateWooProductAttribute} Async. Error:  {ex.Message}");
            }
            return _ProductAttributeTerm;
        }

    }
}
