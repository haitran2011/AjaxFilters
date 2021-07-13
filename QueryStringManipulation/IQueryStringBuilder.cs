﻿using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.QueryStringManipulation
{
    public interface IQueryStringBuilder
	{
		void SetDataForQueryString(SpecificationFilterModelSpikes specificationFilterModelSpikes, AttributeFilterModelSpikes attributeFilterModelSpikes, CategoryFilterModelSpikes categoryFilterModelSpikes, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes, VendorFilterModelSpikes vendorFilterModelSpikes, RatingFilterModelSpikes ratingFilterModelSpikes, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes, CatalogPagingFilteringModel catalogPagingFilteringModel, OnSaleFilterModelSpikes onSaleFilterModel, InStockFilterModelSpikes inStockFilterModel);

		string GetQueryString(bool shouldRebuildQueryString);
	}
}
