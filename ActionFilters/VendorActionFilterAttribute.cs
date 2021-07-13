using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Domain;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class VendorActionFilterAttribute : ActionFilterAttribute
	{
		private NopAjaxFiltersSettings _ajaxFiltersSettings;

		private WidgetSettings _widgetSettings;

		private NopAjaxFiltersSettings AjaxFiltersSettings => _ajaxFiltersSettings ?? (_ajaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>());

		private WidgetSettings WidgetSettings => _widgetSettings ?? (_widgetSettings = EngineContext.Current.Resolve<WidgetSettings>());

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			ViewResult viewResult;
			VendorModel vendorModel;
			if (AjaxFiltersSettings.EnableAjaxFilters && AjaxFiltersSettings.ShowFiltersOnManufacturerPage && AjaxFiltersSettings.ShowFiltersOnVendorPage && WidgetSettings.ActiveWidgetSystemNames.Contains("Spikes.Nop.Plugins.AjaxFilters") && !string.IsNullOrEmpty(AjaxFiltersSettings.WidgetZone) && (AjaxFiltersSettings.EnableAttributesFilter || AjaxFiltersSettings.EnableManufacturersFilter || AjaxFiltersSettings.EnableOnSaleFilter || AjaxFiltersSettings.EnableInStockFilter || AjaxFiltersSettings.EnablePriceRangeFilter || AjaxFiltersSettings.EnableSpecificationsFilter) && (viewResult = (filterContext.Result as ViewResult)) != null && (vendorModel = (viewResult.Model as VendorModel)) != null)
			{
				vendorModel.PagingFilteringContext.PriceRangeFilter.Enabled = false;
				vendorModel.PagingFilteringContext.SpecificationFilter.Enabled = false;
			}
		}
	}
}
