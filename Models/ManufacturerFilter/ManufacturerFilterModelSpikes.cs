using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class ManufacturerFilterModel7Spikes
	{
		public int CategoryId
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

		public IList<ManufacturerFilterItem> ManufacturerFilterItems
		{
			get;
			set;
		}

		public ManufacturerFilterModel7Spikes()
		{
			ManufacturerFilterItems = new List<ManufacturerFilterItem>();
		}
	}
}
