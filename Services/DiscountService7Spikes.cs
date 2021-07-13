using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Discounts;
using Spikes.Nop.Services.Catalog;
using Spikes.Nop.Services.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
	public class DiscountServiceSpikes : IDiscountServiceSpikes
	{
		private readonly IProductServiceNopAjaxFilters _productServiceNopAjaxFilters;

		private readonly IAclHelper _aclHelper;

		private readonly IStoreContext _storeContext;

		private const string DISCOUNT_APPLIED_TO_CATEGORY_EXISTS = "nop.discount.applied.to.category.{0}.exists.store.id-{1}";

		private const string DISCOUNT_APPLIED_TO_PRODUCT_VARIANT_IN_CATEGORY_EXISTS = "nop.discount.applied.to.product.variant.in.category.{0}.exists.store.id-{1}";

		public IRepository<Discount> DiscountRepository
		{
			get;
			set;
		}

		public IRepository<Product> ProductRepository
		{
			get;
			set;
		}

		public IRepository<ProductCategory> ProductCategoryRepository
		{
			get;
			set;
		}

		public IWorkContext WorkContext
		{
			get;
			set;
		}

		public ICacheManager CacheManager
		{
			get;
			set;
		}

		public IDiscountService DiscountService
		{
			get;
			set;
		}

		public DiscountServiceSpikes(IProductServiceNopAjaxFilters productServiceNopAjaxFilters, IRepository<Discount> discountRepository, IRepository<Product> productRepository, IRepository<ProductCategory> productCategoryRepository, IWorkContext workContext, ICacheManager cacheManager, IAclHelper aclHelper, IStoreContext storeContext)
		{
			_productServiceNopAjaxFilters = productServiceNopAjaxFilters;
			_aclHelper = aclHelper;
			_storeContext = storeContext;
			DiscountRepository = discountRepository;
			ProductRepository = productRepository;
			ProductCategoryRepository = productCategoryRepository;
			WorkContext = workContext;
			CacheManager = cacheManager;
		}

		public bool DiscountAppliedToCategoryExists(int categoryId)
		{
			return DiscountAppliedToCategoryExistsInternal(categoryId);
		}

		public bool DiscountAppliedToProductVariantsInCategoryExists(int categoryId)
		{
			return DiscountAppliedToProductVariantsInCategoryExistsInternal(categoryId);
		}

		private bool DiscountAppliedToCategoryExistsInternal(int categoryId)
		{
			string key = $"nop.discount.applied.to.category.{categoryId}.exists.store.id-{_storeContext.CurrentStore.Id}";
			return CacheManager.Get(key, () => DiscountRepository.Table.Where((Discount d) => d.DiscountCategoryMappings.FirstOrDefault((DiscountCategoryMapping x) => x.CategoryId == categoryId) != null).FirstOrDefault() != null);
		}

		private bool DiscountAppliedToProductVariantsInCategoryExistsInternal(int categoryId)
		{
			string key = $"nop.discount.applied.to.product.variant.in.category.{categoryId}.exists.store.id-{_storeContext.CurrentStore.Id}";
			return CacheManager.Get(key, delegate
			{
				IList<int> groupProductIds = _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategory(categoryId);
				IQueryable<Product> source = from p in _aclHelper.GetAvailableProductsForCurrentCustomer()
					join pc in ProductCategoryRepository.Table on p.Id equals pc.ProductId into p_pc
					from pc in p_pc.DefaultIfEmpty()
					where (pc != null && pc.CategoryId == categoryId) || (pc == null && groupProductIds.Contains(p.ParentGroupedProductId))
					select p;
				List<int> productVariantIds = (from x in source.ToList()
					select x.Id).ToList();
				return DiscountRepository.Table.Where((Discount d) => d.DiscountProductMappings.Any()).ToList().Any((Discount discount) => discount.DiscountProductMappings.Select((DiscountProductMapping x) => x.ProductId).Intersect(productVariantIds).Any());
			});
		}
	}
}
