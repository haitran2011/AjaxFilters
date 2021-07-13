using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class SpecificationFilterGroup
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

		public int DisplayOrder
		{
			get;
			set;
		}

		public bool IsMain
		{
			get;
			set;
		}

		public IList<SpecificationFilterItem> FilterItems
		{
			get;
			set;
		}

		public SpecificationFilterGroup()
		{
			FilterItems = new List<SpecificationFilterItem>();
		}
	}
}
