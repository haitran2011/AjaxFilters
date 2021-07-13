using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Tax;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
	public class TaxServiceNopAjaxFilters : TaxService, ITaxServiceNopAjaxFilters
	{
		public TaxServiceNopAjaxFilters(AddressSettings addressSettings, CustomerSettings customerSettings, IAddressService addressService, ICountryService countryService, IGenericAttributeService genericAttributeService, IGeoLookupService geoLookupService, ILogger logger, IStateProvinceService stateProvinceService, IStaticCacheManager cacheManager, IStoreContext storeContext, ITaxPluginManager taxPluginManager, IWebHelper webHelper, IWorkContext workContext, ShippingSettings shippingSettings, TaxSettings taxSettings)
			: base(addressSettings, customerSettings, addressService, countryService, genericAttributeService, geoLookupService, logger, stateProvinceService, cacheManager, storeContext, taxPluginManager, webHelper, workContext, shippingSettings, taxSettings)
		{
		}

		public decimal GetTaxRateForProduct(Product product, int taxCategoryId, Customer customer)
		{
			GetTaxRate(product, taxCategoryId, customer, product.Price, out decimal taxRate, out bool _);
			return taxRate;
		}
	}
}
