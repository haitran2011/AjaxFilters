using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.Factories
{
    public interface INopAjaxFilterModelFactory
    {
        ProductsModel PrepareSearchModel(
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
            bool searchDescriptions = false);
        (SearchModel searchModel, GetFilteredProductsModel filteredProductsModel) PrepareFilteredProductsModel(string queryString = null,
           bool isOnSearchPage = false,
           int categoryId = 0,
           int manufacturerId = 0, int vendorId = 0,
           int productTagId = 0,
           SearchModel searchModel = null,
           CatalogPagingFilteringModel catalogPagingFilteringModel = null);
    }
}
