using AutoMapper;
using Blazorise.RichTextEdit;
using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Items;
using RainbowOF.Tools;
using RainbowOF.Tools.Services;
using RainbowOF.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Items
{
    public partial class ItemEdit : ComponentBase
    {
        [Inject]
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        [Inject]
        private ApplicationState _AppState { get; set; }
        [Inject]
        public ILoggerManager _Logger { get; set; }
        [Inject]
        private IMapper _Mapper { get; set; }
        [Parameter]
        public string Id { get; set; }

        public ItemView ItemEditting = null;
        public List<ItemVariant> childItems = null;
        //public List<ItemCategory> itemCategories = null;
        //public List<ItemAttribute> itemAttributes = null;
        //public List<ItemAttributeVariety> itemAttributeVarieties = null;

        public bool collapseItemDetailsVisible = true;
        public bool collapseCategoriesVisible = true;
        public bool collapseAttributesVisible = true;

        private RichTextEdit richTextEditRef;

        class CategoryNode
        {
            public Guid CategoryId { get; set; }
            public string CategoryName { get; set; }
            public bool IsChecked { get; set; } = true;
        }

        List<CategoryNode> categoryNodes = new();

        //IList<CategoryNode> ExpandedNodes = new List<CategoryNode>();
        //CategoryNode selectedNode;

        protected async override Task OnInitializedAsync()
        {
            if (Guid.TryParse(Id, out Guid ItemId))
            {
                if (ItemId != Guid.Empty)
                {
                    IItemRepository _itemRepository = _AppUnitOfWork.itemRepository();
                    Item entity = await _itemRepository.FindFirstEagerLoadingItemAsync(it => it.ItemId == ItemId);

                    IAppRepository<WooProductMap> _wooAttributeMapRepository = _AppUnitOfWork.Repository<WooProductMap>();

                    WooProductMap _wooProductMap = await _wooAttributeMapRepository.FindFirstAsync(wcm => wcm.ItemId == ItemId);
                    //  map all the items across to the view then allocate extra woo stuff if exists.
                    ItemEditting = new();
                    _Mapper.Map(entity, ItemEditting);

                    //ItemEditting.ItemName = entity.ItemName;
                    //ItemEditting.SKU = entity.SKU;
                    //ItemEditting.IsEnabled = entity.IsEnabled;
                    //ItemEditting.ItemDetail = entity.ItemDetail;
                    //ItemEditting.PrimaryItemCategoryLookupId = ((entity.PrimaryItemCategoryLookupId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.PrimaryItemCategoryLookupId;
                    //ItemEditting.ParentItemId = ((entity.ParentItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ParentItemId;
                    //ItemEditting.ReplacementItemId = ((entity.ReplacementItemId ?? Guid.Empty) == Guid.Empty) ? null : (Guid)entity.ReplacementItemId;
                    //ItemEditting.ItemAbbreviatedName = entity.ItemAbbreviatedName;
                    //ItemEditting.ParentItem = entity.ParentItem;
                    //ItemEditting.ReplacementItem = entity.ReplacementItem;
                    //ItemEditting.ItemCategories = entity.ItemCategories;
                    //ItemEditting.ItemAttributes = entity.ItemAttributes;
                    //ItemEditting.ItemImages = entity.ItemImages;              
                    //ItemEditting.SortOrder = entity.SortOrder;
                    //ItemEditting.BasePrice = entity.BasePrice;
                    //ItemEditting.ManageStock = entity.ManageStock;
                    //ItemEditting.QtyInStock = entity.QtyInStock;
                    //ItemEditting.CanUpdateECommerceMap = (_wooProductMap == null) ? null : _wooProductMap.CanUpdate;


                    /////////
                    ///
                    /// this sort of works now need to create a complete list with true /false
                    /// 
                    IAppRepository<ItemVariant> _itemVariantRepo = _AppUnitOfWork.Repository<ItemVariant>();

                    childItems = (await _itemVariantRepo.GetByAsync(iv => iv.ItemId == ItemId))
                        .OrderBy(iv => iv.SortOrder)
                        .ToList();

                    var selectedCats = entity.ItemCategories.Select(ic => new { ic.ItemCategoryDetail.ItemCategoryLookupId, ic.ItemCategoryDetail.FullCategoryName }).OrderBy(ob => ob.FullCategoryName);
                    if (selectedCats != null)
                    {
                        foreach (var cat in selectedCats)
                        {
                            categoryNodes.Add(new CategoryNode
                            {
                                CategoryId = cat.ItemCategoryLookupId,
                                CategoryName = cat.FullCategoryName,
                                IsChecked = true
                            }) ;
                        }
                    }

//                    IAppRepository<ItemCategoryLookup> _itemCategoryLookupRepository = _AppUnitOfWork.Repository<ItemCategoryLookup>();
//                    var _itemcats = (await _itemCategoryLookupRepository.GetAllAsync()).ToList().OrderBy(ic=>ic.FullCategoryName); // forces the parents at the top
//                    bool _isChecked;

////var BunchOfCats = _itemcats.Select(c => new { c.ItemCategoryLookupId, c.CategoryName}).ToList();

//                    if (ItemEditting.ItemCategories != null)
//                    {
//                        categoryNodes = new();
//                        foreach (var itemCat in _itemcats)
//                        {
//                            _isChecked = (entity.ItemCategories != null) && (entity.ItemCategories.Exists(ic => ic.ItemCategoryLookupId == itemCat.ItemCategoryLookupId));
//                            CategoryNode _categoryNode= new CategoryNode
//                            {
//                                CategoryId = itemCat.ItemCategoryLookupId,
//                                CategoryName = itemCat.CategoryName,
//                                IsChecked = _isChecked,
//                                ChildrenCategories = null
//                            };
//                            if (itemCat.ParentCategoryId == null)
//                            {
//                                categoryNodes.Add(_categoryNode);
//                            }
//                            else   //(itemCat.ItemCategoryDetail.ParentCategoryId != null)
//                            {
//                                var allcats = categoryNodes.Select
//                                var catNode = categoryNodes.Find(cn => cn.CategoryId == itemCat.ParentCategoryId);
//                                if (catNode == null)
//                                {

//                                    catNode = categoryNodes.Find(cn => cn.ChildrenCategories.Find(ccn=> ccn.CategoryId == itemCat.ParentCategoryId).CategoryId == itemCat.ParentCategoryId);// find the child node
//                                    if (catNode != null) catNode = catNode.ChildrenCategories.Find(cn => cn.CategoryId == itemCat.ParentCategoryId);  // get the child
//                                }
//                                if (catNode != null)
//                                {
//                                    if (catNode.ChildrenCategories == null) catNode.ChildrenCategories = new();
//                                    catNode.ChildrenCategories.Add(_categoryNode);
//                                }
//                            }
//                        }
//                        // ExpandedNodes.Concat(categoryNodes);
//                    }

                }
            }
        }

        public async Task OnContentChanged()
        {
            var content = await richTextEditRef.GetHtmlAsync();
            ItemEditting.ItemDetail = content;
        }
    }
}
