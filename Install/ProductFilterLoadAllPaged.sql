USE [cartzilla]
GO
/****** Object:  StoredProcedure [dbo].[ProductFilterLoadAllPaged]    Script Date: 1/16/2020 4:54:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ProductFilterLoadAllPaged]
(
	@CategoryIds		nvarchar(MAX) = null,	--a list of category IDs (comma-separated list). e.g. 1,2,3
	@ManufacturerId		int = 0,
	@StoreId			int = 0,
	@VendorId			int = 0,
	@WarehouseId		int = 0,
	@ProductTypeId		int = null, --product type identifier, null - load all products
	@VisibleIndividuallyOnly bit = 0, 	--0 - load all products , 1 - "visible indivially" only
	@MarkedAsNewOnly	bit = 0, 	--0 - load all products , 1 - "marked as new" only
	@ProductTagId		int = 0,
	@FeaturedProducts	bit = null,	--0 featured only , 1 not featured only, null - load all products
	@Keywords			nvarchar(4000) = null,
	@SearchDescriptions bit = 0, --a value indicating whether to search by a specified "keyword" in product descriptions
	@SearchManufacturerPartNumber bit = 0, -- a value indicating whether to search by a specified "keyword" in manufacturer part number
	@SearchSku			bit = 0, --a value indicating whether to search by a specified "keyword" in product SKU
	@SearchProductTags  bit = 0, --a value indicating whether to search by a specified "keyword" in product tags
	@UseFullTextSearch  bit = 0,
	@FullTextMode		int = 0, --0 - using CONTAINS with <prefix_term>, 5 - using CONTAINS and OR with <prefix_term>, 10 - using CONTAINS and AND with <prefix_term>
	@LanguageId			int = 0,
	@OrderBy			int = 0, --0 - position, 5 - Name: A to Z, 6 - Name: Z to A, 10 - Price: Low to High, 11 - Price: High to Low, 15 - creation date
	@AllowedCustomerRoleIds	nvarchar(MAX) = null,	--a list of customer role IDs (comma-separated list) for which a product should be shown (if a subject to ACL)
	@PageIndex			int = 0, 
	@PageSize			int = 2147483644,
	@ShowHidden			bit = 0,
	@OverridePublished	bit = null, --null - process "Published" property according to "showHidden" parameter, true - load only "Published" products, false - load only "Unpublished" products
	@LoadFilterableSpecificationAttributeOptions bit = 0, --a value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)
	@FilterableSpecificationAttributeOptions nvarchar(MAX) = null OUTPUT, --the specification attribute option identifiers applied to loaded products (all pages). returned as a comma separated list of identifiers
	@LoadFilterableCategories bit = 0,
	@FilterableCategories nvarchar(MAX) = null OUTPUT,
	@LoadFilterableManufacturers bit = 0, --a value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)
	@FilterableManufacturers nvarchar(MAX) = null OUTPUT,
	@LoadFilterableProductVariantAttributes bit = 0, --a value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)
	@FilterableProductVariantAttributes nvarchar(MAX) = null OUTPUT,
	@LoadFilterableVendors bit = 0,
	@FilterableVendors nvarchar(MAX) = null OUTPUT,
	@LoadFilterableProductTags bit = 0,
	@FilterableProductTags nvarchar(MAX) = null OUTPUT,
	@LoadFilterableProductReviews bit = 0,
	@FilterableProductReviews nvarchar(MAX) = null OUTPUT,
	@LoadFilterablePriceRange BIT = 0,
	@PriceMin			decimal(18, 4) = null OUTPUT,
	@PriceMax			decimal(18, 4) = null OUTPUT,
	@LoadFilterableOnSale BIT = 0,
	@ProductsOnSale INT = 0 OUTPUT,
	@LoadFilterableInStock BIT = 0,
	@ProductsInStock INT = 0 OUTPUT,
	@TotalRecords		int = null OUTPUT,
	@FilterTypes		nvarchar(MAX) = Null
)
AS
BEGIN
	
	/* Products that filtered by keywords */
	CREATE TABLE #KeywordProducts
	(
		[ProductId] int NOT NULL
	)

	DECLARE
		@SearchKeywords bit,
		@OriginalKeywords nvarchar(4000),
		@sql nvarchar(max),
		@sql_orderby nvarchar(max)

	SET NOCOUNT ON
	
	--filter by keywords
	SET @Keywords = isnull(@Keywords, '')
	SET @Keywords = rtrim(ltrim(@Keywords))
	SET @OriginalKeywords = @Keywords
	SET @sql = 'INSERT INTO #KeywordProducts ([ProductId]) '
	IF ISNULL(@Keywords, '') != ''
	BEGIN
		SET @SearchKeywords = 1
		
		IF @UseFullTextSearch = 1
		BEGIN
			--remove wrong chars (' ")
			SET @Keywords = REPLACE(@Keywords, '''', '')
			SET @Keywords = REPLACE(@Keywords, '"', '')
			
			--full-text search
			IF @FullTextMode = 0 
			BEGIN
				--0 - using CONTAINS with <prefix_term>
				SET @Keywords = ' "' + @Keywords + '*" '
			END
			ELSE
			BEGIN
				--5 - using CONTAINS and OR with <prefix_term>
				--10 - using CONTAINS and AND with <prefix_term>

				--clean multiple spaces
				WHILE CHARINDEX('  ', @Keywords) > 0 
					SET @Keywords = REPLACE(@Keywords, '  ', ' ')

				DECLARE @concat_term nvarchar(100)				
				IF @FullTextMode = 5 --5 - using CONTAINS and OR with <prefix_term>
				BEGIN
					SET @concat_term = 'OR'
				END 
				IF @FullTextMode = 10 --10 - using CONTAINS and AND with <prefix_term>
				BEGIN
					SET @concat_term = 'AND'
				END

				--now let's build search string
				declare @fulltext_keywords nvarchar(4000)
				set @fulltext_keywords = N''
				declare @index int		
		
				set @index = CHARINDEX(' ', @Keywords, 0)

				-- if index = 0, then only one field was passed
				IF(@index = 0)
					set @fulltext_keywords = ' "' + @Keywords + '*" '
				ELSE
				BEGIN		
					DECLARE @first BIT
					SET  @first = 1			
					WHILE @index > 0
					BEGIN
						IF (@first = 0)
							SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' '
						ELSE
							SET @first = 0

						SET @fulltext_keywords = @fulltext_keywords + '"' + SUBSTRING(@Keywords, 1, @index - 1) + '*"'					
						SET @Keywords = SUBSTRING(@Keywords, @index + 1, LEN(@Keywords) - @index)						
						SET @index = CHARINDEX(' ', @Keywords, 0)
					end
					
					-- add the last field
					IF LEN(@fulltext_keywords) > 0
						SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' ' + '"' + SUBSTRING(@Keywords, 1, LEN(@Keywords)) + '*"'	
				END
				SET @Keywords = @fulltext_keywords
			END
		END
		ELSE
		BEGIN
			--usual search by PATINDEX
			SET @Keywords = '%' + @Keywords + '%'
		END
		--PRINT @Keywords
		SET @sql = @sql + ' SELECT p.Id FROM Product p with (NOLOCK) WHERE '
		IF @UseFullTextSearch = 1
			SET @sql = @sql + 'CONTAINS(p.[Name], @Keywords) '
		ELSE
			SET @sql = @sql + 'PATINDEX(@Keywords, p.[Name]) > 0 '


		--localized product name
		SET @sql = @sql + '
		UNION
		SELECT lp.EntityId
		FROM LocalizedProperty lp with (NOLOCK)
		WHERE
			lp.LocaleKeyGroup = N''Product''
			AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
			AND lp.LocaleKey = N''Name'''
		IF @UseFullTextSearch = 1
			SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
		ELSE
			SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
	

		IF @SearchDescriptions = 1
		BEGIN
			--product short description
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(p.[ShortDescription], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[ShortDescription]) > 0 '


			--product full description
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			WHERE '
			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(p.[FullDescription], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[FullDescription]) > 0 '



			--localized product short description
			SET @sql = @sql + '
			UNION
			SELECT lp.EntityId
			FROM LocalizedProperty lp with (NOLOCK)
			WHERE
				lp.LocaleKeyGroup = N''Product''
				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
				AND lp.LocaleKey = N''ShortDescription'''
			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
				

			--localized product full description
			SET @sql = @sql + '
			UNION
			SELECT lp.EntityId
			FROM LocalizedProperty lp with (NOLOCK)
			WHERE
				lp.LocaleKeyGroup = N''Product''
				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
				AND lp.LocaleKey = N''FullDescription'''
			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
		END

		--manufacturer part number (exact match)
		IF @SearchManufacturerPartNumber = 1
		BEGIN
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			WHERE p.[ManufacturerPartNumber] = @OriginalKeywords '
		END

		--SKU (exact match)
		IF @SearchSku = 1
		BEGIN
			SET @sql = @sql + '
			UNION
			SELECT p.Id
			FROM Product p with (NOLOCK)
			WHERE p.[Sku] = @OriginalKeywords '
		END

		IF @SearchProductTags = 1
		BEGIN
			--product tags (exact match)
			SET @sql = @sql + '
			UNION
			SELECT pptm.Product_Id
			FROM Product_ProductTag_Mapping pptm with(NOLOCK) INNER JOIN ProductTag pt with(NOLOCK) ON pt.Id = pptm.ProductTag_Id
			WHERE pt.[Name] = @OriginalKeywords '

			--localized product tags
			SET @sql = @sql + '
			UNION
			SELECT pptm.Product_Id
			FROM LocalizedProperty lp with (NOLOCK) INNER JOIN Product_ProductTag_Mapping pptm with(NOLOCK) ON lp.EntityId = pptm.ProductTag_Id
			WHERE
				lp.LocaleKeyGroup = N''ProductTag''
				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '
				AND lp.LocaleKey = N''Name''
				AND lp.[LocaleValue] = @OriginalKeywords '
		END

		--PRINT (@sql)
		EXEC sp_executesql @sql, N'@Keywords nvarchar(4000), @OriginalKeywords nvarchar(4000)', @Keywords, @OriginalKeywords

	END
	ELSE
	BEGIN
		SET @SearchKeywords = 0
	END

	--filter by category IDs
	SET @CategoryIds = isnull(@CategoryIds, '')	
	CREATE TABLE #FilteredCategoryIds
	(
		CategoryId int not null
	)
	INSERT INTO #FilteredCategoryIds (CategoryId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@CategoryIds, ',')	
	DECLARE @CategoryIdsCount int	
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #FilteredCategoryIds)

	--filter by FilterTypes
	SET @FilterTypes = isnull(@FilterTypes, '')	
	CREATE TABLE #FilterTypes ( Name CHAR(3) not null)

	INSERT INTO #FilterTypes (Name)
	SELECT data FROM [nop_splitstring_to_table](@FilterTypes, ',')

	--filter by customer role IDs (access control list)
	SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')	
	CREATE TABLE #FilteredCustomerRoleIds
	(
		CustomerRoleId int not null
	)
	INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@AllowedCustomerRoleIds, ',')
	DECLARE @FilteredCustomerRoleIdsCount int	
	SET @FilteredCustomerRoleIdsCount = (SELECT COUNT(1) FROM #FilteredCustomerRoleIds)
	
	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn	int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1
	
	CREATE TABLE #DisplayOrderTmp 
	(
		[Id] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL
	)

	IF @CategoryIdsCount > 0
	BEGIN
		SET @sql = @sql + ' SELECT pcm.ProductId FROM Product_Category_Mapping pcm with (NOLOCK) '

		SET @sql = @sql + '
		WHERE pcm.CategoryId IN ('
		
		SET @sql = @sql + + CAST(@CategoryIds AS nvarchar(max))

		SET @sql = @sql + ')'

		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + ' AND pcm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))
		END

		EXEC sp_executesql @sql
	END
	
	IF @ManufacturerId > 0
	BEGIN
		SET @sql = @sql + '
		SELECT pmm.ProductId FROM Product_Manufacturer_Mapping pmm with (NOLOCK) '

		SET @sql = @sql + ' WHERE pmm.ManufacturerId = ' + CAST(@ManufacturerId AS nvarchar(max))
		
		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + ' AND pmm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))
		END
		EXEC sp_executesql @sql
	END

	IF @VendorId > 0
	BEGIN
		SET @sql = @sql + ' SELECT p.Id FROM Product p with (NOLOCK) '
		SET @sql = @sql + ' WHERE p.VendorId = ' + CAST(@VendorId AS nvarchar(max))
		EXEC sp_executesql @sql
	END
	
	IF ISNULL(@ProductTagId, 0) != 0
	BEGIN
		SET @sql = @sql + ' SELECT pptm.Product_Id FROM Product_ProductTag_Mapping pptm with (NOLOCK) '

		SET @sql = @sql + ' WHERE pptm.ProductTag_Id = ' + CAST(@ProductTagId AS nvarchar(max))
		EXEC sp_executesql @sql
	END
	
	SET @sql = '
	SELECT p.Id
	FROM
		Product p with (NOLOCK)'

	SET @sql = @sql + '
	JOIN #KeywordProducts kp
		ON  p.Id = kp.ProductId'
	
	SET @sql = @sql + '
	WHERE
		p.Deleted = 0'
	
	--filter by warehouse
	IF @WarehouseId > 0
	BEGIN
		--we should also ensure that 'ManageInventoryMethodId' is set to 'ManageStock' (1)
		--but we skip it in order to prevent hard-coded values (e.g. 1) and for better performance
		SET @sql = @sql + '
		AND  
			(
				(p.UseMultipleWarehouses = 0 AND
					p.WarehouseId = ' + CAST(@WarehouseId AS nvarchar(max)) + ')
				OR
				(p.UseMultipleWarehouses > 0 AND
					EXISTS (SELECT 1 FROM ProductWarehouseInventory [pwi]
					WHERE [pwi].WarehouseId = ' + CAST(@WarehouseId AS nvarchar(max)) + ' AND [pwi].ProductId = p.Id))
			)'
	END
	
	--filter by product type
	IF @ProductTypeId is not null
	BEGIN
		SET @sql = @sql + '
		AND p.ProductTypeId = ' + CAST(@ProductTypeId AS nvarchar(max))
	END
	
	--filter by "visible individually"
	IF @VisibleIndividuallyOnly = 1
	BEGIN
		SET @sql = @sql + '
		AND p.VisibleIndividually = 1'
	END
	
	--filter by "marked as new"
	IF @MarkedAsNewOnly = 1
	BEGIN
		SET @sql = @sql + '
		AND p.MarkAsNew = 1
		AND (getutcdate() BETWEEN ISNULL(p.MarkAsNewStartDateTimeUtc, ''1/1/1900'') and ISNULL(p.MarkAsNewEndDateTimeUtc, ''1/1/2999''))'
	END
	
	----filter by product tag
	--IF ISNULL(@ProductTagId, 0) != 0
	--BEGIN
	--	SET @sql = @sql + '
	--	AND pptm.ProductTag_Id = ' + CAST(@ProductTagId AS nvarchar(max))
	--END
	
	--"Published" property
	IF (@OverridePublished is null)
	BEGIN
		--process according to "showHidden"
		IF @ShowHidden = 0
		BEGIN
			SET @sql = @sql + '
			AND p.Published = 1'
		END
	END
	ELSE IF (@OverridePublished = 1)
	BEGIN
		--published only
		SET @sql = @sql + '
		AND p.Published = 1'
	END
	ELSE IF (@OverridePublished = 0)
	BEGIN
		--unpublished only
		SET @sql = @sql + '
		AND p.Published = 0'
	END
	
	--show hidden
	IF @ShowHidden = 0
	BEGIN
		SET @sql = @sql + '
		AND p.Deleted = 0
		AND (getutcdate() BETWEEN ISNULL(p.AvailableStartDateTimeUtc, ''1/1/1900'') and ISNULL(p.AvailableEndDateTimeUtc, ''1/1/2999''))'
	END
	
	--min price
	IF @PriceMin is not null
	BEGIN
		SET @sql = @sql + '
		AND (p.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')'
	END
	
	--max price
	IF @PriceMax is not null
	BEGIN
		SET @sql = @sql + '
		AND (p.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')'
	END
	
	--show hidden and ACL
	IF  @ShowHidden = 0 and @FilteredCustomerRoleIdsCount > 0
	BEGIN
		SET @sql = @sql + '
		AND (p.SubjectToAcl = 0 OR EXISTS (
			SELECT 1 FROM #FilteredCustomerRoleIds [fcr]
			WHERE
				[fcr].CustomerRoleId IN (
					SELECT [acl].CustomerRoleId
					FROM [AclRecord] acl with (NOLOCK)
					WHERE [acl].EntityId = p.Id AND [acl].EntityName = ''Product''
				)
			))'
	END
	
	--filter by store
	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
			WHERE [sm].EntityId = p.Id AND [sm].EntityName = ''Product'' and [sm].StoreId=' + CAST(@StoreId AS nvarchar(max)) + '
			))'
	END
	
    --prepare filterable specification attribute option identifier (if requested)
    IF @LoadFilterableSpecificationAttributeOptions = 1 And (EXISTS (SELECT 1 FROM  #FilterTypes WHERE name = 'SPE' )) 
	BEGIN		
		CREATE TABLE #FilterableSpecs 
		(
			[SpecificationAttributeId] int NOT NULL,
			[SpecificationAttributeOptionId] int NOT NULL,
			[SpecificationAttributeOptionName] nvarchar(max) NOT NULL,
			[ColorSquaresRgb] nvarchar(max) ,
			[FilterCount] int,
			[DisplayOrder] Int NOT NULL
		)
        DECLARE @sql_filterableSpecs nvarchar(max)
        SET @sql_filterableSpecs = '
	        INSERT INTO #FilterableSpecs ([SpecificationAttributeId],[SpecificationAttributeOptionId], [SpecificationAttributeOptionName], [ColorSquaresRgb], [FilterCount],[DisplayOrder])
	        SELECT DISTINCT [sao].SpecificationAttributeId,[sao].Id,  [sao].Name,[sao].ColorSquaresRgb, COUNT([psam].productId), [sao].DisplayOrder
	        FROM [Product_SpecificationAttribute_Mapping] [psam] WITH (NOLOCK)
			INNER JOIN [SpecificationAttributeOption] [sao] On [psam].SpecificationAttributeOptionId = [sao].Id
	            WHERE [psam].[AllowFiltering] = 1
	            AND [psam].[ProductId] IN (' + @sql + ')' + ' GROUP BY [sao].SpecificationAttributeId, [sao].Id, [sao].Name, [sao].ColorSquaresRgb, [sao].DisplayOrder'

        EXEC sp_executesql @sql_filterableSpecs

		--build comma separated list of filterable identifiers
		SELECT @FilterableSpecificationAttributeOptions = 
		(SELECT  distinct sa.Id, sa.Name, sa.DisplayOrder,(SELECT fs.SpecificationAttributeOptionId as Id, fs.SpecificationAttributeOptionName as Name, fs.ColorSquaresRgb, fs.FilterCount,fs.DisplayOrder FROM #FilterableSpecs fs WHERE sa.Id = fs.SpecificationAttributeId FOR JSON PATH) AS FilterItems
		FROM SpecificationAttribute sa INNER JOIN #FilterableSpecs ON  #FilterableSpecs.SpecificationAttributeId = sa.Id FOR JSON PATH)
		
		 --AS products  FROM #FilterableSpecs  FOR JSON PATH, INCLUDE_NULL_VALUES)

		DROP TABLE #FilterableSpecs
 	END



	--prepare filterable product variant attribute option identifier (if requested)
	IF @LoadFilterableProductVariantAttributes = 1  And (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'ATR' ))
	BEGIN		
		CREATE TABLE #FilterableVariantAttributes
		(
			[ProductAttributeId] int NOT NULL,
			[ProductAttributeValueId] int NOT NULL,
			[ProductAttributeValueName] nvarchar(max) NOT NULL,
			[ProductAttributeMappingId] varchar(256) NOT NULL,
			[ColorSquaresRgb] nvarchar(max),
			[ImageSquaresPictureId] int NOT NULL,	
			[FilterCount] int
		)

        DECLARE @sql_filterableVariantAttributes nvarchar(max)
        SET @sql_filterableVariantAttributes = '
	        INSERT INTO #FilterableVariantAttributes ([ProductAttributeId],[ProductAttributeValueId], [ProductAttributeValueName],[ProductAttributeMappingId],[ColorSquaresRgb],[ImageSquaresPictureId],[FilterCount])
	        SELECT DISTINCT [pam].ProductAttributeId,[pav].Id, [pav].Name, CAST([pav].ProductAttributeMappingId AS VARCHAR(256)),[pav].ColorSquaresRgb, [pav].ImageSquaresPictureId, COUNT([pam].productId)
	        FROM [Product_ProductAttribute_Mapping] [pam] WITH (NOLOCK)
			INNER JOIN [ProductAttributeValue] [pav] on [pam].Id = [pav].ProductAttributeMappingId
	            WHERE [pam].[ProductId] IN (' + @sql + ')' + ' GROUP BY [pam].ProductAttributeId, [pav].Id, [pav].Name, [pav].ProductAttributeMappingId, [pav].ColorSquaresRgb, [pav].ImageSquaresPictureId'




        EXEC sp_executesql @sql_filterableVariantAttributes
		
		--SELECT @FilterableProductVariantAttributes = 
		--(SELECT  distinct Id, Name,
		--(SELECT fva.ProductAttributeId,fva.ProductAttributeValueName AS Name,fva.ColorSquaresRgb, fva.ImageSquaresPictureId, fva.FilterCount ,
		--json_query(QUOTENAME(STRING_AGG(STRING_ESCAPE(fva.ProductAttributeMappingId, 'json'), char(44)))) AS [ProductAttributeMappingIds]
		--FROM #FilterableVariantAttributes fva WHERE pa.Id = fva.ProductAttributeId 
		--group by fva.ProductAttributeValueName,fva.ColorSquaresRgb, fva.ImageSquaresPictureId, fva.FilterCount, fva.ProductAttributeId  FOR JSON PATH) AS FilterItems

		--FROM ProductAttribute pa INNER JOIN #FilterableVariantAttributes ON pa.Id = #FilterableVariantAttributes.ProductAttributeId FOR JSON PATH)


		;With CTE(Name,ProductAttributeMappingIds) AS (
		SELECT pav.ProductAttributeValueName,JSON_QUERY(QUOTENAME(STRING_AGG(STRING_ESCAPE(pav.ProductAttributeMappingId, 'json'), char(44)))) AS [ProductAttributeMappingIds]
		from #FilterableVariantAttributes pav GROUP BY pav.ProductAttributeValueName
		), FilterableVariantAttributesCTE AS (
		SELECT fva.ProductAttributeId,fva.ProductAttributeValueId AS ProductAttributeValueId, 
		fva.ProductAttributeValueName AS ProductAttributeValueName,
		fva.ColorSquaresRgb, fva.ImageSquaresPictureId, fva.FilterCount , ProductAttributeMappingIds
		FROM #FilterableVariantAttributes fva INNER JOIN CTE ON fva.ProductAttributeValueName = CTE.Name)
	
		SELECT @FilterableProductVariantAttributes = 
		(SELECT  distinct Id, Name,(SELECT fva.ProductAttributeValueId AS ValueId, fva.ProductAttributeValueName AS Name, pa.Id AS AttributeId,
									fva.ColorSquaresRgb, fva.ImageSquaresPictureId, fva.FilterCount ,ProductAttributeMappingIds AS ProductVariantAttributeIds
									FROM FilterableVariantAttributesCTE fva WHERE pa.Id = fva.ProductAttributeId ORDER BY fva.ProductAttributeValueId FOR JSON PATH) AS FilterItems

		FROM ProductAttribute pa INNER JOIN FilterableVariantAttributesCTE ON pa.Id = FilterableVariantAttributesCTE.ProductAttributeId   FOR JSON PATH)

		DROP TABLE #FilterableVariantAttributes
 	END

	 --prepare filterable Manufacturers  identifier (if requested)
    IF @LoadFilterableManufacturers = 1 AND @ManufacturerId = 0 And (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'MAN' )) 
	BEGIN		
		CREATE TABLE #FilterableManufacturers 
		(
			[ManufacturerId] int NOT NULL,
			[FilterCount] int
		)
        DECLARE @sql_filterableManufacturers nvarchar(max)
        SET @sql_filterableManufacturers = '
	        INSERT INTO #FilterableManufacturers ([ManufacturerId],[FilterCount])
	        SELECT DISTINCT [pmm].ManufacturerId, COUNT([pmm].ProductId)
	        FROM [Product_Manufacturer_Mapping] [pmm] WITH (NOLOCK)
	            WHERE [pmm].[ProductId] IN (' + @sql + ')' + ' GROUP BY [pmm].ManufacturerId'

        EXEC sp_executesql @sql_filterableManufacturers

		SELECT @FilterableManufacturers = (Select man.Id As Id, man.Name As Name, fm.FilterCount As FilterCount From Manufacturer man WITH (NOLOCK) Inner Join #FilterableManufacturers fm On man.Id = fm.ManufacturerId FOR JSON PATH) 

		DROP TABLE #FilterableManufacturers
 	END


	IF @LoadFilterableCategories = 1 AND (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'CAT' )) 
	BEGIN		
		CREATE TABLE #FilterableCategories 
		(
			[CategoryId] int NOT NULL,
			[FilterCount] int
		)
        DECLARE @sql_filterableCategories nvarchar(max)
        SET @sql_filterableCategories = '
	        INSERT INTO #FilterableCategories ([CategoryId],[FilterCount])
	        SELECT DISTINCT [pcm].CategoryId, COUNT([pcm].ProductId)
	        FROM [Product_Category_Mapping] [pcm] WITH (NOLOCK)
	            WHERE [pcm].[ProductId] IN (' + @sql + ')' + ' GROUP BY [pcm].CategoryId'

        EXEC sp_executesql @sql_filterableCategories

		SELECT @FilterableCategories = (Select cat.Id As Id, cat.Name As Name, fc.FilterCount As FilterCount From Category cat WITH (NOLOCK) Inner Join #FilterableCategories fc On cat.Id = fc.CategoryId FOR JSON PATH) 

		DROP TABLE #FilterableCategories
 	END

	IF @LoadFilterableVendors = 1 AND @VendorId = 0 And (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'VEN' )) 
	BEGIN		
		CREATE TABLE #FilterableVendors 
		(
			[VendorId] int NOT NULL,
			[FilterCount] int
		)
        DECLARE @sql_filterableVendors nvarchar(max)
        SET @sql_filterableVendors = '
	        INSERT INTO #FilterableVendors ([VendorId],[FilterCount])
	        SELECT DISTINCT [p].VendorId, COUNT([p].Id)
	        FROM [Product] [p] WITH (NOLOCK)
	            WHERE [p].[Id] IN (' + @sql + ')' + ' GROUP BY [p].VendorId'

        EXEC sp_executesql @sql_filterableVendors

		--build comma separated list of filterable identifiers
		SELECT @FilterableVendors = (Select ven.Id As Id, ven.Name As Name, fv.FilterCount As FilterCount From Vendor ven WITH (NOLOCK) Inner Join #FilterableVendors fv On ven.Id = fv.VendorId FOR JSON PATH) 

		DROP TABLE #FilterableVendors
 	END


	IF @LoadFilterablePriceRange = 1 AND (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'PRI' )) 
	BEGIN		
		CREATE TABLE #FilterablePriceRange 
		(
			[PriceMin] Decimal (18, 4),
			[PriceMax] Decimal (18, 4)
		)
        DECLARE @sql_filterablePriceRange nvarchar(max)
        SET @sql_filterablePriceRange = '
	        INSERT INTO #FilterablePriceRange ([PriceMin],[PriceMax])
	        SELECT  MIN([p].price), MAX([p].price)
	        FROM [Product] [p] WITH (NOLOCK)
	            WHERE [p].[Id] IN (' + @sql + ')'

        EXEC sp_executesql @sql_filterablePriceRange

		--build comma separated list of filterable identifiers
		SELECT @PriceMin = PriceMin, @PriceMax = PriceMax  FROM #FilterablePriceRange
		DROP TABLE #FilterablePriceRange
 	END


	IF @LoadFilterableProductTags = 1 AND @ProductTagId = 0 And (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'TAG' )) 
	BEGIN		
		CREATE TABLE #FilterableProductTags 
		(
			[ProductTagId] int NOT NULL,
			[FilterCount] int
		)
        DECLARE @sql_filterableProductTags nvarchar(max)
        SET @sql_filterableProductTags = '
	        INSERT INTO #FilterableProductTags ([ProductTagId],[FilterCount])
	        SELECT DISTINCT [pptm].ProductTag_Id, COUNT([pptm].Product_Id)
	        FROM [Product_ProductTag_Mapping] [pptm] WITH (NOLOCK)
	            WHERE [pptm].Product_Id IN (' + @sql + ')' + ' GROUP BY [pptm].ProductTag_Id'
			

        EXEC sp_executesql @sql_filterableProductTags

		--build comma separated list of filterable identifiers
		SELECT @FilterableProductTags = (Select pt.Id As Id, pt.Name As Name, fpt.FilterCount As FilterCount From ProductTag pt WITH (NOLOCK) Inner Join #FilterableProductTags fpt On pt.Id = fpt.ProductTagId FOR JSON PATH) 
		DROP TABLE #FilterableProductTags
 	END


	IF @LoadFilterableProductReviews = 1 And (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'RAT' )) 
	BEGIN		
		CREATE TABLE #FilterableProductReviews 
		(
			[ApprovedRatingSum] int NOT NULL,
			[ApprovedTotalReviews] INT NOT NULL,
			[FilterCount] int
		)
        DECLARE @sql_filterableProductReviews nvarchar(max)
        SET @sql_filterableProductReviews = '
	        INSERT INTO #FilterableProductReviews ([ApprovedRatingSum],[ApprovedTotalReviews],[FilterCount])
	        SELECT DISTINCT [p].ApprovedRatingSum, [p].ApprovedTotalReviews,COUNT([p].Id)
	        FROM [Product] [p] WITH (NOLOCK)
	            WHERE [p].Id IN (' + @sql + ')' + 'AND ApprovedRatingSum > 0 GROUP BY [p].ApprovedRatingSum, [p].ApprovedTotalReviews'

        EXEC sp_executesql @sql_filterableProductReviews

		--build comma separated list of filterable identifiers
 
		SELECT @FilterableProductReviews = (Select ApprovedRatingSum AS Id,ApprovedRatingSum AS RatingSum, ApprovedTotalReviews AS TotalReviews, FilterCount FROM #FilterableProductReviews FOR JSON PATH) 

		DROP TABLE #FilterableProductReviews
 	END

	IF @LoadFilterableOnSale = 1 And (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'SAL' )) 
	BEGIN

		SELECT @ProductsOnSale  = (SELECT COUNT(1) FROM Product p INNER JOIN #KeywordProducts kp ON kp.ProductId = p.Id  AND (p.OldPrice > 0 AND p.Price != p.OldPrice))

	END

	IF @LoadFilterableInStock = 1 And (EXISTS (SELECT 1 FROM  #FilterTypes where name = 'STO' )) 
	BEGIN
		SELECT @ProductsInStock  =  (
				SELECT COUNT(1) FROM Product p INNER JOIN #KeywordProducts kp ON kp.ProductId = p.Id 
				WHERE (((p.ManageInventoryMethodId = 0) OR (P.ManageInventoryMethodId = 1 
				AND (( p.StockQuantity > 0 AND p.UseMultipleWarehouses = 0) 
				OR (EXISTS ( SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = p.Id
				AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND p.UseMultipleWarehouses = 1))))))
	END

	--sorting
	SET @sql_orderby = ''	
	IF @OrderBy = 5 /* Name: A to Z */
		SET @sql_orderby = ' p.[Name] ASC'
	ELSE IF @OrderBy = 6 /* Name: Z to A */
		SET @sql_orderby = ' p.[Name] DESC'
	ELSE IF @OrderBy = 10 /* Price: Low to High */
		SET @sql_orderby = ' p.[Price] ASC'
	ELSE IF @OrderBy = 11 /* Price: High to Low */
		SET @sql_orderby = ' p.[Price] DESC'
	ELSE IF @OrderBy = 15 /* creation date */
		SET @sql_orderby = ' p.[CreatedOnUtc] DESC'
	ELSE /* default sorting, 0 (position) */
	BEGIN
		--name
		IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
		SET @sql_orderby = @sql_orderby + ' p.[Name] ASC'
	END
	
	SET @sql = @sql + '
	ORDER BY' + @sql_orderby
	IF @SearchKeywords = 1
		BEGIN
			SET @sql = '
			INSERT INTO #DisplayOrderTmp ([ProductId])' + @sql

			--PRINT (@sql)
			EXEC sp_executesql @sql
		END
	DROP TABLE #FilteredCategoryIds
	DROP TABLE #FilteredCustomerRoleIds
	DROP TABLE #KeywordProducts


	CREATE TABLE #PageIndex 
		(
			[IndexId] int IDENTITY (1, 1) NOT NULL,
			[ProductId] int NOT NULL
		)

	IF @SearchKeywords = 1
	BEGIN
		
		INSERT INTO #PageIndex ([ProductId])
		SELECT ProductId
		FROM #DisplayOrderTmp
		GROUP BY ProductId
		ORDER BY min([Id])

		--total records
		SET @TotalRecords = @@rowcount
	END
		
	DROP TABLE #DisplayOrderTmp

	IF @SearchKeywords = 1
	BEGIN
		SELECT TOP (@RowsToReturn)
			p.*
		FROM
			#PageIndex [pi]
			INNER JOIN Product p with (NOLOCK) on p.Id = [pi].[ProductId]
		WHERE
			[pi].IndexId > @PageLowerBound AND 
			[pi].IndexId < @PageUpperBound
		ORDER BY
			[pi].IndexId
	END
	ELSE
		SELECT TOP 0 p.* From Product p

	DROP TABLE #PageIndex
END
GO
