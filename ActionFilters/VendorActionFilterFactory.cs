using Microsoft.AspNetCore.Mvc.Filters;
using Spikes.Nop.Framework.ActionFilters;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class VendorActionFilterFactory : IControllerActionFilterFactory
	{
		public string ControllerName => "Catalog";

		public string ActionName => "Vendor";

		public ActionFilterAttribute GetActionFilterAttribute()
		{
			return new VendorActionFilterAttribute();
		}
	}
}
