using Spikes.Nop.Framework.AutoMapper;
using Spikes.Nop.Plugins.AjaxFilters.Models;
using Spikes.Nop.Services.Catalog.DTO;

namespace Spikes.Nop.Plugins.AjaxFilters.Extensions
{
    public static class MappingExtensions
	{
		public static SpecificationFilterDTO ToDTO(this SpecificationFilterGroup specificationFilterGroup)
		{
			return specificationFilterGroup.MapTo<SpecificationFilterGroup, SpecificationFilterDTO>();
		}

		public static SpecificationFilterModelDTO ToDTO(this SpecificationFilterModel7Spikes specificationFilterModel7Spikes)
		{
			return specificationFilterModel7Spikes.MapTo<SpecificationFilterModel7Spikes, SpecificationFilterModelDTO>();
		}

		public static AttributeFilterDTO ToDTO(this AttributeFilterGroup attributeFilterGroup)
		{
			return attributeFilterGroup.MapTo<AttributeFilterGroup, AttributeFilterDTO>();
		}

		public static AttributeFilterModelDTO ToDTO(this AttributeFilterModel7Spikes attributeFilterModel7Spikes)
		{
			return attributeFilterModel7Spikes.MapTo<AttributeFilterModel7Spikes, AttributeFilterModelDTO>();
		}

		public static ManufacturerFilterModelDTO ToDTO(this ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes)
		{
			return manufacturerFilterModel7Spikes.MapTo<ManufacturerFilterModel7Spikes, ManufacturerFilterModelDTO>();
		}

        public static CategoryFilterModelDTO ToDTO(this CategoryFilterModel7Spikes categoryFilterModel7Spikes)
        {
            return categoryFilterModel7Spikes.MapTo<CategoryFilterModel7Spikes, CategoryFilterModelDTO>();
        }

        public static VendorFilterModelDTO ToDTO(this VendorFilterModel7Spikes vendorFilterModel7Spikes)
		{
			return vendorFilterModel7Spikes.MapTo<VendorFilterModel7Spikes, VendorFilterModelDTO>();
		}

    }
}
