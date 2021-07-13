using System;
using System.Linq;
using System.Text;
using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using Spikes.Nop.Plugins.AjaxFilters.Helpers;
using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.QueryStringManipulation
{
    public class QueryStringBuilder : IQueryStringBuilder
	{
		private readonly IFiltersPageHelper _filtersPageHelper;

		private SpecificationFilterModelSpikes _specificationFilterModelSpikes;

		private AttributeFilterModelSpikes _attributeFilterModelSpikes;

		private ManufacturerFilterModelSpikes _manufacturerFilterModelSpikes;

        private CategoryFilterModelSpikes _categoryFilterModelSpikes;

        private RatingFilterModelSpikes _ratingFilterModelSpikes;

        private VendorFilterModelSpikes _vendorFilterModelSpikes;

		private OnSaleFilterModelSpikes _onSaleFilterModelSpikes;

		private InStockFilterModelSpikes _inStockFilterModelSpikes;

		private PriceRangeFilterModelSpikes _priceRangeFilterModelSpikes;

		private CatalogPagingFilteringModel _catalogPagingFilteringModel;

		public string SpecificationsQueryString
		{
			get;
			set;
		}

		public string PagingQueryString
		{
			get;
			set;
		}

		public string PriceRangeQueryString
		{
			get;
			set;
		}

		public string ManufacturersQueryString
		{
			get;
			set;
		}

        public string CategoriesQueryString
        {
            get;
            set;
        }

        public string RatingQueryString
        {
            get;
            set;
        }

        public string VendorsQueryString
		{
			get;
			set;
		}

		public string OnSaleQueryString
		{
			get;
			set;
		}

		public string InStockQueryString
		{
			get;
			set;
		}

		public string AttributesQueryString
		{
			get;
			set;
		}

		public QueryStringBuilder(IFiltersPageHelper filtersPageHelper)
		{
			_filtersPageHelper = filtersPageHelper;
		}

		public string GetQueryString(bool shouldRebuildQueryString)
		{
			if (shouldRebuildQueryString)
			{
				BuildQueryString();
			}
			return GetQueryStringInternal();
		}

		public void SetDataForQueryString(SpecificationFilterModelSpikes specificationFilterModelSpikes, AttributeFilterModelSpikes attributeFilterModelSpikes, CategoryFilterModelSpikes categoryFilterModelSpikes, ManufacturerFilterModelSpikes manufacturerFilterModelSpikes, VendorFilterModelSpikes vendorFilterModelSpikes, RatingFilterModelSpikes ratingFilterModelSpikes, PriceRangeFilterModelSpikes priceRangeFilterModelSpikes, CatalogPagingFilteringModel catalogPagingFilteringModel, OnSaleFilterModelSpikes onSaleFilterModel, InStockFilterModelSpikes inStockFilterModelSpikes)
		{
			_catalogPagingFilteringModel = catalogPagingFilteringModel;
			_priceRangeFilterModelSpikes = priceRangeFilterModelSpikes;
			_manufacturerFilterModelSpikes = manufacturerFilterModelSpikes;
            _categoryFilterModelSpikes = categoryFilterModelSpikes;
            _ratingFilterModelSpikes = ratingFilterModelSpikes;
            _vendorFilterModelSpikes = vendorFilterModelSpikes;
			_onSaleFilterModelSpikes = onSaleFilterModel;
			_inStockFilterModelSpikes = inStockFilterModelSpikes;
			_attributeFilterModelSpikes = attributeFilterModelSpikes;
			_specificationFilterModelSpikes = specificationFilterModelSpikes;
		}

		private void BuildQueryString()
		{
			BuildSpecificationsQueryString(_specificationFilterModelSpikes);
			BuildAttributesQueryString(_attributeFilterModelSpikes);
            BuildCategoryQueryString(_categoryFilterModelSpikes);
            BuildRatingQueryString(_ratingFilterModelSpikes);
            BuildManufacturerQueryString(_manufacturerFilterModelSpikes);
			BuildVendorQueryString(_vendorFilterModelSpikes);
			BuildOnSaleQueryString(_onSaleFilterModelSpikes);
			BuildInStockQueryString(_inStockFilterModelSpikes);
			BuildPriceRangeQueryString(_priceRangeFilterModelSpikes);
			BuildPagingFilterQueryString(_catalogPagingFilteringModel);
		}

		private void BuildInStockQueryString(InStockFilterModelSpikes inStockFilterModelSpikes)
		{
			InStockQueryString = string.Empty;
			if (inStockFilterModelSpikes != null && (inStockFilterModelSpikes.FilterItemState == FilterItemState.Checked || inStockFilterModelSpikes.FilterItemState == FilterItemState.CheckedDisabled))
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append("isFilters=");
				stringBuilder.Append(inStockFilterModelSpikes.Id);
				stringBuilder.Append("!##!");
				TrimEndDelimiter(stringBuilder, "!##!");
				InStockQueryString = stringBuilder.ToString();
			}
		}

		private void BuildAttributesQueryString(AttributeFilterModelSpikes attributeFilterModelSpikes)
		{
			AttributesQueryString = string.Empty;
			if (attributeFilterModelSpikes != null && attributeFilterModelSpikes.AttributeFilterGroups.SelectMany((AttributeFilterGroup fi) => fi.FilterItems).Any((AttributeFilterItem fis) => fis.FilterItemState == FilterItemState.Checked || fis.FilterItemState == FilterItemState.CheckedDisabled))
			{
				var stringBuilder = new StringBuilder();
				if (attributeFilterModelSpikes.AttributeFilterGroups.Count > 0)
				{
					stringBuilder.Append("attrFilters=");
				}
				foreach (var attributeFilterGroup in attributeFilterModelSpikes.AttributeFilterGroups)
				{
					if (stringBuilder.ToString() != "attrFilters=")
					{
						stringBuilder.Append("!-#!");
					}
					var stringBuilder2 = new StringBuilder();
					foreach (var item in attributeFilterGroup.FilterItems.Where((AttributeFilterItem attrbuteItem) => attrbuteItem.FilterItemState == FilterItemState.Checked || attrbuteItem.FilterItemState == FilterItemState.CheckedDisabled))
					{
						stringBuilder2.Append(item.ValueId);
						stringBuilder2.Append("!##!");
					}
					TrimEndDelimiter(stringBuilder2, "!##!");
					if (attributeFilterGroup.IsMain && stringBuilder2.Length == 0)
					{
						if (stringBuilder.ToString() != "attrFilters=")
						{
							stringBuilder.Append("!-#!");
						}
						stringBuilder.Append(attributeFilterGroup.Id);
						stringBuilder.Append("m");
					}
					if (stringBuilder2.Length > 0)
					{
						stringBuilder.Append(attributeFilterGroup.Id);
						if (attributeFilterGroup.IsMain)
						{
							stringBuilder.Append("m");
						}
						stringBuilder.Append("!#-!");
						stringBuilder.Append(stringBuilder2);
					}
				}
				if (stringBuilder.ToString() != "attrFilters=")
				{
					AttributesQueryString = stringBuilder.ToString();
				}
			}
		}

		private void BuildSpecificationsQueryString(SpecificationFilterModelSpikes specificationFilterModelSpikes)
		{
			SpecificationsQueryString = string.Empty;
			if (specificationFilterModelSpikes != null && specificationFilterModelSpikes.SpecificationFilterGroups.SelectMany((SpecificationFilterGroup fi) => fi.FilterItems).Any((SpecificationFilterItem fis) => fis.FilterItemState == FilterItemState.Checked || fis.FilterItemState == FilterItemState.CheckedDisabled))
			{
				var stringBuilder = new StringBuilder();
				if (specificationFilterModelSpikes.SpecificationFilterGroups.Count > 0)
				{
					stringBuilder.Append("specFilters=");
				}
				foreach (var specificationFilterGroup in specificationFilterModelSpikes.SpecificationFilterGroups)
				{
					var stringBuilder2 = new StringBuilder();
					foreach (var item in specificationFilterGroup.FilterItems.Where((SpecificationFilterItem specificationItem) => specificationItem.FilterItemState == FilterItemState.Checked || specificationItem.FilterItemState == FilterItemState.CheckedDisabled))
					{
						stringBuilder2.Append(item.Id);
						stringBuilder2.Append("!##!");
					}
					TrimEndDelimiter(stringBuilder2, "!##!");
					if (specificationFilterGroup.IsMain && stringBuilder2.Length == 0)
					{
						if (stringBuilder.ToString() != "specFilters=")
						{
							stringBuilder.Append("!-#!");
						}
						stringBuilder.Append(specificationFilterGroup.Id);
						stringBuilder.Append("m");
					}
					if (stringBuilder2.Length > 0)
					{
						if (stringBuilder.ToString() != "specFilters=")
						{
							stringBuilder.Append("!-#!");
						}
						stringBuilder.Append(specificationFilterGroup.Id);
						if (specificationFilterGroup.IsMain)
						{
							stringBuilder.Append("m");
						}
						stringBuilder.Append("!#-!");
						stringBuilder.Append(stringBuilder2);
					}
				}
				if (stringBuilder.ToString() != "specFilters=")
				{
					SpecificationsQueryString = stringBuilder.ToString();
				}
			}
		}

		private void BuildPagingFilterQueryString(CatalogPagingFilteringModel pagingFilteringModel)
		{
			PagingQueryString = string.Empty;
			if (pagingFilteringModel == null)
			{
				return;
			}
			var stringBuilder = new StringBuilder();
			if (pagingFilteringModel.PageSize != _filtersPageHelper.GetDefaultPageSize() || pagingFilteringModel.ViewMode != _filtersPageHelper.GetDefaultViewMode() || pagingFilteringModel.OrderBy != _filtersPageHelper.GetDefaultOrderBy() || pagingFilteringModel.PageNumber != _filtersPageHelper.GetDefaultPageNumber())
			{
				stringBuilder.Append("pageSize=");
				stringBuilder.Append(pagingFilteringModel.PageSize);
				if (!string.IsNullOrEmpty(pagingFilteringModel.ViewMode))
				{
					AppendSeparator(stringBuilder);
					stringBuilder.Append("viewMode=");
					stringBuilder.Append(pagingFilteringModel.ViewMode);
				}
				AppendSeparator(stringBuilder);
				stringBuilder.Append("orderBy=");
				stringBuilder.Append(pagingFilteringModel.OrderBy);
				AppendSeparator(stringBuilder);
				stringBuilder.Append("pageNumber=");
				stringBuilder.Append(pagingFilteringModel.PageNumber);
			}
			PagingQueryString = stringBuilder.ToString();
		}

		private void BuildPriceRangeQueryString(PriceRangeFilterModelSpikes priceRangeFilterModelSpikes)
		{
			PriceRangeQueryString = string.Empty;
			if (priceRangeFilterModelSpikes == null || priceRangeFilterModelSpikes.SelectedPriceRange == null)
			{
				return;
			}
			var stringBuilder = new StringBuilder();
			var hasValue = priceRangeFilterModelSpikes.SelectedPriceRange.From.HasValue;
			var hasValue2 = priceRangeFilterModelSpikes.SelectedPriceRange.To.HasValue;
			if (!hasValue && !hasValue2)
			{
				return;
			}
			stringBuilder.Append("prFilter=");
			if (hasValue)
			{
				stringBuilder.Append("From-");
				stringBuilder.Append(Convert.ToInt32(priceRangeFilterModelSpikes.SelectedPriceRange.From));
			}
			if (hasValue2)
			{
				if (hasValue)
				{
					stringBuilder.Append("!-#!");
				}
				stringBuilder.Append("To-");
				stringBuilder.Append(Convert.ToInt32(priceRangeFilterModelSpikes.SelectedPriceRange.To));
			}
			if (stringBuilder.ToString() != "prFilter=")
			{
				PriceRangeQueryString = stringBuilder.ToString();
			}
		}

		private void BuildManufacturerQueryString(ManufacturerFilterModelSpikes manufacturerFilterModelSpikes)
		{
			ManufacturersQueryString = string.Empty;
			if (manufacturerFilterModelSpikes != null && manufacturerFilterModelSpikes.ManufacturerFilterItems.Count > 0 && manufacturerFilterModelSpikes.ManufacturerFilterItems.Any((ManufacturerFilterItem m) => m.FilterItemState == FilterItemState.Checked || m.FilterItemState == FilterItemState.CheckedDisabled))
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append("manFilters=");
				foreach (var manufacturerFilterItem in manufacturerFilterModelSpikes.ManufacturerFilterItems)
				{
					if (manufacturerFilterItem.FilterItemState == FilterItemState.Checked || manufacturerFilterItem.FilterItemState == FilterItemState.CheckedDisabled)
					{
						stringBuilder.Append(manufacturerFilterItem.Id);
						stringBuilder.Append(",");
					}
				}
				TrimEndDelimiter(stringBuilder, ",");
				ManufacturersQueryString = stringBuilder.ToString();
			}
		}

        private void BuildCategoryQueryString(CategoryFilterModelSpikes categoryFilterModelSpikes)
        {
            CategoriesQueryString = string.Empty;
            if (categoryFilterModelSpikes != null && categoryFilterModelSpikes.CategoryFilterItems.Count > 0 && categoryFilterModelSpikes.CategoryFilterItems.Any((CategoryFilterItem m) => m.FilterItemState == FilterItemState.Checked || m.FilterItemState == FilterItemState.CheckedDisabled))
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("catFilters=");
                foreach (var categoryFilterItem in categoryFilterModelSpikes.CategoryFilterItems)
                {
                    if (categoryFilterItem.FilterItemState == FilterItemState.Checked || categoryFilterItem.FilterItemState == FilterItemState.CheckedDisabled)
                    {
                        stringBuilder.Append(categoryFilterItem.Id);
                        stringBuilder.Append(",");
                    }
                }
                TrimEndDelimiter(stringBuilder, ",");
                CategoriesQueryString = stringBuilder.ToString();
            }
        }

        private void BuildRatingQueryString(RatingFilterModelSpikes ratingFilterModelSpikes)
        {
            RatingQueryString = string.Empty;
            if (ratingFilterModelSpikes != null && ratingFilterModelSpikes.RatingFilterItems.Count > 0 && ratingFilterModelSpikes.RatingFilterItems.Any((RatingFilterItem m) => m.FilterItemState == FilterItemState.Checked || m.FilterItemState == FilterItemState.CheckedDisabled))
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("ratFilters=");
                foreach (var ratingFilterItem in ratingFilterModelSpikes.RatingFilterItems)
                {
                    if (ratingFilterItem.FilterItemState == FilterItemState.Checked || ratingFilterItem.FilterItemState == FilterItemState.CheckedDisabled)
                    {
                        stringBuilder.Append(ratingFilterItem.Id);
                        stringBuilder.Append(",");
                    }
                }
                TrimEndDelimiter(stringBuilder, ",");
                RatingQueryString = stringBuilder.ToString();
            }
        }

        private void BuildOnSaleQueryString(OnSaleFilterModelSpikes onSaleFilterModelSpikes)
		{
			OnSaleQueryString = string.Empty;
			if (onSaleFilterModelSpikes != null && (onSaleFilterModelSpikes.FilterItemState == FilterItemState.Checked || onSaleFilterModelSpikes.FilterItemState == FilterItemState.CheckedDisabled))
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append("osFilters=");
				if (onSaleFilterModelSpikes.FilterItemState == FilterItemState.Checked || onSaleFilterModelSpikes.FilterItemState == FilterItemState.CheckedDisabled)
				{
					stringBuilder.Append(onSaleFilterModelSpikes.Id);
					stringBuilder.Append(",");
				}
				TrimEndDelimiter(stringBuilder, ",");
				OnSaleQueryString = stringBuilder.ToString();
			}
		}

		private void BuildVendorQueryString(VendorFilterModelSpikes vendorFilterModelSpikes)
		{
			VendorsQueryString = string.Empty;
			if (vendorFilterModelSpikes != null && vendorFilterModelSpikes.VendorFilterItems.Count > 0 && vendorFilterModelSpikes.VendorFilterItems.Any((VendorFilterItem v) => v.FilterItemState == FilterItemState.Checked || v.FilterItemState == FilterItemState.CheckedDisabled))
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append("venFilters=");
				foreach (var vendorFilterItem in vendorFilterModelSpikes.VendorFilterItems)
				{
					if (vendorFilterItem.FilterItemState == FilterItemState.Checked || vendorFilterItem.FilterItemState == FilterItemState.CheckedDisabled)
					{
						stringBuilder.Append(vendorFilterItem.Id);
						stringBuilder.Append("!##!");
					}
				}
				TrimEndDelimiter(stringBuilder, "!##!");
				VendorsQueryString = stringBuilder.ToString();
			}
		}

		private static void AppendSeparator(StringBuilder sb)
		{
			if (sb.Length > 0)
			{
				sb.Append("&");
			}
		}

		private static void AppendQueryString(string queryString, StringBuilder sb)
		{
			if (!string.IsNullOrEmpty(queryString))
			{
				AppendSeparator(sb);
				sb.Append(queryString);
			}
		}

		private static void TrimEndDelimiter(StringBuilder itemSb, string delimeterToBeTrimmed)
		{
			if (itemSb.Length > 0)
			{
				var length = delimeterToBeTrimmed.Length;
				itemSb.Remove(itemSb.Length - length, length);
			}
		}

		private string GetQueryStringInternal()
		{
			var stringBuilder = new StringBuilder();
			AppendQueryString(SpecificationsQueryString, stringBuilder);
			AppendQueryString(AttributesQueryString, stringBuilder);
            AppendQueryString(CategoriesQueryString, stringBuilder);
            AppendQueryString(RatingQueryString, stringBuilder);
            AppendQueryString(ManufacturersQueryString, stringBuilder);
			AppendQueryString(VendorsQueryString, stringBuilder);
			AppendQueryString(OnSaleQueryString, stringBuilder);
			AppendQueryString(InStockQueryString, stringBuilder);
			AppendQueryString(PriceRangeQueryString, stringBuilder);
			AppendQueryString(PagingQueryString, stringBuilder);
			return stringBuilder.ToString();
		}
	}
}
