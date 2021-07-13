using System.Net;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Web.Controllers;
using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Factories;
using Spikes.Nop.Plugins.AjaxFilters.Helpers;
using Spikes.Nop.Plugins.AjaxFilters.Models;
using Spikes.Nop.Plugins.AjaxFilters.QueryStringManipulation;

namespace Spikes.Nop.Plugins.AjaxFilters.Controllers
{
    public class CatalogSpikesController : BasePublicController
	{
		private readonly IFiltersPageHelper _filtersPageHelper;
        private readonly INopAjaxFilterModelFactory _nopAjaxFilterModelFactory;
        public CatalogSpikesController(INopAjaxFilterModelFactory nopAjaxFilterModelFactory, IFiltersPageHelper filtersPageHelper)
		{
            _nopAjaxFilterModelFactory = nopAjaxFilterModelFactory;
            _filtersPageHelper = filtersPageHelper;
        }

		[HttpGet]
		public ActionResult GetFilteredProducts()
		{
			return new EmptyResult();
		}

		[HttpPost]
		public ActionResult GetFilteredProducts([FromBody] GetFilteredProductsModel model)
		{
			if (model == null)
			{
				return new EmptyResult();
			}
			
            if (model.CategoryId > 0 && !RouteData.Values.ContainsKey("categoryid"))
			{
				RouteData.Values.Add("categoryid", model.CategoryId.ToString());
			}
			if (model.ManufacturerId > 0 && !RouteData.Values.ContainsKey("manufacturerid"))
			{
				RouteData.Values.Add("manufacturerid", model.ManufacturerId.ToString());
			}
			if (model.VendorId > 0 && !RouteData.Values.ContainsKey("vendorid"))
			{
				RouteData.Values.Add("vendorid", model.VendorId.ToString());
			}
            if (model.ProductTagId > 0 && !RouteData.Values.ContainsKey("productTagId"))
            {
                RouteData.Values.Add("productTagId", model.ProductTagId.ToString());
            }
            var catalogPagingFilteringModel = CreateAndPopulatePagingModel(model);
            var filteredProductsInternal = GetFilteredProductsInternal(model, catalogPagingFilteringModel, out var templateViewPath);
            return PartialView(templateViewPath, filteredProductsInternal);
		}

        private CatalogPagingFilteringModel CreateAndPopulatePagingModel(GetFilteredProductsModel model)
        {
            return new CatalogPagingFilteringModel
            {
                OrderBy = model.Orderby,
                PageNumber = (model.PageNumber ?? 0),
                PageSize = model.Pagesize,
                ViewMode = model.Viewmode
            };
        }
        private ProductsModel GetFilteredProductsInternal(GetFilteredProductsModel model, CatalogPagingFilteringModel catalogPagingFilteringModel, out string templateViewPath)
		{
            var queryString = model.QueryString;
            var specificationFilterModelSpikes = model.SpecificationFiltersModelSpikes;
            var attributeFilterModelSpikes = model.AttributeFiltersModelSpikes;
            var categoryFiltersModelSpikes = model.CategoryFiltersModelSpikes;
            var manufacturerFiltersModelSpikes = model.ManufacturerFiltersModelSpikes;
            var vendorFiltersModelSpikes = model.VendorFiltersModelSpikes;
            var priceRangeFilterModelSpikes = model.PriceRangeFilterModelSpikes;
            var ratingFiltersModelSpikes = model.RatingFiltersModelSpikes;
            var onSaleFilterModelSpikes = model.OnSaleFilterModel;
            var inStockFilterModelSpikes = model.InStockFilterModel;
            var categoryId = model.CategoryId;
            var manufacturerId = model.ManufacturerId;
            var vendorId = model.VendorId;
            var productTagId = model.ProductTagId;
            var isOnSearchPage = model.IsOnSearchPage;
            var searchTerms = model.Keyword.Trim();
            var searchDescriptions = model.SearchInProductDescriptions;
            if (!model.ShouldNotStartFromFirstPage)
                catalogPagingFilteringModel.PageNumber = 1;
            var filtersPageParameters = new FiltersPageParameters
			{
				CategoryId = categoryId,
				ManufacturerId = manufacturerId,
				VendorId = vendorId,
                ProductTagId = productTagId,
				SearchQueryStringParameters = new SearchQueryStringParameters
				{
					IsOnSearchPage = isOnSearchPage,
					Keyword = searchTerms,
					AdvancedSearch = model.AdvancedSearch,
					IncludeSubcategories = model.IncludeSubcategories,
					PriceFrom = model.PriceFrom,
					PriceTo = model.PriceTo,
					SearchCategoryId = model.SearchCategoryId,
					SearchManufacturerId = model.SearchManufacturerId,
					SearchInProductDescriptions = model.SearchInProductDescriptions,
					SearchVendorId = model.SearchVendorId
				},
				OrderBy = catalogPagingFilteringModel.OrderBy,
				PageSize = catalogPagingFilteringModel.PageSize,
				PageNumber = catalogPagingFilteringModel.PageNumber,
				ViewMode = catalogPagingFilteringModel.ViewMode
			};
			_filtersPageHelper.Initialize(filtersPageParameters);
			_filtersPageHelper.AdjustPagingFilteringModelPageSizeAndPageNumber(catalogPagingFilteringModel);
            queryString = queryString.TrimStart('#', '/');
			var value = WebUtility.UrlEncode("#");
            if (!queryString.Contains("!#-!") && queryString.Contains(value))
                queryString = WebUtility.UrlDecode(queryString);
   
            templateViewPath = _filtersPageHelper.GetTemplateViewPath();
            return _nopAjaxFilterModelFactory.PrepareSearchModel(specificationFilterModelSpikes: specificationFilterModelSpikes,
                attributeFilterModelSpikes: attributeFilterModelSpikes, categoryFilterModelSpikes: categoryFiltersModelSpikes,manufacturerFilterModelSpikes: manufacturerFiltersModelSpikes,
                vendorFilterModelSpikes: vendorFiltersModelSpikes, 
                ratingFilterModelSpikes: ratingFiltersModelSpikes,
                priceRangeFilterModelSpikes: priceRangeFilterModelSpikes,
                onSaleFilterModelSpikes, inStockFilterModelSpikes: inStockFilterModelSpikes,
                catalogPagingFilteringModel: catalogPagingFilteringModel, queryString: queryString, 
                isOnSearchPage: isOnSearchPage, categoryId: categoryId, manufacturerId: manufacturerId,
                vendorId: vendorId, productTagId: productTagId, searchTerms: searchTerms, searchDescriptions: searchDescriptions);
		}

        public virtual IActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            if (model == null)
                model = new SearchModel();
            var (searchModel, filteredProductsModel) = _nopAjaxFilterModelFactory.PrepareFilteredProductsModel(isOnSearchPage: true, searchModel: model,catalogPagingFilteringModel: command);
            ViewData["SearchFilterModel.FilteredProductsModel"] = filteredProductsModel;
            return View(searchModel);
        }

    }
}
