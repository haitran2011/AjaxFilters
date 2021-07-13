using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Spikes.Nop.Framework.Mappings;
using Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Extensions;
using Spikes.Nop.Plugins.AjaxFilters.Domain;
using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using Spikes.Nop.Plugins.AjaxFilters.Extensions;
using Spikes.Nop.Plugins.AjaxFilters.Helpers;
using Spikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using Spikes.Nop.Plugins.AjaxFilters.Models;
using Spikes.Nop.Plugins.AjaxFilters.QueryStringManipulation;
using Spikes.Nop.Plugins.AjaxFilters.Services;
using Spikes.Nop.Services.Catalog.DTO;

namespace Spikes.Nop.Plugins.AjaxFilters.Factories
{
    public class NopAjaxFilterModelFactory : INopAjaxFilterModelFactory
    {
        #region field
        private readonly CatalogSettings _catalogSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IProductServiceNopAjaxFilters _productServiceNopAjaxFilters;
        private readonly IPriceCalculationServiceNopAjaxFilters _priceCalculationServiceNopAjaxFilters;
        private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;
        private readonly IQueryStringToModelUpdater _queryStringToModelUpdater;
        private readonly IQueryStringBuilder _queryStringBuilder;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICatalogModelFactory _catalogModelFactory;
        #endregion

        #region ctor
        public NopAjaxFilterModelFactory(CatalogSettings catalogSettings,
            IProductServiceNopAjaxFilters productServiceNopAjaxFilters,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            IPriceCalculationServiceNopAjaxFilters priceCalculationServiceNopAjaxFilters,
            NopAjaxFiltersSettings nopAjaxFiltersSettings,
            MediaSettings mediaSettings,
            ICategoryService categoryService,
            IProductModelFactory productModelFactory,
            ICatalogModelFactory catalogModelFactory,
            IQueryStringToModelUpdater queryStringToModelUpdater,
            IQueryStringBuilder queryStringBuilder,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService, IPictureService pictureService)
        {
            _catalogSettings = catalogSettings;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _priceCalculationServiceNopAjaxFilters = priceCalculationServiceNopAjaxFilters;
            _nopAjaxFiltersSettings = nopAjaxFiltersSettings;
            _categoryService = categoryService;
            _productModelFactory = productModelFactory;
            _catalogModelFactory = catalogModelFactory;
            _queryStringToModelUpdater = queryStringToModelUpdater;
            _queryStringBuilder = queryStringBuilder;
            _productServiceNopAjaxFilters = productServiceNopAjaxFilters;
            _priceFormatter = priceFormatter;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;

        }
        #endregion

        #region SearchModel

        public virtual ProductsModel PrepareSearchModel(
            SpecificationFilterModelSpikes specificationFilterModelSpikes,
            AttributeFilterModelSpikes attributeFilterModelSpikes,
            CategoryFilterModelSpikes categoryFilterModelSpikes,
            ManufacturerFilterModelSpikes manufacturerFilterModelSpikes,
            VendorFilterModelSpikes vendorFilterModelSpikes,
            RatingFilterModelSpikes ratingFilterModelSpikes,
            PriceRangeFilterModelSpikes priceRangeFilterModelSpikes,
            OnSaleFilterModelSpikes onSaleFilterModelSpikes, InStockFilterModelSpikes inStockFilterModelSpikes,
            CatalogPagingFilteringModel catalogPagingFilteringModel,
            string queryString,
            string searchTerms,
            bool isOnSearchPage = false,
            int categoryId = 0,
            int manufacturerId = 0, int vendorId = 0,
            int productTagId = 0,
            bool searchDescriptions = false)
        {
            var storeId = _storeContext.CurrentStore.Id;
            var featuredProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : new bool?(false);
            var languageId = _workContext.WorkingLanguage.Id;
            var currencyId = _workContext.WorkingCurrency.Id;
            var includeProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
            _queryStringBuilder.SetDataForQueryString(
                specificationFilterModelSpikes, attributeFilterModelSpikes, categoryFilterModelSpikes,
                manufacturerFilterModelSpikes, vendorFilterModelSpikes, ratingFilterModelSpikes,
                priceRangeFilterModelSpikes, catalogPagingFilteringModel, onSaleFilterModelSpikes, inStockFilterModelSpikes);

            var rebuildCacheQueryString = string.Empty;
            if (!string.IsNullOrEmpty(queryString))
                rebuildCacheQueryString = queryString;
            else
                rebuildCacheQueryString = _queryStringBuilder.GetQueryString(shouldRebuildQueryString: true);

            var cacheKey = string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY, storeId, languageId, currencyId, categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, rebuildCacheQueryString, searchTerms, includeProductsFromSubcategories, featuredProducts);;
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var loadAvailableFilters = _nopAjaxFiltersSettings.EnableAjaxFilters;
                var searchProductTags = _nopAjaxFiltersSettings.SearchInProductTags || searchDescriptions;
                var enableCategoriesFilter = _nopAjaxFiltersSettings.EnableCategoriesFilter;
                var enableManufacturersFilter = _nopAjaxFiltersSettings.EnableManufacturersFilter;
                var enableVendorsFilter = _nopAjaxFiltersSettings.EnableVendorsFilter;
                var enablePriceRangeFilter = _nopAjaxFiltersSettings.EnablePriceRangeFilter;
                var enableSpecificationsFilter = _nopAjaxFiltersSettings.EnableSpecificationsFilter;
                var enableAttributesFilter = _nopAjaxFiltersSettings.EnableAttributesFilter;
                var enableRatingFilter = _nopAjaxFiltersSettings.EnableRatingFilter;
                var enableOnSaleFilter = _nopAjaxFiltersSettings.EnableOnSaleFilter;
                var enableInStockFilter = _nopAjaxFiltersSettings.EnableInStockFilter;

                var categoryIds = new List<int>();
                if (categoryId > 0)
                {
                    categoryIds.Add(categoryId);
                    if (includeProductsFromSubcategories)
                    {
                        categoryIds.AddRange(_categoryService.GetChildCategoryIds(categoryId, _storeContext.CurrentStore.Id));
                    }
                }

                (decimal minPriceConverted, decimal maxPriceConverted) priceRangeFilterModel = (decimal.Zero, decimal.Zero);
                if (priceRangeFilterModelSpikes != null)
                    priceRangeFilterModel = (priceRangeFilterModelSpikes.MinPrice, priceRangeFilterModelSpikes.MaxPrice);

                var filterModelDTO = SetFilterDTOs(specificationFilterModelSpikes, attributeFilterModelSpikes, categoryFilterModelSpikes, manufacturerFilterModelSpikes, vendorFilterModelSpikes);
                var specificationFilterModelDTO = filterModelDTO.specificationFilterModelDTO;
                var attributeFilterModelDTO = filterModelDTO.attributeFilterModelDTO;
                var categoryFilterModelDTO = filterModelDTO.categoryFilterModelDTO;
                var manufacturerFilterModelDTO = filterModelDTO.manufacturerFilterModelDTO;
                var vendorFilterModelDTO = filterModelDTO.vendorFilterModelDTO;
                var filteredSpecOptionIds = (specificationFilterModelDTO != null && specificationFilterModelDTO.SpecificationFilterDTOs.Any()) ? specificationFilterModelDTO.SpecificationFilterDTOs.SelectMany((SpecificationFilterDTO x) => x.SelectedFilterIds).ToList() : null;
                var filteredProductVariantAttributesIds = (attributeFilterModelDTO != null && filterModelDTO.attributeFilterModelDTO.AttributeFilterDTOs.Any()) ? filterModelDTO.attributeFilterModelDTO.AttributeFilterDTOs.SelectMany((AttributeFilterDTO x) => x.SelectedProductVariantIds).ToList() : null;
                var filteredManufacturersIds = (manufacturerFilterModelDTO != null && manufacturerFilterModelDTO.SelectedFilterIds.Any()) ? manufacturerFilterModelDTO.SelectedFilterIds.ToList() : null;
                var filteredVendorsIds = (vendorFilterModelDTO != null && vendorFilterModelDTO.SelectedFilterIds.Any()) ? vendorFilterModelDTO.SelectedFilterIds.ToList() : null;
                var minRating = ratingFilterModelSpikes.MinRating;
                var filteredCategoriesIds = (categoryFilterModelDTO != null && categoryFilterModelDTO.SelectedFilterIds.Any()) ? categoryFilterModelDTO.SelectedFilterIds.ToList() : categoryIds;
             
