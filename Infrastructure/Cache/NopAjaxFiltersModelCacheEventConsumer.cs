using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Spikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache
{
	public class NopAjaxFiltersModelCacheEventConsumer : IConsumer<EntityInsertedEvent<ProductSpecificationAttribute>>, IConsumer<EntityUpdatedEvent<ProductSpecificationAttribute>>, IConsumer<EntityDeletedEvent<ProductSpecificationAttribute>>, IConsumer<EntityInsertedEvent<ProductAttributeMapping>>, IConsumer<EntityUpdatedEvent<ProductAttributeMapping>>, IConsumer<EntityDeletedEvent<ProductAttributeMapping>>, IConsumer<EntityInsertedEvent<ProductAttributeValue>>, IConsumer<EntityUpdatedEvent<ProductAttributeValue>>, IConsumer<EntityDeletedEvent<ProductAttributeValue>>, IConsumer<EntityInsertedEvent<PredefinedProductAttributeValue>>, IConsumer<EntityUpdatedEvent<PredefinedProductAttributeValue>>, IConsumer<EntityDeletedEvent<PredefinedProductAttributeValue>>, IConsumer<EntityInsertedEvent<ProductManufacturer>>, IConsumer<EntityUpdatedEvent<ProductManufacturer>>, IConsumer<EntityDeletedEvent<ProductManufacturer>>, IConsumer<EntityInsertedEvent<Product>>, IConsumer<EntityUpdatedEvent<Product>>, IConsumer<EntityDeletedEvent<Product>>, IConsumer<EntityInsertedEvent<Discount>>, IConsumer<EntityUpdatedEvent<Discount>>, IConsumer<EntityDeletedEvent<Discount>>, IConsumer<EntityInsertedEvent<TaxCategory>>, IConsumer<EntityUpdatedEvent<TaxCategory>>, IConsumer<EntityDeletedEvent<TaxCategory>>, IConsumer<EntityInsertedEvent<Currency>>, IConsumer<EntityUpdatedEvent<Currency>>, IConsumer<EntityDeletedEvent<Currency>>, IConsumer<EntityInsertedEvent<Setting>>, IConsumer<EntityUpdatedEvent<Setting>>, IConsumer<EntityDeletedEvent<Setting>>, IConsumer<EntityInsertedEvent<AclRecord>>, IConsumer<EntityUpdatedEvent<AclRecord>>, IConsumer<EntityDeletedEvent<AclRecord>>, IConsumer<EntityInsertedEvent<Language>>, IConsumer<EntityUpdatedEvent<Language>>, IConsumer<EntityDeletedEvent<Language>>, IConsumer<EntityUpdatedEvent<Category>>
	{
		public static readonly string NOP_AJAX_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}";

		public static readonly string NOP_AJAX_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters";

		public static readonly string NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.specification.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";

		public static readonly string NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.specification.filters";

		public static readonly string NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY = "nop.pres.nop.ajax.filters.predefined.attribute.values";

		public static readonly string NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.attribute.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";

		public static readonly string NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.attribute.filters";

        public static readonly string NOP_AJAX_FILTERS_CATEGORY_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.category.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";

        public static readonly string NOP_AJAX_FILTERS_RATING_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.rating.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";

        public static readonly string NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.manufacturer.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";

		public static readonly string NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.manufacturer.filters";

		public static readonly string NOP_AJAX_FILTERS_ONSALE_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.onsale.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";
    
        public static readonly string NOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.instock.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";

        public static readonly string NOP_AJAX_FILTERS_VENDOR_FILTERS_MODEL_KEY = "nop.pres.nop.ajax.filters.vendor.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";

        public static readonly string NOP_AJAX_FILTERS_VENDOR_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.vednor.filters";

		public static readonly string NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_MODEL_KEY = "nop.pres.nop.ajax.filters.price.range.filter-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}";

		public static readonly string NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY = "nop.pres.nop.ajax.filters.price.range.filter-dto-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}";

		public static readonly string NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY = "nop.pres.nop.ajax.filters.price.range.filter";

		public const string NOP_AJAX_FILTERS_FILTERED_PRODUCTS_PATTERN = "nop.pres.nop.ajax.filters.filtered.products";

		public const string NOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY = "nop.pres.nop.ajax.filters.filtered.products.{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}.{8}.{9}.{10}.{11}";

        public static readonly string NOP_AJAX_FILTERS_MODELS_KEY = "nop.pres.nop.ajax.filters.model-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}";
        
        private readonly IStaticCacheManager _cacheManager;
        public NopAjaxFiltersModelCacheEventConsumer(IStaticCacheManager cacheManager)
		{
			_cacheManager = cacheManager;
		}

		public void HandleEvent(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityInsertedEvent<ProductAttributeMapping> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityUpdatedEvent<ProductAttributeMapping> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityDeletedEvent<ProductAttributeMapping> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityInsertedEvent<ProductAttributeValue> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityUpdatedEvent<ProductAttributeValue> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityDeletedEvent<ProductAttributeValue> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityInsertedEvent<PredefinedProductAttributeValue> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY);
		}

		public void HandleEvent(EntityUpdatedEvent<PredefinedProductAttributeValue> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY);
		}

		public void HandleEvent(EntityDeletedEvent<PredefinedProductAttributeValue> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY);
		}

		public void HandleEvent(EntityInsertedEvent<ProductManufacturer> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityUpdatedEvent<ProductManufacturer> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityDeletedEvent<ProductManufacturer> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityInsertedEvent<Setting> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityUpdatedEvent<Setting> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityDeletedEvent<Setting> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityUpdatedEvent<Category> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityInsertedEvent<Product> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
		}

		public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
		}

		public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
		}

		public void HandleEvent(EntityInsertedEvent<Discount> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityUpdatedEvent<Discount> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityDeletedEvent<Discount> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityInsertedEvent<TaxCategory> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityUpdatedEvent<TaxCategory> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityDeletedEvent<TaxCategory> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityInsertedEvent<Currency> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityUpdatedEvent<Currency> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityDeletedEvent<Currency> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
		}

		public void HandleEvent(EntityInsertedEvent<AclRecord> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityUpdatedEvent<AclRecord> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityDeletedEvent<AclRecord> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY);
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY);
			_cacheManager.RemoveByPrefix("nop.pres.nop.ajax.filters.filtered.products");
		}

		public void HandleEvent(EntityInsertedEvent<Language> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
		}

		public void HandleEvent(EntityUpdatedEvent<Language> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
		}

		public void HandleEvent(EntityDeletedEvent<Language> eventMessage)
		{
			_cacheManager.RemoveByPrefix(NOP_AJAX_FILTERS_PATTERN_KEY);
		}
	}
}
