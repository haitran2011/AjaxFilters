using Spikes.Nop.Plugins.AjaxFilters.Helpers;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
	public class NopAjaxFiltersModel
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

        public string DefaultViewMode
		{
			get;
			set;
		}

		public string AvailableSortOptionsJson
		{
			get;
			set;
		}

		public string AvailableViewModesJson
		{
			get;
			set;
		}

		public string AvailablePageSizesJson
		{
			get;
			set;
		}

		public SearchQueryStringParameters SearchQueryStringParameters
		{
			get;
			set;
		}

        public NopAjaxFiltersModel()
		{
			SearchQueryStringParameters = new SearchQueryStringParameters();
		}
	}
}
