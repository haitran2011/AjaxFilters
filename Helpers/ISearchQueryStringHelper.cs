using Microsoft.AspNetCore.Routing;
using Nop.Web.Models.Catalog;

namespace Spikes.Nop.Plugins.AjaxFilters.Helpers
{
	public interface ISearchQueryStringHelper
	{
		SearchQueryStringParameters GetQueryStringParameters(string queryString);

		RouteValueDictionary PrepareSearchRouteValues(SearchModel model, CatalogPagingFilteringModel command);
	}
}
