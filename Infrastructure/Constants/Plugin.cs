using Spikes.Nop.Framework.Plugin;
using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Infrastructure.Constants
{
	public class Plugin
	{
		public const string SystemName = "Spikes.Nop.Plugins.AjaxFilters";

		public const string FolderName = "Spikes.Nop.Plugins.AjaxFilters";

		public const string Name = "Nop Ajax Filters";

		public const string ResourceName = "Spikes.Plugins.AjaxFilters.Admin.Menu.MenuName";

		public const string UrlInStore = "http://www.google.com";

		public static List<MenuItemSpikes> MenuItems => new List<MenuItemSpikes>
		{
			new MenuItemSpikes
			{
				SubMenuName = "Spikes.NopAjaxFilters.Admin.Submenus.Settings",
				SubMenuRelativePath = "NopAjaxFiltersAdmin/Settings"
			}
		};

	}
}
