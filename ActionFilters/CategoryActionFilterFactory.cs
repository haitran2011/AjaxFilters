using Microsoft.AspNetCore.Mvc.Filters;
using Spikes.Nop.Framework.ActionFilters;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class CategoryActionFilterFactory : IControllerActionFilterFactory
	{
		public string ControllerName => "Catalog";

		public string ActionName => "Category";

		public ActionFilterAttribute GetActionFilterAttribute()
		{
			return new CategoryActionFilterAttribute();
		}
	}
}
