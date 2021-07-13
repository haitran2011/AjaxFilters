using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Domain;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class CategoryActionFilterAttribute : ActionFilterAttribute
	{
		private NopAjaxFiltersSettings _ajaxFiltersSettings;

		private WidgetSettings _widgetSettings;

		protected NopAjaxFiltersSettings AjaxFiltersSettings => _ajaxFiltersSettings ?? (_ajaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>());

		protected WidgetSettings WidgetSettings => _widgetSettings ?? (_widgetSettings = EngineContext.Current.Resolve<WidgetSettings>());

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			ViewResult viewResult;
			CategoryModel categoryModel;
			if (AjaxFiltersSettings.EnableAjaxFilters && (AjaxFiltersSettings.EnableAttributesFilter || AjaxFiltersSettings.EnableManufacturersFilter || AjaxFiltersSettings.EnableOnSaleFilter || AjaxFiltersSettings.EnableInStockFilter || AjaxFiltersSettings.EnableVendorsFilter || AjaxFiltersSettings.EnablePriceRangeFilter || AjaxFiltersSettings.EnableSpecificationsFilter) && WidgetSettings.ActiveWidgetSystemNames.Contains("Spikes.Nop.Plugins.AjaxFilters") && !string.IsNullOrEmpty(AjaxFiltersSettings.WidgetZone) && (viewResult = (filterContext.Result as ViewResult)) != null && (categoryModel = (viewResult.Model as CategoryModel)) != null)
			{
				categoryModel.PagingFilteringContext.PriceRangeFilter.Enabled = false;
				categoryModel.PagingFilteringContext.SpecificationFilter.Enabled = false;
			}
		}
	}
}
