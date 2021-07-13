using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class VendorFilterItem
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

        public FilterItemState FilterItemState
		{
			get;
			set;
		}
	}
}
