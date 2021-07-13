using Spikes.Nop.Plugins.AjaxFilters.Models;
using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
	public interface IProductAttributeServiceAjaxFilters
	{
		IList<AttributeFilterItem> GetSortedAttributeValuesBasedOnTheirPredefinedDisplayOrder(IEnumerable<AttributeFilterItem> attributeValues);
	}
}
