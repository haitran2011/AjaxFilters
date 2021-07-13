using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
    public interface IPriceCalculationServiceNopAjaxFilters
	{
		PriceRangeFilterDto GetPriceRangeFilterDto(int categoryId, int manufacturerId, int vendorId);

		decimal CalculateBasePrice(decimal price, PriceRangeFilterDto priceRangeModel, bool isFromPrice);
	}
}
