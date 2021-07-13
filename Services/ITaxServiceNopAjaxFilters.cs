using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
	public interface ITaxServiceNopAjaxFilters
	{
		decimal GetTaxRateForProduct(Product product, int taxCategoryId, Customer customer);
	}
}
