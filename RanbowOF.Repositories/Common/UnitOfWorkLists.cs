using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Common
{
    /// <summary>
    /// TThis part of the partial class is all lists that are used for select lists combo boxes
    /// </summary>
    /// Other partial classes:
    /// AppUnitOfWork      - the general database and app related routines and variables
    /// AppUnitOfWorkRepos - Generic and Custom Repos
    /// 
    public partial class UnitOfWork : IUnitOfWork
    {
        #region Internal List vars
        private Dictionary<Type, object> _lookupLists = new();
        private Dictionary<Guid, string> _listOfUoMSymbols = null;
        private Dictionary<Guid, List<ItemCategoryLookup>> _listOfAnItemsCategories = null;
        private Dictionary<Guid, List<Item>> _listOfSimilarItems = null;
        private Dictionary<Guid, string> _listOfAttributes = null;
        private List<ItemAttributeVarietyLookup> _listOfAttributeVarieties = null;
        private Dictionary<Guid, List<ItemAttribute>> _listOfAnItemsAttributes = null;
        private Dictionary<Guid, List<ItemAttributeVariety>> _listOfAnItemsAttributeVarieties = null;
        #endregion
        #region lists and variables from database
        public bool DBTransactionIsStillRunning()
        {
            return false; // appDbTransaction != null;
        }
        public Dictionary<Type, object> LookupLists
        {
            get { return _lookupLists; }
            set { _lookupLists = value; }
        }
        public List<TLookupItem> GetListOf<TLookupItem>(bool IsForceReload = false, Func<TLookupItem, object> orderByExpression = null) where TLookupItem : class
        {
            if (LookupLists.ContainsKey(typeof(TLookupItem)))
            {
                if (IsForceReload)
                {
                    LookupLists.Remove(typeof(TLookupItem)); // of we are forcing reload delete the old list
                }
                else
                    return LookupLists[typeof(TLookupItem)] as List<TLookupItem>;
            }
            IRepository<TLookupItem> appRepository = Repository<TLookupItem>();
            if (appRepository == null)
                return null;   // problem so return null

            List<TLookupItem> lookupItems = (orderByExpression == null)
                ? appRepository.GetAll().ToList()
                : appRepository.GetAllOrderBy(orderByExpression, false).ToList();
            //List<TLookupItem> lookupItems = appRepository.GetAll().ToList(); // : appRepository.GetAllOrderBy(orderByExpression, false).ToList();
            LookupLists.Add(typeof(TLookupItem), lookupItems);
            return lookupItems;
        }

        public async Task<List<TLookupItem>> GetListOfAsync<TLookupItem>(bool IsForceReload = false, Expression<Func<TLookupItem, object>> orderByExpression = null) where TLookupItem : class
        {
            ///->> I am not sure this routine works seems to have an async error some where linked to order expression
            if (LookupLists.ContainsKey(typeof(TLookupItem)))
            {
                if (IsForceReload)
                {
                    LookupLists.Remove(typeof(TLookupItem)); // of we are forcing reload delete the old list
                }
                else
                    return LookupLists[typeof(TLookupItem)] as List<TLookupItem>;
            }
            IRepository<TLookupItem> appRepository = Repository<TLookupItem>();
            if (appRepository == null)
            {
                // problem so return null
                return null;
            }
            var lookupItems = (orderByExpression == null)
                 ? await appRepository.GetAllAsync()
                 : await appRepository.GetAllOrderByAsync(orderByExpression, false);
            LookupLists.Add(typeof(TLookupItem), lookupItems);
            return lookupItems.ToList();
        }

        //public Dictionary<Guid, string> GetListOfCategories(bool IsForceReload = false)
        //{
        //    if-`+ ((IsForceReload) || (_listOfCategories == null))
        //    {
        //        if (_listOfCategories != null) _listOfCategories.Clear();
        //        else _listOfCategories = new Dictionary<Guid, string>();

        //        IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = Repository<ItemCategoryLookup>();
        //        var _itemCategories = _itemCategoryLookupRepository.GetAll()
        //            .OrderBy(ic => ic.FullCategoryName)
        //            .ToList();  // cannot async as part of UI for
        //        foreach (var _itemCategory in _itemCategories)
        //        {
        //            _listOfCategories.Add(_itemCategory.ItemCategoryLookupId, _itemCategory.FullCategoryName);
        //        }
        //    }
        //    return _listOfCategories;
        //}
        public Dictionary<Guid, string> GetListOfUoMSymbols(bool IsForceReload = false)
        {
            if (IsForceReload)
            {
                _listOfUoMSymbols.Clear();
                _listOfUoMSymbols = null; // would prefer to dispose it but cannot see how
            }
            if (_listOfUoMSymbols == null)
            {
                IRepository<ItemUoMLookup> appRepository = this.Repository<ItemUoMLookup>();
                var _itemUoMs = appRepository.GetAll(); // (await _UoMRepository.GetAllAsync()).ToList();
                _listOfUoMSymbols = new();
                if (_itemUoMs != null)
                {
                    foreach (var item in _itemUoMs)
                    {
                        _listOfUoMSymbols.Add(item.ItemUoMLookupId, item.UoMSymbol);
                    }
                }
            }
            return _listOfUoMSymbols;
        }
        /// <summary>
        /// Get or return a list of Categories based on the sourceItemId
        /// </summary>
        /// <param name="parentAttributeLookupId">Id of parent</param>
        /// <param name="IsForceReload">must we force reload</param>
        /// <returns>List of Categories</returns>
        //public List<ItemAttributeVariety> GetListOfAttributeVarieties(Guid parentItemAttributeId, bool IsForceReload = false)
        public List<ItemCategoryLookup> GetListOfAnItemsCategories(Guid sourceItemId, bool IsForceReload = false)
        {
            if ((_listOfAnItemsCategories == null) || (IsForceReload) || (!_listOfAnItemsCategories.ContainsKey(sourceItemId)))
            {
                List<ItemCategoryLookup> _listOfCategories = ItemRepository.GetEagerItemsCategoryLookupsByItemId(sourceItemId)
                                                                            .OrderBy(icl => icl.FullCategoryName)
                                                                            .ToList();   // get a list of only the category lookups this Item order by full category name
                if ((_listOfAnItemsCategories != null) && (_listOfAnItemsCategories.ContainsKey(sourceItemId)))
                {
                    _listOfAnItemsCategories[sourceItemId].Clear();
                    _listOfAnItemsCategories[sourceItemId] = _listOfCategories;
                }
                else
                {
                    if (_listOfAnItemsCategories == null) _listOfAnItemsCategories = new();
                    _listOfAnItemsCategories.Add(sourceItemId, _listOfCategories);
                }
            }
            return _listOfAnItemsCategories[sourceItemId];
        }
        public List<Item> GetListOfSimilarItems(Guid sourceItemId, Guid? sourceItemPrimaryCategoryId, bool IsForceReload = false)
        {
            if ((_listOfSimilarItems == null) || (IsForceReload) || (!_listOfSimilarItems.ContainsKey(sourceItemId)))
            {
                List<Item> _listOfSimilarItems = ItemRepository.GetSimilarItems(sourceItemId, sourceItemPrimaryCategoryId);   // get a list of items with the same parent category or all with parent category null
                if ((this._listOfSimilarItems != null) && (this._listOfSimilarItems.ContainsKey(sourceItemId)))
                {
                    this._listOfSimilarItems[sourceItemId].Clear();
                    this._listOfSimilarItems[sourceItemId] = _listOfSimilarItems;
                }
                else
                {
                    if (this._listOfSimilarItems == null) this._listOfSimilarItems = new();
                    this._listOfSimilarItems.Add(sourceItemId, _listOfSimilarItems);
                }
            }
            return _listOfSimilarItems[sourceItemId];
        }
        public Dictionary<Guid, string> GetListOfAttributes(bool IsForceReload = false)
        {
            if ((IsForceReload) || (_listOfAttributes == null))
            {
                if (_listOfAttributes != null) _listOfAttributes.Clear();
                else _listOfAttributes = new Dictionary<Guid, string>();

                IRepository<ItemAttributeLookup> _itemAttributeLookupRepository = Repository<ItemAttributeLookup>();
                var _itemAttributes = _itemAttributeLookupRepository.GetAll()
                    .OrderBy(ia => ia.AttributeName)
                    .ToList();  // cannot async as part of UI for
                foreach (var _itemAttribute in _itemAttributes)
                {
                    _listOfAttributes.Add(_itemAttribute.ItemAttributeLookupId, _itemAttribute.AttributeName);
                }
            }
            return _listOfAttributes;
        }
        public List<ItemAttributeVarietyLookup> GetListOfAttributeVarieties(Guid parentAttributeLookupId, bool IsForceReload = false)
        {
            if ((IsForceReload) || (_listOfAttributeVarieties == null))
            {
                if (_listOfAttributeVarieties != null) _listOfAttributeVarieties.Clear();
                else _listOfAttributeVarieties = new List<ItemAttributeVarietyLookup>();

                IRepository<ItemAttributeVarietyLookup> _itemAttributeVarietyLookupRepository = Repository<ItemAttributeVarietyLookup>();
                _listOfAttributeVarieties = _itemAttributeVarietyLookupRepository.GetBy(avl => avl.ItemAttributeLookupId == parentAttributeLookupId)
                    .OrderBy(iav => iav.VarietyName)
                    .ToList();  // cannot async as part of UI for

            }
            return _listOfAttributeVarieties;
        }
        /// <summary>
        /// Get or return a list of Attributes based on the sourceItemId
        /// </summary>
        /// <param name="parentAttributeLookupId">Id of parent</param>
        /// <param name="IsForceReload">must we force reload</param>
        /// <returns>List of attributes</returns>
        //public List<ItemAttributeVariety> GetListOfAttributeVarieties(Guid parentItemAttributeId, bool IsForceReload = false)
        public List<ItemAttribute> GetListOfAnItemsVariableAttributes(Guid sourceItemId, bool IsForceReload = false)
        {
            if ((_listOfAnItemsAttributes == null) || (IsForceReload) || (!_listOfAnItemsAttributes.ContainsKey(sourceItemId)))
            {
                List<ItemAttribute> _listOfAttributes = ItemRepository.GetEagerItemVariableAttributeByItemId(sourceItemId);
                if ((_listOfAnItemsAttributes != null) && (_listOfAnItemsAttributes.ContainsKey(sourceItemId)))
                {
                    _listOfAnItemsAttributes[sourceItemId].Clear();
                    _listOfAnItemsAttributes[sourceItemId] = _listOfAttributes;
                }
                else
                {
                    if (_listOfAnItemsAttributes == null) _listOfAnItemsAttributes = new();
                    _listOfAnItemsAttributes.Add(sourceItemId, _listOfAttributes);
                }
            }
            return _listOfAnItemsAttributes[sourceItemId];
        }
        /// <summary>
        /// Get or return a list of Attribute Varieties based on the sourceItemId and associatedAttributeId
        /// </summary>
        /// <param name="sourceItemId"></param>
        /// <param name="sourceAssoicatedAttributeId"></param>
        /// <param name="IsForceReload"></param>
        /// <returns></returns>
        public List<ItemAttributeVariety> GetListOfAnItemsAttributeVarieties(Guid sourceItemId, Guid sourceAttributeLookupId, bool IsForceReload = false)
        {
            if ((_listOfAnItemsAttributeVarieties == null) || (IsForceReload) || (!_listOfAnItemsAttributeVarieties.ContainsKey(sourceAttributeLookupId)))
            {
                List<ItemAttributeVariety> _listOfItemsAttributeVarieties = ItemRepository.GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupId(sourceItemId, sourceAttributeLookupId);
                if ((_listOfAnItemsAttributeVarieties != null) && (_listOfAnItemsAttributeVarieties.ContainsKey(sourceAttributeLookupId)))
                {
                    _listOfAnItemsAttributeVarieties[sourceAttributeLookupId].Clear();
                    _listOfAnItemsAttributeVarieties[sourceAttributeLookupId] = _listOfItemsAttributeVarieties;
                }
                else
                {
                    if (_listOfAnItemsAttributeVarieties == null) _listOfAnItemsAttributeVarieties = new();
                    _listOfAnItemsAttributeVarieties.Add(sourceAttributeLookupId, _listOfItemsAttributeVarieties);
                }
            }
            return _listOfAnItemsAttributeVarieties[sourceAttributeLookupId];
        }
        public async Task<List<ItemAttributeVariety>> GetListOfItemAttributeVarietiesAsync(Guid sourceItemId, Guid sourceAttributeLookupId, bool IsForceReload = false)
        {
            if ((IsForceReload) || (!_listOfAnItemsAttributeVarieties.ContainsKey(sourceAttributeLookupId)))
            {
                List<ItemAttributeVariety> _listOfItemsAttributeVarieties = await ItemRepository.GetEagerItemAttributeVarietiesByItemIdAndAttributeLookupIdAsync(sourceItemId, sourceAttributeLookupId);
                if (_listOfAnItemsAttributeVarieties.ContainsKey(sourceItemId))
                {
                    _listOfAnItemsAttributeVarieties[sourceAttributeLookupId].Clear();
                    _listOfAnItemsAttributeVarieties[sourceAttributeLookupId] = _listOfItemsAttributeVarieties;
                }
                else
                    _listOfAnItemsAttributeVarieties.Add(sourceAttributeLookupId, _listOfItemsAttributeVarieties);
            }
            return _listOfAnItemsAttributeVarieties[sourceAttributeLookupId];
        }

        #endregion
    }
}
