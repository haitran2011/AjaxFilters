using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Framework.Kendoui;
using Spikes.Nop.Core.Helpers;
using Spikes.Nop.Framework;
using Spikes.Nop.Framework.Areas.Admin.ControllerAttributes;
using Spikes.Nop.Framework.Areas.Admin.Controllers;
using Spikes.Nop.Framework.Areas.Admin.Helpers;
using Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Extensions;
using Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;
using Spikes.Nop.Plugins.AjaxFilters.Domain;
using Spikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using System.Collections.Generic;
using System.Linq;

namespace Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Controllers
{
	[ManagePluginsAdminAuthorize("Spikes.Nop.Plugins.AjaxFilters", false)]
	public class NopAjaxFiltersAdminController : Base7SpikesAdminController
	{
		private readonly IConvertToDictionaryHelper _convertToDictionaryHelper;

		private readonly ICustomerActivityService _customerActivityService;

		private readonly ILocalizationService _localizationService;

		private readonly IProductAttributeService _productAttributeService;

		private readonly ISettingService _settingService;

		private readonly ISpecificationAttributeService _specificationAttributeService;

		private readonly IStaticCacheManager _staticCacheManager;

		private readonly WidgetSettings _widgetSettings;

		private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;

		public NopAjaxFiltersAdminController(IConvertToDictionaryHelper convertToDictionaryHelper, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IProductAttributeService productAttributeService, ISettingService settingService, ISpecificationAttributeService specificationAttributeService, IStaticCacheManager staticCacheManager, WidgetSettings widgetSettings, NopAjaxFiltersSettings nopAjaxFiltersSettings)
		{
			_convertToDictionaryHelper = convertToDictionaryHelper;
			_customerActivityService = customerActivityService;
			_localizationService = localizationService;
			_productAttributeService = productAttributeService;
			_settingService = settingService;
			_specificationAttributeService = specificationAttributeService;
			_staticCacheManager = staticCacheManager;
			_widgetSettings = widgetSettings;
			_nopAjaxFiltersSettings = nopAjaxFiltersSettings;
		}

