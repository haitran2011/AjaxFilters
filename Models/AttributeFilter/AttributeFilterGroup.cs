using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class AttributeFilterGroup
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

		public bool IsMain
		{
			get;
			set;
		}
        public IList<AttributeFilterItem> FilterItems
		{
			get;
			set;
		}

		public AttributeFilterGroup()
		{
			FilterItems = new List<AttributeFilterItem>();
		}
	}
}
