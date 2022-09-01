using RainbowOF.Tools;
using RainbowOF.Woo.REST.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Woo.REST.Repositories
{
    public class WooProduct : IWooProduct
    {
        private readonly IWooBase _wooBase;

        #region Initialisation Stuff
        public WooProduct(WooAPISettings wooAPISettings, ILoggerManager logger)
        {
            _wooBase = new WooBase(wooAPISettings, logger);

            //wooBaseAPISettings = wooAPISettings;
            // this.appLoggerManager = logger;
        }
        #endregion
        //private RestAPI GetJSONRestAPI
        //{
        //    get
        //    {
        //        return new RestAPI(wooBaseAPISettings.FullSourceURL + wooBaseAPISettings.JSONAPIPostFix,
        //       wooBaseAPISettings.ConsumerKey,
        //       wooBaseAPISettings.ConsumerSecret,
        //       !wooBaseAPISettings.IsSecureURL);
        //    }
        //}


        //private RestAPI GetRootRestAPI
        //{
        //    get
        //    {
        //        return new RestAPI(wooBaseAPISettings.FullSourceURL + wooBaseAPISettings.RootAPIPostFix,
        //       wooBaseAPISettings.ConsumerKey,
        //       wooBaseAPISettings.ConsumerSecret,
        //       !wooBaseAPISettings.IsSecureURL);
        //    }
        //}
        #region Private VARS
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
                            _wooBase.Logger.LogError("WooProduct - Error setting up WOO REST API: " + ex.Message);
                    }
                }
                return _localWCObject;
            }
            set
            {
                _localWCObject = value;
            }
        }
        #endregion
        #region Data Retrieval stuff
        private async Task<List<Product>> GetAllWithParamsAsync(Dictionary<string, string> pProductParams)
        {
            List<Product> wooBaseProducts = null;
            int _Page = 1;
            bool _GetMore = true;
            // if there is no page setting adding
            if (pProductParams.ContainsKey("page")) pProductParams["page"] = "0";
            else pProductParams.Add("page", "0");
            if (pProductParams.ContainsKey("per_page")) pProductParams["per_page"] = "20";
            else pProductParams.Add("per_page", "20");
            // retrieve products until the number of products returned is 0.
            try
            {
                RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
                WCObject _WC = new(_RestAPI);
                while (_GetMore)
                {
                    pProductParams["page"] = _Page.ToString();
                    List<Product> TwentyProducts = await _WC.Product.GetAll(pProductParams);

                    if (TwentyProducts.Count > 0)
                    {
                        if (wooBaseProducts == null)
                            wooBaseProducts = TwentyProducts;
                        else
                            wooBaseProducts.AddRange(TwentyProducts);
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
            return wooBaseProducts;
        }
        private async Task<List<Product>> GetAllAsync(Dictionary<string, string> pProductParams)
        {

            if (pProductParams == null)
                pProductParams = new Dictionary<string, string>();
            return await GetAllWithParamsAsync(pProductParams);
        }
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await GetAllAsync(null);
        }

        public async Task<List<Product>> GetAllProductsInStockAsync()
        {
            Dictionary<string, string> _ProductParams = new();
            _ProductParams.Add("stock_status", "instock");
            _ProductParams.Add("status", "publish");
            List<Product> wooBaseProducts = await GetAllAsync(_ProductParams);
            _ProductParams["status"] = "private";
            List<Product> wooBasePrivateProducts = await GetAllAsync(_ProductParams);
            if ((wooBaseProducts != null) && (wooBasePrivateProducts != null))
                wooBaseProducts.AddRange(wooBasePrivateProducts);

            return wooBaseProducts;
        }
        public async Task<bool> CheckProductLinkAsync()
        {
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;

            int count = 0;
            try
            {
                WCObject _WC = new(_RestAPI);
                List<Product> TenProducts = await _WC.Product.GetAll();

                count = TenProducts.Count;
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError("Error calling WOO REST JSON API: " + ex.Message);
            }
            return count > 0;
        }

        public async Task<List<Product>> GetProductsOfTypeAsync(string pProductType)
        {
            return await GetAllAsync(new Dictionary<string, string>() { { "type", pProductType } });
        }

        public async Task<int> GetProductCountAsync()
        {
            int _count = 0;
            RestAPI _RestAPI = _wooBase.GetRootRestAPI;

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
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError("Error calling WOO REST ROOT API: " + ex.Message);
            }
            return _count;
        }
        #endregion
        #region UILinkedRoutiones
        public async Task<Product> GetProductByIdAsync(int deleteWooEntityId)
        {
            RestAPI _RestAPI = _wooBase.GetJSONRestAPI;
            Product _Product = null;
            try
            {
                WCObject _WC = new(_RestAPI);
                _Product = await _WC.Product.Get(deleteWooEntityId);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError("WooProduct - Error calling WOO REST JSON API: " + ex.Message);
            }
            return _Product;
        }

        public async Task<Product> DeleteProductByIdAsync(int deleteWooProductId)
        {
            Product _Product = null;
            WCObject _WC = getWCObject;
            try
            {
                // looks like it may need a force parameter, is this a good thing?
                _Product = await _WC.Product.Delete(deleteWooProductId, true);   // force = true
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"WooProduct - Error calling deleting product category by id: {deleteWooProductId} Async. Error:  {ex.Message}");
            }
            return _Product;
        }

        public async Task<Product> AddProductAsync(Product addWooProduct)
        {
            Product _Product = null;
            WCObject _WC = getWCObject;

            //var x = await _WC.ProductAttribute.Add(new ProductAttribute { id = 0 });

            try
            {
                _Product = await _WC.Product.Add(addWooProduct);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"WooProduct - Error calling Add ProductAsync for product: {addWooProduct.name}. Error:  {ex.Message}");
            }
            return _Product;
        }
        public async Task<Product> UpdateProductAsync(Product updateWooProduct)
        {
            Product _Product = null;
            WCObject _WC = getWCObject;
            try
            {
                _Product = await _WC.Product.Update((int)updateWooProduct.id, updateWooProduct);
            }
            catch (Exception ex)
            {
                if (_wooBase.Logger != null)
                    _wooBase.Logger.LogError($"WooProduct - Error calling Update ProductAsync for product: {updateWooProduct.name}. Error: {ex.Message}");
            }
            return _Product;
        }

        #endregion
    }
}
