using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Spikes.Nop.Services.Catalog;
using Spikes.Nop.Services.Catalog.DTO;
using Spikes.Nop.Services.Helpers;

namespace Spikes.Nop.Plugins.AjaxFilters.Helpers
{
    public class ManufacturerOptionsHelper : IManufacturerOptionsHelper
	{
		private IManufacturerServiceSpikes ManufacturerServiceSpikes
		{
			get;
			set;
		}

		private List<int> AvailableManufacturerIds
		{
			get;
			set;
		}

		public ManufacturerOptionsHelper(IManufacturerServiceSpikes manufacturerServiceSpikes)
		{
			ManufacturerServiceSpikes = manufacturerServiceSpikes;
			AvailableManufacturerIds = new List<int>();
		}

		public IQueryable<Product> GetProductsForManufacturerFiltersAndDetermineAvailableManufacturerOptionsForLaterRetrieval(IQueryable<Product> query, ManufacturerFilterModelDTO manufacturerFilterModelDTO)
		{
			if (query.Any())
			{
				IList<int> collection = (from x in ManufacturerServiceSpikes.GetManufacturersByProductIds(query.Select((Product x) => x.Id).ToList())
					select x.Id).ToList();
				AvailableManufacturerIds.AddRange(collection);
				if (manufacturerFilterModelDTO != null && manufacturerFilterModelDTO.SelectedFilterIds != null && manufacturerFilterModelDTO.SelectedFilterIds.Count > 0)
				{
					List<int> selectedManufacturerIds = manufacturerFilterModelDTO.SelectedFilterIds.ToList();
					query = from p in query
						from pm in from pm in p.ProductManufacturers
							where selectedManufacturerIds.Contains(pm.ManufacturerId)
							select pm
						select (p);
				}
			}
			return query;
		}

		public IList<int> GetAvailableManufacturerIds()
		{
			return AvailableManufacturerIds;
		}

		public bool DetermineWhetherPotentialProductMeetsManufacturerFilters(Product product, ManufacturerFilterModelDTO manufacturerFilterModelDTO)
		{
			if (manufacturerFilterModelDTO != null && manufacturerFilterModelDTO.SelectedFilterIds.Count != 0)
			{
				return product.ProductManufacturers.Select((ProductManufacturer x) => x.ManufacturerId).Intersect(manufacturerFilterModelDTO.SelectedFilterIds).Any();
			}
			return true;
		}
	}
}
