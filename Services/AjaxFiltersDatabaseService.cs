using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Spikes.Nop.Plugins.AjaxFilters.Domain;
using System;
using System.Linq;
using System.Text;

namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
	public class AjaxFiltersDatabaseService : IAjaxFiltersDatabaseService
	{
		private readonly IDbContext _dbContext;

		private readonly IDataProvider _dataProvider;

		private readonly IEncryptionService _encryptionService;

		private readonly ISettingService _settingService;

		private string CreateSortFunctionSql = "CREATE FUNCTION [dbo].[seven_spikes_ajax_filters_product_sorting] (@OrderBy  INT, @CategoryIdsCount INT, @ManufacturerId INT, @ParentGroupedProductId INT)  RETURNS VARCHAR(250)  AS BEGIN      DECLARE @sql_orderby VARCHAR(250) = ''  \t  \t IF @OrderBy = 5   \t\tSET @sql_orderby = ' p.[Name] ASC'  \tELSE IF @OrderBy = 6   \t\tSET @sql_orderby = ' p.[Name] DESC'  \tELSE IF @OrderBy = 10   \t\tSET @sql_orderby = ' p.[Price] ASC'  \tELSE IF @OrderBy = 11   \t\tSET @sql_orderby = ' p.[Price] DESC'  \tELSE IF @OrderBy = 15   \t\tSET @sql_orderby = ' p.[CreatedOnUtc] DESC'  \tELSE   \tBEGIN  \t\t \t\tIF @CategoryIdsCount > 0 SET @sql_orderby = ' pcm.DisplayOrder ASC'  \t\t  \t\t \t\tIF @ManufacturerId > 0  \t\tBEGIN  \t\t\tIF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '  \t\t\tSET @sql_orderby = @sql_orderby + ' pmm.DisplayOrder ASC'  \t\tEND  \t\t  \t\t \t\tIF @ParentGroupedProductId > 0  \t\tBEGIN  \t\t\tIF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '  \t\t\tSET @sql_orderby = @sql_orderby + ' p.[DisplayOrder] ASC'  \t\tEND  \t\t  \t\t \t\tIF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '  \t\tSET @sql_orderby = @sql_orderby + ' p.[Name] ASC'  \tEND    \tRETURN @sql_orderby  END";

		private string DropSortFunctionSql = "IF EXISTS (      SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[seven_spikes_ajax_filters_product_sorting]')       AND xtype IN (N'FN', N'IF', N'TF')  )      DROP FUNCTION [dbo].[seven_spikes_ajax_filters_product_sorting]";

		private string DropProductSpecificationAttributeMappingPerformanceProblemIndex = "IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product_SpecificationAttribute_Mapping]') AND name = N'IX_PSAM_SpecificationAttributeOptionId_AllowFiltering') DROP INDEX [IX_PSAM_SpecificationAttributeOptionId_AllowFiltering] ON [dbo].[Product_SpecificationAttribute_Mapping]";

		private string CreateIndexOnProductSpecificationAttributeMappingTable = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product_SpecificationAttribute_Mapping]') AND name = N'IX_Product_SpecificationAttribute_Mapping_ProductId') CREATE NONCLUSTERED INDEX [IX_Product_SpecificationAttribute_Mapping_ProductId] ON [dbo].[Product_SpecificationAttribute_Mapping]( [ProductId] ASC )";

		private string CreateProductSpecificationAttributeMappingPerformanceProblemIndex = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product_SpecificationAttribute_Mapping]') AND name = N'IX_PSAM_SpecificationAttributeOptionId_AllowFiltering') CREATE NONCLUSTERED INDEX [IX_PSAM_SpecificationAttributeOptionId_AllowFiltering] ON [dbo].[Product_SpecificationAttribute_Mapping]( [SpecificationAttributeOptionId] ASC, [AllowFiltering] ASC) ON [PRIMARY]";

		private string DropProductCategoryMappingPerformanceProblemIndex = "IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product_Category_Mapping]') AND name = N'IX_Product_Category_Mapping_ProductId') DROP INDEX [IX_Product_Category_Mapping_ProductId] ON [dbo].[Product_Category_Mapping]";

		private string CreateProductCategoryMappingTablePerformanceProblemIndex = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product_Category_Mapping]') AND name = N'IX_Product_Category_Mapping_ProductId') CREATE NONCLUSTERED INDEX [IX_Product_Category_Mapping_ProductId] ON [dbo].[Product_Category_Mapping]( [ProductId] ASC ) INCLUDE ( [CategoryId] ) ON [PRIMARY]";

		private const string GetEngineEdition = "select SERVERPROPERTY('EngineEdition') as Value";

		public AjaxFiltersDatabaseService(IDbContext dbContext, IDataProvider dataProvider, IEncryptionService encryptionService, ISettingService settingService)
		{
			_dbContext = dbContext;
			_dataProvider = dataProvider;
			_encryptionService = encryptionService;
			_settingService = settingService;
			ChangeScriptsDatabaseSchema();
		}

		public void CreateDatabaseScripts()
		{
			ExecuteCreateDatabaseScripts();
			string encryptedDatabaseHashKey = GenerateEncryptedDatabaseKey();
			NopAjaxFiltersSettings nopAjaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>();
			nopAjaxFiltersSettings.EncryptedDatabaseHashKey = encryptedDatabaseHashKey;
			_settingService.SaveSetting(nopAjaxFiltersSettings);
		}

		public void UpdateDatabaseScripts()
		{
			string text = GenerateEncryptedDatabaseKey();
			NopAjaxFiltersSettings nopAjaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>();
			string encryptedDatabaseHashKey = nopAjaxFiltersSettings.EncryptedDatabaseHashKey;
			if (string.IsNullOrEmpty(encryptedDatabaseHashKey))
			{
				RemoveDatabaseScripts();
				CreateDatabaseScripts();
			}
			else if (!string.Equals(text, encryptedDatabaseHashKey, StringComparison.Ordinal))
			{
				bool dropSortFunctionDuringUpdate = nopAjaxFiltersSettings.DropSortFunctionDuringUpdate;
				ExecuteDropDatabaseScripts(dropSortFunctionDuringUpdate);
				ExecuteCreateDatabaseScripts(dropSortFunctionDuringUpdate);
				nopAjaxFiltersSettings.EncryptedDatabaseHashKey = text;
				_settingService.SaveSetting(nopAjaxFiltersSettings);
			}
		}

		public void RemoveDatabaseScripts()
		{
			ExecuteDropDatabaseScripts();
		}

		private void ExecuteDropDatabaseScripts(bool dropSortFunction = true)
		{
			_dbContext.ExecuteSqlCommand(CreateProductSpecificationAttributeMappingPerformanceProblemIndex, false, 180);
			if (dropSortFunction)
			{
				_dbContext.ExecuteSqlCommand(DropSortFunctionSql, false, 180);
			}
			NopAjaxFiltersSettings nopAjaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>();
			nopAjaxFiltersSettings.EncryptedDatabaseHashKey = string.Empty;
			_settingService.SaveSetting(nopAjaxFiltersSettings);
		}

		private void ExecuteCreateDatabaseScripts(bool createSortFunction = true)
		{
			if (createSortFunction)
			{
				_dbContext.ExecuteSqlCommand(CreateSortFunctionSql, false, 180);
			}
			_dbContext.ExecuteSqlCommand(DropProductSpecificationAttributeMappingPerformanceProblemIndex, false, 180);
			_dbContext.ExecuteSqlCommand(CreateIndexOnProductSpecificationAttributeMappingTable, false, 180);
			_dbContext.ExecuteSqlCommand(DropProductCategoryMappingPerformanceProblemIndex, false, 180);
			_dbContext.ExecuteSqlCommand(CreateProductCategoryMappingTablePerformanceProblemIndex, false, 180);
		}

		private string GenerateEncryptedDatabaseKey()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(CreateSortFunctionSql);
			stringBuilder.Append(DropSortFunctionSql);
			stringBuilder.Append(DropProductSpecificationAttributeMappingPerformanceProblemIndex);
			stringBuilder.Append(CreateIndexOnProductSpecificationAttributeMappingTable);
			stringBuilder.Append(CreateProductSpecificationAttributeMappingPerformanceProblemIndex);
			stringBuilder.Append(DropProductCategoryMappingPerformanceProblemIndex);
			stringBuilder.Append(CreateProductCategoryMappingTablePerformanceProblemIndex);
			return _encryptionService.CreatePasswordHash(stringBuilder.ToString(), "SS_AjaxFilters_EK_351", "SHA1");
		}

		private void ChangeScriptsDatabaseSchema()
		{
			string settingByKey = _settingService.GetSettingByKey("ajaxFilterSettings.DboSchema", "[dbo]");
			if (!settingByKey.Equals("[dbo]"))
			{
				CreateSortFunctionSql = CreateSortFunctionSql.Replace("[dbo]", settingByKey);
				DropSortFunctionSql = DropSortFunctionSql.Replace("[dbo]", settingByKey);
				//CreateStoredProcedureSql = CreateStoredProcedureSql.Replace("[dbo]", settingByKey);
				DropProductSpecificationAttributeMappingPerformanceProblemIndex = DropProductSpecificationAttributeMappingPerformanceProblemIndex.Replace("[dbo]", settingByKey);
				CreateIndexOnProductSpecificationAttributeMappingTable = CreateIndexOnProductSpecificationAttributeMappingTable.Replace("[dbo]", settingByKey);
				CreateProductSpecificationAttributeMappingPerformanceProblemIndex = CreateProductSpecificationAttributeMappingPerformanceProblemIndex.Replace("[dbo]", settingByKey);
				DropProductCategoryMappingPerformanceProblemIndex = DropProductCategoryMappingPerformanceProblemIndex.Replace("[dbo]", settingByKey);
				CreateProductCategoryMappingTablePerformanceProblemIndex = CreateProductCategoryMappingTablePerformanceProblemIndex.Replace("[dbo]", settingByKey);
			}
		}
	}
}
