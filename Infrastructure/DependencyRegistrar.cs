using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using AutoMapper;
using Spikes.Nop.Core.Helpers;
using Spikes.Nop.Framework.AutoMapper;
using Spikes.Nop.Framework.DependancyRegistrar;
using Spikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;
using Spikes.Nop.Plugins.AjaxFilters.Domain;
using Spikes.Nop.Plugins.AjaxFilters.Extensions;
using Spikes.Nop.Plugins.AjaxFilters.Factories;
using Spikes.Nop.Plugins.AjaxFilters.Helpers;
using Spikes.Nop.Plugins.AjaxFilters.Models;
using Spikes.Nop.Plugins.AjaxFilters.QueryStringManipulation;
using Spikes.Nop.Plugins.AjaxFilters.Services;
using Spikes.Nop.Services.Catalog;
using Spikes.Nop.Services.Catalog.DTO;
using Spikes.Nop.Services.Helpers;

namespace Spikes.Nop.Plugins.AjaxFilters.Infrastructure
{
    public class DependencyRegistrar : BaseDependancyRegistrarSpikes
	{
		protected override void CreateModelMappings()
		{
			AutoMapperConfigurationSpikes.MapperConfigurationExpression.CreateMap<SpecificationFilterModelSpikes, SpecificationFilterModelDTO>().ForMember((SpecificationFilterModelDTO dest) => dest.SpecificationFilterDTOs, delegate(IMemberConfigurationExpression<SpecificationFilterModelSpikes, SpecificationFilterModelDTO, IList<SpecificationFilterDTO>> opt)
			{
				opt.MapFrom((Expression<Func<SpecificationFilterModelSpikes, IList<SpecificationFilterDTO>>>)((SpecificationFilterModelSpikes x) => x.SpecificationFilterGroups.Select((SpecificationFilterGroup group) => group.ToDTO()).ToList()));
			});
			AutoMapperConfigurationSpikes.MapperConfigurationExpression.CreateMap<SpecificationFilterGroup, SpecificationFilterDTO>().ForMember((SpecificationFilterDTO dest) => dest.Id, delegate(IMemberConfigurationExpression<SpecificationFilterGroup, SpecificationFilterDTO, int> opt)
			{
				opt.MapFrom((SpecificationFilterGroup x) => x.Id);
			}).ForMember((SpecificationFilterDTO dest) => dest.IsMain, delegate(IMemberConfigurationExpression<SpecificationFilterGroup, SpecificationFilterDTO, bool> opt)
			{
				opt.MapFrom((SpecificationFilterGroup x) => x.IsMain);
			})
				.ForMember((SpecificationFilterDTO dest) => dest.SelectedFilterIds, delegate(IMemberConfigurationExpression<SpecificationFilterGroup, SpecificationFilterDTO, IList<int>> opt)
				{
					opt.MapFrom((Expression<Func<SpecificationFilterGroup, IList<int>>>)((SpecificationFilterGroup x) => (from filterItem in x.FilterItems
						where (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2
						select filterItem into item
						select item.Id).ToList()));
				});
			AutoMapperConfigurationSpikes.MapperConfigurationExpression.CreateMap<AttributeFilterModelSpikes, AttributeFilterModelDTO>().ForMember((AttributeFilterModelDTO dest) => dest.AttributeFilterDTOs, delegate(IMemberConfigurationExpression<AttributeFilterModelSpikes, AttributeFilterModelDTO, IList<AttributeFilterDTO>> opt)
			{
				opt.MapFrom((Expression<Func<AttributeFilterModelSpikes, IList<AttributeFilterDTO>>>)((AttributeFilterModelSpikes x) => x.AttributeFilterGroups.Select((AttributeFilterGroup group) => group.ToDTO()).ToList()));
			});
			AutoMapperConfigurationSpikes.MapperConfigurationExpression.CreateMap<AttributeFilterGroup, AttributeFilterDTO>().ForMember((AttributeFilterDTO dest) => dest.Id, delegate(IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, int> opt)
			{
				opt.MapFrom((AttributeFilterGroup x) => x.Id);
			}).ForMember((AttributeFilterDTO dest) => dest.IsMain, delegate(IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, bool> opt)
			{
				opt.MapFrom((AttributeFilterGroup x) => x.IsMain);
			})
				.ForMember((AttributeFilterDTO dest) => dest.AllProductVariantIds, delegate(IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, IList<int>> opt)
				{
					opt.MapFrom((Expression<Func<AttributeFilterGroup, IList<int>>>)((AttributeFilterGroup x) => x.FilterItems.SelectMany((AttributeFilterItem item) => item.ProductVariantAttributeIds).ToList()));
				})
				.ForMember((AttributeFilterDTO dest) => dest.SelectedProductVariantIds, delegate(IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, IList<int>> opt)
				{
					opt.MapFrom((Expression<Func<AttributeFilterGroup, IList<int>>>)((AttributeFilterGroup x) => x.FilterItems.Where((AttributeFilterItem filterItem) => (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2).SelectMany((AttributeFilterItem item) => item.ProductVariantAttributeIds).ToList()));
				});
			AutoMapperConfigurationSpikes.MapperConfigurationExpression.CreateMap<ManufacturerFilterModelSpikes, ManufacturerFilterModelDTO>().ForMember((ManufacturerFilterModelDTO dest) => dest.SelectedFilterIds, delegate(IMemberConfigurationExpression<ManufacturerFilterModelSpikes, ManufacturerFilterModelDTO, IList<int>> opt)
			{
				opt.MapFrom((ManufacturerFilterModelSpikes x) => (from filterItem in x.ManufacturerFilterItems
					where (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2
					select filterItem into item
					select item.Id).ToList());
			});
            AutoMapperConfigurationSpikes.MapperConfigurationExpression.CreateMap<CategoryFilterModelSpikes, CategoryFilterModelDTO>().ForMember((CategoryFilterModelDTO dest) => dest.SelectedFilterIds, delegate (IMemberConfigurationExpression<CategoryFilterModelSpikes, CategoryFilterModelDTO, IList<int>> opt)
            {
                opt.MapFrom((CategoryFilterModelSpikes x) => (from filterItem in x.CategoryFilterItems
                                                                   where (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2
                                                                   select filterItem into item
                                                                   select item.Id).ToList());
            });
            AutoMapperConfigurationSpikes.MapperConfigurationExpression.CreateMap<VendorFilterModelSpikes, VendorFilterModelDTO>().ForMember((VendorFilterModelDTO dest) => dest.SelectedFilterIds, delegate(IMemberConfigurationExpression<VendorFilterModelSpikes, VendorFilterModelDTO, IList<int>> opt)
			{
				opt.MapFrom((VendorFilterModelSpikes x) => (from filterItem in x.VendorFilterItems
					where (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2
					select filterItem into item
					select item.Id).ToList());
			});
            CreateMvcModelMap<NopAjaxFiltersSettingsModel, NopAjaxFiltersSettings>();
		}

		protected override void RegisterPluginServices(ContainerBuilder builder)
		{
			builder.RegisterType<ProductServiceNopAjaxFilters>().As<IProductServiceNopAjaxFilters>().InstancePerLifetimeScope();
			builder.RegisterType<DiscountServiceSpikes>().As<IDiscountServiceSpikes>().InstancePerLifetimeScope();
			builder.RegisterType<QueryStringToModelUpdater>().As<IQueryStringToModelUpdater>().InstancePerLifetimeScope();
			builder.RegisterType<QueryStringBuilder>().As<IQueryStringBuilder>().InstancePerLifetimeScope();
			builder.RegisterType<ProductAttributeServiceAjaxFilters>().As<IProductAttributeServiceAjaxFilters>().InstancePerLifetimeScope();
			builder.RegisterType<PriceCalculationServiceNopAjaxFilters>().As<IPriceCalculationServiceNopAjaxFilters>().InstancePerLifetimeScope();
			builder.RegisterType<ManufacturerOptionsHelper>().As<IManufacturerOptionsHelper>().InstancePerDependency();
			//builder.RegisterType<FiltersAvailabilityHelper>().As<IFiltersAvailabilityHelper>().InstancePerDependency();
			builder.RegisterType<FiltersPageHelper>().As<IFiltersPageHelper>().InstancePerLifetimeScope();
			builder.RegisterType<TaxServiceNopAjaxFilters>().As<ITaxServiceNopAjaxFilters>().InstancePerLifetimeScope();
			builder.RegisterType<SearchQueryStringHelper>().As<ISearchQueryStringHelper>().InstancePerLifetimeScope();
			builder.RegisterType<AjaxFiltersDatabaseService>().As<IAjaxFiltersDatabaseService>().InstancePerLifetimeScope();
            builder.RegisterType<NopAjaxFilterModelFactory>().As<INopAjaxFilterModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ConvertToDictionaryHelper>().As<IConvertToDictionaryHelper>().InstancePerLifetimeScope();
		}
	}
}
