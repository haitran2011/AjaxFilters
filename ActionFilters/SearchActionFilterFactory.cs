using Microsoft.AspNetCore.Mvc.Filters;
using Spikes.Nop.Framework.ActionFilters;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class SearchActionFilterFactory : IControllerActionFilterFactory
	{
		public string ControllerName => "Catalog";

		public string ActionName => "Search";

		public ActionFilterAttribute GetActionFilterAttribute()
		{
			return new SearchActionFilterAttribute();
		}
	}
}
