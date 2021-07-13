using System.Collections.Generic;
using System.Linq;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
    public class ProductsModel : BaseNopModel
    {
		public IEnumerable<ProductOverviewModel> Products
		{
			get;
			set;
		}

		public IList<int> ProductIdsToDetermineFilters
		{
			get;
			set;
		}

		public string SpecificationFilterModelSpikesJson
		{
			get;
			set;
		}

		public string AttributeFilterModelSpikesJson
		{
			get;
			set;
		}

        public string CategoryFilterModelSpikesJson
        {
            get;
            set;
        }
        public string ManufacturerFilterModelSpikesJson
		{
			get;
			set;
		}

		public string VendorFilterModelSpikesJson
		{
			get;
			set;
		}

        public string RatingFilterModelSpikesJson
        {
            get;
            set;
        }

        public string OnSaleFilterModelSpikesJson
		{
			get;
			set;
		}

		public string InStockFilterModelSpikesJson
		{
			get;
			set;
		}

		public string ViewMode
		{
			get;
			set;
		}

		public CatalogPagingFilteringModel PagingFilteringContext
		{
			get;
			set;
		}

		public NopAjaxFiltersSettingsModel NopAjaxFiltersSettingsModel
		{
			get;
			set;
		}

		public string HashQuery
		{
			get;
			set;
		}

		public string PriceRangeFromJson
		{
			get;
			set;
		}

		public string PriceRangeToJson
		{
			get;
			set;
		}

		public string CurrentPageSizeJson
		{
			get;
			set;
		}

		public string CurrentViewModeJson
		{
			get;
			set;
		}

		public string CurrentOrderByJson
		{
			get;
			set;
		}

		public string CurrentPageNumberJson
		{
			get;
			set;
		}

		public int TotalCount
		{
			get;
			set;
		}

		public ProductsModel()
		{
			Products = new List<ProductOverviewModel>();
			PagingFilteringContext = new CatalogPagingFilteringModel();
		}

		public IList<ProductOverviewModel> GetRollOverModel()
		{
			return Products.ToList();
		}
	}
}
