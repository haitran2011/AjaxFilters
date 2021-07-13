using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Spikes.Nop.Framework.Plugin;
using Spikes.Nop.Plugins.AjaxFilters.Domain;
using Spikes.Nop.Plugins.AjaxFilters.Infrastructure.Constants;
using Spikes.Nop.Plugins.AjaxFilters.Services;

namespace Spikes.Nop.Plugins.AjaxFilters.Infrastructure
{
	public class AjaxFiltersPlugin : BaseAdminWidgetPluginSpikes
	{
		private readonly WidgetSettings _widgetSettings;

		private readonly ISettingService _settingService;

		private readonly IAjaxFiltersDatabaseService _ajaxFiltersDatabaseService;

		public AjaxFiltersPlugin(WidgetSettings widgetSettings, ISettingService settingService, IAjaxFiltersDatabaseService ajaxFiltersDatabaseService)
			: base(Plugin.MenuItems, "Spikes.Plugins.AjaxFilters.Admin.Menu.MenuName", "Spikes.Nop.Plugins.AjaxFilters", null, "")
		{
			_widgetSettings = widgetSettings;
			_settingService = settingService;
			_ajaxFiltersDatabaseService = ajaxFiltersDatabaseService;
		}

		public override string GetConfigurationPageUrl()
		{
			return base.StoreLocation + "Admin/NopAjaxFiltersAdmin/Settings";
		}

		public override string GetWidgetViewComponentName(string widgetZone)
		{
			return "NopAjaxFilters";
		}

		protected override void InstallDatabase()
		{
			_ajaxFiltersDatabaseService.RemoveDatabaseScripts();
			_ajaxFiltersDatabaseService.CreateDatabaseScripts();
		}

		protected override void UninstallDatabase()
		{
			_ajaxFiltersDatabaseService.RemoveDatabaseScripts();
		}

		protected override void InstallAdditionalSettings()
		{
			if (!_widgetSettings.ActiveWidgetSystemNames.Contains("Spikes.Nop.Plugins.AjaxFilters"))
			{
				_widgetSettings.ActiveWidgetSystemNames.Add("Spikes.Nop.Plugins.AjaxFilters");
				_settingService.SaveSetting(_widgetSettings);
			}
		}

		protected override void UninstallAdditionalSettings()
		{
			NopAjaxFiltersSettings nopAjaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>();
			nopAjaxFiltersSettings.WidgetZone = string.Empty;
			_settingService.SaveSetting(nopAjaxFiltersSettings);
		}
	}
}
