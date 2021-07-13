using System.Collections.Generic;
using Spikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
    public class AttributeFilterItem
	{
		public int ValueId
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public int AttributeId
		{
			get;
			set;
		}
        public IList<int> ProductVariantAttributeIds
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

		public string ImageSquaresUrl
		{
			get;
			set;
		}

        public int ImageSquaresPictureId
        {
            get;
            set;
        }

        public int FilterCount
        {
            get;
            set;
        }

        public AttributeFilterItem()
		{
			ProductVariantAttributeIds = new List<int>();
		}
	}
}
