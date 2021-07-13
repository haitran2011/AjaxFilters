using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Directory;
using Spikes.Nop.Plugins.AjaxFilters.Models;
using Spikes.Nop.Services.Catalog;
using Spikes.Nop.Services.Helpers;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
    public class PriceCalculationServiceNopAjaxFilters : IPriceCalculationServiceNopAjaxFilters
	{
		private readonly IWorkContext _workContext;

		private readonly CatalogSettings _catalogSettings;

		private readonly TaxSettings _taxSettings;

		private readonly ICategoryServiceSpikes _categoryServiceSpikes;

		private readonly IAclHelper _aclHelper;

		private readonly IRepository<ProductCategory> _productCategoryRepository;

		private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

		private readonly IRepository<Vendor> _vendorRepository;

		private readonly ITaxServiceNopAjaxFilters _taxServiceNopAjaxFilters;

		private readonly ICurrencyService _currencyService;

		private readonly IProductServiceNopAjaxFilters _productServiceNopAjaxFilters;

		private readonly IStoreHelper _storeHelper;

		public PriceCalculationServiceNopAjaxFilters(IWorkContext workContext, CatalogSettings catalogSettings, TaxSettings taxSettings, ICategoryServiceSpikes categoryServiceSpikes, IAclHelper aclHelper, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<Vendor> vendorRepository, ITaxServiceNopAjaxFilters taxServiceNopAjaxFilters, ICurrencyService currencyService, IProductServiceNopAjaxFilters productServiceNopAjaxFilters, IStoreHelper storeHelper)
		{
			_workContext = workContext;
			_catalogSettings = catalogSettings;
			_taxSettings = taxSettings;
			_categoryServiceSpikes = categoryServiceSpikes;
			_aclHelper = aclHelper;
			_vendorRepository = vendorRepository;
			_productCategoryRepository = productCategoryRepository;
			_productManufacturerRepository = productManufacturerRepository;
			_taxServiceNopAjaxFilters = taxServiceNopAjaxFilters;
			_currencyService = currencyService;
			_productServiceNopAjaxFilters = productServiceNopAjaxFilters;
			_storeHelper = storeHelper;
		}

		public PriceRangeFilterDto GetPriceRangeFilterDto(int categoryId, int manufacturerId, int vendorId)
		{
			var priceRangeFilterDto = new PriceRangeFilterDto();
			if (categoryId > 0)
			{
				SetDiscountAmountPercentageForCategory(categoryId, priceRangeFilterDto);
			}
			switch (_workContext.TaxDisplayType)
			{
			case TaxDisplayType.ExcludingTax:
				priceRangeFilterDto.TaxDisplayTypeIncludingTax = false;
				break;
			case TaxDisplayType.IncludingTax:
				priceRangeFilterDto.TaxDisplayTypeIncludingTax = true;
				break;
			}
			priceRangeFilterDto.TaxPriceIncludeTax = _taxSettings.PricesIncludeTax;
			SetMinMaxPrices(categoryId, manufacturerId, vendorId, priceRangeFilterDto);
			priceRangeFilterDto.MinPrice = _currencyService.ConvertFromPrimaryStoreCurrency(priceRangeFilterDto.MinPrice, _workContext.WorkingCurrency);
			priceRangeFilterDto.MaxPrice = _currencyService.ConvertFromPrimaryStoreCurrency(priceRangeFilterDto.MaxPrice, _workContext.WorkingCurrency);
			return priceRangeFilterDto;
		}

		public decimal CalculateBasePrice(decimal price, PriceRangeFilterDto priceRangeModel, bool isFromPrice)
		{
			price = GetPriceWithoutDiscount(price, priceRangeModel);
			price = GetPriceWithoutTax(price, priceRangeModel);
			price = _currencyService.ConvertToPrimaryStoreCurrency(price, _workContext.WorkingCurrency);
			price = Math.Round(price, 2);
			return price;
		}

		private static decimal GetPriceWithoutTax(decimal price, PriceRangeFilterDto priceRangeModel)
		{
			var num = price;
			if (priceRangeModel.TaxPriceIncludeTax)
			{
				if (!priceRangeModel.TaxDisplayTypeIncludingTax)
				{
					num = CalculatePriceWithoutTax(price, priceRangeModel.TaxRatePercentage, increase: false);
				}
			}
			else if (priceRangeModel.TaxDisplayTypeIncludingTax)
			{
				num = CalculatePriceWithoutTax(num, priceRangeModel.TaxRatePercentage, increase: true);
			}
			return num;
		}

		private static decimal CalculatePriceWithoutTax(decimal price, decimal percent, bool increase)
		{
            if (percent == 0m)
			{
				return price;
			}
			if (increase)
			{
				return price / (1m + percent / 100m);
			}
			return price * (100m + percent) / (100m + percent - percent);
		}

		private static decimal GetPriceWithoutDiscount(decimal price, PriceRangeFilterDto priceRangeModel)
		{
			if (priceRangeModel.MaxDiscountAmount == 0m && priceRangeModel.MaxDiscountPercentage == 0m)
			{
				return price;
			}
			var result = price;
			var num = (decimal)((float)price * 100f / (100f - (float)priceRangeModel.MaxDiscountPercentage));
			var num2 = price + priceRangeModel.MaxDiscountAmount;
			if (num > 0m && num > num2)
			{
				result = num;
			}
			else if (num2 > 0m)
			{
				result = num2;
			}
			return result;
		}

		private void SetMinMaxPrices(int categoryId, int manufacturerId, int vendorId, PriceRangeFilterDto priceRangeModel)
		{
			var source = PrepareMinMaxPriceProductVariantQuery(categoryId, manufacturerId, vendorId);
			if (source.Any())
			{
				var minPrice = source.Min((Expression<Func<Product, decimal?>>)((Product pv) => pv.Price));
				var maxPrice = source.Max((Expression<Func<Product, decimal?>>)((Product pv) => pv.Price));
				var productVariant = source.Take(1).FirstOrDefault();
				SetMinPrice(priceRangeModel, minPrice);
				SetMaxPrice(priceRangeModel, maxPrice);
				priceRangeModel.TaxRatePercentage = GetTaxRatePercentage(productVariant);
				if (priceRangeModel.TaxRatePercentage > 0m)
				{
					SetTaxForMinMaxPrice(priceRangeModel);
				}
			}
		}

		private void SetTaxForMinMaxPrice(PriceRangeFilterDto priceRangeModel)
		{
			if (priceRangeModel.TaxPriceIncludeTax)
			{
				if (!priceRangeModel.TaxDisplayTypeIncludingTax)
				{
					priceRangeModel.MinPrice = CalculatePrice(priceRangeModel.MinPrice, priceRangeModel.TaxRatePercentage, increase: false);
					priceRangeModel.MaxPrice = CalculatePrice(priceRangeModel.MaxPrice, priceRangeModel.TaxRatePercentage, increase: false);
				}
			}
			else if (priceRangeModel.TaxDisplayTypeIncludingTax)
			{
				priceRangeModel.MinPrice = CalculatePrice(priceRangeModel.MinPrice, priceRangeModel.TaxRatePercentage, increase: true);
				priceRangeModel.MaxPrice = CalculatePrice(priceRangeModel.MaxPrice, priceRangeModel.TaxRatePercentage, increase: true);
			}
		}

		private decimal CalculatePrice(decimal? nulablePrice, decimal percent, bool increase)
		{
			var num = default(decimal);
			if (nulablePrice.HasValue)
			{
				num = nulablePrice.Value;
			}
			if (percent == 0m)
			{
				return num;
			}
			if (increase)
			{
				return num * (1m + percent / 100m);
			}
			return num - num / (100m + percent) * percent;
		}

		private decimal GetTaxRatePercentage(Product productVariant)
		{
			var currentCustomer = _workContext.CurrentCustomer;
			if (currentCustomer != null)
			{
				if (currentCustomer.IsTaxExempt)
				{
					return 0m;
				}
				if (currentCustomer.CustomerRoles.Where((CustomerRole cr) => cr.Active).Any((CustomerRole cr) => cr.TaxExempt))
				{
					return 0m;
				}
			}
			return _taxServiceNopAjaxFilters.GetTaxRateForProduct(productVariant, 0, currentCustomer);
		}

		private void SetMaxPrice(PriceRangeFilterDto priceRangeModel, decimal? maxPrice)
		{
			if (!maxPrice.HasValue)
			{
				priceRangeModel.MaxPrice = 0m;
				return;
			}
			maxPrice = ApplyDiscount(maxPrice.Value, priceRangeModel);
			priceRangeModel.MaxPrice = maxPrice.Value;
		}

		private void SetMinPrice(PriceRangeFilterDto priceRangeModel, decimal? minPrice)
		{
			if (!minPrice.HasValue)
			{
				priceRangeModel.MinPrice = 0m;
				return;
			}
			minPrice = ApplyDiscount(minPrice.Value, priceRangeModel);
			priceRangeModel.MinPrice = minPrice.Value;
		}

		private decimal ApplyDiscount(decimal price, PriceRangeFilterDto priceRangeModel)
		{
			var result = default(decimal);
			var num = price - (decimal)((float)price * (float)priceRangeModel.MaxDiscountPercentage / 100f);
			var num2 = price - priceRangeModel.MaxDiscountAmount;
			if (num > 0m && num < num2)
			{
				return num;
			}
			if (num2 > 0m)
			{
				return num2;
			}
			return result;
		}

		private IQueryable<Product> PrepareMinMaxPriceProductVariantQuery(int categoryId, int manufacturerId, int vendorId)
		{
			var utcNow = DateTime.UtcNow;
			var availableProductsForCurrentCustomer = _aclHelper.GetAvailableProductsForCurrentCustomer();
			availableProductsForCurrentCustomer = _storeHelper.GetProductsForCurrentStore(availableProductsForCurrentCustomer);
			if (manufacturerId > 0)
			{
				return PrepareMinMaxPriceProductVariantForManufacturerQuery(manufacturerId, availableProductsForCurrentCustomer, utcNow);
			}
			if (vendorId > 0)
			{
				return PrepareMinMaxPriceProductVariantForVendorQuery(vendorId, availableProductsForCurrentCustomer, utcNow);
			}
			return PrepareMinMaxPriceProductVariantForCategoryQuery(categoryId, availableProductsForCurrentCustomer, utcNow);
		}

		private IQueryable<Product> PrepareMinMaxPriceProductVariantForManufacturerQuery(int manufacturerId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
		{
			var includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
			var groupProductIds = _productServiceNopAjaxFilters.GetAllGroupProductIdsInManufacturer(manufacturerId);
			return from p in availableProductsQuery
				join pm in _productManufacturerRepository.TableNoTracking on p.Id equals pm.ProductId into p_pm
				from pm in p_pm.DefaultIfEmpty()
				where ((pm != null && pm.ManufacturerId == manufacturerId && (pm.IsFeaturedProduct == includeFeaturedProducts || !pm.IsFeaturedProduct)) || (pm == null && groupProductIds.Contains(p.ParentGroupedProductId))) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
				select p;
		}

		private IQueryable<Product> PrepareMinMaxPriceProductVariantForVendorQuery(int vendorId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
		{
			var groupProductIds = _productServiceNopAjaxFilters.GetAllGroupProductIdsInVendor(vendorId);
			return from p in availableProductsQuery
				join v in _vendorRepository.TableNoTracking on p.VendorId equals v.Id into p_pv
				from v in p_pv.DefaultIfEmpty()
				where ((v != null && v.Id == vendorId) || (v == null && groupProductIds.Contains(p.ParentGroupedProductId))) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
				select p;
		}

		private IQueryable<Product> PrepareMinMaxPriceProductVariantForCategoryQuery(int categoryId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
		{
			var showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
			var includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
			var categoryIds = new List<int>
			{
				categoryId
			};
			if (showProductsFromSubcategories)
			{
				var categoryIdsByParentCategory = _categoryServiceSpikes.GetCategoryIdsByParentCategory(categoryId);
				categoryIds.AddRange(categoryIdsByParentCategory);
			}
			var groupProductIds = _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategories(categoryIds);
			return from p in availableProductsQuery
				join pc in _productCategoryRepository.TableNoTracking on p.Id equals pc.ProductId into p_pc
				from pc in p_pc.DefaultIfEmpty()
				where ((pc != null && categoryIds.Contains(pc.CategoryId) && p.ProductTypeId != 10 && (pc.IsFeaturedProduct == includeFeaturedProducts || !pc.IsFeaturedProduct)) || (pc == null && groupProductIds.Contains(p.ParentGroupedProductId))) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
				select p;
		}

		private void SetDiscountAmountPercentageForCategory(int categoryId, PriceRangeFilterDto priceRangeModel)
		{
			var allowedDiscountsForCategory = GetAllowedDiscountsForCategory(categoryId);
			var num = default(decimal);
			var num2 = default(decimal);
			foreach (var item in allowedDiscountsForCategory)
			{
				if (item.DiscountAmount > num)
				{
					num = item.DiscountAmount;
				}
				if (item.DiscountPercentage > num2)
				{
					num2 = item.DiscountPercentage;
				}
			}
			priceRangeModel.MaxDiscountAmount = num;
			priceRangeModel.MaxDiscountPercentage = num2;
		}

		private IEnumerable<Discount> GetAllowedDiscountsForCategory(int categoryId)
		{
			return new List<Discount>();
		}
	}
}
