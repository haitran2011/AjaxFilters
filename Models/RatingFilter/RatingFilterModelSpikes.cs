using System.Collections.Generic;
using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class RatingFilterModelSpikes
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

        public IList<RatingFilterItem> RatingFilterItems
        {
            get;
            set;
        }
        public int MinRating
        {
            get;
            set;
        }
        public RatingFilterModelSpikes()
        {
            RatingFilterItems = new List<RatingFilterItem>();
        }
    }
}
