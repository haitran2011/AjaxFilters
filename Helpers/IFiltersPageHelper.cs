using Microsoft.AspNetCore.Mvc;
using Nop.Web.Models.Catalog;

namespace Spikes.Nop.Plugins.AjaxFilters.Helpers
{
	public interface IFiltersPageHelper
	{
		FiltersPageParameters GetFiltersPageParameters();

		bool ValidateParameters(FiltersPageParameters filtersPageParameters);

		string[] GetPageSizes();

		void AdjustPagingFilteringModelPageSizeAndPageNumber(CatalogPagingFilteringModel catalogPagingFilteringModel);

		string GetTemplateViewPath();

		void Initialize(ActionContext requestContext, string query);

		void Initialize(ActionContext requestContext);

		void Initialize(FiltersPageParameters filtersPageParameters);

		int GetDefaultPageSize();

		int GetDefaultOrderBy();

		int GetDefaultPageNumber();

		string GetDefaultViewMode();
	}
}
