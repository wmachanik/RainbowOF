﻿using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Woo.REST.Repositories
{
    public class WooProductAttributeTerm : IWooProductAttributeTerm
    {
        private readonly IWooBase _wooBase;

        public WooProductAttributeTerm(WooAPISettings wooAPISettings, ILoggerManager logger)
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
        public async Task<List<ProductAttributeTerm>> GetAttributeTermsByAtttributeAsync(uint parentProductAttributeId)
        {
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;

            List<ProductAttributeTerm> wooBaseProductAttributeTerms = null;
            bool _GetMore = true;
            int _Page = 1;
            Dictionary<string, string> _ProductAttributeTermParams = new();
            _ProductAttributeTermParams.Add("per_page", "20");
            _ProductAttributeTermParams.Add("page", "0");
            try
            {
                WCObject _WC = new(_RestAPI);
                //Get all ProductAttributeTerms
                while (_GetMore)
                {
                    _ProductAttributeTermParams["page"] = _Page.ToString();
                    List<ProductAttributeTerm> TwentyProductAttributeTerms = await _WC.Attribute.Terms.GetAll(parentProductAttributeId, _ProductAttributeTermParams);

                    if (TwentyProductAttributeTerms.Count > 0)
                    {
                        if (wooBaseProductAttributeTerms == null)
                            wooBaseProductAttributeTerms = TwentyProductAttributeTerms;
                        else
                            wooBaseProductAttributeTerms.AddRange(TwentyProductAttributeTerms);
                        _Page++;
                    }
                    else
                        _GetMore = false;
                }
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError("Error calling WOO REST API: " + ex.Message);
            }
            return wooBaseProductAttributeTerms;
        }
        public async Task<ProductAttributeTerm> GetProductAttributeTermByIdAsync(int sourceWooEntityId, int parentWooAttributeId)
        {
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
            ProductAttributeTerm _productAttributeTerm = null;
            try
            {
                WCObject _WC = new(_RestAPI);
                _productAttributeTerm = await _WC.Attribute.Terms.Get(sourceWooEntityId, parentWooAttributeId);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return _productAttributeTerm;
        }

        public async Task<ProductAttributeTerm> AddProductAttributeTermAsync(ProductAttributeTerm addWooProductAttribute, int parentWooAttributeId)
        {
            ProductAttributeTerm _ProductAttributeTerm = null;
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
            try
            {
                WCObject _WC = new(_RestAPI);
                _ProductAttributeTerm = await _WC.Attribute.Terms.Add(addWooProductAttribute, parentWooAttributeId);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"Error calling Add ProductAttributeAsync for product: {addWooProductAttribute.name}. Error:  {ex.Message}");
            }
            return _ProductAttributeTerm;
        }
        public async Task<string> DeleteProductAttributeTermByIdAsync(int deleteWooAttributeTermId, int parentWooAttributeId)
        {
            string _ProductAttributeTerm = null;
            WCObject _WC = getWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _ProductAttributeTerm = await _WC.Attribute.Terms.Delete(deleteWooAttributeTermId, parentWooAttributeId, true);   // force = true
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"Error calling deleting product category by id: {deleteWooAttributeTermId} Async. Error:  {ex.Message}");
            }
            return _ProductAttributeTerm;
        }
        public async Task<ProductAttributeTerm> UpdateProductAttributeTermAsync(ProductAttributeTerm updateWooProductAttribute, int parentWooAttributeId)
        {
            ProductAttributeTerm _ProductAttributeTerm = null;
            WCObject _WC = getWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _ProductAttributeTerm = await _WC.Attribute.Terms.Update((int)updateWooProductAttribute.id, updateWooProductAttribute, parentWooAttributeId);   // force = true
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"Error calling deleting product category by id: {updateWooProductAttribute} Async. Error:  {ex.Message}");
            }
            return _ProductAttributeTerm;
        }

    }
}
