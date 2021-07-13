using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class AttributeFilterModelSpikes
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

		public IList<AttributeFilterGroup> AttributeFilterGroups
		{
			get;
			set;
		}

		public AttributeFilterModelSpikes()
		{
			AttributeFilterGroups = new List<AttributeFilterGroup>();
		}
	}
}
