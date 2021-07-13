using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Spikes.Nop.Framework.Components;
using Spikes.Nop.Plugins.AjaxFilters.Domain;
using Spikes.Nop.Plugins.AjaxFilters.Factories;
using Spikes.Nop.Plugins.AjaxFilters.Helpers;
using Spikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.Components
{
    [ViewComponent(Name = "NopAjaxFilters")]
	public class NopAjaxFiltersComponent : BaseSpikesComponent
    {
		private readonly IStoreContext _storeContext;
		private readonly IWorkContext _workContext;
		private readonly IStaticCacheManager _cacheManager;
		private readonly CatalogSettings _catalogSettings;
		private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;
		private readonly IFiltersPageHelper _filtersPageHelper;
        private readonly INopAjaxFilterModelFactory _nopAjaxFilterModelFactory;
        private readonly ISearchQueryStringHelper _searchQueryStringHelper;
        private IFiltersPageHelper FiltersPageHelper
		{
			get
			{
				var value = Request.QueryString.Value;
				if (!string.IsNullOrEmpty(value))
				{
					_filtersPageHelper.Initialize(base.Url.ActionContext, value);
				}
				else
				{
					_filtersPageHelper.Initialize(base.Url.ActionContext);
				}
				return _filtersPageHelper;
			}
		}
		public NopAjaxFiltersComponent(IStoreContext storeContext, IWorkContext workContext, INopAjaxFilterModelFactory nopAjaxFilterModelFactory, ISearchQueryStringHelper searchQueryStringHelper, IStaticCacheManager cacheManager, CatalogSettings catalogSettings, NopAjaxFiltersSettings nopAjaxFiltersSettings, IFiltersPageHelper filtersPageHelper)
		{
            _nopAjaxFilterModelFactory = nopAjaxFilterModelFactory;
            _storeContext = storeContext;
			_workContext = workContext;
            _cacheManager = cacheManager;
			_catalogSettings = catalogSettings;
			_nopAjaxFiltersSettings = nopAjaxFiltersSettings;
			_filtersPageHelper = filtersPageHelper;
            _searchQueryStringHelper = searchQueryStringHelper;
        }

		public IViewComponentResult Invoke(string widgetZone)
		{
            if (_nopAjaxFiltersSettings.WidgetZone == widgetZone && _nopAjaxFiltersSettings.EnableAjaxFilters)
			{
                var queryStringParameters = _searchQueryStringHelper.GetQueryStringParameters(base.Request.QueryString.Value);
                GetFilteredProductsModel filteredProductInternal;
                var filtersPageParameters = FiltersPageHelper.GetFiltersPageParameters();           
                var model = ViewData["SearchFilterModel.FilteredProductsModel"];
                var isOnSearchPage = false;
                var searchTerms = queryStringParameters.Keyword;
                if (model != null && queryStringParameters.IsOnSearchPage)
                {
                    filteredProductInternal = model as GetFilteredProductsModel;
                    isOnSearchPage = true;
                }
                else
                    filteredProductInternal = GetFilteredProductInternal(filtersPageParameters.CategoryId, filtersPageParameters.ManufacturerId, filtersPageParameters.VendorId, productTagId: filtersPageParameters.ProductTagId);
                if (filteredProductInternal ==  null)
                    return Content(string.Empty);
                var cahedNopAjaxFiltersSettingsModel = GetCahedNopAjaxFiltersSettingsModel(filtersPageParameters, isOnSearchPage, searchTerms);
                return View("NopFilters", (nopAjaxFiltersModel: cahedNopAjaxFiltersSettingsModel, filteredProductsModel: filteredProductInternal));
			}
			return Content(string.Empty);
		}

		private NopAjaxFiltersModel GetCahedNopAjaxFiltersSettingsModel(FiltersPageParameters filtersPageParameters, bool isOnSearchPage, string searchTerms)
		{
			var key = string.Format(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_MODEL_KEY, _storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id, filtersPageParameters.CategoryId, filtersPageParameters.ManufacturerId, filtersPageParameters.VendorId, filtersPageParameters.ProductTagId, isOnSearchPage, searchTerms);
			return _cacheManager.Get(key, delegate
			{
				var availableSortOptionsJson = GetAvailableSortOptionsJson(filtersPageParameters.OrderBy);
				var availableViewModesJson = GetAvailableViewModesJson(filtersPageParameters.ViewMode);
				var availablePageSizesJson = GetAvailablePageSizesJson(filtersPageParameters.PageSize);
				return new NopAjaxFiltersModel
				{
					CategoryId = filtersPageParameters.CategoryId,
					ManufacturerId = filtersPageParameters.ManufacturerId,
					VendorId = filtersPageParameters.VendorId,
                    ProductTagId = filtersPageParameters.ProductTagId,
					SearchQueryStringParameters = filtersPageParameters.SearchQueryStringParameters,
					DefaultViewMode = _catalogSettings.DefaultViewMode,
					AvailableSortOptionsJson = availableSortOptionsJson,
					AvailableViewModesJson = availableViewModesJson,
					AvailablePageSizesJson = availablePageSizesJson
				};
			});
		}
        private GetFilteredProductsModel GetFilteredProductInternal(int categoryId, int manufacturerId, int vendorId, int productTagId)
        {
            GetFilteredProductsModel model = null;
            if (categoryId > 0 || manufacturerId > 0 || vendorId > 0 || productTagId > 0)
                model = _nopAjaxFilterModelFactory.PrepareFilteredProductsModel(isOnSearchPage: false, categoryId: categoryId, vendorId: vendorId, manufacturerId: manufacturerId, productTagId: productTagId).filteredProductsModel;
            return model;
        }
        #region common
        private bool ShouldShowFiltersInCategory(int categoryId)
        {
            bool result = _nopAjaxFiltersSettings.ShowFiltersOnCategoryPage;
            if (categoryId > 0 && !string.IsNullOrEmpty(_nopAjaxFiltersSettings.CategoriesWithoutFilters))
            {
                string[] array = _nopAjaxFiltersSettings.CategoriesWithoutFilters.Split(',');
                for (int i = 0; i < array.Length; i++)
                {
                    if (int.TryParse(array[i], out int result2) && result2 == categoryId)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }
        private string GetAvailableSortOptionsJson(int? orderBy)
        {
            var flag = _catalogSettings.ProductSortingEnumDisabled.Count == Enum.GetValues(typeof(ProductSortingEnum)).Length;
            var orderedEnumerable = from idOption in Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>().Except(_catalogSettings.ProductSortingEnumDisabled)
                                    select new KeyValuePair<int, int>(idOption, _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out var value) ? value : idOption) into x
                                    orderby x.Value
                                    select x;
            if (!orderBy.HasValue)
            {
                orderBy = ((!flag) ? orderedEnumerable.First().Key : 0);
            }
            IList<SelectListItem> list = null;
            if (_catalogSettings.AllowProductSorting && !flag)
            {
                list = new List<SelectListItem>();
                foreach (KeyValuePair<int, int> item in orderedEnumerable)
                {
                    var localizedEnum = base.LocalizationService.GetLocalizedEnum((ProductSortingEnum)item.Key);
                    list.Add(new SelectListItem
                    {
                        Text = localizedEnum,
                        Value = item.Key.ToString(),
                        Selected = (item.Key == orderBy)
                    });
                }
            }
            var result = string.Empty;
            if (list != null)
            {
                result = JsonConvert.SerializeObject(list);
            }
            return result;
        }

        private string GetAvailableViewModesJson(string viewMode)
        {
            IList<SelectListItem> list = null;
            var text = viewMode;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = _catalogSettings.DefaultViewMode;
            }
            text = text.ToLowerInvariant();
            if (text != "grid" && text != "list")
            {
                text = "grid";
            }
            if (_catalogSettings.AllowProductViewModeChanging)
            {
                list = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Text = base.LocalizationService.GetResource("Catalog.ViewMode.Grid"),
                        Value = "grid",
                        Selected = (text == "grid")
                    },
                    new SelectListItem
                    {
                        Text = base.LocalizationService.GetResource("Catalog.ViewMode.List"),
                        Value = "list",
                        Selected = (text == "list")
                    }
                };
            }
            var result = string.Empty;
            if (list != null)
            {
                result = JsonConvert.SerializeObject(list);
            }
            return result;
        }

        private string GetAvailablePageSizesJson(int pageSize)
        {
            var list = new List<SelectListItem>();
            string[] pageSizes = _filtersPageHelper.GetPageSizes();
            if (pageSizes.Any())
            {
                if (pageSize <= 0 || !pageSizes.Contains(pageSize.ToString()))
                {
                    pageSize = _filtersPageHelper.GetDefaultPageSize();
                }
                string[] array = pageSizes;
                foreach (string text in array)
                {
                    if (int.TryParse(text, out int result) && result > 0)
                    {
                        list.Add(new SelectListItem
                        {
                            Text = text,
                            Value = text,
                            Selected = text.Equals(pageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }
                }
                if (list.Any())
                {
                    list = list.OrderBy((SelectListItem x) => int.Parse(x.Text)).ToList();
                }
            }
            string result2 = string.Empty;
            if (list.Any())
            {
                result2 = JsonConvert.SerializeObject(list);
            }
            return result2;
        }
        #endregion

	}
}
