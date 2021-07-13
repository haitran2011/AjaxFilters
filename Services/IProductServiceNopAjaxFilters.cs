using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Spikes.Nop.Plugins.AjaxFilters.Models;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
    public interface IProductServiceNopAjaxFilters
	{
		IList<int> GetAllGroupProductIdsInCategories(List<int> categoriesIds);
		IList<int> GetAllGroupProductIdsInCategory(int categoryId);
		IList<int> GetAllGroupProductIdsInManufacturer(int manufacturerId);
		IList<int> GetAllGroupProductIdsInVendor(int vendorId);
		bool HasProductsOnSale(int categoryId, int manufacturerId, int vendorId);
		bool HasProductsInStock(int categoryId, int manufacturerId, int vendorId);
        (IPagedList<Product> productsPagedList, IList<(int Id, int FilterCount)> specificationAttributeFilters, IList<(int Id, int FilterCount)> productAttributeFilters, IList<(int Id, int FilterCount)> categoryFilters, IList<(int Id, int FilterCount)> manufacturerFilters, IList<(int Id, int FilterCount)> vendorFilters, IList<(int Id, int FilterCount)> ratingFilters, int productsOnSale, int productsInStock) SearchProducts(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int vendorId = 0,
            int storeId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = false,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            IList<int> filteredProductVariantAttributes = null,
            IList<int> filteredManufacturers = null,
            IList<int> filteredVendors = null,
            int filteredRating = 0,
            bool onSale = false,
            bool inStock = false,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool loadAvailableFilters = false,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            bool loadFilterableProductVariantAttributeIds = false,
            bool loadFilterableCategoryIds = false,
            bool loadFilterableManufacturerIds = false,
            bool loadFilterableVendorIds = false,
            bool loadFilterableProductTagIds = false,
            bool loadFilterableProductReviewIds = false,
            bool loadFilterableOnSale = false,
            bool loadFilterableInStock = false);

        (IPagedList<Product> productsPagedList, IList<SpecificationFilterGroup> specificationAttributeFilters, IList<AttributeFilterGroup> productAttributeFilters, IList<CategoryFilterItem> categoryFilters, IList<ManufacturerFilterItem> manufacturerFilters, IList<VendorFilterItem> vendorFilters, IList<RatingFilterItem> ratingFilters, int productsOnSale, int productsInStock) SearchProducts(
            ref decimal? priceMin,
            ref decimal? priceMax,
            bool loadFilterableOnSale = false,
            bool loadFilterableInStock = false,
            bool loadFilterablePriceRange = false,
            bool loadFilterableSpecificationAttributeOptions = false,
            bool loadFilterableProductVariantAttributes = false,
            bool loadFilterableCategories = false,
            bool loadFilterableManufacturers = false,
            bool loadFilterableVendors = false,
            bool loadFilterableProductTags = false,
            bool loadFilterableProductReviews = false,
            int? pageIndex = null,
            int? pageSize  = null,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null,
            IList<string> filterTypes = null);
    }
}
