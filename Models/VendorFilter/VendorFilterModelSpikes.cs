using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class VendorFilterModelSpikes
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

		public IList<VendorFilterItem> VendorFilterItems
		{
			get;
			set;
		}

		public VendorFilterModelSpikes()
		{
			VendorFilterItems = new List<VendorFilterItem>();
		}
	}
}
