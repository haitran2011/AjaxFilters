using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class SpecificationFilterItem
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

        public int DisplayOrder
		{
			get;
			set;
		}

		public FilterItemState FilterItemState
		{
			get;
			set;
		}

		public string ColorSquaresRgb
		{
			get;
			set;
		}
	}
}
