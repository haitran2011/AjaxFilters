using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using System.Globalization;

namespace Spikes.Nop.Plugins.AjaxFilters.Helpers
{
	public static class PriceRangeHelper
	{
		public static PriceRange GetSelectedPriceRange(decimal priceRangeTollerance = 0m)
		{
			string text = EngineContext.Current.Resolve<IWebHelper>().QueryString<string>("price");
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			string[] array = text.Trim().Split('-');
			if (array.Length == 2)
			{
				CultureInfo provider = CultureInfo.CreateSpecificCulture("en-us");
				decimal.TryParse(array[0].Trim(), NumberStyles.Number, provider, out decimal result);
				decimal.TryParse(array[1].Trim(), NumberStyles.Number, provider, out decimal result2);
				if (result != 0m || result2 != 0m)
				{
					return new PriceRange
					{
						From = result - priceRangeTollerance,
						To = result2 + priceRangeTollerance
					};
				}
			}
			return null;
		}
	}
}