                var pageIndex = catalogPagingFilteringModel.PageNumber > 0 ? catalogPagingFilteringModel.PageNumber - 1 : 0;
                var pageSize = catalogPagingFilteringModel.PageSize;
                var orderBy = (ProductSortingEnum)catalogPagingFilteringModel.OrderBy.Value;
                bool onSale = false, inStock = false;
                if (onSaleFilterModelSpikes != null)
                    onSale = (onSaleFilterModelSpikes.FilterItemState == FilterItemState.Checked);
                if (inStockFilterModelSpikes != null)
                    inStock = (inStockFilterModelSpikes.FilterItemState == FilterItemState.Checked);
                _queryStringToModelUpdater.UpdateModelsFromQueryString(queryString, specificationFilterModelSpikes, attributeFilterModelSpikes, manufacturerFilterModelSpikes, vendorFilterModelSpikes, categoryFilterModelSpikes,ratingFilterModelSpikes,priceRangeFilterModelSpikes, catalogPagingFilteringModel, onSaleFilterModelSpikes, inStockFilterModelSpikes);
                if (priceRangeFilterModelSpikes != null && priceRangeFilterModelSpikes.SelectedPriceRange != null)
                    priceRangeFilterModel = AdjustMinAndMaxPriceRangeWithDiscountsAndTax(categoryId, manufacturerId, vendorId, productTagId,priceRangeFilterModelSpikes);

                var productFilterModel = _productServiceNopAjaxFilters.SearchProducts(
                categoryIds: filteredCategoriesIds,
                filteredSpecs: filteredSpecOptionIds,
                filteredProductVariantAttributes: filteredProductVariantAttributesIds,
                filteredManufacturers: filteredManufacturersIds,
                filteredVendors: filteredVendorsIds,
                filteredRating: minRating,
                manufacturerId: manufacturerId,
                vendorId: vendorId,
                productTagId: productTagId,
                storeId: storeId,
                languageId: languageId,
                featuredProducts: featuredProducts,
                keywords: searchTerms,
                priceMin: priceRangeFilterModel.minPriceConverted,
                priceMax: priceRangeFilterModel.maxPriceConverted,
                searchProductTags: searchProductTags,
                searchDescriptions : searchProductTags,
                pageIndex: pageIndex,
                pageSize: pageSize,
                orderBy: orderBy,
                onSale: onSale,
                inStock: inStock,
                loadFilterableCategoryIds : enableCategoriesFilter,
                loadFilterableManufacturerIds: enableManufacturersFilter,
                loadFilterableVendorIds: enableVendorsFilter,
                loadFilterableSpecificationAttributeOptionIds: enableSpecificationsFilter,
                loadFilterableProductVariantAttributeIds: enableAttributesFilter,
                loadFilterableProductReviewIds: enableRatingFilter,
                loadFilterableOnSale: enableOnSaleFilter,
                loadFilterableInStock: enableInStockFilter,
                loadAvailableFilters: loadAvailableFilters);

                var productsPagedList = productFilterModel.productsPagedList;
                var filtersModel = (productFilterModel.specificationAttributeFilters, productFilterModel.productAttributeFilters, productFilterModel.categoryFilters,productFilterModel.manufacturerFilters, productFilterModel.vendorFilters,productFilterModel.ratingFilters ,productFilterModel.productsOnSale, productFilterModel.productsInStock);
                var viewMode = (!string.IsNullOrWhiteSpace(catalogPagingFilteringModel.ViewMode)) ? catalogPagingFilteringModel.ViewMode : _catalogSettings.DefaultViewMode;
                var productsModel = new ProductsModel
                {
                    NopAjaxFiltersSettingsModel = _nopAjaxFiltersSettings.ToModel(),
                    Products = _productModelFactory.PrepareProductOverviewModels(productsPagedList, preparePriceModel: true, preparePictureModel: true, null, _nopAjaxFiltersSettings.PrepareSpecificationAttributes),
                    ViewMode = viewMode,
                    HashQuery = _queryStringBuilder.GetQueryString(shouldRebuildQueryString: true),
                    TotalCount = productFilterModel.productsPagedList.TotalCount
                };
                productsModel.PagingFilteringContext.LoadPagedList(productFilterModel.productsPagedList);
                AdjustModelsFilterItemsStateWithSelectedOptionIds(filtersModel, 
                    priceRangeFilterModelSpikes, categoryFilterModelSpikes,
                    specificationFilterModelSpikes,attributeFilterModelSpikes,
                    manufacturerFilterModelSpikes,vendorFilterModelSpikes,ratingFilterModelSpikes,
                    onSaleFilterModelSpikes,inStockFilterModelSpikes);

