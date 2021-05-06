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
    public class WooProductCategory : IWooProductCategory
    {
        private readonly IWooBase _Woo;

        public WooProductCategory(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            _Woo = new WooBase(wooAPISettings, logger);

            //_WooAPISettings = wooAPISettings;
            //this._Logger = logger;
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

        private async Task<List<ProductCategory>> GetAll(Dictionary<string, string> ProductCategoryParams)
        {
            //            RestAPI _RestAPI = _Woo.GetJSONRestAPI;

            List<ProductCategory> _WooProductCategories = null;
            bool _GetMore = true;
            int _Page = 1;
            if (ProductCategoryParams == null)
                ProductCategoryParams = new Dictionary<string, string>();

            ProductCategoryParams.Add("per_page", "20");
            ProductCategoryParams.Add("page", "0");
            try
            {
                WCObject _WC = GetWCObject;
                //Get all ProductCategories
                while (_GetMore)
                {
                    ProductCategoryParams["page"] = _Page.ToString();
                    List<ProductCategory> TwentyProductCategories = await _WC.Category.GetAll(ProductCategoryParams);

                    if (TwentyProductCategories.Count > 0)
                    {
                        if (_WooProductCategories == null)
                            _WooProductCategories = TwentyProductCategories;
                        else
                            _WooProductCategories.AddRange(TwentyProductCategories);
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
            return _WooProductCategories;
        }
        public async Task<List<ProductCategory>> GetAllProductCategoriesAsync()
        {
            return await GetAll(null);
        }

        public async Task<bool> CheckProductCategoryLinkAsync()
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;

            int count = 0;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                List<ProductCategory> TenProductCategories = await _WC.Category.GetAll();

                count = TenProductCategories.Count;
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return count > 0;
        }

        public async Task<List<ProductCategory>> GetProductCategoriesOfTypeAsync(string pProductCategoryType)
        {
            return await GetAll(new Dictionary<string, string>() { { "type", pProductCategoryType } });
        }

        public async Task<int> GetProductCategoryCountAsync()
        {
            int _count = 0;
            RestAPI _RestAPI = _Woo.GetRootRestAPI;

            try
            {
                string Result = await _RestAPI.GetRestful("ProductCategories/count");

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
        public async Task<ProductCategory> GetProductCategoryByIdAsync(int deleteWooEntityId)
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;
            ProductCategory _productCategory = null;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                _productCategory = await _WC.Category.Get(deleteWooEntityId);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return _productCategory;
        }

        //public async Task<ProductCategory> DeleteProductCategoryById(int wooProductCategoryId)
        //{
        //    RestAPI _RestAPI = _Woo.GetJSONRestAPI;
        //    ProductCategory _productCategory = null;
        //    try
        //    {
        //        WCObject _WC = new WCObject(_RestAPI);
        //        _productCategory = await _WC.Category.Delete(wooProductCategoryId);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (_Woo.Logger != null)
        //            _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
        //    }
        //    return _productCategory;
        //}

        public async Task<ProductCategory> DeleteProductCategoryByIdAsync(int deleteWooProductCategoryId)
        {
            ProductCategory _ProductCategory = null;
            WCObject _WC = GetWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _ProductCategory = await _WC.Category.Delete(deleteWooProductCategoryId, true);   // force = true
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling deleting product category by id: {deleteWooProductCategoryId} Async. Error:  {ex.Message}");
            }
            return _ProductCategory;
        }

        public async Task<ProductCategory> AddProductCategoryAsync(ProductCategory addWooProductCategory)
        {
            ProductCategory _ProductCategory = null;
            WCObject _WC = GetWCObject;
            try
            {
                _ProductCategory = await _WC.Category.Add(addWooProductCategory);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling Add ProductCategoryAsync for product: {addWooProductCategory.name}. Error:  {ex.Message}");
            }
            return _ProductCategory;
        }


        public async Task<ProductCategory> UpdateProductCategoryAsync(ProductCategory updateWooProductCategory)
        {
            ProductCategory _ProductCategory = null;
            WCObject _WC = GetWCObject;
            try
            {
                _ProductCategory = await _WC.Category.Update((int)updateWooProductCategory.id, updateWooProductCategory);
            }
            catch (Exception ex)
            {
                if (_Woo.Logger != null)
                    _Woo.Logger.LogError($"Error calling Update ProductCategoryAsync for product: {updateWooProductCategory.name}. Error: {ex.Message}");
            }
            return _ProductCategory;
        }

        //public async Task<ProductCategory> FindProductCategoryByNameAsync(string findCategoryName)
        //{
        //    ProductCategory _ProductCategory = null;
        //    WCObject _WC = GetWCObject;
        //    try
        //    {
        //        _ProductCategory = await _WC.Category. Update((int)updateWooProductCategory.id, updateWooProductCategory);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (_Woo.Logger != null)
        //            _Woo.Logger.LogError($"Error calling Update ProductCategoryAsync for product: {updateWooProductCategory.name}. Error: {ex.Message}");
        //    }
        //    return _ProductCategory;
        //}
    }

}
