using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class SpecificationFilterModelSpikes
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

		public IList<SpecificationFilterGroup> SpecificationFilterGroups
		{
			get;
			set;
		}

		public SpecificationFilterModelSpikes()
		{
			SpecificationFilterGroups = new List<SpecificationFilterGroup>();
		}
	}
}
