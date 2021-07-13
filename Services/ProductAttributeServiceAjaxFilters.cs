using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using Spikes.Nop.Plugins.AjaxFilters.Models;
using System.Collections.Generic;
using System.Linq;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
	public class ProductAttributeServiceAjaxFilters : IProductAttributeServiceAjaxFilters
	{
		private readonly IRepository<PredefinedProductAttributeValue> _predefinedAttributeValueRepository;

		private readonly IStaticCacheManager _staticCacheManager;

		public ProductAttributeServiceAjaxFilters(IRepository<PredefinedProductAttributeValue> predefinedAttributeValueRepository, IStaticCacheManager staticCacheManager)
		{
			_predefinedAttributeValueRepository = predefinedAttributeValueRepository;
			_staticCacheManager = staticCacheManager;
		}

		public IList<AttributeFilterItem> GetSortedAttributeValuesBasedOnTheirPredefinedDisplayOrder(IEnumerable<AttributeFilterItem> attributeValues)
		{
			IList<PredefinedProductAttributeValue> inner = _staticCacheManager.Get(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY, () => _predefinedAttributeValueRepository.Table.ToList());
			return (from attributeValue in attributeValues
				join predefinedAttribute in inner on new
				{
					AttributeId = attributeValue.AttributeId,
					AttributeValue = attributeValue.Name
				} equals new
				{
					AttributeId = predefinedAttribute.ProductAttributeId,
					AttributeValue = predefinedAttribute.Name
				} into temp
				from predefinedAttributeValue in temp.DefaultIfEmpty(new PredefinedProductAttributeValue
				{
					DisplayOrder = int.MaxValue
				})
				orderby predefinedAttributeValue.DisplayOrder
				select attributeValue).ToList();
		}
	}
}
