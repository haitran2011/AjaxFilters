using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Data.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Localization;
using Spikes.Nop.Plugins.AjaxFilters.Models;
using Spikes.Nop.Services.Catalog;
using Spikes.Nop.Services.Helpers;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
    public class ProductServiceNopAjaxFilters : IProductServiceNopAjaxFilters
	{
	
		private readonly ILanguageService _languageService;
        private readonly CommonSettings _commonSettings;
        private readonly IWorkContext _workContext;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ICategoryServiceSpikes _categoryServiceSpikes;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
		private readonly IRepository<Manufacturer> _manufacturerRepository;
		private readonly IRepository<Vendor> _vendorRepository;
		private readonly CatalogSettings _catalogSettings;
		private ICacheManager _cacheManager;
		private IAclHelper _aclHelper;

        public ProductServiceNopAjaxFilters(CommonSettings commonSettings,IDataProvider dataProvider, IDbContext dbContext, ILanguageService languageService, ICategoryServiceSpikes categoryServiceSpikes, IRepository<ProductManufacturer> productManufacturer, IRepository<Manufacturer> manufacturerRepository, IRepository<Vendor> vendorRepository, IRepository<ProductCategory> productCategoryRepository, IWorkContext workContext, CatalogSettings catalogSettings, ICacheManager cacheManager, IAclHelper aclHelper)
		{
            _commonSettings = commonSettings;
            _dataProvider = dataProvider;
            _dbContext = dbContext;
			_languageService = languageService;
			_categoryServiceSpikes = categoryServiceSpikes;
			_productCategoryRepository = productCategoryRepository;
			_productManufacturerRepository = productManufacturer;
			_manufacturerRepository = manufacturerRepository;
			_vendorRepository = vendorRepository;
			_workContext = workContext;
			_cacheManager = cacheManager;
			_aclHelper = aclHelper;
			_catalogSettings = catalogSettings;
        }
        #region method
        
        public bool HasProductsOnSale(int categoryId, int manufacturerId, int vendorId)
		{
			return HasProductsOnSaleInternal(categoryId, manufacturerId, vendorId);
		}

		public bool HasProductsInStock(int categoryId, int manufacturerId, int vendorId)
		{
			return HasProductsInStockInternal(categoryId, manufacturerId, vendorId);
		}

		public IList<int> GetAllGroupProductIdsInCategories(List<int> categoriesIds)
		{
			return GetAllGroupProductIdsInCategoriesInternal(categoriesIds).ToList();
		}

		public IList<int> GetAllGroupProductIdsInCategory(int categoryId)
		{
			return GetAllGroupProductIdsInCategoriesInternal(new List<int>
			{
				categoryId
			}).ToList();
		}

		public IList<int> GetAllGroupProductIdsInManufacturer(int manufacturerId)
		{
			return GetAllGroupProductIdsInManufacturerInternal(manufacturerId).ToList();
		}

		public IList<int> GetAllGroupProductIdsInVendor(int vendorId)
		{
			return GetAllGroupProductIdsInVendorInternal(vendorId).ToList();
		}

		public bool HasProductsOnSaleInternal(int categoryId, int manufacturerId, int vendorId)
		{
			var key = $"Nop.onsalestate.productids-{categoryId}-{manufacturerId}-{vendorId}";
			return _cacheManager.Get(key, delegate
			{
				var result = false;
				var availableProductsForCurrentCustomer = _aclHelper.GetAvailableProductsForCurrentCustomer();
				var showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
				var includeFeaturedProductsInNormalLists = _catalogSettings.IncludeFeaturedProductsInNormalLists;
				if (categoryId > 0)
				{
					var list = new List<int>
					{
						categoryId
					};
					if (showProductsFromSubcategories)
					{
						var collection = from x in _categoryServiceSpikes.GetCategoriesByParentCategoryId(categoryId, includeSubCategoriesFromAllLevels: true)
							select x.Id;
						list.AddRange(collection);
					}
					var allGroupProductIdsInCategoriesInternal = GetAllGroupProductIdsInCategoriesInternal(list);
					result = HasAvailableProductsOnSaleInCategory(availableProductsForCurrentCustomer, allGroupProductIdsInCategoriesInternal, list, includeFeaturedProductsInNormalLists);
				}
				else if (manufacturerId > 0)
				{
					var allGroupProductIdsInManufacturerInternal = GetAllGroupProductIdsInManufacturerInternal(manufacturerId);
					result = HasAvailableProductsOnSaleInManufacturer(availableProductsForCurrentCustomer, allGroupProductIdsInManufacturerInternal, manufacturerId, includeFeaturedProductsInNormalLists);
				}
				else if (vendorId > 0)
				{
					var allGroupProductIdsInVendorInternal = GetAllGroupProductIdsInVendorInternal(vendorId);
					result = HasAvailableProductsOnSaleInVendor(availableProductsForCurrentCustomer, allGroupProductIdsInVendorInternal, vendorId, includeFeaturedProductsInNormalLists);
				}
				return result;
			});
		}

		public bool HasProductsInStockInternal(int categoryId, int manufacturerId, int vendorId)
		{
			var key = $"nop.instock.productids-{categoryId}-{manufacturerId}-{vendorId}";
			return _cacheManager.Get(key, delegate
			{
				var result = false;
				var availableProductsForCurrentCustomer = _aclHelper.GetAvailableProductsForCurrentCustomer();
				var showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
				var includeFeaturedProductsInNormalLists = _catalogSettings.IncludeFeaturedProductsInNormalLists;
				if (categoryId > 0)
				{
					var list = new List<int>
					{
						categoryId
					};
					if (showProductsFromSubcategories)
					{
						var collection = from x in _categoryServiceSpikes.GetCategoriesByParentCategoryId(categoryId, includeSubCategoriesFromAllLevels: true)
							select x.Id;
						list.AddRange(collection);
					}
					var allGroupProductIdsInCategoriesInternal = GetAllGroupProductIdsInCategoriesInternal(list);
					result = HasAvailableProductsInStockInCategory(availableProductsForCurrentCustomer, allGroupProductIdsInCategoriesInternal, list, includeFeaturedProductsInNormalLists);
				}
				else if (manufacturerId > 0)
				{
					var allGroupProductIdsInManufacturerInternal = GetAllGroupProductIdsInManufacturerInternal(manufacturerId);
					result = HasAvailableProductsInStockInManufacturer(availableProductsForCurrentCustomer, allGroupProductIdsInManufacturerInternal, manufacturerId, includeFeaturedProductsInNormalLists);
				}
				else if (vendorId > 0)
				{
					var allGroupProductIdsInVendorInternal = GetAllGroupProductIdsInVendorInternal(vendorId);
					result = HasAvailableProductsInStockInVendor(availableProductsForCurrentCustomer, allGroupProductIdsInVendorInternal, vendorId, includeFeaturedProductsInNormalLists);
				}
				return result;
			});
		}

		private bool HasAvailableProductsInStockInVendor(IQueryable<Product> availableProducts, IList<int> groupProductIds, int vendorId, bool includeFeaturedProducts)
		{
			return (from p in availableProducts
				join v in from v in _vendorRepository.Table
					where v.Active && !v.Deleted
					select v on p.VendorId equals v.Id into p_pv
				from v in p_pv.DefaultIfEmpty()
				where ((v != null && v.Id == vendorId && p.ProductTypeId != 10 && p.VisibleIndividually) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (p.ManageInventoryMethodId == 0 || (p.ManageInventoryMethodId == 1 && ((p.StockQuantity > 0 && !p.UseMultipleWarehouses) || (p.ProductWarehouseInventory.Any((ProductWarehouseInventory prodWare) => prodWare.StockQuantity > 0 && prodWare.StockQuantity > prodWare.ReservedQuantity) && p.UseMultipleWarehouses))))
				select p).Any();
		}

		private bool HasAvailableProductsInStockInManufacturer(IQueryable<Product> availableProducts, IList<int> groupProductIds, int manufacturerId, bool includeFeaturedProducts)
		{
			var inner = from pm in _productManufacturerRepository.Table
				join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
				where m.Published && !m.Deleted
				select pm;
			return (from p in availableProducts
				join pm in inner on p.Id equals pm.ProductId into p_pm
				from pm in p_pm.DefaultIfEmpty()
				where (pm != null && pm.ManufacturerId == manufacturerId && p.ProductTypeId != 10 && p.VisibleIndividually && (pm.IsFeaturedProduct == includeFeaturedProducts || !pm.IsFeaturedProduct)) || (groupProductIds.Contains(p.ParentGroupedProductId) && p.Published && !p.Deleted && (p.ManageInventoryMethodId == 0 || (p.ManageInventoryMethodId == 1 && ((p.StockQuantity > 0 && !p.UseMultipleWarehouses) || (p.ProductWarehouseInventory.Any((ProductWarehouseInventory prodWare) => prodWare.StockQuantity > 0 && prodWare.StockQuantity > prodWare.ReservedQuantity) && p.UseMultipleWarehouses)))))
				select p).Any();
		}

		private bool HasAvailableProductsInStockInCategory(IQueryable<Product> availableProducts, IList<int> groupProductIds, List<int> categoryIds, bool includeFeaturedProducts)
		{
			return (from p in availableProducts
				join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId into p_pc
				from pc in p_pc.DefaultIfEmpty()
				where ((pc != null && categoryIds.Contains(pc.CategoryId) && p.ProductTypeId != 10 && p.VisibleIndividually && (pc.IsFeaturedProduct == includeFeaturedProducts || !pc.IsFeaturedProduct)) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (p.ManageInventoryMethodId == 0 || (p.ManageInventoryMethodId == 1 && ((p.StockQuantity > 0 && !p.UseMultipleWarehouses) || (p.ProductWarehouseInventory.Any((ProductWarehouseInventory prodWare) => prodWare.StockQuantity > 0 && prodWare.StockQuantity > prodWare.ReservedQuantity) && p.UseMultipleWarehouses))))
				select p).Any();
		}

		private IList<int> GetAllGroupProductIdsInCategoriesInternal(List<int> categoryIds)
		{
			var key = string.Format("nop.groupproductids.categoryids.{0}", string.Join(",", categoryIds));
			return _cacheManager.Get(key, delegate
			{
				var availableProductsForCurrentCustomer = _aclHelper.GetAvailableProductsForCurrentCustomer();
				availableProductsForCurrentCustomer = availableProductsForCurrentCustomer.Where((Product p) => !p.Deleted);
				availableProductsForCurrentCustomer = availableProductsForCurrentCustomer.Where((Product p) => p.Published);
				availableProductsForCurrentCustomer = availableProductsForCurrentCustomer.Where((Product p) => p.ProductTypeId == 10);
				if (categoryIds != null && categoryIds.Count > 0)
				{
					availableProductsForCurrentCustomer = from p in availableProductsForCurrentCustomer
						from pc in from pc in p.ProductCategories
							where categoryIds.Contains(pc.CategoryId)
							select pc
						select (p);
				}
				return availableProductsForCurrentCustomer.Select((Product x) => x.Id).ToList();
			});
		}

		private IList<int> GetAllGroupProductIdsInManufacturerInternal(int manufacturerId)
		{
			var key = $"nop.groupproductids.manufacturerid.{manufacturerId}";
			return _cacheManager.Get(key, () => (from p in _aclHelper.GetAvailableProductsForCurrentCustomer()
				where !p.Deleted
				where p.Published
				where p.ProductTypeId == 10
				from pm in from x in p.ProductManufacturers
					where x.ManufacturerId == manufacturerId
					select x
				select (p) into x
				select x.Id).ToList());
		}

		private IList<int> GetAllGroupProductIdsInVendorInternal(int vendorId)
		{
			var key = $"nop.groupproductids.vendorid.{vendorId}";
			return _cacheManager.Get(key, () => (from p in _aclHelper.GetAvailableProductsForCurrentCustomer()
				where !p.Deleted
				where p.Published
				where p.ProductTypeId == 10
				where p.VendorId == vendorId
				select p into x
				select x.Id).ToList());
		}

		private bool HasAvailableProductsOnSaleInCategory(IQueryable<Product> availableProducts, IList<int> groupProductIds, IList<int> categoryIds, bool includeFeaturedProducts)
		{
			var nowUtc = DateTime.UtcNow;
			return (from p in availableProducts
				join pc in _productCategoryRepository.TableNoTracking on p.Id equals pc.ProductId into p_pc
				from pc in p_pc.DefaultIfEmpty()
				where ((pc != null && categoryIds.Contains(pc.CategoryId) && p.ProductTypeId != 10 && p.VisibleIndividually && (pc.IsFeaturedProduct == includeFeaturedProducts || !pc.IsFeaturedProduct)) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && p.OldPrice > 0m && p.OldPrice != p.Price
				select p).Any();
		}

		private bool HasAvailableProductsOnSaleInVendor(IQueryable<Product> availableProducts, IList<int> groupProductIds, int vendorId, bool includeFeaturedProducts)
		{
			var nowUtc = DateTime.UtcNow;
			return (from p in availableProducts
				join v in from v in _vendorRepository.Table
					where v.Active && !v.Deleted
					select v on p.VendorId equals v.Id into p_pv
				from v in p_pv.DefaultIfEmpty()
				where ((v != null && v.Id == vendorId && p.ProductTypeId != 10 && p.VisibleIndividually) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && p.OldPrice > 0m && p.OldPrice != p.Price
				select p).Any();
		}

		private bool HasAvailableProductsOnSaleInManufacturer(IQueryable<Product> availableProducts, IList<int> groupProductIds, int manufacturerId, bool includeFeaturedProducts)
		{
			var nowUtc = DateTime.UtcNow;
			var inner = from pm in _productManufacturerRepository.TableNoTracking
				join m in _manufacturerRepository.TableNoTracking on pm.ManufacturerId equals m.Id
				where m.Published && !m.Deleted
				select pm;
			return (from p in availableProducts
				join pm in inner on p.Id equals pm.ProductId into p_pm
				from pm in p_pm.DefaultIfEmpty()
				where ((pm != null && pm.ManufacturerId == manufacturerId && p.ProductTypeId != 10 && p.VisibleIndividually && (pm.IsFeaturedProduct == includeFeaturedProducts || !pm.IsFeaturedProduct)) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && p.OldPrice > 0m && p.OldPrice != p.Price
				select p).Any();
		}
        #endregion
    

        #region Ajax Filter
        public virtual (IPagedList<Product> productsPagedList, IList<(int Id,int FilterCount)> specificationAttributeFilters, IList<(int Id, int FilterCount)> productAttributeFilters, IList<(int Id, int FilterCount)> categoryFilters, IList<(int Id, int FilterCount)> manufacturerFilters, IList<(int Id, int FilterCount)> vendorFilters, IList<(int Id, int FilterCount)> ratingFilters, int productsOnSale, int productsInStock) SearchProducts(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int vendorId = 0,
            int storeId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = false,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            IList<int> filteredProductVariantAttributes = null,
            IList<int> filteredManufacturers = null,
            IList<int> filteredVendors = null,
            int filteredRating = 0,
            bool onSale = false,
            bool inStock = false,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool loadAvailableFilters = false,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            bool loadFilterableProductVariantAttributeIds = false,
            bool loadFilterableCategoryIds = false,
            bool loadFilterableManufacturerIds = false,
            bool loadFilterableVendorIds = false,
            bool loadFilterableProductTagIds = false,
            bool loadFilterableProductReviewIds = false,
            bool loadFilterableOnSale = false,
            bool loadFilterableInStock = false)
        {

            //search by keyword
           var searchLocalizedValue = false;
            if (languageId > 0)
                searchLocalizedValue = (showHidden || _languageService.GetAllLanguages(showHidden).Count >= 2);

            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
            var commaSeparatedCategoryIds = categoryIds == null ? string.Empty : string.Join(",", categoryIds);
            var commaSeparatedAllowedCustomerRoleIds = string.Join(",", allowedCustomerRolesIds);

            var commaSeparatedSpecIds = string.Empty;
            if (filteredSpecs != null)
            {
                ((List<int>)filteredSpecs).Sort();
                commaSeparatedSpecIds = string.Join(",", filteredSpecs);
            }

            var commaSeparatedVariantAttributesIds = string.Empty;
            if (filteredProductVariantAttributes != null)
            {
                ((List<int>)filteredProductVariantAttributes).Sort();
                commaSeparatedVariantAttributesIds = string.Join(",", filteredProductVariantAttributes);
            }

            var commaSeparatedManufacturersIds = string.Empty;
            if (filteredManufacturers != null)
            {
                ((List<int>)filteredManufacturers).Sort();
                commaSeparatedManufacturersIds = string.Join(",", filteredManufacturers);
            }

            var commaSeparatedVendorsIds = string.Empty;
            if (filteredVendors != null)
            {
                ((List<int>)filteredVendors).Sort();
                commaSeparatedVendorsIds = string.Join(",", filteredVendors);
            }
            if (pageSize == int.MaxValue)
                pageSize = 2147483646;

            var pCategoryIds = _dataProvider.GetStringParameter("CategoryIds", commaSeparatedCategoryIds);
            var pManufacturerId = _dataProvider.GetInt32Parameter("ManufacturerId", manufacturerId);
            var pStoreId = _dataProvider.GetInt32Parameter("StoreId", !_catalogSettings.IgnoreStoreLimitations ? storeId : 0);
            var pVendorId = _dataProvider.GetInt32Parameter("VendorId", vendorId);
            var pWarehouseId = _dataProvider.GetInt32Parameter("WarehouseId", 0);
            var pParentGroupedProductId = _dataProvider.GetInt32Parameter("ParentGroupedProductId", 0);
            var pProductTypeId = _dataProvider.GetInt32Parameter("ProductTypeId", (int?)productType);
            var pVisibleIndividuallyOnly = _dataProvider.GetBooleanParameter("VisibleIndividuallyOnly", visibleIndividuallyOnly);
            var pProductTagId = _dataProvider.GetInt32Parameter("ProductTagId", productTagId);
            var pFeaturedProducts = _dataProvider.GetBooleanParameter("FeaturedProducts", featuredProducts);
            var pPriceMin = _dataProvider.GetDecimalParameter("PriceMin", priceMin);
            var pPriceMax = _dataProvider.GetDecimalParameter("PriceMax", priceMax);
            var pKeywords = _dataProvider.GetStringParameter("Keywords", keywords);
            var pSearchDescriptions = _dataProvider.GetBooleanParameter("SearchDescriptions", searchDescriptions);
            var pSearchManufacturerPartNumber = _dataProvider.GetBooleanParameter("SearchManufacturerPartNumber", searchManufacturerPartNumber);
            var pSearchSku = _dataProvider.GetBooleanParameter("SearchSku", searchSku);
            var pSearchProductTags = _dataProvider.GetBooleanParameter("SearchProductTags", searchProductTags);
            var pUseFullTextSearch = _dataProvider.GetBooleanParameter("UseFullTextSearch", _commonSettings.UseFullTextSearch);
            var pFullTextMode = _dataProvider.GetInt32Parameter("FullTextMode", (int)_commonSettings.FullTextMode);
            var pFilteredSpecs = _dataProvider.GetStringParameter("FilteredSpecs", commaSeparatedSpecIds);
            var pFilteredProductVariantAttributes = _dataProvider.GetStringParameter("FilteredProductVariantAttributes", commaSeparatedVariantAttributesIds);
            var pFilteredManufacturers = _dataProvider.GetStringParameter("FilteredManufacturers", commaSeparatedManufacturersIds);
            var pFilteredVendors = _dataProvider.GetStringParameter("FilteredVendors", commaSeparatedVendorsIds);
            var pFilteredRating = _dataProvider.GetInt32Parameter("FilteredRating", filteredRating);
            var pOnSale = _dataProvider.GetBooleanParameter("OnSale", onSale);
            var pInStock = _dataProvider.GetBooleanParameter("InStock", inStock);
            var pLanguageId = _dataProvider.GetInt32Parameter("LanguageId", searchLocalizedValue ? languageId : 0);
            var pOrderBy = _dataProvider.GetInt32Parameter("OrderBy", (int)orderBy);
            var pAllowedCustomerRoleIds = _dataProvider.GetStringParameter("AllowedCustomerRoleIds", !_catalogSettings.IgnoreAcl ? commaSeparatedAllowedCustomerRoleIds : string.Empty);
            var pPageIndex = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = _dataProvider.GetInt32Parameter("PageSize", pageSize);
            var pShowHidden = _dataProvider.GetBooleanParameter("ShowHidden", showHidden);
            var pLoadAvailableFilters = _dataProvider.GetBooleanParameter("LoadAvailableFilters", loadAvailableFilters);
            var pLoadFilterableSpecificationAttributeOptionIds = _dataProvider.GetBooleanParameter("LoadFilterableSpecificationAttributeOptionIds", loadFilterableSpecificationAttributeOptionIds);
            var pLoadFilterableProductVariantAttributeIds = _dataProvider.GetBooleanParameter("LoadFilterableProductVariantAttributeIds", loadFilterableProductVariantAttributeIds);
            var pLoadFilterableCategoryIds = _dataProvider.GetBooleanParameter("LoadFilterableCategoryIds", loadFilterableCategoryIds);
            var pLoadFilterableManufacturerIds = _dataProvider.GetBooleanParameter("LoadFilterableManufacturerIds", loadFilterableManufacturerIds);
            var pLoadFilterableVendorIds = _dataProvider.GetBooleanParameter("LoadFilterableVendorIds", loadFilterableVendorIds);
            var pLoadFilterableProductTagIds = _dataProvider.GetBooleanParameter("LoadFilterableProductTagIds", loadFilterableProductTagIds);
            var pLoadFilterableProductReviewIds = _dataProvider.GetBooleanParameter("LoadFilterableProductReviewIds", loadFilterableProductReviewIds);
            var pLoadFilterableOnSale = _dataProvider.GetBooleanParameter("LoadFilterableOnSale", loadFilterableOnSale);
            var pLoadFilterableInStock = _dataProvider.GetBooleanParameter("LoadFilterableInStock", loadFilterableInStock);
            //prepare output parameters
            var pFilterableSpecificationAttributeOptionIds = _dataProvider.GetOutputStringParameter("FilterableSpecificationAttributeOptionIds");
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue;
            var pFilterableProductVariantAttributeIds = _dataProvider.GetOutputStringParameter("FilterableProductVariantAttributeIds");
            pFilterableProductVariantAttributeIds.Size = int.MaxValue;
            var pFilterableManufacturerIds = _dataProvider.GetOutputStringParameter("FilterableManufacturerIds");
            pFilterableManufacturerIds.Size = int.MaxValue;
            var pFilterableVendorIds = _dataProvider.GetOutputStringParameter("FilterableVendorIds");
            pFilterableVendorIds.Size = int.MaxValue;
            var pProductsOnSale = _dataProvider.GetOutputInt32Parameter("ProductsOnSale");
            var pProductsInStock = _dataProvider.GetOutputInt32Parameter("ProductsInStock");
            var pTotalRecords = _dataProvider.GetOutputInt32Parameter("TotalRecords");
            var pFilterableCategoryIds = _dataProvider.GetOutputStringParameter("FilterableCategoryIds");
            pFilterableCategoryIds.Size = int.MaxValue;
            var pFilterableRatingIds = _dataProvider.GetOutputStringParameter("FilterableRatingIds");
            pFilterableRatingIds.Size = int.MaxValue;
            var products = _dbContext.EntityFromSql<Product>("ProductLoadAllPagedNopAjaxFilters",
                pCategoryIds,
                pManufacturerId,
                pStoreId,
                pVendorId,
                pParentGroupedProductId,
                pProductTypeId,
                pVisibleIndividuallyOnly,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pSearchManufacturerPartNumber,
                pSearchSku,
                pSearchProductTags,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pFilteredProductVariantAttributes,
                pFilteredManufacturers,
                pFilteredVendors,
                pFilteredRating,
                pOnSale,
                pInStock,
                pLanguageId,
                pOrderBy,
                pAllowedCustomerRoleIds,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pLoadAvailableFilters,
                pFilterableSpecificationAttributeOptionIds,
                pFilterableProductVariantAttributeIds,
                pFilterableManufacturerIds,
                pFilterableVendorIds,
                pFilterableCategoryIds,
                pFilterableRatingIds,
                pLoadFilterableSpecificationAttributeOptionIds,
                pLoadFilterableProductVariantAttributeIds,
                pLoadFilterableCategoryIds,
                pLoadFilterableManufacturerIds,
                pLoadFilterableVendorIds,
                pLoadFilterableProductTagIds,
                pLoadFilterableProductReviewIds,
                pLoadFilterableOnSale,
                pLoadFilterableInStock,
                pProductsOnSale,
                pProductsInStock,
                pTotalRecords).ToList();

            var filterableSpecificationAttributeOptionIdsStr = pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value ? (string)pFilterableSpecificationAttributeOptionIds.Value : string.Empty;
            var filterableSpecificationOptionIds = new List<(int Id, int FilterCount)>();
            if (!string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationOptionIds = filterableSpecificationAttributeOptionIdsStr
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => (Convert.ToInt32(x.Substring(0, x.IndexOf(':'))), Convert.ToInt32(x.Substring(x.IndexOf(':') + 1))))
                    .ToList();
            }

            var pFilterableProductVariantAttributeIdsStr = pFilterableProductVariantAttributeIds.Value != DBNull.Value ? (string)pFilterableProductVariantAttributeIds.Value : string.Empty;
            var filterableProductVariantIds = new List<(int Id, int FilterCount)>();
            if (!string.IsNullOrWhiteSpace(pFilterableProductVariantAttributeIdsStr))
            {
                filterableProductVariantIds = pFilterableProductVariantAttributeIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => (Convert.ToInt32(x.Substring(0, x.IndexOf(':'))), Convert.ToInt32(x.Substring(x.IndexOf(':') + 1))))
                   .ToList();
            }

            var pFilterableManufacturerIdsStr = pFilterableManufacturerIds.Value != DBNull.Value ? (string)pFilterableManufacturerIds.Value : string.Empty;
            var filterableManufacturerIds = new List<(int Id, int FilterCount)>();
            if (!string.IsNullOrWhiteSpace(pFilterableManufacturerIdsStr))
            {
                filterableManufacturerIds = pFilterableManufacturerIdsStr
                  .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(x => (Convert.ToInt32(x.Substring(0, x.IndexOf(':'))), Convert.ToInt32(x.Substring(x.IndexOf(':') + 1))))
                  .ToList();
            }

            var pFilterableVendorIdsStr = pFilterableVendorIds.Value != DBNull.Value ? (string)pFilterableVendorIds.Value : string.Empty;
            var filterableVendorIds = new List<(int Id, int FilterCount)>();
            if (!string.IsNullOrWhiteSpace(pFilterableVendorIdsStr))
            {
                filterableVendorIds = pFilterableVendorIdsStr
                  .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(x => (Convert.ToInt32(x.Substring(0, x.IndexOf(':'))), Convert.ToInt32(x.Substring(x.IndexOf(':') + 1))))
                  .ToList();
            }

            var pFilterableCategoryIdsStr = pFilterableCategoryIds.Value != DBNull.Value ? (string)pFilterableCategoryIds.Value : string.Empty;
            var filterableCategoryIds = new List<(int Id, int FilterCount)>();
            if (!string.IsNullOrWhiteSpace(pFilterableCategoryIdsStr))
            {
                  filterableCategoryIds = pFilterableCategoryIdsStr
                  .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(x => (Convert.ToInt32(x.Substring(0, x.IndexOf(':'))), Convert.ToInt32(x.Substring(x.IndexOf(':') + 1))))
                  .ToList();
            }

            var pFilterableRatingIdsStr = pFilterableRatingIds.Value != DBNull.Value ? (string)pFilterableRatingIds.Value : string.Empty;
            var filterableRatingIds = new List<(int Id, int FilterCount)>();
            if (!string.IsNullOrWhiteSpace(pFilterableRatingIdsStr))
            {
                filterableRatingIds = pFilterableRatingIdsStr
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (Convert.ToInt32(x.Substring(0, x.IndexOf(':'))), Convert.ToInt32(x.Substring(x.IndexOf(':') + 1))))
                .ToList();
            }

            //return products

            var totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;
            var productsOnSale = pProductsOnSale.Value != DBNull.Value ? Convert.ToInt32(pProductsOnSale.Value) : 0;
            var productsInStock = pProductsInStock.Value != DBNull.Value ? Convert.ToInt32(pProductsInStock.Value) : 0;
            var productsPagedList = new PagedList<Product>(products, pageIndex, pageSize, totalRecords);
            return (productsPagedList, filterableSpecificationOptionIds, filterableProductVariantIds, filterableCategoryIds, filterableManufacturerIds, filterableVendorIds, filterableRatingIds,productsOnSale, productsInStock);
        }
        
        public virtual (IPagedList<Product> productsPagedList, IList<SpecificationFilterGroup> specificationAttributeFilters, IList<AttributeFilterGroup> productAttributeFilters, IList<CategoryFilterItem> categoryFilters, IList<ManufacturerFilterItem> manufacturerFilters, IList<VendorFilterItem> vendorFilters, IList<RatingFilterItem> ratingFilters, int productsOnSale, int productsInStock) SearchProducts(
           ref decimal? priceMin,
           ref decimal? priceMax,
           bool loadFilterableOnSale = false,
           bool loadFilterableInStock = false,
           bool loadFilterablePriceRange = false,
           bool loadFilterableSpecificationAttributeOptions = false,
           bool loadFilterableProductVariantAttributes = false,
           bool loadFilterableCategories = false,
           bool loadFilterableManufacturers = false,
           bool loadFilterableVendors = false,
           bool loadFilterableProductTags = false,
           bool loadFilterableProductReviews = false,
           int? pageIndex = null,
           int? pageSize = null,
           IList<int> categoryIds = null,
           int manufacturerId = 0,
           int storeId = 0,
           int vendorId = 0,
           int warehouseId = 0,
           ProductType? productType = null,
           bool visibleIndividuallyOnly = false,
           bool markedAsNewOnly = false,
           bool? featuredProducts = null,
           int productTagId = 0,
           string keywords = null,
           bool searchDescriptions = false,
           bool searchManufacturerPartNumber = true,
           bool searchSku = true,
           bool searchProductTags = false,
           int languageId = 0,
           ProductSortingEnum orderBy = ProductSortingEnum.Position,
           bool showHidden = false,
           bool? overridePublished = null, IList<string> filterTypes = null)
        {

            if (filterTypes.Any())
            {
                //search by keyword
                var searchLocalizedValue = false;
                if (languageId > 0)
                {
                    if (showHidden)
                    {
                        searchLocalizedValue = true;
                    }
                    else
                    {
                        //ensure that we have at least two published languages
                        var totalPublishedLanguages = _languageService.GetAllLanguages().Count;
                        searchLocalizedValue = totalPublishedLanguages >= 2;
                    }
                }

                //validate "categoryIds" parameter
                if (categoryIds != null && categoryIds.Contains(0))
                    categoryIds.Remove(0);

                //Access control list. Allowed customer roles
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();

                //pass category identifiers as comma-delimited string
                var commaSeparatedCategoryIds = categoryIds == null ? string.Empty : string.Join(",", categoryIds);

                //pass customer role identifiers as comma-delimited string
                var commaSeparatedAllowedCustomerRoleIds = string.Join(",", allowedCustomerRolesIds);

                var commaSeparatedFilterTypes = string.Empty;
                if (filterTypes != null)
                {
                    commaSeparatedFilterTypes = string.Join(",", filterTypes);
                }

                //some databases don't support int.MaxValue
                var pageOfIndex = pageIndex ?? 0;
                var pageOfSize = pageSize ?? int.MaxValue - 1;

                //prepare input parameters
                var pCategoryIds = _dataProvider.GetStringParameter("CategoryIds", commaSeparatedCategoryIds);
                var pManufacturerId = _dataProvider.GetInt32Parameter("ManufacturerId", manufacturerId);
                var pStoreId = _dataProvider.GetInt32Parameter("StoreId", !_catalogSettings.IgnoreStoreLimitations ? storeId : 0);
                var pVendorId = _dataProvider.GetInt32Parameter("VendorId", vendorId);
                var pWarehouseId = _dataProvider.GetInt32Parameter("WarehouseId", warehouseId);
                var pProductTypeId = _dataProvider.GetInt32Parameter("ProductTypeId", (int?)productType);
                var pVisibleIndividuallyOnly = _dataProvider.GetBooleanParameter("VisibleIndividuallyOnly", visibleIndividuallyOnly);
                var pMarkedAsNewOnly = _dataProvider.GetBooleanParameter("MarkedAsNewOnly", markedAsNewOnly);
                var pProductTagId = _dataProvider.GetInt32Parameter("ProductTagId", productTagId);
                var pFeaturedProducts = _dataProvider.GetBooleanParameter("FeaturedProducts", featuredProducts);
                var pKeywords = _dataProvider.GetStringParameter("Keywords", keywords);
                var pSearchDescriptions = _dataProvider.GetBooleanParameter("SearchDescriptions", searchDescriptions);
                var pSearchManufacturerPartNumber = _dataProvider.GetBooleanParameter("SearchManufacturerPartNumber", searchManufacturerPartNumber);
                var pSearchSku = _dataProvider.GetBooleanParameter("SearchSku", searchSku);
                var pSearchProductTags = _dataProvider.GetBooleanParameter("SearchProductTags", searchProductTags);
                var pUseFullTextSearch = _dataProvider.GetBooleanParameter("UseFullTextSearch", _commonSettings.UseFullTextSearch);
                var pFullTextMode = _dataProvider.GetInt32Parameter("FullTextMode", (int)_commonSettings.FullTextMode);
                var pLanguageId = _dataProvider.GetInt32Parameter("LanguageId", searchLocalizedValue ? languageId : 0);
                var pOrderBy = _dataProvider.GetInt32Parameter("OrderBy", (int)orderBy);
                var pAllowedCustomerRoleIds = _dataProvider.GetStringParameter("AllowedCustomerRoleIds", !_catalogSettings.IgnoreAcl ? commaSeparatedAllowedCustomerRoleIds : string.Empty);
                var pPageIndex = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
                var pPageSize = _dataProvider.GetInt32Parameter("PageSize", pageSize);
                var pShowHidden = _dataProvider.GetBooleanParameter("ShowHidden", showHidden);
                var pOverridePublished = _dataProvider.GetBooleanParameter("OverridePublished", overridePublished);

                var pLoadFilterablePriceRange = _dataProvider.GetBooleanParameter("LoadFilterablePriceRange", loadFilterablePriceRange);
                var pLoadFilterableOnSale = _dataProvider.GetBooleanParameter("LoadFilterableOnSale", loadFilterableOnSale);
                var pLoadFilterableInStock = _dataProvider.GetBooleanParameter("LoadFilterableInStock", loadFilterableInStock);
                var pLoadFilterableSpecificationAttributeOptions = _dataProvider.GetBooleanParameter("LoadFilterableSpecificationAttributeOptions", loadFilterableSpecificationAttributeOptions);
                var pLoadFilterableProductVariantAttributes = _dataProvider.GetBooleanParameter("LoadFilterableProductVariantAttributes", loadFilterableProductVariantAttributes);
                var pLoadFilterableManufacturers = _dataProvider.GetBooleanParameter("LoadFilterableManufacturers", loadFilterableManufacturers);
                var pLoadFilterableCategories = _dataProvider.GetBooleanParameter("LoadFilterableCategories", loadFilterableCategories);
                var pLoadFilterableVendors = _dataProvider.GetBooleanParameter("LoadFilterableVendors", loadFilterableVendors);
                var pLoadFilterableProductTags = _dataProvider.GetBooleanParameter("LoadFilterableProductTags", loadFilterableProductTags);
                var pLoadFilterableProductReviews = _dataProvider.GetBooleanParameter("LoadFilterableProductReviews", loadFilterableProductReviews);
                var pFilterTypes = _dataProvider.GetStringParameter("FilterTypes", commaSeparatedFilterTypes);
                //prepare Input output parameters
                var pPriceMin = _dataProvider.GetDecimalParameter("PriceMin", priceMin);
                pPriceMin.Direction = System.Data.ParameterDirection.InputOutput;
                var pPriceMax = _dataProvider.GetDecimalParameter("PriceMax", priceMax);
                pPriceMax.Direction = System.Data.ParameterDirection.InputOutput;
                //prepare output parameters
                var pFilterableSpecificationAttributeOptions = _dataProvider.GetOutputStringParameter("FilterableSpecificationAttributeOptions");
                pFilterableSpecificationAttributeOptions.Size = int.MaxValue - 1;

                var pFilterableProductVariantAttributes = _dataProvider.GetOutputStringParameter("FilterableProductVariantAttributes");
                pFilterableProductVariantAttributes.Size = int.MaxValue - 1;

                var pFilterableManufacturers = _dataProvider.GetOutputStringParameter("FilterableManufacturers");
                pFilterableManufacturers.Size = int.MaxValue - 1;

                var pFilterableCategories = _dataProvider.GetOutputStringParameter("FilterableCategories");
                pFilterableCategories.Size = int.MaxValue - 1;

                var pFilterableVendors = _dataProvider.GetOutputStringParameter("FilterableVendors");
                pFilterableVendors.Size = int.MaxValue - 1;

                var pFilterableProductTags = _dataProvider.GetOutputStringParameter("FilterableProductTags");
                pFilterableProductTags.Size = int.MaxValue - 1;

                var pFilterableProductReviews = _dataProvider.GetOutputStringParameter("FilterableProductReviews");
                pFilterableProductReviews.Size = int.MaxValue - 1;

                var pProductsOnSale = _dataProvider.GetOutputInt32Parameter("ProductsOnSale");
                var pProductsInStock = _dataProvider.GetOutputInt32Parameter("ProductsInStock");
                var pTotalRecords = _dataProvider.GetOutputInt32Parameter("TotalRecords");

                //invoke stored procedure
                var products = _dbContext.EntityFromSql<Product>("ProductFilterLoadAllPaged",
                    pCategoryIds,
                    pManufacturerId,
                    pStoreId,
                    pVendorId,
                    pWarehouseId,
                    pProductTypeId,
                    pVisibleIndividuallyOnly,
                    pMarkedAsNewOnly,
                    pProductTagId,
                    pFeaturedProducts,
                    pKeywords,
                    pSearchDescriptions,
                    pSearchManufacturerPartNumber,
                    pSearchSku,
                    pSearchProductTags,
                    pUseFullTextSearch,
                    pFullTextMode,
                    pLanguageId,
                    pOrderBy,
                    pAllowedCustomerRoleIds,
                    pPageIndex,
                    pPageSize,
                    pShowHidden,
                    pOverridePublished,
                    pLoadFilterableSpecificationAttributeOptions, pFilterableSpecificationAttributeOptions,
                    pLoadFilterableCategories,pFilterableCategories,
                    pLoadFilterableManufacturers, pFilterableManufacturers,
                    pLoadFilterableProductVariantAttributes, pFilterableProductVariantAttributes,
                    pLoadFilterableVendors, pFilterableVendors,
                    pLoadFilterableProductTags, pFilterableProductTags,
                    pLoadFilterableProductReviews, pFilterableProductReviews,
                    pLoadFilterablePriceRange, pPriceMin,pPriceMax,
                    pLoadFilterableOnSale, pProductsOnSale,
                    pLoadFilterableInStock, pProductsInStock,
                    pTotalRecords, pFilterTypes).ToList();


                //get filterable specification attribute option identifier
                var filterableSpecificationAttributeOptionsStr =
                    pFilterableSpecificationAttributeOptions.Value != DBNull.Value
                        ? (string)pFilterableSpecificationAttributeOptions.Value
                        : string.Empty;
                IList<SpecificationFilterGroup> specificationFilters = null;
                if (loadFilterableSpecificationAttributeOptions &&
                    !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionsStr))
                    specificationFilters = JsonConvert.DeserializeObject<IList<SpecificationFilterGroup>>(filterableSpecificationAttributeOptionsStr);

                var filterableProductVariantAttributesStr =
                        pFilterableProductVariantAttributes.Value != DBNull.Value
                            ? (string)pFilterableProductVariantAttributes.Value
                            : string.Empty;
                IList<AttributeFilterGroup> attributeFilters = null;
                if (loadFilterableProductVariantAttributes &&
                    !string.IsNullOrWhiteSpace(filterableProductVariantAttributesStr))
                    attributeFilters = JsonConvert.DeserializeObject<IList<AttributeFilterGroup>>(filterableProductVariantAttributesStr);

                var filterableManufacturersStr =
                        pFilterableManufacturers.Value != DBNull.Value
                            ? (string)pFilterableManufacturers.Value
                            : string.Empty;
                IList<ManufacturerFilterItem> manufacturerFilters = null;
                if (loadFilterableManufacturers &&
                    !string.IsNullOrWhiteSpace(filterableManufacturersStr))
                    manufacturerFilters = JsonConvert.DeserializeObject<IList<ManufacturerFilterItem>>(filterableManufacturersStr);

                var filterableCategoriesStr =
                        pFilterableCategories.Value != DBNull.Value
                            ? (string)pFilterableCategories.Value
                            : string.Empty;
                IList<CategoryFilterItem> categoryFilters = null;
                if (loadFilterableCategories &&
                    !string.IsNullOrWhiteSpace(filterableCategoriesStr))
                    categoryFilters = JsonConvert.DeserializeObject<IList<CategoryFilterItem>>(filterableCategoriesStr);

                var filterableVendorsStr =
                      pFilterableVendors.Value != DBNull.Value
                          ? (string)pFilterableVendors.Value
                          : string.Empty;
                IList<VendorFilterItem> vendorFilters = null;
                if (loadFilterableVendors &&
                    !string.IsNullOrWhiteSpace(filterableVendorsStr))
                    vendorFilters = JsonConvert.DeserializeObject<IList<VendorFilterItem>>(filterableVendorsStr);

                var filterableProductReviewsStr =
                    pFilterableProductReviews.Value != DBNull.Value
                        ? (string)pFilterableProductReviews.Value
                        : string.Empty;
                IList <RatingFilterItem> ratingFilters = null;
                if (loadFilterableProductReviews &&
                    !string.IsNullOrWhiteSpace(filterableProductReviewsStr))
                    ratingFilters = JsonConvert.DeserializeObject<IList<RatingFilterItem>>(filterableProductReviewsStr);

                //return products
                var productsOnSale = pProductsOnSale.Value != DBNull.Value ? Convert.ToInt32(pProductsOnSale.Value) : 0;
                var productsInStock = pProductsInStock.Value != DBNull.Value ? Convert.ToInt32(pProductsInStock.Value) : 0;
                priceMin = pPriceMin.Value != DBNull.Value ? Convert.ToDecimal(pPriceMin.Value) : decimal.Zero;
                priceMax = pPriceMax.Value != DBNull.Value ? Convert.ToDecimal(pPriceMax.Value) : decimal.Zero;
                var totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;
                var productsPagedList = new PagedList<Product>(products, pageOfIndex, pageOfSize, totalRecords);
                return (productsPagedList, specificationFilters, attributeFilters, categoryFilters,manufacturerFilters, vendorFilters,ratingFilters, productsOnSale, productsInStock);
            }
            else
            {
                IPagedList<Product> productsPagedList = new PagedList<Product>(new List<Product>(), 0, 1);
                IList<SpecificationFilterGroup> specificationFilters = null;
                IList<ManufacturerFilterItem> manufacturerFilters = null;
                IList<AttributeFilterGroup> attributeFilters = null;
                IList<VendorFilterItem> vendorFilters = null;
                IList<CategoryFilterItem> categoryFilters = null;
                IList <RatingFilterItem> ratingFilters = null;
                return (productsPagedList, specificationFilters, attributeFilters, categoryFilters, manufacturerFilters, vendorFilters, ratingFilters, 0, 0);
            }
        }

        #endregion
    }
}