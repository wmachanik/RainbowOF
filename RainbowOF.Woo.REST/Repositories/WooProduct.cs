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
    public class WooProduct : IWooProduct
    {
        private readonly IWooBase _Woo;

        // private readonly WooAPISettings _WooAPISettings;
        // private readonly ILoggerManager _Logger;

        public WooProduct(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            _Woo = new WooBase(wooAPISettings, logger);

            //_WooAPISettings = wooAPISettings;
            // this._Logger = logger;
        }

        //private RestAPI GetJSONRestAPI
        //{
        //    get
        //    {
        //        return new RestAPI(_WooAPISettings.FullSourceURL + _WooAPISettings.JSONAPIPostFix,
        //       _WooAPISettings.ConsumerKey,
        //       _WooAPISettings.ConsumerSecret,
        //       !_WooAPISettings.IsSecureURL);
        //    }
        //}


        //private RestAPI GetRootRestAPI
        //{
        //    get
        //    {
        //        return new RestAPI(_WooAPISettings.FullSourceURL + _WooAPISettings.RootAPIPostFix,
        //       _WooAPISettings.ConsumerKey,
        //       _WooAPISettings.ConsumerSecret,
        //       !_WooAPISettings.IsSecureURL);
        //    }
        //}

        private async Task<List<Product>> GetAll(Dictionary<string, string> ProductParams)
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;

            List<Product> _WooProducts = null;
            bool _GetMore = true;
            int _Page = 1;
            if (ProductParams == null)
                ProductParams = new Dictionary<string, string>();

            ProductParams.Add("per_page", "20");
            ProductParams.Add("page", "0");
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                //Get all products
                while (_GetMore)
                {
                    ProductParams["page"] = _Page.ToString();
                    List<Product> TwentyProducts = await _WC.Product.GetAll(ProductParams);

                    if (TwentyProducts.Count > 0)
                    {
                        if (_WooProducts == null)
                            _WooProducts = TwentyProducts;
                        else
                            _WooProducts.AddRange(TwentyProducts);
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
            return _WooProducts;
        }
        public async Task<List<Product>> GetAllProducts()
        {
            return await GetAll(null);
        }

        public async Task<bool> CheckProductLink()
        {
            RestAPI _RestAPI = _Woo.GetJSONRestAPI;

            int count = 0;
            try
            {
                WCObject _WC = new WCObject(_RestAPI);
                List<Product> TenProducts = await _WC.Product.GetAll();

                count = TenProducts.Count;
            }
            catch (Exception ex)
            {
                if (_Woo.Logger!= null)
                    _Woo.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return count > 0;
        }

        public async Task<List<Product>> GetProductsOfType(string pProductType)
        {
            return await GetAll(new Dictionary<string, string>() { { "type", pProductType } });
        }

        public async Task<int> GetProductCount()
        {
            int _count = 0;
            RestAPI _RestAPI = _Woo.GetRootRestAPI;

            try
            {
                string Result = await _RestAPI.GetRestful("products/count");

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
