using System;
using System.Linq;
using System.Text.RegularExpressions;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using Spikes.Nop.Plugins.AjaxFilters.Helpers;
using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.QueryStringManipulation
{
    public class QueryStringToModelUpdater : IQueryStringToModelUpdater
	{
		private readonly IFiltersPageHelper _filtersPageHelper;

		public QueryStringToModelUpdater(IFiltersPageHelper filtersPageHelper)
		{
			_filtersPageHelper = filtersPageHelper;
		}

		public void UpdateModelsFromQueryString(string queryString, SpecificationFilterModelSpikes specificationFiltersModelSpikes, AttributeFilterModelSpikes attributeFilterModelSpikes, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes, VendorFilterModelSpikes vendorFilterModelSpikes, CategoryFilterModelSpikes categoryFilterModelSpikes, RatingFilterModelSpikes ratingFilterModelSpikes, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes, CatalogPagingFilteringModel pagingFilteringModel, OnSaleFilterModelSpikes onSaleFilterModel, InStockFilterModelSpikes inStockFilterModelSpikes)
		{
			UpdateModelsFromQueryStringInternal(queryString, specificationFiltersModelSpikes, attributeFilterModelSpikes, manufacturerFilterModelSpikes, vendorFilterModelSpikes, categoryFilterModelSpikes, ratingFilterModelSpikes,priceRangeFilterModelSpikes, pagingFilteringModel, onSaleFilterModel, inStockFilterModelSpikes);
		}

		public void UpdateOnSaleModel(string queryStringParameter, OnSaleFilterModelSpikes onSaleFilterModelSpikes)
		{
			UpdateOnSaleModelInternal(queryStringParameter, onSaleFilterModelSpikes);
		}

		public void UpdateInStockModel(string queryStringParameter, InStockFilterModelSpikes inStockFilterModelSpikes)
		{
			UpdateInStockModelInternal(queryStringParameter, inStockFilterModelSpikes);
		}

		public void UpdateSpecificationModel(string queryStringParameter, SpecificationFilterModelSpikes specificationFiltersModelSpikes)
		{
			UpdateSpecificationModelInternal(queryStringParameter, specificationFiltersModelSpikes);
		}

		public void UpdateAttributesFilterModel(string queryStringParameter, AttributeFilterModelSpikes attributeFilterModelSpikes)
		{
			UpdateAttributesFilterModelInternal(queryStringParameter, attributeFilterModelSpikes);
		}

		public void UpdateManufacturerFilterModel(string queryStringParameter, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes)
		{
			UpdateManufacturerFilterModelInternal(queryStringParameter, manufacturerFilterModelSpikes);
		}
        public void UpdateCategoryFilterModel(string queryStringParameter, CategoryFilterModelSpikes categoryFilterModelSpikes)
        {
            UpdateCategoryFilterModelInternal(queryStringParameter, categoryFilterModelSpikes);
        }
        public void UpdateRatingFilterModel(string queryStringParameter, RatingFilterModelSpikes ratingFilterModelSpikes)
        {
            UpdateRatingFilterModelInternal(queryStringParameter, ratingFilterModelSpikes);
        }

        public void UpdateVendorFilterModel(string queryStringParameter, VendorFilterModelSpikes vendorFilterModelSpikes)
		{
			UpdateVendorFilterModelInternal(queryStringParameter, vendorFilterModelSpikes);
		}

		public void UpdatePriceRangeModel(string queryStringParameter, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes)
		{
			UpdatePriceRangeModelInternal(queryStringParameter, priceRangeFilterModelSpikes);
		}

		public void UpdatePagingFilterModelWithPageSize(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			UpdatePagingFilterModelWithPageSizeInternal(queryStringParameter, pagingFilteringModel);
		}

		public void UpdatePagingFilterModelWithViewMode(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			UpdatePagingFilterModelWithViewModeInternal(queryStringParameter, pagingFilteringModel);
		}

		public void UpdatePagingFilterModelWithOrderBy(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			UpdatePagingFilterModelWithOrderByInternal(queryStringParameter, pagingFilteringModel);
		}

		public void UpdatePagingFilterModelWithPageNumber(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			UpdatePagingFilterModelWithPageNumberInternal(queryStringParameter, pagingFilteringModel);
		}

		private static int SetSelectedPriceRange(string priceRangeWithPrefix, string prefix)
		{
			if (!string.IsNullOrEmpty(priceRangeWithPrefix))
			{
				string[] array = Regex.Split(priceRangeWithPrefix, prefix);
				if (!string.IsNullOrEmpty(array[1]) && int.TryParse(array[1], out int result) && result > 0)
				{
					return result;
				}
			}
			return 0;
		}

		private QueryParameterTypes GetParameterType(string queryParameter)
		{
			if (queryParameter.IndexOf("specFilters=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.SpecificationFilter;
			}
			if (queryParameter.IndexOf("attrFilters=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.AttributeFilter;
			}
			if (queryParameter.IndexOf("manFilters=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.ManufacturerFilter;
			}
            if (queryParameter.IndexOf("catFilters=", StringComparison.Ordinal) >= 0)
            {
                return QueryParameterTypes.CategoryFilter;
            }
            if (queryParameter.IndexOf("ratFilters=", StringComparison.Ordinal) >= 0)
            {
                return QueryParameterTypes.RatingFilter;
            }
            if (queryParameter.IndexOf("venFilters=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.VendorFilter;
			}
			if (queryParameter.IndexOf("prFilter=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.PriceRangeFilter;
			}
			if (queryParameter.IndexOf("orderBy=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.OrderBy;
			}
			if (queryParameter.IndexOf("pageSize=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.PageSize;
			}
			if (queryParameter.IndexOf("pageNumber=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.PageNumber;
			}
			if (queryParameter.IndexOf("viewMode=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.ViewMode;
			}
			if (queryParameter.IndexOf("osFilters=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.OnSaleFilter;
			}
			if (queryParameter.IndexOf("isFilters=", StringComparison.Ordinal) >= 0)
			{
				return QueryParameterTypes.InStockFilter;
			}
			return QueryParameterTypes.UnknownType;
		}

		private static void ClearOnSaleFilterModel(OnSaleFilterModelSpikes onSaleFilterModel)
		{
			if (onSaleFilterModel != null)
			{
				onSaleFilterModel.Priority = 0;
				onSaleFilterModel.FilterItemState = FilterItemState.Unchecked;
			}
		}

		private static void ClearSpecificationFilterModel(SpecificationFilterModelSpikes specificationFilterModelSpikes)
		{
			if (specificationFilterModelSpikes != null)
			{
				specificationFilterModelSpikes.Priority = 0;
				foreach (var specificationFilterGroup in specificationFilterModelSpikes.SpecificationFilterGroups)
				{
					specificationFilterGroup.IsMain = false;
					foreach (var filterItem in specificationFilterGroup.FilterItems)
					{
						filterItem.FilterItemState = FilterItemState.Unchecked;
					}
				}
			}
		}

		private static void ClearAttributeFilterModel(AttributeFilterModelSpikes attributeFilterModelSpikes)
		{
			if (attributeFilterModelSpikes != null)
			{
				attributeFilterModelSpikes.Priority = 0;
				foreach (var attributeFilterGroup in attributeFilterModelSpikes.AttributeFilterGroups)
				{
					attributeFilterGroup.IsMain = false;
					foreach (var filterItem in attributeFilterGroup.FilterItems)
					{
						filterItem.FilterItemState = FilterItemState.Unchecked;
					}
				}
			}
		}

		private static void ClearManufacturerFilterModel(ManufacturerFilterModelSpikes manufacturerFilterModelSpikes)
		{
			if (manufacturerFilterModelSpikes != null)
			{
				manufacturerFilterModelSpikes.Priority = 0;
				foreach (var manufacturerFilterItem in manufacturerFilterModelSpikes.ManufacturerFilterItems)
				{
					manufacturerFilterItem.FilterItemState = FilterItemState.Unchecked;
				}
			}
		}

        private static void ClearCategoryFilterModel(CategoryFilterModelSpikes categoryFilterModelSpikes)
        {
            if (categoryFilterModelSpikes != null)
            {
                categoryFilterModelSpikes.Priority = 0;
                foreach (var categoryFilterItem in categoryFilterModelSpikes.CategoryFilterItems)
                {
                    categoryFilterItem.FilterItemState = FilterItemState.Unchecked;
                }
            }
        }
        private static void ClearRatingFilterModel(RatingFilterModelSpikes ratingFilterModelSpikes)
        {
            if (ratingFilterModelSpikes != null)
            {
                ratingFilterModelSpikes.Priority = 0;
                foreach (var ratingFilterItem in ratingFilterModelSpikes.RatingFilterItems)
                {
                    ratingFilterItem.FilterItemState = FilterItemState.Unchecked;
                }
            }
        }

        private static void ClearVendorFilterModel(VendorFilterModelSpikes vendorFilterModelSpikes)
		{
			if (vendorFilterModelSpikes != null)
			{
				vendorFilterModelSpikes.Priority = 0;
				foreach (var vendorFilterItem in vendorFilterModelSpikes.VendorFilterItems)
				{
					vendorFilterItem.FilterItemState = FilterItemState.Unchecked;
				}
			}
		}

		private static void ClearPriceRangeFilterModel(PriceRangeFilterModelSpikes priceRangeFilterModelSpikes)
		{
			if (priceRangeFilterModelSpikes != null && priceRangeFilterModelSpikes.SelectedPriceRange != null)
			{
				priceRangeFilterModelSpikes.Priority = 0;
				priceRangeFilterModelSpikes.SelectedPriceRange.From = null;
				priceRangeFilterModelSpikes.SelectedPriceRange.To = null;
			}
		}

		private void SetPagingFilteringModelToDefaultValues(CatalogPagingFilteringModel pagingFilteringModel)
		{
			pagingFilteringModel.PageSize = _filtersPageHelper.GetDefaultPageSize();
			pagingFilteringModel.ViewMode = _filtersPageHelper.GetDefaultViewMode();
			pagingFilteringModel.OrderBy = _filtersPageHelper.GetDefaultOrderBy();
		}

		private void UpdateModelsFromQueryStringInternal(string queryString, SpecificationFilterModelSpikes specificationFiltersModelSpikes, AttributeFilterModelSpikes attributeFilterModelSpikes, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes, VendorFilterModelSpikes vendorFilterModelSpikes, CategoryFilterModelSpikes categoryFilterModelSpikes, RatingFilterModelSpikes ratingFilterModelSpikes, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes, CatalogPagingFilteringModel pagingFilteringModel, OnSaleFilterModelSpikes onSaleFilterModel, InStockFilterModelSpikes inStockFilterModelSpikes)
		{
			if (string.IsNullOrEmpty(queryString))
			{
				return;
			}
			ClearSpecificationFilterModel(specificationFiltersModelSpikes);
			ClearAttributeFilterModel(attributeFilterModelSpikes);
			ClearManufacturerFilterModel(manufacturerFilterModelSpikes);
            ClearCategoryFilterModel(categoryFilterModelSpikes);
            ClearRatingFilterModel(ratingFilterModelSpikes);
            ClearVendorFilterModel(vendorFilterModelSpikes);
			ClearPriceRangeFilterModel(priceRangeFilterModelSpikes);
			ClearOnSaleFilterModel(onSaleFilterModel);
			ClearInStockFilterModel(inStockFilterModelSpikes);
			SetPagingFilteringModelToDefaultValues(pagingFilteringModel);
			string[] array = Regex.Split(queryString, "&");
			foreach (var text in array)
			{
				switch (GetParameterType(text))
				{
				case QueryParameterTypes.SpecificationFilter:
					UpdateSpecificationModel(text, specificationFiltersModelSpikes);
					break;
				case QueryParameterTypes.OnSaleFilter:
					UpdateOnSaleModel(text, onSaleFilterModel);
					break;
				case QueryParameterTypes.InStockFilter:
					UpdateInStockModel(text, inStockFilterModelSpikes);
					break;
				case QueryParameterTypes.AttributeFilter:
					UpdateAttributesFilterModel(text, attributeFilterModelSpikes);
					break;
				case QueryParameterTypes.ManufacturerFilter:
					UpdateManufacturerFilterModel(text, manufacturerFilterModelSpikes);
					break;
                case QueryParameterTypes.CategoryFilter:
                    UpdateCategoryFilterModel(text, categoryFilterModelSpikes);
                    break;
                case QueryParameterTypes.RatingFilter:
                    UpdateRatingFilterModel(text, ratingFilterModelSpikes);
                    break;
                    case QueryParameterTypes.VendorFilter:
					UpdateVendorFilterModel(text, vendorFilterModelSpikes);
					break;
				case QueryParameterTypes.PriceRangeFilter:
					UpdatePriceRangeModel(text, priceRangeFilterModelSpikes);
					break;
				case QueryParameterTypes.PageSize:
					UpdatePagingFilterModelWithPageSize(text, pagingFilteringModel);
					break;
				case QueryParameterTypes.ViewMode:
					UpdatePagingFilterModelWithViewMode(text, pagingFilteringModel);
					break;
				case QueryParameterTypes.OrderBy:
					UpdatePagingFilterModelWithOrderBy(text, pagingFilteringModel);
					break;
				case QueryParameterTypes.PageNumber:
					UpdatePagingFilterModelWithPageNumber(text, pagingFilteringModel);
					break;
				}
			}
		}

		private void ClearInStockFilterModel(InStockFilterModelSpikes inStockFilterModelSpikes)
		{
			if (inStockFilterModelSpikes != null)
			{
				inStockFilterModelSpikes.Priority = 0;
				inStockFilterModelSpikes.FilterItemState = FilterItemState.Unchecked;
			}
		}

		private void UpdateOnSaleModelInternal(string queryStringParameter, OnSaleFilterModelSpikes onSaleFilterModel)
		{
			if (!string.IsNullOrEmpty(queryStringParameter) && onSaleFilterModel != null && Regex.Split(queryStringParameter, "osFilters=").Length >= 2)
			{
				onSaleFilterModel.Priority = 1;
				onSaleFilterModel.FilterItemState = FilterItemState.Checked;
			}
		}

		private void UpdateSpecificationModelInternal(string queryStringParameter, SpecificationFilterModelSpikes specificationFiltersModelSpikes)
		{
			if (string.IsNullOrEmpty(queryStringParameter) || specificationFiltersModelSpikes == null)
			{
				return;
			}
			string[] array = Regex.Split(queryStringParameter, "specFilters=");
			if (array.Length < 2)
			{
				return;
			}
			string[] array2 = Regex.Split(array[1], "!-#!");
			specificationFiltersModelSpikes.Priority = 1;
			string[] array3 = array2;
			foreach (string input in array3)
			{
				string[] array4 = Regex.Split(input, "!#-!");
				if (array4.Length > 2)
				{
					continue;
				}
				string text = array4[0];
				string s = text;
				bool isMain = false;
				if (text.IndexOf("m", StringComparison.Ordinal) >= 0)
				{
					s = Regex.Split(text, "m")[0];
					isMain = true;
				}
				if (!int.TryParse(s, out int groupId) || groupId <= 0)
				{
					continue;
				}
				SpecificationFilterGroup specificationFilterGroup = specificationFiltersModelSpikes.SpecificationFilterGroups.FirstOrDefault((SpecificationFilterGroup x) => x.Id == groupId);
				if (specificationFilterGroup == null)
				{
					continue;
				}
				specificationFilterGroup.IsMain = isMain;
				if (array4.Length < 2)
				{
					continue;
				}
				string[] array5 = Regex.Split(array4[1], "!##!");
				foreach (string s2 in array5)
				{
					if (int.TryParse(s2, out int itemId) && itemId > 0)
					{
						SpecificationFilterItem specificationFilterItem = specificationFilterGroup.FilterItems.FirstOrDefault((SpecificationFilterItem x) => x.Id == itemId);
						if (specificationFilterItem != null)
						{
							specificationFilterItem.FilterItemState = FilterItemState.Checked;
						}
					}
				}
			}
		}

		private void UpdateAttributesFilterModelInternal(string queryStringParameter, AttributeFilterModelSpikes attributeFilterModelSpikes)
		{
			if (string.IsNullOrEmpty(queryStringParameter) || attributeFilterModelSpikes == null)
			{
				return;
			}
			string[] array = Regex.Split(queryStringParameter, "attrFilters=");
			if (array.Length < 2)
			{
				return;
			}
			string[] array2 = Regex.Split(array[1], "!-#!");
			attributeFilterModelSpikes.Priority = 1;
			string[] array3 = array2;
			foreach (string input in array3)
			{
				string[] array4 = Regex.Split(input, "!#-!");
				if (array4.Length > 2)
				{
					continue;
				}
				string text = array4[0];
				string s = text;
				bool isMain = false;
				if (text.IndexOf("m", StringComparison.Ordinal) >= 0)
				{
					s = Regex.Split(text, "m")[0];
					isMain = true;
				}
				if (!int.TryParse(s, out int groupId) || groupId <= 0)
				{
					continue;
				}
				AttributeFilterGroup attributeFilterGroup = attributeFilterModelSpikes.AttributeFilterGroups.FirstOrDefault((AttributeFilterGroup a) => a.Id == groupId);
				if (attributeFilterGroup == null)
				{
					continue;
				}
				attributeFilterGroup.IsMain = isMain;
				if (array4.Length < 2)
				{
					continue;
				}
				string[] array5 = Regex.Split(array4[1], "!##!");
				foreach (string attributeFilterItem in array5)
				{
					if (!string.IsNullOrEmpty(attributeFilterItem))
					{
						AttributeFilterItem attributeFilterItem2 = attributeFilterGroup.FilterItems.FirstOrDefault((AttributeFilterItem a) => a.ValueId.ToString() == attributeFilterItem);
						if (attributeFilterItem2 != null)
						{
							attributeFilterItem2.FilterItemState = FilterItemState.Checked;
						}
					}
				}
			}
		}

		private void UpdateManufacturerFilterModelInternal(string queryStringParameter, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes)
		{
			if (string.IsNullOrEmpty(queryStringParameter) || manufacturerFilterModelSpikes == null)
			{
				return;
			}
			string[] array = Regex.Split(queryStringParameter, "manFilters=");
			if (array.Length < 2)
			{
				return;
			}
			string[] array2 = Regex.Split(array[1], ",");
			manufacturerFilterModelSpikes.Priority = 1;
			string[] array3 = array2;
			foreach (string s in array3)
			{
				if (int.TryParse(s, out int manufacturer) && manufacturer > 0)
				{
					ManufacturerFilterItem manufacturerFilterItem = manufacturerFilterModelSpikes.ManufacturerFilterItems.FirstOrDefault((ManufacturerFilterItem m) => m.Id == manufacturer);
					if (manufacturerFilterItem != null)
					{
						manufacturerFilterItem.FilterItemState = FilterItemState.Checked;
					}
				}
			}
		}


        private void UpdateCategoryFilterModelInternal(string queryStringParameter, CategoryFilterModelSpikes categoryFilterModelSpikes)
        {
            if (string.IsNullOrEmpty(queryStringParameter) || categoryFilterModelSpikes == null)
            {
                return;
            }
            string[] array = Regex.Split(queryStringParameter, "catFilters=");
            if (array.Length < 2)
            {
                return;
            }
            string[] array2 = Regex.Split(array[1], ",");
            categoryFilterModelSpikes.Priority = 1;
            string[] array3 = array2;
            foreach (string s in array3)
            {
                if (int.TryParse(s, out int category) && category > 0)
                {
                    var categoryFilterItem = categoryFilterModelSpikes.CategoryFilterItems.FirstOrDefault((CategoryFilterItem m) => m.Id == category);
                    if (categoryFilterItem != null)
                    {
                        categoryFilterItem.FilterItemState = FilterItemState.Checked;
                    }
                }
            }
        }


        private void UpdateRatingFilterModelInternal(string queryStringParameter, RatingFilterModelSpikes ratingFilterModelSpikes)
        {
            if (string.IsNullOrEmpty(queryStringParameter) || ratingFilterModelSpikes == null)
            {
                return;
            }
            string[] array = Regex.Split(queryStringParameter, "ratFilters=");
            if (array.Length < 2)
            {
                return;
            }
            string[] array2 = Regex.Split(array[1], ",");
            ratingFilterModelSpikes.Priority = 1;
            string[] array3 = array2;
            foreach (string s in array3)
            {
                if (int.TryParse(s, out int rating) && rating > 0)
                {
                    var ratingFilterItem = ratingFilterModelSpikes.RatingFilterItems.FirstOrDefault((RatingFilterItem m) => m.Id == rating);
                    if (ratingFilterItem != null)
                    {
                        ratingFilterItem.FilterItemState = FilterItemState.Checked;
                    }
                }
            }
        }

        private void UpdateVendorFilterModelInternal(string queryStringParameter, VendorFilterModelSpikes vendorFilterModelSpikes)
		{
			if (string.IsNullOrEmpty(queryStringParameter) || vendorFilterModelSpikes == null)
			{
				return;
			}
			string[] array = Regex.Split(queryStringParameter, "venFilters=");
			if (array.Length < 2)
			{
				return;
			}
			string[] array2 = Regex.Split(array[1], "!##!");
			vendorFilterModelSpikes.Priority = 1;
			string[] array3 = array2;
			foreach (string s in array3)
			{
				if (int.TryParse(s, out int vendor) && vendor > 0)
				{
					VendorFilterItem vendorFilterItem = vendorFilterModelSpikes.VendorFilterItems.FirstOrDefault((VendorFilterItem m) => m.Id == vendor);
					if (vendorFilterItem != null)
					{
						vendorFilterItem.FilterItemState = FilterItemState.Checked;
					}
				}
			}
		}

		private void UpdatePriceRangeModelInternal(string queryStringParameter, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes)
		{
			if (string.IsNullOrEmpty(queryStringParameter) || priceRangeFilterModelSpikes == null)
			{
				return;
			}
			string[] array = Regex.Split(queryStringParameter, "prFilter=");
			if (string.IsNullOrEmpty(array[1]))
			{
				return;
			}
			string[] array2 = Regex.Split(array[1], "!-#!");
			string priceRangeWithPrefix = string.Empty;
			string priceRangeWithPrefix2 = string.Empty;
			if (array2.Length < 2)
			{
				string text = array2[0];
				if (text.IndexOf("From-", StringComparison.Ordinal) > -1)
				{
					priceRangeWithPrefix = text;
				}
				if (text.IndexOf("To-", StringComparison.Ordinal) > -1)
				{
					priceRangeWithPrefix2 = text;
				}
			}
			else
			{
				priceRangeWithPrefix = array2[0];
				priceRangeWithPrefix2 = array2[1];
			}
			var num = SetSelectedPriceRange(priceRangeWithPrefix, "From-");
			var num2 = SetSelectedPriceRange(priceRangeWithPrefix2, "To-");
			priceRangeFilterModelSpikes.Priority = 1;
			if (priceRangeFilterModelSpikes.SelectedPriceRange == null && (num > 0 || num2 > 0))
			{
				priceRangeFilterModelSpikes.SelectedPriceRange = new PriceRange();
			}
			if (num > 0 && priceRangeFilterModelSpikes.SelectedPriceRange != null)
			{
				priceRangeFilterModelSpikes.SelectedPriceRange.From = num;
			}
			if (num2 > 0 && priceRangeFilterModelSpikes.SelectedPriceRange != null)
			{
				priceRangeFilterModelSpikes.SelectedPriceRange.To = num2;
			}
		}

		private void UpdatePagingFilterModelWithPageSizeInternal(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			if (!string.IsNullOrEmpty(queryStringParameter) && pagingFilteringModel != null && int.TryParse(Regex.Split(queryStringParameter, "pageSize=")[1], out int result) && result > 0)
			{
				pagingFilteringModel.PageSize = result;
			}
		}

		private void UpdatePagingFilterModelWithViewModeInternal(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			if (!string.IsNullOrEmpty(queryStringParameter) && pagingFilteringModel != null)
			{
				string[] array = Regex.Split(queryStringParameter, "viewMode=");
				if (array.Length > 1)
				{
					pagingFilteringModel.ViewMode = array[1];
				}
			}
		}

		private void UpdatePagingFilterModelWithOrderByInternal(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			if (!string.IsNullOrEmpty(queryStringParameter) && pagingFilteringModel != null && int.TryParse(Regex.Split(queryStringParameter, "orderBy=")[1], out int result) && result > -1)
			{
				pagingFilteringModel.OrderBy = result;
			}
		}

		private void UpdatePagingFilterModelWithPageNumberInternal(string queryStringParameter, CatalogPagingFilteringModel pagingFilteringModel)
		{
			if (!string.IsNullOrEmpty(queryStringParameter) && pagingFilteringModel != null && int.TryParse(Regex.Split(queryStringParameter, "pageNumber=")[1], out int result) && result > -1)
			{
				pagingFilteringModel.PageNumber = result;
			}
		}

		private void UpdateInStockModelInternal(string queryStringParameter, InStockFilterModelSpikes inStockFilterModelSpikes)
		{
			if (!string.IsNullOrEmpty(queryStringParameter) && inStockFilterModelSpikes != null && Regex.Split(queryStringParameter, "isFilters=").Length >= 2)
			{
				inStockFilterModelSpikes.Priority = 1;
				inStockFilterModelSpikes.FilterItemState = FilterItemState.Checked;
			}
		}
	}
}
