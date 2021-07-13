using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Localization;
using Spikes.Nop.Framework.ActionFilters;
using Spikes.Nop.Framework.Routing;
using Spikes.Nop.Framework.ViewLocations;
using Spikes.Nop.Plugins.AjaxFilters.ActionFilters;
using Spikes.Nop.Plugins.AjaxFilters.Services;
using System.Collections.Generic;
using System.Linq;

namespace Spikes.Nop.Plugins.AjaxFilters.Infrastructure
{
	public class RouteProvider : BaseRouteProvider
	{
		protected override string PluginSystemName => "Spikes.Nop.Plugins.AjaxFilters";

		protected override void RegisterDuplicateControllers(IViewLocationsManager viewLocationsManager)
		{
			List<DuplicateControllerInfo> list = new List<DuplicateControllerInfo>();
			DuplicateControllerInfo item = new DuplicateControllerInfo
			{
				DuplicateControllerName = "CatalogSpikes",
				DuplicateOfControllerName = "Catalog"
			};
			list.Add(item);
			viewLocationsManager.AddDuplicateControllers(list);
		}

		protected override void RegisterPluginActionFilters(IList<IFilterProvider> providers)
		{
            GeneralActionFilterProvider generalActionFilterProvider = new GeneralActionFilterProvider();
            //generalActionFilterProvider.Add(new CategoryActionFilterFactory());
            //generalActionFilterProvider.Add(new ManufacturerActionFilterFactory());
            //generalActionFilterProvider.Add(new VendorActionFilterFactory());
            //generalActionFilterProvider.Add(new SearchActionFilterFactory());
            providers.Add(generalActionFilterProvider);
		}

		protected override void RegisterRoutesAccessibleByName(IRouteBuilder routeBuilder)
		{
			base.RegisterRoutesAccessibleByName(routeBuilder);
            var routeSearch = routeBuilder.Routes.FirstOrDefault(x => ((Route)x).Name == "ProductSearch");
            routeBuilder.Routes.Remove(routeSearch);
            routeBuilder.MapLocalizedRoute("ProductSearch", "search/",
               new { controller = "CatalogSpikes", action = "Search" });

            routeBuilder.MapLocalizedRoute("GetFilteredProducts", "getFilteredProducts/", new
            {
                controller = "CatalogSpikes",
                action = "GetFilteredProducts"
            });
        }

		protected override void UpdateDatabase()
		{
			EngineContext.Current.Resolve<IAjaxFiltersDatabaseService>().UpdateDatabaseScripts();
		}

		protected override bool ShouldAddPluginViewLocationsBeforeNopViewLocations()
		{
			return true;
		}
	}
}
