using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Domain;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class ManufacturerActionFilterAttribute : ActionFilterAttribute
	{
		private NopAjaxFiltersSettings _ajaxFiltersSettings;

		private WidgetSettings _widgetSettings;

		private NopAjaxFiltersSettings AjaxFiltersSettings => _ajaxFiltersSettings ?? (_ajaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>());

		private WidgetSettings WidgetSettings => _widgetSettings ?? (_widgetSettings = EngineContext.Current.Resolve<WidgetSettings>());

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			ViewResult viewResult;
			ManufacturerModel manufacturerModel;
			if (AjaxFiltersSettings.EnableAjaxFilters && (AjaxFiltersSettings.EnableAttributesFilter || AjaxFiltersSettings.EnableManufacturersFilter || AjaxFiltersSettings.EnableOnSaleFilter || AjaxFiltersSettings.EnableInStockFilter || AjaxFiltersSettings.EnableVendorsFilter || AjaxFiltersSettings.EnablePriceRangeFilter || AjaxFiltersSettings.EnableSpecificationsFilter) && WidgetSettings.ActiveWidgetSystemNames.Contains("Spikes.Nop.Plugins.AjaxFilters") && !string.IsNullOrEmpty(AjaxFiltersSettings.WidgetZone) && (viewResult = (filterContext.Result as ViewResult)) != null && (manufacturerModel = (viewResult.Model as ManufacturerModel)) != null)
			{
				manufacturerModel.PagingFilteringContext.PriceRangeFilter.Enabled = false;
				manufacturerModel.PagingFilteringContext.SpecificationFilter.Enabled = false;
			}
		}
	}
}
