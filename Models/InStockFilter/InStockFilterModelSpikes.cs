using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class InStockFilterModelSpikes
	{
		public int Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}
        public int FilterCount
        {
            get;
            set;
        }
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

		public FilterItemState FilterItemState
		{
			get;
			set;
		}
	}
}
