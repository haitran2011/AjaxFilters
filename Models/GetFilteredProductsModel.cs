namespace Spikes.Nop.Plugins.AjaxFilters.Models
{
    public class GetFilteredProductsModel
	{
        public GetFilteredProductsModel()
        {
            PriceRangeFilterModelSpikes = new PriceRangeFilterModelSpikes();
            SpecificationFiltersModelSpikes = new SpecificationFilterModelSpikes();
            AttributeFiltersModelSpikes = new AttributeFilterModelSpikes();
            CategoryFiltersModelSpikes = new CategoryFilterModelSpikes();
            RatingFiltersModelSpikes = new RatingFilterModelSpikes();
            ManufacturerFiltersModelSpikes = new ManufacturerFilterModelSpikes();
            VendorFiltersModelSpikes = new VendorFilterModelSpikes();
            OnSaleFilterModel = new OnSaleFilterModelSpikes();
            InStockFilterModel = new InStockFilterModelSpikes();
        }
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

        public PriceRangeFilterModelSpikes PriceRangeFilterModelSpikes
		{
			get;
			set;
		}

		public SpecificationFilterModelSpikes SpecificationFiltersModelSpikes
		{
			get;
			set;
		}

		public AttributeFilterModelSpikes AttributeFiltersModelSpikes
		{
			get;
			set;
		}

		public ManufacturerFilterModelSpikes ManufacturerFiltersModelSpikes
		{
			get;
			set;
		}

        public CategoryFilterModelSpikes CategoryFiltersModelSpikes
        {
            get;
            set;
        }

        public VendorFilterModelSpikes VendorFiltersModelSpikes
		{
			get;
			set;
		}

        public RatingFilterModelSpikes RatingFiltersModelSpikes
        {
            get;
            set;
        }

        public OnSaleFilterModelSpikes OnSaleFilterModel
		{
			get;
			set;
		}

		public InStockFilterModelSpikes InStockFilterModel
		{
			get;
			set;
		}

		public string QueryString
		{
			get;
			set;
		}

		public bool ShouldNotStartFromFirstPage
		{
			get;
			set;
		}

		public string Keyword
		{
			get;
			set;
		}

        public int SearchCategoryId
		{
			get;
			set;
		}

		public int SearchManufacturerId
		{
			get;
			set;
		}

		public int SearchVendorId
		{
			get;
			set;
		}

		public decimal? PriceFrom
		{
			get;
			set;
		}

		public decimal? PriceTo
		{
			get;
			set;
		}

		public bool IncludeSubcategories
		{
			get;
			set;
		}

		public bool SearchInProductDescriptions
		{
			get;
			set;
		}

		public bool AdvancedSearch
		{
			get;
			set;
		}

		public bool IsOnSearchPage
		{
			get;
			set;
		}

		public int? Orderby
		{
			get;
			set;
		}

		public string Viewmode
		{
			get;
			set;
		}

		public int? PageNumber
		{
			get;
			set;
		}

		public int Pagesize
		{
			get;
			set;
		}
	}
}
