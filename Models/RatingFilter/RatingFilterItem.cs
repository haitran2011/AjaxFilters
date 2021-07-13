using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class RatingFilterItem
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

        public int RatingSum
        {
            get;
            set;
        }
        public int TotalReviews
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
