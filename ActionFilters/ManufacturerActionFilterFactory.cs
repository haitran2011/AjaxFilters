using Microsoft.AspNetCore.Mvc.Filters;
using Spikes.Nop.Framework.ActionFilters;

namespace Spikes.Nop.Plugins.AjaxFilters.ActionFilters
{
	public class ManufacturerActionFilterFactory : IControllerActionFilterFactory
	{
		public string ControllerName => "Catalog";

		public string ActionName => "Manufacturer";

		public ActionFilterAttribute GetActionFilterAttribute()
		{
			return new ManufacturerActionFilterAttribute();
		}
	}
}
