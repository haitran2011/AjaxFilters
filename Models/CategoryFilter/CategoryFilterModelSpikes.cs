using System.Collections.Generic;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
    public class CategoryFilterModel7Spikes
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

     

        public IList<CategoryFilterItem> CategoryFilterItems
        {
            get;
            set;
        }

        public CategoryFilterModel7Spikes()
        {
            CategoryFilterItems = new List<CategoryFilterItem>();
        }
    }
}