		public ActionResult Settings()
		{
			int activeStoreScopeConfiguration = base.StoreContext.ActiveStoreScopeConfiguration;
			NopAjaxFiltersSettings nopAjaxFiltersSettings = _settingService.LoadSetting<NopAjaxFiltersSettings>(activeStoreScopeConfiguration);
			NopAjaxFiltersSettingsModel nopAjaxFiltersSettingsModel = nopAjaxFiltersSettings.ToModel();
			InitializeModel(nopAjaxFiltersSettingsModel, activeStoreScopeConfiguration);
			nopAjaxFiltersSettingsModel.ActiveStoreScopeConfiguration = activeStoreScopeConfiguration;
			if (activeStoreScopeConfiguration > 0)
			{
				StoreScopeSettingsHelper<NopAjaxFiltersSettings> storeScopeSettingsHelper = new StoreScopeSettingsHelper<NopAjaxFiltersSettings>(nopAjaxFiltersSettings, activeStoreScopeConfiguration, _settingService);
				nopAjaxFiltersSettingsModel.EnableAjaxFilters_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableAjaxFilters);
				nopAjaxFiltersSettingsModel.FiltersUIMode_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.FiltersUIMode);
				nopAjaxFiltersSettingsModel.EnableInfiniteScroll_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableInfiniteScroll);
				nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltration_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltration);
				nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltrationMobile_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltrationMobile);
				nopAjaxFiltersSettingsModel.ElementToScrollAfterFiltrationSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ElementToScrollAfterFiltrationSelector);
				nopAjaxFiltersSettingsModel.WidgetZone_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.WidgetZone);
				nopAjaxFiltersSettingsModel.EnablePriceRangeFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnablePriceRangeFilter);
				nopAjaxFiltersSettingsModel.ClosePriceRangeFilterBox_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ClosePriceRangeFilterBox);
				nopAjaxFiltersSettingsModel.EnableSpecificationsFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableSpecificationsFilter);
				nopAjaxFiltersSettingsModel.CloseSpecificationsFilterBox_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CloseSpecificationsFilterBox);
				nopAjaxFiltersSettingsModel.NumberOfSpecificationFilters_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.NumberOfSpecificationFilters);
				nopAjaxFiltersSettingsModel.EnableAttributesFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableAttributesFilter);
				nopAjaxFiltersSettingsModel.CloseAttributesFilterBox_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CloseAttributesFilterBox);
				nopAjaxFiltersSettingsModel.NumberOfAttributeFilters_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.NumberOfAttributeFilters);
				nopAjaxFiltersSettingsModel.EnableManufacturersFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableManufacturersFilter);
                nopAjaxFiltersSettingsModel.EnableCategoriesFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableCategoriesFilter);
                nopAjaxFiltersSettingsModel.EnableRatingFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableRatingFilter);
                nopAjaxFiltersSettingsModel.CloseManufacturersFilterBox_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CloseManufacturersFilterBox);
				nopAjaxFiltersSettingsModel.EnableOnSaleFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableOnSaleFilter);
				nopAjaxFiltersSettingsModel.CloseOnSaleFilterBox_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CloseOnSaleFilterBox);
				nopAjaxFiltersSettingsModel.EnableVendorsFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableVendorsFilter);
				nopAjaxFiltersSettingsModel.CloseVendorsFilterBox_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CloseVendorsFilterBox);
				nopAjaxFiltersSettingsModel.EnableInStockFilter_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.EnableInStockFilter);
				nopAjaxFiltersSettingsModel.CloseInStockFilterBox_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CloseInStockFilterBox);
				nopAjaxFiltersSettingsModel.ProductsListPanelSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ProductsListPanelSelector);
				nopAjaxFiltersSettingsModel.CategoriesWithoutFilters_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CategoriesWithoutFilters);
				nopAjaxFiltersSettingsModel.ProductsGridPanelSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ProductsGridPanelSelector);
				nopAjaxFiltersSettingsModel.PagerPanelSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.PagerPanelSelector);
				nopAjaxFiltersSettingsModel.PagerPanelIntegrationSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.PagerPanelIntegrationSelector);
				nopAjaxFiltersSettingsModel.SortOptionsDropDownSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.SortOptionsDropDownSelector);
				nopAjaxFiltersSettingsModel.ViewOptionsDropDownSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ViewOptionsDropDownSelector);
				nopAjaxFiltersSettingsModel.ProductPageSizeDropDownSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ProductPageSizeDropDownSelector);
				nopAjaxFiltersSettingsModel.ShowFiltersOnCategoryPage_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ShowFiltersOnCategoryPage);
				nopAjaxFiltersSettingsModel.ShowFiltersOnManufacturerPage_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ShowFiltersOnManufacturerPage);
				nopAjaxFiltersSettingsModel.ShowFiltersOnVendorPage_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ShowFiltersOnVendorPage);
				nopAjaxFiltersSettingsModel.ShowFiltersOnSearchPage_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ShowFiltersOnSearchPage);
				nopAjaxFiltersSettingsModel.ShowSelectedFiltersPanel_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ShowSelectedFiltersPanel);
				nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProducts_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProducts);
				nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProductsSelector_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProductsSelector);
				nopAjaxFiltersSettingsModel.TrailingZeroesSeparator_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.TrailingZeroesSeparator);
				nopAjaxFiltersSettingsModel.SearchInProductTags_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.SearchInProductTags);
				nopAjaxFiltersSettingsModel.CloseFiltersPanelAfterFiltrationInMobile_OverrideForStore = storeScopeSettingsHelper.SettingExists((NopAjaxFiltersSettings x) => x.CloseFiltersPanelAfterFiltrationInMobile);
			}
			return View("Settings", nopAjaxFiltersSettingsModel);
		}

		[HttpPost]
		public ActionResult Settings(NopAjaxFiltersSettingsModel nopAjaxFiltersSettingsModel)
		{
			if (!base.ModelState.IsValid)
			{
				return RedirectToAction("Settings");
			}
			if (nopAjaxFiltersSettingsModel.EnableAjaxFilters && !_widgetSettings.ActiveWidgetSystemNames.Contains("Spikes.Nop.Plugins.AjaxFilters"))
			{
				_widgetSettings.ActiveWidgetSystemNames.Add("Spikes.Nop.Plugins.AjaxFilters");
				_settingService.SaveSetting(_widgetSettings);
			}
			int activeStoreScopeConfiguration = base.StoreContext.ActiveStoreScopeConfiguration;
			StoreScopeSettingsHelper<NopAjaxFiltersSettings> storeScopeSettingsHelper = new StoreScopeSettingsHelper<NopAjaxFiltersSettings>(nopAjaxFiltersSettingsModel.ToEntity(), activeStoreScopeConfiguration, _settingService);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableAjaxFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableAjaxFilters);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.FiltersUIMode_OverrideForStore, (NopAjaxFiltersSettings x) => x.FiltersUIMode);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableInfiniteScroll_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableInfiniteScroll);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltration_OverrideForStore, (NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltration);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltrationMobile_OverrideForStore, (NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltrationMobile);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ElementToScrollAfterFiltrationSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ElementToScrollAfterFiltrationSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.WidgetZone_OverrideForStore, (NopAjaxFiltersSettings x) => x.WidgetZone);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnablePriceRangeFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnablePriceRangeFilter);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ClosePriceRangeFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.ClosePriceRangeFilterBox);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableSpecificationsFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableSpecificationsFilter);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CloseSpecificationsFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseSpecificationsFilterBox);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.NumberOfSpecificationFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.NumberOfSpecificationFilters);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableAttributesFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableAttributesFilter);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CloseAttributesFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseAttributesFilterBox);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.NumberOfAttributeFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.NumberOfAttributeFilters);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableManufacturersFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableManufacturersFilter);
            storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableCategoriesFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableCategoriesFilter);
            storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableRatingFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableRatingFilter);
            storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CloseManufacturersFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseManufacturersFilterBox);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableOnSaleFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableOnSaleFilter);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CloseOnSaleFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseOnSaleFilterBox);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableVendorsFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableVendorsFilter);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CloseVendorsFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseVendorsFilterBox);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.EnableInStockFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableInStockFilter);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CloseInStockFilterBox, (NopAjaxFiltersSettings x) => x.CloseInStockFilterBox);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ProductsListPanelSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ProductsListPanelSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CategoriesWithoutFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.CategoriesWithoutFilters);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ProductsGridPanelSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ProductsGridPanelSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.PagerPanelSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.PagerPanelSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.PagerPanelIntegrationSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.PagerPanelIntegrationSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.SortOptionsDropDownSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.SortOptionsDropDownSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ViewOptionsDropDownSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ViewOptionsDropDownSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ProductPageSizeDropDownSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ProductPageSizeDropDownSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ShowFiltersOnManufacturerPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnManufacturerPage);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ShowFiltersOnVendorPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnVendorPage);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ShowFiltersOnCategoryPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnCategoryPage);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ShowFiltersOnSearchPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnSearchPage);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ShowSelectedFiltersPanel_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowSelectedFiltersPanel);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProducts_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProducts);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProductsSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProductsSelector);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.TrailingZeroesSeparator_OverrideForStore, (NopAjaxFiltersSettings x) => x.TrailingZeroesSeparator);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.SearchInProductTags_OverrideForStore, (NopAjaxFiltersSettings x) => x.SearchInProductTags);
			storeScopeSettingsHelper.SaveStoreSetting(nopAjaxFiltersSettingsModel.CloseFiltersPanelAfterFiltrationInMobile_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseFiltersPanelAfterFiltrationInMobile);
			_settingService.ClearCache();
			_staticCacheManager.RemoveByPrefix(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PATTERN_KEY);
			_customerActivityService.InsertActivity("EditNopAjaxFiltersSettings", "Edit Nop Ajax Filters Settings");
			SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
			SaveSelectedTabName();
			return RedirectToAction("Settings");
		}

		private void InitializeModel(NopAjaxFiltersSettingsModel model, int storeScope)
		{
			IEnumerable<string> supportedWidgetZones = EngineContext.Current.Resolve<IInstallHelper>().GetSupportedWidgetZones("Spikes.Nop.Plugins.AjaxFilters", storeScope);
			model.SupportedWidgetZones = new SelectList(supportedWidgetZones);
			model.AvailableFiltersUIModes = model.FiltersUIMode.ToSelectList();
			model.EnableAjaxFilters = (model.EnableAjaxFilters && _widgetSettings.ActiveWidgetSystemNames.Contains("Spikes.Nop.Plugins.AjaxFilters"));
		}

		[HttpPost]
		public ActionResult GridList(DataSourceRequest command)
		{
			IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
			IList<ProductAttribute> list = new List<ProductAttribute>();
			IDictionary<int, int> dictionary = new Dictionary<int, int>();
			if (activeProductAttributes.Count > 0)
			{
				dictionary = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
				list = _productAttributeService.GetProductAttributesByIds(dictionary.Select((KeyValuePair<int, int> x) => x.Key).ToArray());
			}
			DataSourceResult data = new DataSourceResult
			{
				Data = list.Select(delegate(ProductAttribute x)
				{
					int displayOrder = dictionary[x.Id];
					return new
					{
						Id = x.Id,
						Name = x.Name,
						DisplayOrder = displayOrder
					};
				}),
				Total = list.Count
			};
			return Json(data);
		}

		[HttpPost]
		public ActionResult Update(DataSourceRequest command, int id, int displayOrder)
		{
			if (id == 0)
			{
				return base.EmptyJson;
			}
			IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
			IDictionary<int, int> dictionary = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
			if (dictionary.ContainsKey(id))
			{
				_nopAjaxFiltersSettings.ActiveProductAttributes.Remove($"{id}:{dictionary[id]}");
				_nopAjaxFiltersSettings.ActiveProductAttributes.Add($"{id}:{displayOrder}");
				_settingService.SaveSetting(_nopAjaxFiltersSettings);
			}
			return GridList(command);
		}

		[HttpPost]
		public ActionResult Delete(DataSourceRequest command, int id)
		{
			if (id == 0)
			{
				return base.EmptyJson;
			}
			IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
			IDictionary<int, int> dictionary = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
			if (dictionary.ContainsKey(id))
			{
				_nopAjaxFiltersSettings.ActiveProductAttributes.Remove($"{id}:{dictionary[id]}");
				_settingService.SaveSetting(_nopAjaxFiltersSettings);
			}
			return GridList(command);
		}

		[HttpPost]
		public ActionResult Create(DataSourceRequest command, string name, int displayOrder)
		{
			if (!string.IsNullOrEmpty(name))
			{
				ProductAttribute productAttributeByName = _productAttributeService.GetProductAttributeByName(name);
				if (productAttributeByName != null)
				{
					IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
					if (!_convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes).ContainsKey(productAttributeByName.Id))
					{
						_nopAjaxFiltersSettings.ActiveProductAttributes.Add($"{productAttributeByName.Id}:{displayOrder}");
						_settingService.SaveSetting(_nopAjaxFiltersSettings);
					}
				}
			}
			return GridList(command);
		}

		[HttpGet]
		public ActionResult GetProductAttributes()
		{
			IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
			IDictionary<int, int> source = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
			IEnumerable<int> currentProductAttributeIds = source.Select((KeyValuePair<int, int> x) => x.Key);
			IList<SelectListItem> data = (from x in _productAttributeService.GetAllProductAttributes()
				where !currentProductAttributeIds.Contains(x.Id)
				select new SelectListItem
				{
					Value = x.Id.ToString(),
					Text = x.Name
				}).ToList();
			return Json(data);
		}

		[HttpPost]
		public ActionResult SpecificationAttributeSlidersGridList(DataSourceRequest command)
		{
			IList<int> specificationAttributeSliders = _nopAjaxFiltersSettings.SpecificationAttributeSliders;
			IList<SpecificationAttribute> list = new List<SpecificationAttribute>();
			if (specificationAttributeSliders.Count > 0)
			{
				list = _specificationAttributeService.GetSpecificationAttributeByIds(specificationAttributeSliders.ToArray());
			}
			DataSourceResult data = new DataSourceResult
			{
				Data = list.Select((SpecificationAttribute x) => new
				{
					x.Id,
					x.Name
				}),
				Total = list.Count
			};
			return Json(data);
		}

		[HttpPost]
		public ActionResult SpecificationAttributeSlidersDelete(DataSourceRequest command, int id)
		{
			if (id == 0)
			{
				return base.EmptyJson;
			}
			if (((ICollection<int>)_nopAjaxFiltersSettings.SpecificationAttributeSliders).Contains(id))
			{
				_nopAjaxFiltersSettings.SpecificationAttributeSliders.Remove(id);
				_settingService.SaveSetting(_nopAjaxFiltersSettings);
			}
			return GridList(command);
		}

		[HttpPost]
		public ActionResult SpecificationAttributeSlidersCreate(DataSourceRequest command, string name, int displayOrder)
		{
			if (!string.IsNullOrEmpty(name))
			{
				SpecificationAttribute specificationAttributeByName = _specificationAttributeService.GetSpecificationAttributeByName(name);
				if (specificationAttributeByName != null && !((ICollection<int>)_nopAjaxFiltersSettings.SpecificationAttributeSliders).Contains(specificationAttributeByName.Id))
				{
					_nopAjaxFiltersSettings.SpecificationAttributeSliders.Add(specificationAttributeByName.Id);
					_settingService.SaveSetting(_nopAjaxFiltersSettings);
				}
			}
			return GridList(command);
		}

		[HttpGet]
		public ActionResult GetSpecifications()
		{
			IList<int> currentSpecifications = _nopAjaxFiltersSettings.SpecificationAttributeSliders;
			IList<SelectListItem> data = (from x in _specificationAttributeService.GetSpecificationAttributes()
				where !currentSpecifications.Contains(x.Id)
				select new SelectListItem
				{
					Value = x.Id.ToString(),
					Text = x.Name
				}).ToList();
			return Json(data);
		}
	}
}
