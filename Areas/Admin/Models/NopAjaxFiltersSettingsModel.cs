using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models
{
	public class NopAjaxFiltersSettingsModel
	{

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableAjaxFilters")]
		public bool EnableAjaxFilters
		{
			get;
			set;
		}

		public bool EnableAjaxFilters_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.FiltersUIMode")]
		public FiltersUIMode FiltersUIMode
		{
			get;
			set;
		}

		public bool FiltersUIMode_OverrideForStore
		{
			get;
			set;
		}

		public SelectList AvailableFiltersUIModes
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableInfiniteScroll")]
		public bool EnableInfiniteScroll
		{
			get;
			set;
		}

		public bool EnableInfiniteScroll_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ScrollToElementOnThePageAfterFiltration")]
		public bool ScrollToElementOnThePageAfterFiltration
		{
			get;
			set;
		}

		public bool ScrollToElementOnThePageAfterFiltration_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ScrollToElementOnThePageAfterFiltrationMobile")]
		public bool ScrollToElementOnThePageAfterFiltrationMobile
		{
			get;
			set;
		}

		public bool ScrollToElementOnThePageAfterFiltrationMobile_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ElementToScrollAfterFiltrationSelector")]
		public string ElementToScrollAfterFiltrationSelector
		{
			get;
			set;
		}

		public bool ElementToScrollAfterFiltrationSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.WidgetZone")]
		public string WidgetZone
		{
			get;
			set;
		}

		public bool WidgetZone_OverrideForStore
		{
			get;
			set;
		}

		public SelectList SupportedWidgetZones
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnablePriceRangeFilter")]
		public bool EnablePriceRangeFilter
		{
			get;
			set;
		}

		public bool EnablePriceRangeFilter_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ClosePriceRangeFilterBox")]
		public bool ClosePriceRangeFilterBox
		{
			get;
			set;
		}

		public bool ClosePriceRangeFilterBox_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableSpecificationsFilter")]
		public bool EnableSpecificationsFilter
		{
			get;
			set;
		}

		public bool EnableSpecificationsFilter_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CloseSpecificationsFilterBox")]
		public bool CloseSpecificationsFilterBox
		{
			get;
			set;
		}

		public bool CloseSpecificationsFilterBox_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.NumberOfSpecificationFilters")]
		public int NumberOfSpecificationFilters
		{
			get;
			set;
		}

		public bool NumberOfSpecificationFilters_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableAttributesFilter")]
		public bool EnableAttributesFilter
		{
			get;
			set;
		}

		public bool EnableAttributesFilter_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.NumberOfAttributeFilters")]
		public int NumberOfAttributeFilters
		{
			get;
			set;
		}

		public bool NumberOfAttributeFilters_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CloseAttributesFilterBox")]
		public bool CloseAttributesFilterBox
		{
			get;
			set;
		}

		public bool CloseAttributesFilterBox_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableManufacturersFilter")]
		public bool EnableManufacturersFilter
		{
			get;
			set;
		}

		public bool EnableManufacturersFilter_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CloseManufacturersFilterBox")]
		public bool CloseManufacturersFilterBox
		{
			get;
			set;
		}

		public bool CloseManufacturersFilterBox_OverrideForStore
		{
			get;
			set;
		}

        [NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableCategoriesFilter")]
        public bool EnableCategoriesFilter
        {
            get;
            set;
        }

        public bool EnableCategoriesFilter_OverrideForStore
        {
            get;
            set;
        }


        [NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableRatingFilter")]
        public bool EnableRatingFilter
        {
            get;
            set;
        }

        public bool EnableRatingFilter_OverrideForStore
        {
            get;
            set;
        }

        [NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableOnSaleFilter")]
		public bool EnableOnSaleFilter
		{
			get;
			set;
		}

		public bool EnableOnSaleFilter_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CloseOnSaleFilterBox")]
		public bool CloseOnSaleFilterBox
		{
			get;
			set;
		}

		public bool CloseOnSaleFilterBox_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableVendorsFilter")]
		public bool EnableVendorsFilter
		{
			get;
			set;
		}

		public bool EnableVendorsFilter_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CloseVendorsFilterBox")]
		public bool CloseVendorsFilterBox
		{
			get;
			set;
		}

		public bool CloseVendorsFilterBox_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.EnableInStockFilter")]
		public bool EnableInStockFilter
		{
			get;
			set;
		}

		public bool EnableInStockFilter_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CloseInStockFilterBox")]
		public bool CloseInStockFilterBox
		{
			get;
			set;
		}

		public bool CloseInStockFilterBox_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ProductsListPanelSelector")]
		public string ProductsListPanelSelector
		{
			get;
			set;
		}

		public bool ProductsListPanelSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CategoriesWithoutFilters")]
		public string CategoriesWithoutFilters
		{
			get;
			set;
		}

		public bool CategoriesWithoutFilters_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ProductsGridPanelSelector")]
		public string ProductsGridPanelSelector
		{
			get;
			set;
		}

		public bool ProductsGridPanelSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.PagerPanelSelector")]
		public string PagerPanelSelector
		{
			get;
			set;
		}

		public bool PagerPanelSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.PagerPanelIntegrationSelector")]
		public string PagerPanelIntegrationSelector
		{
			get;
			set;
		}

		public bool PagerPanelIntegrationSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.SortOptionsDropDownSelector")]
		public string SortOptionsDropDownSelector
		{
			get;
			set;
		}

		public bool SortOptionsDropDownSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ViewOptionsDropDownSelector")]
		public string ViewOptionsDropDownSelector
		{
			get;
			set;
		}

		public bool ViewOptionsDropDownSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ProductPageSizeDropDownSelector")]
		public string ProductPageSizeDropDownSelector
		{
			get;
			set;
		}

		public bool ProductPageSizeDropDownSelector_OverrideForStore
		{
			get;
			set;
		}

		public bool DisableDropdownNavigationWithAjaxWhenThereAreNoFilters
		{
			get;
			set;
		}

		public int ActiveStoreScopeConfiguration
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ShowFiltersOnCategoryPage")]
		public bool ShowFiltersOnCategoryPage
		{
			get;
			set;
		}

		public bool ShowFiltersOnCategoryPage_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ShowFiltersOnManufacturerPage")]
		public bool ShowFiltersOnManufacturerPage
		{
			get;
			set;
		}

		public bool ShowFiltersOnManufacturerPage_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ShowFiltersOnVendorPage")]
		public bool ShowFiltersOnVendorPage
		{
			get;
			set;
		}

		public bool ShowFiltersOnVendorPage_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ShowFiltersOnSearchPage")]
		public bool ShowFiltersOnSearchPage
		{
			get;
			set;
		}

		public bool ShowFiltersOnSearchPage_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ShowSelectedFiltersPanel")]
		public bool ShowSelectedFiltersPanel
		{
			get;
			set;
		}

		public bool ShowSelectedFiltersPanel_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ShowNumberOfReturnedProducts")]
		public bool ShowNumberOfReturnedProducts
		{
			get;
			set;
		}

		public bool ShowNumberOfReturnedProducts_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.ShowNumberOfReturnedProductsSelector")]
		public string ShowNumberOfReturnedProductsSelector
		{
			get;
			set;
		}

		public bool ShowNumberOfReturnedProductsSelector_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.TrailingZeroesSeparator")]
		public string TrailingZeroesSeparator
		{
			get;
			set;
		}

		public bool TrailingZeroesSeparator_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.SearchInProductTags")]
		public bool SearchInProductTags
		{
			get;
			set;
		}

		public bool SearchInProductTags_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Spikes.NopAjaxFilters.Admin.CloseFiltersPanelAfterFiltrationInMobile")]
		public bool CloseFiltersPanelAfterFiltrationInMobile
		{
			get;
			set;
		}

		public bool CloseFiltersPanelAfterFiltrationInMobile_OverrideForStore
		{
			get;
			set;
		}
	}
}
