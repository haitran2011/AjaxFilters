using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.QueryStringManipulation
{
    public interface IQueryStringToModelUpdater
	{
		void UpdateModelsFromQueryString(string queryString, SpecificationFilterModelSpikes specificationFiltersModelSpikes, AttributeFilterModelSpikes attributeFilterModelSpikes, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes, VendorFilterModelSpikes vendorFilterModelSpikes, CategoryFilterModelSpikes categoryFilterModelSpikes, RatingFilterModelSpikes ratingFilterModelSpikes, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes, CatalogPagingFilteringModel pagingFilteringModel, OnSaleFilterModelSpikes onSaleFilterModel, InStockFilterModelSpikes inStockFilterModelSpikes);

		void UpdateOnSaleModel(string queryStringParameter, OnSaleFilterModelSpikes onSaleFilterModel);

		void UpdateSpecificationModel(string queryStringParameter, SpecificationFilterModelSpikes specificationFilterModelSpikes);

		void UpdatePagingFilterModelWithPageSize(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel);

		void UpdatePagingFilterModelWithViewMode(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel);

		void UpdatePagingFilterModelWithOrderBy(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel);

		void UpdatePagingFilterModelWithPageNumber(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel);

		void UpdatePriceRangeModel(string queryStringParameter, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes);

		void UpdateManufacturerFilterModel(string queryStringParameter, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes);

        void UpdateCategoryFilterModel(string queryStringParameter, CategoryFilterModelSpikes categoryFilterModelSpikes);

        void UpdateRatingFilterModel(string queryStringParameter, RatingFilterModelSpikes ratingFilterModelSpikes);

        void UpdateVendorFilterModel(string queryStringParameter, VendorFilterModelSpikes vendorFilterModelSpikes);

		void UpdateAttributesFilterModel(string queryStringParameter, AttributeFilterModelSpikes attributeFilterModelSpikes);

		void UpdateInStockModel(string queryStringParameter, InStockFilterModelSpikes instockFilterModel);
	}
}
