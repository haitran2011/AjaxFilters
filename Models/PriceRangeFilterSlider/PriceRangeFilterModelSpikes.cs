using Nop.Core.Domain.Catalog;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class PriceRangeFilterModelSpikes
	{
		public int CategoryId
		{
			get;
			set;
		}

		public int ManufacturerId
		{
			get;
			set;
		}

		public int VendorId
		{
			get;
			set;
		}

        public int ProductTagId
        {
            get;
            set;
        }

        public int Priority
		{
			get;
			set;
		}

		public PriceRange SelectedPriceRange
		{
			get;
			set;
		}

		public decimal MinPrice
		{
			get;
			set;
		}

		public decimal MaxPrice
		{
			get;
			set;
		}

		public string CurrencySymbol
		{
			get;
			set;
		}

		public string Formatting
		{
			get;
			set;
		}

		public string MinPriceFormatted
		{
			get;
			set;
		}

		public string MaxPriceFormatted
		{
			get;
			set;
		}
	}
}