                AddJavaScriptRequiredInfoToProductModel(productsModel,
                    categoryFilterModelSpikes, specificationFilterModelSpikes, attributeFilterModelSpikes, 
                    manufacturerFilterModelSpikes, vendorFilterModelSpikes,ratingFilterModelSpikes,
                    priceRangeFilterModelSpikes, onSaleFilterModelSpikes, inStockFilterModelSpikes, catalogPagingFilteringModel);
                return productsModel;
            });
            return cacheModel;
        }
        #endregion

        #region FilterModel
        public virtual (SearchModel searchModel, GetFilteredProductsModel filteredProductsModel) PrepareFilteredProductsModel(string queryString = null,
           bool isOnSearchPage = false,
           int categoryId = 0,
           int manufacturerId = 0, int vendorId = 0,
           int productTagId = 0,
           SearchModel searchModel = null,
           CatalogPagingFilteringModel catalogPagingFilteringModel = null)
        {
            var searchTerms = string.Empty;
            if (searchModel != null)
                searchTerms = searchModel.q.Trim();

            var storeId = _storeContext.CurrentStore.Id;
            var languageId = _workContext.WorkingLanguage.Id;
            var currencyId = _workContext.WorkingCurrency.Id;
            var includeProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
            var featuredProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : new bool?(false);
            var cacheKey = string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_MODELS_KEY, storeId, languageId, currencyId, categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts);
            return _cacheManager.Get(cacheKey, delegate
            {
                int? pageIndex = null;
                int? pageSize = null;
                decimal? priceMin = null;
                decimal? priceMax = null;
                IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
                if (isOnSearchPage && catalogPagingFilteringModel != null)
                {
                    pageIndex = catalogPagingFilteringModel.PageNumber > 0 ? catalogPagingFilteringModel.PageNumber - 1 : 0;
                    pageSize = catalogPagingFilteringModel.PageSize > 0 ? catalogPagingFilteringModel.PageSize : _catalogSettings.DefaultCategoryPageSize;
                    _catalogModelFactory.PrepareSortingOptions(searchModel.PagingFilteringContext, catalogPagingFilteringModel);
                    _catalogModelFactory.PrepareViewModes(searchModel.PagingFilteringContext, catalogPagingFilteringModel);
                    _catalogModelFactory.PreparePageSizeOptions(searchModel.PagingFilteringContext, catalogPagingFilteringModel,
                    _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                    _catalogSettings.SearchPagePageSizeOptions,
                    _catalogSettings.SearchPageProductsPerPage);
                }
                var categoryIds = new List<int>();
                if (categoryId > 0)
                {
                        categoryIds.Add(categoryId);
                    if (includeProductsFromSubcategories)
                    {
                        (categoryIds as List<int>).AddRange(_categoryService.GetChildCategoryIds(categoryId, storeId));
                    }
                }
                var searchProductTags =  _nopAjaxFiltersSettings.SearchInProductTags;
                var enableCategoriesFilter = _nopAjaxFiltersSettings.EnableCategoriesFilter;
                var enableRatingFilter = _nopAjaxFiltersSettings.EnableRatingFilter;
                var enableManufacturersFilter = _nopAjaxFiltersSettings.EnableManufacturersFilter;
                var enableVendorsFilter = _nopAjaxFiltersSettings.EnableVendorsFilter;
                var enablePriceRangeFilter = _nopAjaxFiltersSettings.EnablePriceRangeFilter;
                var enableSpecificationsFilter = _nopAjaxFiltersSettings.EnableSpecificationsFilter;
                var enableAttributesFilter = _nopAjaxFiltersSettings.EnableAttributesFilter;
                var enableOnSaleFilter = _nopAjaxFiltersSettings.EnableOnSaleFilter;
                var enableInStockFilter = _nopAjaxFiltersSettings.EnableInStockFilter;

                var filterTypeCacheKeys = new Dictionary<(bool filterSetting, string filterTypeName), string>
                {
                    { (enablePriceRangeFilter, FilterType.PRICE_RANGE), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableRatingFilter, FilterType.RATING), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_RATING_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableCategoriesFilter, FilterType.CATEGORY), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_CATEGORY_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableManufacturersFilter, FilterType.MANUFACTURER), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableVendorsFilter, FilterType.VENDOR), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_VENDOR_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableSpecificationsFilter, FilterType.SPECIFICATION), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableAttributesFilter, FilterType.PRODUCT_ATTRIBUTE), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableOnSaleFilter, FilterType.ONSALE), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_ONSALE_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                    { (enableInStockFilter, FilterType.INSTOCK), string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY, storeId, languageId, currencyId,categoryId, manufacturerId, vendorId, productTagId, isOnSearchPage, searchTerms, includeProductsFromSubcategories, featuredProducts) },
                };
                var filterTypes = filterTypeCacheKeys.Where(p => (p.Key.filterSetting.Equals(true) && !_cacheManager.IsSet(p.Value)))?.Select(p => p.Key.filterTypeName).ToList();
               
                var productFilterModel = _productServiceNopAjaxFilters.SearchProducts(ref priceMin, ref priceMax,
                    loadFilterableOnSale: enableOnSaleFilter, loadFilterableInStock: enableInStockFilter,loadFilterablePriceRange : enablePriceRangeFilter ,loadFilterableSpecificationAttributeOptions: enableSpecificationsFilter,
                    loadFilterableProductVariantAttributes: enableAttributesFilter, loadFilterableCategories: enableCategoriesFilter, loadFilterableManufacturers: enableManufacturersFilter, loadFilterableVendors: enableVendorsFilter,
                    loadFilterableProductTags: false, loadFilterableProductReviews: enableRatingFilter, pageIndex: pageIndex, pageSize: pageSize, categoryIds: categoryIds, manufacturerId: manufacturerId,
                    storeId: storeId, vendorId: vendorId, featuredProducts: featuredProducts, productTagId: productTagId, keywords: searchTerms, searchProductTags: searchProductTags,searchDescriptions: searchProductTags,languageId: languageId, filterTypes: filterTypes);
               
                var filtersModel = (productFilterModel.specificationAttributeFilters, productFilterModel.productAttributeFilters, productFilterModel.categoryFilters, productFilterModel.manufacturerFilters, productFilterModel.vendorFilters, productFilterModel.ratingFilters, productFilterModel.productsOnSale, productFilterModel.productsInStock);
                if (searchModel != null && isOnSearchPage)
                {
                    products = productFilterModel.productsPagedList;
                    searchModel.Products = _productModelFactory.PrepareProductOverviewModels(products).ToList();
                    searchModel.NoResults = !searchModel.Products.Any();
                    searchModel.PagingFilteringContext.LoadPagedList(products);
                }
                PriceRangeFilterModelSpikes priceRangeFilterModelSpikes = null;
                if (enablePriceRangeFilter)
                {
                    var key = filterTypeCacheKeys[(enablePriceRangeFilter, FilterType.PRICE_RANGE)];
                    priceRangeFilterModelSpikes = _cacheManager.Get(key, delegate
                    {
                        return (priceMin.HasValue && priceMax.HasValue) ? 
                        GetPriceRangeFilterInternal(categoryId,manufacturerId,vendorId,productTagId, priceMin.Value, priceMax.Value) : null;
                    });
                }
                CategoryFilterModelSpikes categoryFilterModelSpikes = null;
                if (enableCategoriesFilter)
                {
                    var key = filterTypeCacheKeys[(enableCategoriesFilter, FilterType.CATEGORY)];
                    categoryFilterModelSpikes = _cacheManager.Get(key, delegate
                    {
                        var categoryModel = filtersModel.categoryFilters;
                        return (categoryModel != null) ? new CategoryFilterModelSpikes
                        {
                            CategoryId = categoryId,
                            ManufacturerId = manufacturerId,
                            VendorId = vendorId,
                            ProductTagId = productTagId,
                            CategoryFilterItems = categoryModel
                        } : null;
                    });
                }
                ManufacturerFilterModelSpikes manufacturerFilterModelSpikes = null;
                if (enableManufacturersFilter)
                {
                    var key = filterTypeCacheKeys[(enableManufacturersFilter, FilterType.MANUFACTURER)];
                    manufacturerFilterModelSpikes = _cacheManager.Get(key, delegate
                    {
                        var manufacturerModel = filtersModel.manufacturerFilters;
                        return (manufacturerModel != null ) ? new ManufacturerFilterModelSpikes
                        {
                            CategoryId = categoryId,
                            VendorId = vendorId,
                            ProductTagId = productTagId,
                            ManufacturerFilterItems = manufacturerModel
                        } : null;
                    });
                }
                VendorFilterModelSpikes vendorFilterModelSpikes = null;
                if (enableVendorsFilter)
                {
                    var key = filterTypeCacheKeys[(enableVendorsFilter, FilterType.VENDOR)];
                    vendorFilterModelSpikes = _cacheManager.Get(key, delegate
                    {
                        var vendorModel = filtersModel.vendorFilters;
                        return (vendorModel != null && vendorModel.Any()) ? new VendorFilterModelSpikes
                        {
                            CategoryId = categoryId,
                            ManufacturerId = manufacturerId,
                            ProductTagId = productTagId,
                            VendorFilterItems = vendorModel
                        } : null;
                    });
                }
                SpecificationFilterModelSpikes specificationFilterModelSpikes = null;
                if (enableSpecificationsFilter)
                {
                    var key = filterTypeCacheKeys[(enableSpecificationsFilter, FilterType.SPECIFICATION)];
                    specificationFilterModelSpikes = _cacheManager.Get(key, delegate
                    {
                        var specificationAttributeFilters = filtersModel.specificationAttributeFilters;
                        return (specificationAttributeFilters != null && specificationAttributeFilters.Any()) ? new SpecificationFilterModelSpikes
                        {
                            SpecificationFilterGroups = filtersModel.specificationAttributeFilters,
                            CategoryId = categoryId,
                            ManufacturerId = manufacturerId,
                            VendorId = vendorId,
                            ProductTagId = productTagId
                        } : null;

                    });
                }
                AttributeFilterModelSpikes attributeFilterModelSpikes = null;
                if (enableAttributesFilter)
                {
                    var key = filterTypeCacheKeys[(enableAttributesFilter, FilterType.PRODUCT_ATTRIBUTE)];
                    attributeFilterModelSpikes = _cacheManager.Get(key, delegate
                    {
                        var productAttributeFilters = filtersModel.productAttributeFilters;
                        if (productAttributeFilters != null && productAttributeFilters.Any())
                        {
                            var model = productAttributeFilters.ToList().Select(p => new AttributeFilterGroup
                            {
                                Id = p.Id,
                                Name = p.Name,
                                FilterItems = PrepareFiltersProductAttributeValues(p.FilterItems)
                               
                            });
                            return new AttributeFilterModelSpikes
                            {
                                CategoryId = categoryId,
                                ManufacturerId = manufacturerId,
                                VendorId = vendorId,
                                ProductTagId = productTagId,
                                AttributeFilterGroups = model.ToList()
                            };
                        }
                        return null;           
                      });
                    }

                RatingFilterModelSpikes ratingFilterModelSpikes = null;
                if (enableRatingFilter)
                {
                    var key = filterTypeCacheKeys[(enableRatingFilter, FilterType.RATING)];
                    ratingFilterModelSpikes = _cacheManager.Get(key, delegate
                    {
                        var ratingModel = filtersModel.ratingFilters;
                        return (ratingModel != null && ratingModel.Any()) ? new RatingFilterModelSpikes
                        {
                            CategoryId = categoryId,
                            ManufacturerId = manufacturerId,
                            ProductTagId = productTagId,
                            RatingFilterItems = ratingModel
                        } : null;
                    });
                }

                OnSaleFilterModelSpikes onSaleFilterModelSpikes = null;
                    if (enableOnSaleFilter)
                    {
                        var key = filterTypeCacheKeys[(enableOnSaleFilter, FilterType.ONSALE)];
                        onSaleFilterModelSpikes = _cacheManager.Get(key, delegate
                        {
                            return (productFilterModel.productsOnSale > 0) ? new OnSaleFilterModelSpikes
                            {
                                Id = 1,
                                Name = _localizationService.GetResource("Spikes.NopAjaxFilters.Public.OnSale.Option"),
                                FilterCount = productFilterModel.productsOnSale,
                                CategoryId = categoryId,
                                VendorId = vendorId,
                                ManufacturerId = manufacturerId,
                                ProductTagId = productTagId,
                                FilterItemState = FilterItemState.Unchecked
                            } : null;
                        });
                    }

                    InStockFilterModelSpikes inStockFilterModelSpikes = null;
                    if (enableInStockFilter)
                    {
                        var key = filterTypeCacheKeys[(enableInStockFilter, FilterType.INSTOCK)];
                        inStockFilterModelSpikes = _cacheManager.Get( key, delegate
                        {
                            return (productFilterModel.productsInStock > 0) ? new InStockFilterModelSpikes
                            {
                                Id = 1,
                                Name = _localizationService.GetResource("Spikes.NopAjaxFilters.Public.InStock.Option"),
                                CategoryId = categoryId,
                                ManufacturerId = manufacturerId,
                                VendorId = vendorId,
                                ProductTagId = productTagId,
                                FilterCount = productFilterModel.productsInStock,
                                FilterItemState = FilterItemState.Unchecked
                            } : null;
                        });
                    }

                    return (searchModel, (new GetFilteredProductsModel
                    {
                        PriceRangeFilterModelSpikes = priceRangeFilterModelSpikes,
                        CategoryFiltersModelSpikes = categoryFilterModelSpikes,
                        ManufacturerFiltersModelSpikes = manufacturerFilterModelSpikes,
                        VendorFiltersModelSpikes = vendorFilterModelSpikes,
                        RatingFiltersModelSpikes = ratingFilterModelSpikes,
                        SpecificationFiltersModelSpikes = specificationFilterModelSpikes,
                        AttributeFiltersModelSpikes = attributeFilterModelSpikes,
                        OnSaleFilterModel = onSaleFilterModelSpikes,
                        InStockFilterModel = inStockFilterModelSpikes,
                        CategoryId = categoryId,
                        ManufacturerId = manufacturerId,
                        VendorId = vendorId,
                        ProductTagId = productTagId,
                        IsOnSearchPage = isOnSearchPage,
                        Keyword = searchTerms,
                        IncludeSubcategories = includeProductsFromSubcategories
                    }));
            });
        }
        #endregion

        #region Prepare Model ProductAttribute
        private IList<AttributeFilterItem> PrepareFiltersProductAttributeValues(IEnumerable<AttributeFilterItem> productAttributeValuesLocalized)
        {
       
            var list = new List<AttributeFilterItem>();
            foreach (var productVariantAttributeValue in productAttributeValuesLocalized)
            {     
                var attributeFilterItem = list.FirstOrDefault(p => p.Name.Equals(productVariantAttributeValue.Name));
                if (attributeFilterItem == null)
                {
                    attributeFilterItem = new AttributeFilterItem();
                    if (!string.IsNullOrEmpty(productVariantAttributeValue.ColorSquaresRgb))
                    {
                        attributeFilterItem.ColorSquaresRgb = productVariantAttributeValue.ColorSquaresRgb;
                    }
                    else if (productVariantAttributeValue.ImageSquaresPictureId > 0)
                    {
                        var pictureModel = PrepareImageSquares(productVariantAttributeValue.ImageSquaresPictureId);
                        attributeFilterItem.ImageSquaresUrl = pictureModel?.ImageUrl;
                    }
                    attributeFilterItem.ValueId = productVariantAttributeValue.ValueId;
                    attributeFilterItem.AttributeId = productVariantAttributeValue.AttributeId;
                    attributeFilterItem.Name = productVariantAttributeValue.Name;
                    attributeFilterItem.FilterCount = productVariantAttributeValue.FilterCount;
                    attributeFilterItem.ProductVariantAttributeIds = productVariantAttributeValue.ProductVariantAttributeIds;
                    attributeFilterItem.FilterItemState = FilterItemState.Unchecked;
                    list.Add(attributeFilterItem);
                }            
            }
            return list;
        }
        #endregion

        #region PrepareImageSquare    
        private PictureModel PrepareImageSquares(int imageSquaresPictureId)
        {
            var key = string.Format(NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey, _storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id, imageSquaresPictureId);
            var pictureModel = _cacheManager.Get(key, delegate
            {
                var picture = _pictureService.GetPictureById(imageSquaresPictureId);
                return (picture != null) ? new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(imageSquaresPictureId),
                    ImageUrl = _pictureService.GetPictureUrl(imageSquaresPictureId, _mediaSettings.ImageSquarePictureSize)
                } : null;
            });
            return pictureModel;
        }
        #endregion


        #region PriceRange Calculation
        private PriceRangeFilterModelSpikes GetPriceRangeFilterInternal(int categoryId, int manufacturerId, int vendorId, int productTagId, decimal priceMin, decimal priceMax)
        {
                var minPrice = priceMin;
                var num = priceMax;
                var customFormatting = _workContext.WorkingCurrency.CustomFormatting;
                PriceRangeFilterModelSpikes priceRangeFilterModelSpikes = null;
            if (minPrice != 0m || num != 0m)
            {
                if (num - minPrice < 1m)
                {
                    minPrice = num;
                }
                else
                {
                    minPrice = Math.Floor(minPrice);
                    num = Math.Ceiling(num);
                }
                var currencySymbol = string.Empty;
                if (!string.IsNullOrEmpty(_workContext.WorkingCurrency.DisplayLocale))
                {
                    currencySymbol = System.Globalization.CultureInfo.GetCultureInfo(_workContext.WorkingCurrency.DisplayLocale).NumberFormat.CurrencySymbol;
                }
                var formattedPrice = GetFormattedPrice(minPrice);
                var formattedPrice2 = GetFormattedPrice(num);
                priceRangeFilterModelSpikes = new PriceRangeFilterModelSpikes
                {
                    CategoryId = categoryId,
                    ManufacturerId = manufacturerId,
                    VendorId = vendorId,
                    ProductTagId = productTagId,
                    MinPrice = minPrice,
                    MaxPrice = num,
                    MinPriceFormatted = formattedPrice,
                    MaxPriceFormatted = formattedPrice2,
                    Formatting = customFormatting,
                    CurrencySymbol = currencySymbol
                };
            }
            
            if (priceRangeFilterModelSpikes == null || priceRangeFilterModelSpikes.MinPrice == priceRangeFilterModelSpikes.MaxPrice)
            {
                return new PriceRangeFilterModelSpikes();
            }
            priceRangeFilterModelSpikes.SelectedPriceRange = (PriceRangeHelper.GetSelectedPriceRange() ?? new PriceRange
            {
                From = Math.Floor(priceRangeFilterModelSpikes.MinPrice),
                To = Math.Ceiling(priceRangeFilterModelSpikes.MaxPrice)
            });
            return priceRangeFilterModelSpikes;
      
        }

        private PriceRangeFilterModelSpikes GetPriceRangeFilterInternal(int categoryId, int manufacturerId, int vendorId, int productTagId)
        {
            var key = string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY, categoryId, manufacturerId, vendorId, productTagId,_workContext.CurrentCustomer.Id, _workContext.WorkingCurrency.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id, _workContext.TaxDisplayType);
            var priceRangeFilterDto = _cacheManager.Get(key, () => _priceCalculationServiceNopAjaxFilters.GetPriceRangeFilterDto(categoryId, manufacturerId, vendorId));
            var key2 = string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_MODEL_KEY, categoryId, manufacturerId, vendorId, productTagId, _workContext.CurrentCustomer.Id, _workContext.WorkingCurrency.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id, _workContext.TaxDisplayType);
            var priceRangeFilterModelSpikes = _cacheManager.Get(key2, delegate
            {
                var minPrice = priceRangeFilterDto.MinPrice;
                var num = priceRangeFilterDto.MaxPrice;
                var customFormatting = _workContext.WorkingCurrency.CustomFormatting;
                PriceRangeFilterModelSpikes result = null;
                if (minPrice != 0m || num != 0m)
                {
                    if (num - minPrice < 1m)
                    {
                        minPrice = num;
                    }
                    else
                    {
                        minPrice = Math.Floor(minPrice);
                        num = Math.Ceiling(num);
                    }
                    var currencySymbol = string.Empty;
                    if (!string.IsNullOrEmpty(_workContext.WorkingCurrency.DisplayLocale))
                    {
                        currencySymbol = System.Globalization.CultureInfo.GetCultureInfo(_workContext.WorkingCurrency.DisplayLocale).NumberFormat.CurrencySymbol;
                    }
                    var formattedPrice = GetFormattedPrice(minPrice);
                    var formattedPrice2 = GetFormattedPrice(num);
                    result = new PriceRangeFilterModelSpikes
                    {
                        CategoryId = categoryId,
                        ManufacturerId = manufacturerId,
                        VendorId = vendorId,
                        ProductTagId = productTagId,
                        MinPrice = minPrice,
                        MaxPrice = num,
                        MinPriceFormatted = formattedPrice,
                        MaxPriceFormatted = formattedPrice2,
                        Formatting = customFormatting,
                        CurrencySymbol = currencySymbol
                    };
                }
                return result;
            });
            if (priceRangeFilterModelSpikes == null || priceRangeFilterModelSpikes.MinPrice == priceRangeFilterModelSpikes.MaxPrice)
            {
                return new PriceRangeFilterModelSpikes();
            }
            var priceRange2 = priceRangeFilterModelSpikes.SelectedPriceRange = (PriceRangeHelper.GetSelectedPriceRange() ?? new PriceRange
            {
                From = Math.Floor(priceRangeFilterModelSpikes.MinPrice),
                To = Math.Ceiling(priceRangeFilterModelSpikes.MaxPrice)
            });
            return priceRangeFilterModelSpikes;
        }

        private string GetFormattedPrice(decimal price)
        {
            var text = _priceFormatter.FormatPrice(price, showCurrency: true, showTax: false);
            var trailingZeroesSeparator = _nopAjaxFiltersSettings.TrailingZeroesSeparator;
            if (string.IsNullOrEmpty(trailingZeroesSeparator))
            {
                return text;
            }
            return new Regex("[" + trailingZeroesSeparator + "][0]{2,}", RegexOptions.RightToLeft).Replace(text, "");
        }
        #endregion

        #region common
        private (SpecificationFilterModelDTO specificationFilterModelDTO, AttributeFilterModelDTO attributeFilterModelDTO, CategoryFilterModelDTO categoryFilterModelDTO, ManufacturerFilterModelDTO manufacturerFilterModelDTO, VendorFilterModelDTO vendorFilterModelDTO) SetFilterDTOs(
            SpecificationFilterModelSpikes specificationFilterModelSpikes,
            AttributeFilterModelSpikes attributeFilterModelSpikes,
            CategoryFilterModelSpikes categoryFilterModelSpikes,
            ManufacturerFilterModelSpikes manufacturerFilterModelSpikes,
            VendorFilterModelSpikes vendorFilterModelSpikes)
        {
            SpecificationFilterModelDTO specificationFilterModelDTO = null;
            AttributeFilterModelDTO attributeFilterModelDTO = null;
            CategoryFilterModelDTO categoryFilterModelDTO = null;
            ManufacturerFilterModelDTO manufacturerFilterModelDTO = null;
            VendorFilterModelDTO vendorFilterModelDTO = null;
            if (specificationFilterModelSpikes != null)
            {
                specificationFilterModelDTO = specificationFilterModelSpikes.ToDTO();
                specificationFilterModelDTO.SpecificationFilterDTOs = specificationFilterModelDTO.SpecificationFilterDTOs.Where((SpecificationFilterDTO x) => x.SelectedFilterIds.Count > 0).ToList();
            }
            if (attributeFilterModelSpikes != null)
            {
                attributeFilterModelDTO = attributeFilterModelSpikes.ToDTO();
                attributeFilterModelDTO.AttributeFilterDTOs = attributeFilterModelDTO.AttributeFilterDTOs.Where((AttributeFilterDTO x) => x.SelectedProductVariantIds.Count > 0).ToList();
            }
            if (categoryFilterModelSpikes != null)
            {
                categoryFilterModelDTO = categoryFilterModelSpikes.ToDTO();
            }
            if (manufacturerFilterModelSpikes != null)
            {
                manufacturerFilterModelDTO = manufacturerFilterModelSpikes.ToDTO();
            }
            if (vendorFilterModelSpikes != null)
            {
                vendorFilterModelDTO = vendorFilterModelSpikes.ToDTO();
            }
            return (specificationFilterModelDTO, attributeFilterModelDTO, categoryFilterModelDTO, manufacturerFilterModelDTO, vendorFilterModelDTO);
        }

        private void AddJavaScriptRequiredInfoToProductModel(ProductsModel productsModel, CategoryFilterModelSpikes categoryFilterModelSpikes,
            SpecificationFilterModelSpikes specificationFilterModelSpikes,
            AttributeFilterModelSpikes attributeFilterModelSpikes,
            ManufacturerFilterModelSpikes manufacturerFilterModelSpikes,
            VendorFilterModelSpikes vendorFilterModelSpikes,
            RatingFilterModelSpikes ratingFilterModelSpikes,
            PriceRangeFilterModelSpikes priceRangeFilterModelSpikes,
            OnSaleFilterModelSpikes onSaleFilterModelSpikes,
            InStockFilterModelSpikes inStockFilterModelSpikes, 
            CatalogPagingFilteringModel catalogPagingFilteringModel)
        {
            SerializeAndPopulateFilterModelsToProductModel(productsModel,categoryFilterModelSpikes,specificationFilterModelSpikes,attributeFilterModelSpikes,manufacturerFilterModelSpikes,vendorFilterModelSpikes,ratingFilterModelSpikes,onSaleFilterModelSpikes,inStockFilterModelSpikes);
            SetCurrentFiltersSelectionToProductModel(productsModel,catalogPagingFilteringModel,priceRangeFilterModelSpikes);
        }
        private void SetCurrentFiltersSelectionToProductModel(ProductsModel productsModel,
            CatalogPagingFilteringModel catalogPagingFilteringModel,
            PriceRangeFilterModelSpikes priceRangeFilterModelSpikes)
        {
            var priceRangeFromJson = string.Empty;
            var priceRangeToJson = string.Empty;
            if (priceRangeFilterModelSpikes != null && priceRangeFilterModelSpikes.SelectedPriceRange != null)
            {
                if (priceRangeFilterModelSpikes.SelectedPriceRange.From.HasValue)
                {
                    priceRangeFromJson = JsonConvert.SerializeObject(Convert.ToInt32(priceRangeFilterModelSpikes.SelectedPriceRange.From));
                }
                if (priceRangeFilterModelSpikes.SelectedPriceRange.To.HasValue)
                {
                    priceRangeToJson = JsonConvert.SerializeObject(Convert.ToInt32(priceRangeFilterModelSpikes.SelectedPriceRange.To));
                }
            }
            var currentPageSizeJson = JsonConvert.SerializeObject(catalogPagingFilteringModel.PageSize);
            var currentViewModeJson = JsonConvert.SerializeObject(catalogPagingFilteringModel.ViewMode);
            var currentOrderByJson = JsonConvert.SerializeObject(catalogPagingFilteringModel.OrderBy);
            var currentPageNumberJson = JsonConvert.SerializeObject(catalogPagingFilteringModel.PageNumber);
            productsModel.PriceRangeFromJson = priceRangeFromJson;
            productsModel.PriceRangeToJson = priceRangeToJson;
            productsModel.CurrentPageSizeJson = currentPageSizeJson;
            productsModel.CurrentViewModeJson = currentViewModeJson;
            productsModel.CurrentOrderByJson = currentOrderByJson;
            productsModel.CurrentPageNumberJson = currentPageNumberJson;
        }

        private void SerializeAndPopulateFilterModelsToProductModel(
            ProductsModel productsModel,
            CategoryFilterModelSpikes categoryFilterModelSpikes,
            SpecificationFilterModelSpikes specificationFilterModelSpikes,
            AttributeFilterModelSpikes attributeFilterModelSpikes,
            ManufacturerFilterModelSpikes manufacturerFilterModelSpikes,
            VendorFilterModelSpikes vendorFilterModelSpikes,
            RatingFilterModelSpikes ratingFilterModelSpikes,
            OnSaleFilterModelSpikes onSaleFilterModelSpikes,
            InStockFilterModelSpikes inStockFilterModelSpikes)
        {

            var categoryFilterModelSpikesJson = string.Empty;
            if (categoryFilterModelSpikes != null)
                categoryFilterModelSpikesJson = JsonConvert.SerializeObject(categoryFilterModelSpikes);
            
            var specificationFilterModelSpikesJson = string.Empty;
            if (specificationFilterModelSpikes != null)
                specificationFilterModelSpikesJson = JsonConvert.SerializeObject(specificationFilterModelSpikes);
            
            var attributeFilterModelSpikesJson = string.Empty;
            if (attributeFilterModelSpikes != null)
                attributeFilterModelSpikesJson = JsonConvert.SerializeObject(attributeFilterModelSpikes);

            var manufacturerFilterModelSpikesJson = string.Empty;
            if (manufacturerFilterModelSpikes != null)
                manufacturerFilterModelSpikesJson = JsonConvert.SerializeObject(manufacturerFilterModelSpikes);
            
            var vendorFilterModelSpikesJson = string.Empty;
            if (vendorFilterModelSpikes != null)
                vendorFilterModelSpikesJson = JsonConvert.SerializeObject(vendorFilterModelSpikes);

            var ratingFilterModelSpikesJson = string.Empty;
            if (ratingFilterModelSpikes != null)
                ratingFilterModelSpikesJson = JsonConvert.SerializeObject(ratingFilterModelSpikes);

            var onSaleFilterModelSpikesJson = string.Empty;
            if (onSaleFilterModelSpikes != null)
                onSaleFilterModelSpikesJson = JsonConvert.SerializeObject(onSaleFilterModelSpikes);
            
            var inStockFilterModelSpikesJson = string.Empty;
            if (inStockFilterModelSpikes != null)
                inStockFilterModelSpikesJson = JsonConvert.SerializeObject(inStockFilterModelSpikes);
            
            productsModel.CategoryFilterModelSpikesJson = categoryFilterModelSpikesJson;
            productsModel.SpecificationFilterModelSpikesJson = specificationFilterModelSpikesJson;
            productsModel.AttributeFilterModelSpikesJson = attributeFilterModelSpikesJson;
            productsModel.ManufacturerFilterModelSpikesJson = manufacturerFilterModelSpikesJson;
            productsModel.VendorFilterModelSpikesJson = vendorFilterModelSpikesJson;
            productsModel.RatingFilterModelSpikesJson = ratingFilterModelSpikesJson;
            productsModel.OnSaleFilterModelSpikesJson = onSaleFilterModelSpikesJson;
            productsModel.InStockFilterModelSpikesJson = inStockFilterModelSpikesJson;
        }
        private (decimal minPriceConverted, decimal maxPriceConverted) AdjustMinAndMaxPriceRangeWithDiscountsAndTax(
            int categoryId, int manufacturerId, int vendorId,int productTagId,
            PriceRangeFilterModelSpikes priceRangeFilterModelSpikes)
        {
            var priceConverted = (minPriceConverted: decimal.Zero, maxPriceConverted: decimal.Zero);
            if (priceRangeFilterModelSpikes != null && priceRangeFilterModelSpikes.SelectedPriceRange != null)
            {
                var selectedPriceRange = priceRangeFilterModelSpikes.SelectedPriceRange;
                var key = string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY, categoryId, manufacturerId, vendorId, productTagId, _workContext.CurrentCustomer.Id, _workContext.WorkingCurrency.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id, _workContext.TaxDisplayType);
                var priceRangeFilterDto = _cacheManager.Get<PriceRangeFilterDto>(key, () => null, (int?)0);
                if (priceRangeFilterDto == null)
                    priceRangeFilterDto = _priceCalculationServiceNopAjaxFilters.GetPriceRangeFilterDto(categoryId, manufacturerId, vendorId);
                if (selectedPriceRange.From.HasValue)
                    priceConverted.maxPriceConverted = _priceCalculationServiceNopAjaxFilters.CalculateBasePrice(selectedPriceRange.From.Value, priceRangeFilterDto, isFromPrice: true);
                if (selectedPriceRange.To.HasValue)
                    priceConverted.maxPriceConverted = _priceCalculationServiceNopAjaxFilters.CalculateBasePrice(selectedPriceRange.To.Value, priceRangeFilterDto, isFromPrice: false);
            }
            return priceConverted;
        }
        private void AdjustModelsFilterItemsStateWithSelectedOptionIds(
            (IList<(int Id, int FilterCount)> specificationAttributeFilters, IList<(int Id, int FilterCount)> productAttributeFilters, IList<(int Id, int FilterCount)> categoryFilters, IList<(int Id, int FilterCount)> manufacturerFilters, IList<(int Id, int FilterCount)> vendorFilters, IList<(int Id, int FilterCount)> ratingFilters, int productsOnSale, int productsInStock) filtersModel,
            PriceRangeFilterModelSpikes priceRangeFilterModelSpikes,
            CategoryFilterModelSpikes categoryFilterModelSpikes,
            SpecificationFilterModelSpikes specificationFilterModelSpikes,
            AttributeFilterModelSpikes attributeFilterModelSpikes,
            ManufacturerFilterModelSpikes manufacturerFilterModelSpikes,
            VendorFilterModelSpikes vendorFilterModelSpikes,
            RatingFilterModelSpikes ratingFilterModelSpikes,
            OnSaleFilterModelSpikes onSaleFilterModelSpikes,
            InStockFilterModelSpikes inStockFilterModelSpikes)
        {

            if (priceRangeFilterModelSpikes != null || specificationFilterModelSpikes != null || attributeFilterModelSpikes != null || manufacturerFilterModelSpikes != null || vendorFilterModelSpikes != null || onSaleFilterModelSpikes != null || inStockFilterModelSpikes != null)
            {
                if (specificationFilterModelSpikes != null && filtersModel.specificationAttributeFilters != null)
                    AdjustSpecificationFilterModelSpikesWithSelectedOptionIds(specificationFilterModelSpikes, filtersModel.specificationAttributeFilters);
                if (attributeFilterModelSpikes != null && filtersModel.productAttributeFilters != null)
                    AdjustAttributeFilterModelSpikesWithSelectedProductVariantIds(attributeFilterModelSpikes, filtersModel.productAttributeFilters);
                if (categoryFilterModelSpikes != null && filtersModel.categoryFilters != null)
                    AdjustCategoryFilterModelSpikesWithSelectedOptionIds(categoryFilterModelSpikes, filtersModel.categoryFilters);
                if (manufacturerFilterModelSpikes != null && filtersModel.manufacturerFilters != null)
                    AdjustManufacturerFilterModelSpikesWithSelectedOptionIds(manufacturerFilterModelSpikes, filtersModel.manufacturerFilters);
                if (vendorFilterModelSpikes != null && filtersModel.vendorFilters != null)
                    AdjustVendorFilterModelSpikesWithSelectedOptionIds(vendorFilterModelSpikes, filtersModel.vendorFilters);

                if (ratingFilterModelSpikes != null && filtersModel.ratingFilters != null)
                    AdjustRatingFilterModelSpikesWithSelectedOptionIds(ratingFilterModelSpikes, filtersModel.ratingFilters);
                if (onSaleFilterModelSpikes != null)
                {
                    onSaleFilterModelSpikes.FilterCount = filtersModel.productsOnSale;
                    onSaleFilterModelSpikes.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(onSaleFilterModelSpikes.FilterItemState, filtersModel.productsOnSale > 0);
                }
                if (inStockFilterModelSpikes != null)
                {
                    inStockFilterModelSpikes.FilterCount = filtersModel.productsInStock;
                    inStockFilterModelSpikes.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(inStockFilterModelSpikes.FilterItemState, filtersModel.productsInStock > 0);
                }
            }
        }

        private void AdjustSpecificationFilterModelSpikesWithSelectedOptionIds(SpecificationFilterModelSpikes specificationFilterModelSpikes, IList<(int Id, int FilterCount)> specificationOptionIds)
        {
            
            foreach (var specificationFilterGroup in specificationFilterModelSpikes.SpecificationFilterGroups)
            {
                foreach (var filterItem in specificationFilterGroup.FilterItems)
                {
                    var list = specificationOptionIds.Select(x => x.Id).ToList();
                    if (specificationFilterGroup.IsMain && filterItem.FilterItemState != FilterItemState.Disabled && filterItem.FilterItemState != FilterItemState.CheckedDisabled)
                    {
                        list.Add(filterItem.Id);
                    }
                    var optionAvailable = list.Contains(filterItem.Id);
                    filterItem.FilterCount = specificationOptionIds.FirstOrDefault(x => x.Id.Equals(filterItem.Id)).FilterCount;
                    filterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(filterItem.FilterItemState, optionAvailable);
                    
                }
            }
        }

        private void AdjustAttributeFilterModelSpikesWithSelectedProductVariantIds(AttributeFilterModelSpikes attributeFilterModelSpikes, IList<(int Id, int FilterCount)> productVariantIds)
        {
            foreach (var attributeFilterGroup in attributeFilterModelSpikes.AttributeFilterGroups)
            {
                foreach (var filterItem in attributeFilterGroup.FilterItems)
                {
                    var list = productVariantIds.Select(x=> x.Id).ToList();
                    if (attributeFilterGroup.IsMain && filterItem.FilterItemState != FilterItemState.Disabled && filterItem.FilterItemState != FilterItemState.CheckedDisabled)
                    {
                        //list.AddRange(filterItem.ProductVariantAttributeIds);
                        list.Add(filterItem.ValueId);
                    }
                    //var optionAvailable = filterItem.ProductVariantAttributeIds.Intersect(list).Any();
                    var optionAvailable = list.Contains(filterItem.ValueId);
                    filterItem.FilterCount = productVariantIds.FirstOrDefault(x => x.Id.Equals(filterItem.ValueId)).FilterCount;
                    filterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(filterItem.FilterItemState, optionAvailable);
                }
            }
        }

        private void AdjustCategoryFilterModelSpikesWithSelectedOptionIds(CategoryFilterModelSpikes categoryFilterModelSpikes, IList<(int Id, int FilterCount)> categoryIds)
        {
            foreach (var categoryFilterItem in categoryFilterModelSpikes.CategoryFilterItems)
            {
                var list = categoryIds.Select(x => x.Id).ToList();
                if (categoryFilterModelSpikes.Priority == 1 && categoryFilterItem.FilterItemState != FilterItemState.Disabled && categoryFilterItem.FilterItemState != FilterItemState.CheckedDisabled)
                    list.Add(categoryFilterItem.Id);
                var optionAvailable = list.Contains(categoryFilterItem.Id);
                categoryFilterItem.FilterCount = categoryIds.FirstOrDefault(x => x.Id.Equals(categoryFilterItem.Id)).FilterCount;
                categoryFilterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(categoryFilterItem.FilterItemState, optionAvailable);
            }
        }

        private void AdjustManufacturerFilterModelSpikesWithSelectedOptionIds(ManufacturerFilterModelSpikes manufacturerFilterModelSpikes, IList<(int Id, int FilterCount)> manufacturerIds)
        {
            foreach (var manufacturerFilterItem in manufacturerFilterModelSpikes.ManufacturerFilterItems)
            {
                var list = manufacturerIds.Select(x=>x.Id).ToList();
                if (manufacturerFilterModelSpikes.Priority == 1 && manufacturerFilterItem.FilterItemState != FilterItemState.Disabled && manufacturerFilterItem.FilterItemState != FilterItemState.CheckedDisabled)
                    list.Add(manufacturerFilterItem.Id);
                
                var optionAvailable = list.Contains(manufacturerFilterItem.Id);
                manufacturerFilterItem.FilterCount = manufacturerIds.FirstOrDefault(x => x.Id.Equals(manufacturerFilterItem.Id)).FilterCount;
                manufacturerFilterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(manufacturerFilterItem.FilterItemState, optionAvailable);
            }
        }
        private void AdjustVendorFilterModelSpikesWithSelectedOptionIds(VendorFilterModelSpikes vendorFilterModelSpikes, IList<(int Id, int FilterCount)> vendorIds)
        {
            foreach (var vendorFilterItem in vendorFilterModelSpikes.VendorFilterItems)
            {
                var list = vendorIds.Select(x=>x.Id).ToList();
                if (vendorFilterModelSpikes.Priority == 1 && vendorFilterItem.FilterItemState != FilterItemState.Disabled && vendorFilterItem.FilterItemState != FilterItemState.CheckedDisabled)
                    list.Add(vendorFilterItem.Id);
                
                var optionAvailable = list.Contains(vendorFilterItem.Id);
                vendorFilterItem.FilterCount = vendorIds.FirstOrDefault(x => x.Id.Equals(vendorFilterItem.Id)).FilterCount;
                vendorFilterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(vendorFilterItem.FilterItemState, optionAvailable);
            }
        }
        private void AdjustRatingFilterModelSpikesWithSelectedOptionIds(RatingFilterModelSpikes ratingFilterModelSpikes, IList<(int Id, int FilterCount)> ratingIds)
        {
            foreach (var ratingFilterItem in ratingFilterModelSpikes.RatingFilterItems)
            {
                var list = ratingIds.Select(x => x.Id).ToList();
                if (ratingFilterModelSpikes.Priority == 1 && ratingFilterItem.FilterItemState != FilterItemState.Disabled && ratingFilterItem.FilterItemState != FilterItemState.CheckedDisabled)
                    list.Add(ratingFilterItem.Id);

                var optionAvailable = list.Contains(ratingFilterItem.Id);
                ratingFilterItem.FilterCount = ratingIds.FirstOrDefault(x => x.Id.Equals(ratingFilterItem.Id)).FilterCount;
                ratingFilterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(ratingFilterItem.FilterItemState, optionAvailable);
            }
        }
        #endregion

        #region PageSize, ValidateOrderBy
        private int GetMaxPageSizeOption(string pageSizeOptions)
        {
            string[] array = pageSizeOptions.Split(new char[2]
            {
                ',',
                ' '
            }, StringSplitOptions.RemoveEmptyEntries);
            var num = 0;
            string[] array2 = array;
            for (var i = 0; i < array2.Length; i++)
            {
                if (int.TryParse(array2[i], out int result))
                {
                    num = Math.Max(result, num);
                }
            }
            return num;
        }


        public void ValidateOrderBy(CatalogPagingFilteringModel pagingFilteringModel)
        {
            if (!pagingFilteringModel.OrderBy.HasValue || _catalogSettings.ProductSortingEnumDisabled.Contains(pagingFilteringModel.OrderBy.Value))
            {
                var value = 0;
                if (_catalogSettings.ProductSortingEnumDisabled.Count != Enum.GetValues(typeof(ProductSortingEnum)).Length)
                {
                    value = (from idOption in Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>().Except(_catalogSettings.ProductSortingEnumDisabled)
                             select new KeyValuePair<int, int>(idOption, _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out int value2) ? value2 : idOption) into x
                             orderby x.Value
                             select x).FirstOrDefault().Value;
                }
                pagingFilteringModel.OrderBy = value;
            }
        }

        //private void ValidatePageSize(CatalogPagingFilteringModel pagingFilteringModel, int categoryId, int manufacturerId, int vendorId)
        //{
        //    int num = int.MinValue;
        //    if (categoryId > 0)
        //    {
        //        Category categoryById = _categoryService.GetCategoryById(categoryId);
        //        if (categoryById.AllowCustomersToSelectPageSize)
        //        {
        //            string text = categoryById.PageSizeOptions;
        //            if (string.IsNullOrEmpty(text))
        //            {
        //                text = _catalogSettings.DefaultCategoryPageSizeOptions;
        //            }
        //            num = GetMaxPageSizeOption(text);
        //        }
        //        else
        //        {
        //            num = categoryById.PageSize;
        //        }
        //    }
        //    if (manufacturerId > 0)
        //    {
        //        Manufacturer manufacturerById = _manufacturerService.GetManufacturerById(manufacturerId);
        //        if (manufacturerById.AllowCustomersToSelectPageSize)
        //        {
        //            string text2 = manufacturerById.PageSizeOptions;
        //            if (string.IsNullOrEmpty(text2))
        //            {
        //                text2 = _catalogSettings.DefaultManufacturerPageSizeOptions;
        //            }
        //            num = GetMaxPageSizeOption(text2);
        //        }
        //        else
        //        {
        //            num = manufacturerById.PageSize;
        //        }
        //    }
        //    if (vendorId > 0)
        //    {
        //        Vendor vendorById = _vendorService.GetVendorById(vendorId);
        //        if (vendorById.AllowCustomersToSelectPageSize)
        //        {
        //            string text3 = vendorById.PageSizeOptions;
        //            if (string.IsNullOrEmpty(text3))
        //            {
        //                text3 = _vendorSettings.DefaultVendorPageSizeOptions;
        //            }
        //            num = GetMaxPageSizeOption(text3);
        //        }
        //        else
        //        {
        //            num = vendorById.PageSize;
        //        }
        //    }
        //    if (_isOnSearchPage)
        //    {
        //        num = ((!_catalogSettings.SearchPageAllowCustomersToSelectPageSize) ? _catalogSettings.SearchPageProductsPerPage : GetMaxPageSizeOption(_catalogSettings.SearchPagePageSizeOptions));
        //    }
        //    if (num != int.MinValue && pagingFilteringModel.PageSize > num)
        //    {
        //        pagingFilteringModel.PageSize = num;
        //        pagingFilteringModel.PageNumber = 1;
        //    }
        //}
        #endregion


      
    }
}
