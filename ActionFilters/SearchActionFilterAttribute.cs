using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Spikes.Nop.Plugins.AjaxFilters.Domain;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class SearchActionFilterAttribute : ActionFilterAttribute
	{
		private NopAjaxFiltersSettings _ajaxFiltersSettings;

		private WidgetSettings _widgetSettings;

		private NopAjaxFiltersSettings AjaxFiltersSettings => _ajaxFiltersSettings ?? (_ajaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>());

		private WidgetSettings WidgetSettings => _widgetSettings ?? (_widgetSettings = EngineContext.Current.Resolve<WidgetSettings>());

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (AjaxFiltersSettings.EnableAjaxFilters && WidgetSettings.ActiveWidgetSystemNames.Contains("Spikes.Nop.Plugins.AjaxFilters") && !string.IsNullOrEmpty(AjaxFiltersSettings.WidgetZone) && AjaxFiltersSettings.ShowFiltersOnSearchPage)
			{
				string url = new UrlHelper(filterContext).RouteUrl("FilterProductSearch") + filterContext.HttpContext.Request.QueryString.ToString();
				filterContext.Result = new RedirectResult(url);
			}
			base.OnActionExecuting(filterContext);
		}
	}
}
