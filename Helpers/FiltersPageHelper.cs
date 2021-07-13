using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;

namespace Spikes.Nop.Plugins.AjaxFilters.Helpers
{
    public class FiltersPageHelper : IFiltersPageHelper
	{
		private readonly CatalogSettings _catalogSettings;
		private readonly ISearchQueryStringHelper _searchQueryStringHelper;
		private ActionContext _requestContext;
		private FiltersPageParameters _filtersPageParameters;

		public FiltersPageHelper( CatalogSettings catalogSettings, ISearchQueryStringHelper searchQueryStringHelper)
		{
			_catalogSettings = catalogSettings;
			_searchQueryStringHelper = searchQueryStringHelper;
		}

		public void Initialize(ActionContext requestContext, string query)
		{
			Initialize(requestContext);
			_filtersPageParameters.SearchQueryStringParameters = _searchQueryStringHelper.GetQueryStringParameters(query);
		}

		public void Initialize(ActionContext requestContext)
		{
			_requestContext = requestContext;
			_filtersPageParameters = SetRequestParametersFromRouteData();
		}

        public void Initialize(FiltersPageParameters filtersPageParameters)
        {
            _filtersPageParameters = filtersPageParameters;
        }

		public FiltersPageParameters GetFiltersPageParameters()
		{
			return _filtersPageParameters;
		}

		private FiltersPageParameters SetRequestParametersFromRouteData()
		{
           
			var filtersPageParameters = new FiltersPageParameters();
			if (_requestContext.RouteData.Values.Keys.Contains("categoryid"))
			{
				filtersPageParameters.CategoryId = int.Parse(_requestContext.RouteData.Values["categoryid"].ToString());
			}
			if (_requestContext.RouteData.Values.Keys.Contains("manufacturerid"))
			{
				filtersPageParameters.ManufacturerId = int.Parse(_requestContext.RouteData.Values["manufacturerid"].ToString());
			}
			if (_requestContext.RouteData.Values.Keys.Contains("vendorid"))
			{
				filtersPageParameters.VendorId = int.Parse(_requestContext.RouteData.Values["vendorid"].ToString());
			}
            if (_requestContext.RouteData.Values.Keys.Contains("productTagId"))
            {
                filtersPageParameters.ProductTagId = int.Parse(_requestContext.RouteData.Values["productTagId"].ToString());
            }
            if (_requestContext.RouteData.Values.Keys.Contains("orderBy"))
			{
				filtersPageParameters.OrderBy = int.Parse(_requestContext.RouteData.Values["orderBy"].ToString());
			}
			if (_requestContext.RouteData.Values.Keys.Contains("viewMode"))
			{
				filtersPageParameters.ViewMode = _requestContext.RouteData.Values["viewMode"].ToString();
			}
			if (_requestContext.RouteData.Values.Keys.Contains("pageSize"))
			{
				filtersPageParameters.PageSize = int.Parse(_requestContext.RouteData.Values["pageSize"].ToString());
			}
			return filtersPageParameters;
		}

		public bool ValidateParameters(FiltersPageParameters filtersPageParameters)
		{
            bool result;
            if (filtersPageParameters.CategoryId == 0 && filtersPageParameters.ManufacturerId == 0 && filtersPageParameters.VendorId == 0 && filtersPageParameters.ProductTagId == 0 && !filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage)
				result = false;
			else
                result = true;
            return result;
		}

		public string[] GetPageSizes()
		{
			return _catalogSettings.SearchPagePageSizeOptions.Split(new char[2]
			{
				',',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
		}

		public int GetDefaultPageSize()
		{
			var result = 0;
			string[] array = null;
			if (_filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage && _catalogSettings.SearchPageAllowCustomersToSelectPageSize && _catalogSettings.SearchPagePageSizeOptions != null)
			{
				array = _catalogSettings.SearchPagePageSizeOptions.Split(new char[2]
				{
					',',
					' '
				}, StringSplitOptions.RemoveEmptyEntries);
			}
			else
			{
				result = _catalogSettings.SearchPageProductsPerPage;
			}
			if (array != null && array.Any() && int.TryParse(array.FirstOrDefault(), out int result2) && result2 > 0)
			{
				result = result2;
			}
			return result;
		}

		public int GetDefaultOrderBy()
		{
			return 0;
		}

		public int GetDefaultPageNumber()
		{
			return 1;
		}

		public string GetDefaultViewMode()
		{
			return _catalogSettings.DefaultViewMode;
		}

		public void AdjustPagingFilteringModelPageSizeAndPageNumber(CatalogPagingFilteringModel catalogPagingFilteringModel)
		{
			var pageSizes = GetPageSizes();
            if (pageSizes.Any() && (catalogPagingFilteringModel.PageSize <= 0 || !pageSizes.Contains(catalogPagingFilteringModel.PageSize.ToString(CultureInfo.InvariantCulture))) && int.TryParse(pageSizes.FirstOrDefault(), out int result) && result > 0)
            {
                catalogPagingFilteringModel.PageSize = result;
            }
            else
                catalogPagingFilteringModel.PageSize = _catalogSettings.SearchPageProductsPerPage;
		}

		public string GetTemplateViewPath()
		{
			var result = string.Empty;
			if (_filtersPageParameters.CategoryId > 0)
                return "CategoryTemplate.ProductsInGridOrLines";
			if (_filtersPageParameters.ManufacturerId > 0)
                return "ManufacturerTemplate.ProductsInGridOrLines";
			if (_filtersPageParameters.VendorId > 0)
				return "VendorTemplate.ProductsInGridOrLines";
            if (_filtersPageParameters.ProductTagId > 0)
                return "ProductTagTemplate.ProductsInGridOrLines";
            if (_filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage)
				return "SearchTemplate.ProductsInGridOrLines";
            return result;
		}
	}
}
