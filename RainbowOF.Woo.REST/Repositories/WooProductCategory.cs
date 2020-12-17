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
        private readonly WooAPISettings _WooAPISettings;
        private readonly ILoggerManager _Logger;

        public WooProductCategory(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            //while (wooAPISettings.QueryURL.EndsWith("/"))
            //    wooAPISettings.QueryURL.Remove(wooAPISettings.QueryURL.Length - 1);

            _WooAPISettings = wooAPISettings;
            //_RestAPI = new RestAPI(wooAPISettings.FullSourceURL + "/wp-json/wc/v3/",
            //    wooAPISettings.ConsumerKey,
            //    wooAPISettings.ConsumerSecret, !wooAPISettings.IsSecureURL);

            this._Logger = logger;
        }

        private RestAPI GetJSONRestAPI
        {
            get
            {
                return new RestAPI(_WooAPISettings.FullSourceURL + _WooAPISettings.JSONAPIPostFix,
               _WooAPISettings.ConsumerKey,
               _WooAPISettings.ConsumerSecret,
               !_WooAPISettings.IsSecureURL);
            }
        }


        private RestAPI GetRootRestAPI
        {
            get
            {
                return new RestAPI(_WooAPISettings.FullSourceURL + _WooAPISettings.RootAPIPostFix,
               _WooAPISettings.ConsumerKey,
               _WooAPISettings.ConsumerSecret,
               !_WooAPISettings.IsSecureURL);
            }
        }

        private async Task<List<ProductCategory>> GetAll(Dictionary<string, string> ProductCategoryParams)
        {
            RestAPI _RestAPI = GetJSONRestAPI;

            List<ProductCategory> _WooProductCategories = null;
            bool _GetMore = true;
            int _Page = 1;
            if (ProductCategoryParams == null)
                ProductCategoryParams = new Dictionary<string, string>();

            ProductCategoryParams.Add("per_page", "20");
            ProductCategoryParams.Add("page", "0");
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
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
                if (_Logger != null)
                    _Logger.LogError("Error calling WOO REST API: " + ex.Message);
            }
            return _WooProductCategories;
        }
        public async Task<List<ProductCategory>> GetAllProductCategories()
        {
            return await GetAll(null);
        }

        public async Task<bool> CheckProductCategoryLink()
        {
            RestAPI _RestAPI = GetJSONRestAPI;

            int count = 0;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                List<ProductCategory> TenProductCategories = await _WC.Category.GetAll();

                count = TenProductCategories.Count;
            }
            catch (Exception ex)
            {
                if (_Logger != null)
                    _Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return count > 0;
        }

        public async Task<List<ProductCategory>> GetProductCategoriesOfType(string pProductCategoryType)
        {
            return await GetAll(new Dictionary<string, string>() { { "type", pProductCategoryType } });
        }

        public async Task<int> GetProductCategoryCount()
        {
            int _count = 0;
            RestAPI _RestAPI = GetRootRestAPI;

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
                if (_Logger != null)
                    _Logger.LogError("Error calling WOO REST ROOT API: " + ex.Message);
            }

            return _count;

        }
    }
}
