using Spikes.Nop.Framework.AutoMapper;
using Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;
using Spikes.Nop.Plugins.AjaxFilters.Domain;

namespace Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Extensions
{
	public static class MappingExtensions
	{
		public static NopAjaxFiltersSettingsModel ToModel(this NopAjaxFiltersSettings nopAjaxFiltersSettings)
		{
			return nopAjaxFiltersSettings.MapTo<NopAjaxFiltersSettings, NopAjaxFiltersSettingsModel>();
		}

		public static NopAjaxFiltersSettings ToEntity(this NopAjaxFiltersSettingsModel nopAjaxFiltersSettingsModel)
		{
			return nopAjaxFiltersSettingsModel.MapTo<NopAjaxFiltersSettingsModel, NopAjaxFiltersSettings>();
		}
	}
}
