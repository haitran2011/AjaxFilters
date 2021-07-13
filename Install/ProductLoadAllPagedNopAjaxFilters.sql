USE [cartzilla]
GO
/****** Object:  StoredProcedure [dbo].[ProductLoadAllPagedNopAjaxFilters]    Script Date: 1/16/2020 4:55:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ProductLoadAllPagedNopAjaxFilters] (
	@CategoryIds NVARCHAR(MAX) = NULL
	,@ManufacturerId INT = 0
	,@StoreId INT = 0
	,@VendorId INT = 0
	,@ParentGroupedProductId INT = 0
	,@ProductTypeId INT = NULL
	,@VisibleIndividuallyOnly BIT = 0
	,@ProductTagId INT = 0
	,@FeaturedProducts BIT = NULL
	,@PriceMin DECIMAL(18, 4) = NULL
	,@PriceMax DECIMAL(18, 4) = NULL
	,@Keywords NVARCHAR(4000) = NULL
	,@SearchDescriptions BIT = 0
	,@SearchManufacturerPartNumber BIT = 0
	,@SearchSku BIT = 0
	,@SearchProductTags BIT = 0
	,@UseFullTextSearch BIT = 0
	,@FullTextMode INT = 0
	,@FilteredSpecs NVARCHAR(MAX) = NULL
	,@FilteredProductVariantAttributes NVARCHAR(MAX) = NULL
	,@FilteredManufacturers NVARCHAR(MAX) = NULL
	,@FilteredVendors NVARCHAR(MAX) = NULL
	,@FilteredRating INT
	,@OnSale BIT = 0
	,@InStock BIT = 0
	,@LanguageId INT = 0
	,@OrderBy INT = 0
	,@AllowedCustomerRoleIds NVARCHAR(MAX) = NULL
	,@PageIndex INT = 0
	,@PageSize INT = 2147483644
	,@ShowHidden BIT = 0
	,@LoadAvailableFilters BIT = 0
	,@FilterableSpecificationAttributeOptionIds NVARCHAR(MAX) = NULL OUTPUT
	,@FilterableProductVariantAttributeIds NVARCHAR(MAX) = NULL OUTPUT
	,@FilterableManufacturerIds NVARCHAR(MAX) = NULL OUTPUT
	,@FilterableVendorIds NVARCHAR(MAX) = NULL OUTPUT
	,@FilterableCategoryIds NVARCHAR(MAX) = NULL OUTPUT
	,@FilterableRatingIds NVARCHAR(MAX) = NULL OUTPUT
	,@LoadFilterableSpecificationAttributeOptionIds bit = 0
	,@LoadFilterableProductVariantAttributeIds bit = 0
	,@LoadFilterableCategoryIds bit = 0
	,@LoadFilterableManufacturerIds bit = 0
	,@LoadFilterableVendorIds bit = 0
	,@LoadFilterableProductTagIds bit = 0
	,@LoadFilterableProductReviewIds bit = 0
	,@LoadFilterableOnSale BIT = 0
	,@LoadFilterableInStock BIT = 0
	,@ProductsOnSale INT = 0 OUTPUT
	,@ProductsInStock INT = 0 OUTPUT
	,@TotalRecords INT = NULL OUTPUT
	)
AS
BEGIN

	CREATE TABLE #KeywordProducts ([ProductId] INT NOT NULL)

	DECLARE @SearchKeywords BIT
		,@OriginalKeywords NVARCHAR(4000)
		,@sql NVARCHAR(max)
		,@sqlWithoutFilters NVARCHAR(max)
		,@sql_orderby NVARCHAR(max)

	SET NOCOUNT ON
	SET @Keywords = ISNULL(@Keywords, '')
	SET @Keywords = RTRIM(LTRIM(@Keywords))
	SET @OriginalKeywords = @Keywords

	IF ISNULL(@Keywords, '') != ''
	BEGIN
		SET @SearchKeywords = 1

		IF @UseFullTextSearch = 1
		BEGIN
			SET @Keywords = REPLACE(@Keywords, '''', '')
			SET @Keywords = REPLACE(@Keywords, '"', '')

			IF @FullTextMode = 0
			BEGIN
				SET @Keywords = ' "' + @Keywords + '*" '
			END
			ELSE
			BEGIN
				WHILE CHARINDEX('  ', @Keywords) > 0
					SET @Keywords = REPLACE(@Keywords, '  ', ' ')

				DECLARE @concat_term NVARCHAR(100)

				IF @FullTextMode = 5
				BEGIN
					SET @concat_term = 'OR'
				END

				IF @FullTextMode = 10
				BEGIN
					SET @concat_term = 'AND'
				END

				DECLARE @fulltext_keywords NVARCHAR(4000)

				SET @fulltext_keywords = N''

				DECLARE @index INT

				SET @index = CHARINDEX(' ', @Keywords, 0)

				IF (@index = 0)
					SET @fulltext_keywords = ' "' + @Keywords + '*" '
				ELSE
				BEGIN
					DECLARE @first BIT

					SET @first = 1

					WHILE @index > 0
					BEGIN
						IF (@first = 0)
							SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' '
						ELSE
							SET @first = 0

						SET @fulltext_keywords = @fulltext_keywords + '"' + SUBSTRING(@Keywords, 1, @index - 1) + '*"'
						SET @Keywords = SUBSTRING(@Keywords, @index + 1, LEN(@Keywords) - @index)
						SET @index = CHARINDEX(' ', @Keywords, 0)
					END

					IF LEN(@fulltext_keywords) > 0
						SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' ' + '"' + SUBSTRING(@Keywords, 1, LEN(@Keywords)) + '*"'
				END

				SET @Keywords = @fulltext_keywords
			END
		END
		ELSE
		BEGIN
			SET @Keywords = '%' + @Keywords + '%'
		END

		SET @sql = '  		INSERT INTO #KeywordProducts ([ProductId])  		SELECT p.Id  		FROM Product p with (NOLOCK)  		WHERE '

		IF @UseFullTextSearch = 1
			SET @sql = @sql + 'CONTAINS(p.[Name], @Keywords) '
		ELSE
			SET @sql = @sql + 'PATINDEX(@Keywords, p.[Name]) > 0 '

		SET @sql = @sql + '  		UNION  		SELECT lp.EntityId  		FROM LocalizedProperty lp with (NOLOCK)  		WHERE  			lp.LocaleKeyGroup = N''Product''  			AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS NVARCHAR(max)), '0') + '  			AND lp.LocaleKey = N''Name'''

		IF @UseFullTextSearch = 1
			SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
		ELSE
			SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '

		IF @SearchDescriptions = 1
		BEGIN
			SET @sql = @sql + '  			UNION  			SELECT p.Id  			FROM Product p with (NOLOCK)  			WHERE '

			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(p.[ShortDescription], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[ShortDescription]) > 0 '

			SET @sql = @sql + '  			UNION  			SELECT p.Id  			FROM Product p with (NOLOCK)  			WHERE '

			IF @UseFullTextSearch = 1
				SET @sql = @sql + 'CONTAINS(p.[FullDescription], @Keywords) '
			ELSE
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[FullDescription]) > 0 '

			SET @sql = @sql + '  			UNION  			SELECT lp.EntityId  			FROM LocalizedProperty lp with (NOLOCK)  			WHERE  				lp.LocaleKeyGroup = N''Product''  				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS NVARCHAR(max)), '0') + '  				AND lp.LocaleKey = N''ShortDescription'''

			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '

			SET @sql = @sql + '  			UNION  			SELECT lp.EntityId  			FROM LocalizedProperty lp with (NOLOCK)  			WHERE  				lp.LocaleKeyGroup = N''Product''  				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS NVARCHAR(max)), '0') + '  				AND lp.LocaleKey = N''FullDescription'''

			IF @UseFullTextSearch = 1
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '
			ELSE
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '
		END

		IF @SearchManufacturerPartNumber = 1
		BEGIN
			SET @sql = @sql + '  			UNION  			SELECT p.Id  			FROM Product p with (NOLOCK)  			WHERE p.[ManufacturerPartNumber] = @OriginalKeywords '
		END

		IF @SearchSku = 1
		BEGIN
			SET @sql = @sql + '  			UNION  			SELECT p.Id  			FROM Product p with (NOLOCK)  			WHERE p.[Sku] = @OriginalKeywords '
		END

		IF @SearchProductTags = 1
		BEGIN
			SET @sql = @sql + '  			UNION  			SELECT pptm.Product_Id  			FROM Product_ProductTag_Mapping pptm with(NOLOCK) INNER JOIN ProductTag pt with(NOLOCK) ON pt.Id = pptm.ProductTag_Id  			WHERE pt.[Name] = @OriginalKeywords '
			SET @sql = @sql + '  			UNION  			SELECT pptm.Product_Id  			FROM LocalizedProperty lp with (NOLOCK) INNER JOIN Product_ProductTag_Mapping pptm with(NOLOCK) ON lp.EntityId = pptm.ProductTag_Id  			WHERE  				lp.LocaleKeyGroup = N''ProductTag''  				AND lp.LanguageId = ' + ISNULL(CAST(@LanguageId AS NVARCHAR(max)), '0') + '  				AND lp.LocaleKey = N''Name''  				AND lp.[LocaleValue] = @OriginalKeywords '
		END

		EXEC sp_executesql @sql
			,N'@Keywords nvarchar(4000), @OriginalKeywords nvarchar(4000)'
			,@Keywords
			,@OriginalKeywords
	END
	ELSE
	BEGIN
		SET @SearchKeywords = 0
	END

	SET @CategoryIds = ISNULL(@CategoryIds, '')

	CREATE TABLE #FilteredCategoryIds (CategoryId INT NOT NULL)

	INSERT INTO #FilteredCategoryIds (CategoryId)
	SELECT CAST(data AS INT)
	FROM [nop_splitstring_to_table](@CategoryIds, ',')

	DECLARE @CategoryIdsCount INT

	SET @CategoryIdsCount = (
			SELECT COUNT(1)
			FROM #FilteredCategoryIds
			)
	SET @FilteredSpecs = ISNULL(@FilteredSpecs, '')

	CREATE TABLE #FilteredSpecificationAttributeOptions (SpecificationAttributeOptionId INT NOT NULL UNIQUE)

	INSERT INTO #FilteredSpecificationAttributeOptions (SpecificationAttributeOptionId)
	SELECT CAST(data AS INT)
	FROM [nop_splitstring_to_table](@FilteredSpecs, ',')

	DECLARE @SpecificationAttributesCount INT

	SET @SpecificationAttributesCount = (
			SELECT COUNT(DISTINCT sao.SpecificationAttributeId)
			FROM #FilteredSpecificationAttributeOptions fs
			INNER JOIN SpecificationAttributeOption sao ON sao.Id = fs.SpecificationAttributeOptionId
			)

	CREATE TABLE #FilteredSpecificationAttributes (AttributeId INT NOT NULL)

	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredSpecificationAttributes_AttributeId ON #FilteredSpecificationAttributes (AttributeId);

	INSERT INTO #FilteredSpecificationAttributes
	SELECT DISTINCT sap.SpecificationAttributeId
	FROM SpecificationAttributeOption sap
	INNER JOIN #FilteredSpecificationAttributeOptions fs ON fs.SpecificationAttributeOptionId = sap.Id

	SET @FilteredProductVariantAttributes = ISNULL(@FilteredProductVariantAttributes, '')

	CREATE TABLE #FilteredProductVariantAttributes (ProductVariantAttributeId INT NOT NULL)

	CREATE INDEX IX_FilteredProductVariantAttributes_ProductVariantAttributeId ON #FilteredProductVariantAttributes (ProductVariantAttributeId);

	INSERT INTO #FilteredProductVariantAttributes (ProductVariantAttributeId)
	SELECT CAST(data AS INT)
	FROM [nop_splitstring_to_table](@FilteredProductVariantAttributes, ',')

	DECLARE @ProductAttributesCount INT

	SET @ProductAttributesCount = (
			SELECT COUNT(DISTINCT ppm.ProductAttributeId)
			FROM #FilteredProductVariantAttributes fpva
			INNER JOIN Product_ProductAttribute_Mapping ppm ON ppm.Id = fpva.ProductVariantAttributeId
			)

	CREATE TABLE #FilteredProductAttributes (AttributeId INT NOT NULL)

	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredAttributes_AttributeId ON #FilteredProductAttributes (AttributeId);

	INSERT INTO #FilteredProductAttributes
	SELECT DISTINCT ProductAttributeId
	FROM Product_ProductAttribute_Mapping ppm
	INNER JOIN #FilteredProductVariantAttributes fpv ON fpv.ProductVariantAttributeId = ppm.Id

	SET @FilteredManufacturers = ISNULL(@FilteredManufacturers, '')

	CREATE TABLE #FilteredManufacturers (ManufacturerId INT NOT NULL)

	INSERT INTO #FilteredManufacturers (ManufacturerId)
	SELECT CAST(data AS INT)
	FROM [nop_splitstring_to_table](@FilteredManufacturers, ',')

	DECLARE @ManufacturersCount INT

	SET @ManufacturersCount = (
			SELECT COUNT(1)
			FROM #FilteredManufacturers
			)


	--SET @FilteredCategories = ISNULL(@FilteredCategories, '')

	--CREATE TABLE #FilteredCategories (CategoryId INT NOT NULL)

	--INSERT INTO #FilteredCategories (CategoryId)
	--SELECT CAST(data AS INT)
	--FROM [nop_splitstring_to_table](@FilteredCategories, ',')

	--DECLARE @CategoriesCount INT

	--SET @CategoriesCount = (
	--		SELECT COUNT(1)
	--		FROM #FilteredCategories
	--		)

	SET @FilteredVendors = ISNULL(@FilteredVendors, '')

	CREATE TABLE #FilteredVendorIds (VendorId INT NOT NULL)

	INSERT INTO #FilteredVendorIds (VendorId)
	SELECT CAST(data AS INT)
	FROM [nop_splitstring_to_table](@FilteredVendors, ',')

	SET @AllowedCustomerRoleIds = ISNULL(@AllowedCustomerRoleIds, '')

	CREATE TABLE #FilteredCustomerRoleIds (CustomerRoleId INT NOT NULL)

	INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)
	SELECT CAST(data AS INT)
	FROM [nop_splitstring_to_table](@AllowedCustomerRoleIds, ',')

	DECLARE @VendorsCount INT

	SET @VendorsCount = (
			SELECT COUNT(1)
			FROM #FilteredVendorIds
			)

	DECLARE @PageLowerBound INT
	DECLARE @PageUpperBound INT
	DECLARE @RowsToReturn INT

	SET @RowsToReturn = @PageSize * (@PageIndex + 1)
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1

	CREATE TABLE #DisplayOrderTmp (
		[Id] INT IDENTITY(1, 1) NOT NULL
		,[ProductId] INT NOT NULL
		,[ChildProductId] INT
		)

	SET @sql = '  	INSERT INTO #DisplayOrderTmp ([ProductId], [ChildProductId])  	SELECT p.Id, ISNULL(cp.Id, 0)  	FROM  		Product p with (NOLOCK)  		LEFT JOIN Product cp with (NOLOCK)  		ON p.Id = cp.ParentGroupedProductId'

	IF @CategoryIdsCount > 0 
	BEGIN
		SET @sql = @sql + '  		LEFT JOIN Product_Category_Mapping pcm with (NOLOCK)  			ON p.Id = pcm.ProductId'
	END

	IF @ManufacturerId > 0
		OR @ManufacturersCount > 0
	BEGIN
		SET @sql = @sql + '  		LEFT JOIN Product_Manufacturer_Mapping pmm with (NOLOCK)  			ON p.Id = pmm.ProductId'
	END

	IF ISNULL(@ProductTagId, 0) != 0
	BEGIN
		SET @sql = @sql + '  		LEFT JOIN Product_ProductTag_Mapping pptm with (NOLOCK)  			ON p.Id = pptm.Product_Id'
	END

	IF @SearchKeywords = 1
	BEGIN
		SET @sql = @sql + '  		JOIN #KeywordProducts kp  			ON  p.Id = kp.ProductId'
	END

	SET @sql = @sql + '  	WHERE  		p.Deleted = 0'
	SET @sql = @sql + '  	AND  		(p.ParentGroupedProductId = 0 OR p.VisibleIndividually = 1)'

	IF @CategoryIdsCount > 0
	BEGIN
		SET @sql = @sql + '  		AND pcm.CategoryId IN (SELECT CategoryId FROM #FilteredCategoryIds)'

		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + '  		AND pcm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS NVARCHAR(max))
		END
	END

	IF @ManufacturerId > 0
	BEGIN
		SET @sql = @sql + '  		AND pmm.ManufacturerId = ' + CAST(@ManufacturerId AS NVARCHAR(max))

		IF @FeaturedProducts IS NOT NULL
		BEGIN
			SET @sql = @sql + '  		AND pmm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS NVARCHAR(max))
		END
	END

	IF @VendorId > 0
	BEGIN
		SET @sql = @sql + '  		AND p.VendorId = ' + CAST(@VendorId AS NVARCHAR(max))
	END

	IF @ParentGroupedProductId > 0
	BEGIN
		SET @sql = @sql + '  		AND p.ParentGroupedProductId = ' + CAST(@ParentGroupedProductId AS NVARCHAR(max))
	END

	IF @OnSale = 1
	BEGIN
		SET @sql = @sql + '  		AND   			(  				(cp.ID IS NULL AND p.OldPrice > 0  AND p.OldPrice != p.Price)  			  			OR  			   				(cp.ID IS NOT NULL AND cp.OldPrice > 0  AND cp.OldPrice != cp.Price)  			)'
	END

	IF @InStock = 1
	BEGIN
		SET @sql = @sql + '  		AND   			(  				(cp.ID IS NULL  AND   					(  						(p.ManageInventoryMethodId = 0) OR  						(P.ManageInventoryMethodId = 1 AND  							(  								(p.StockQuantity > 0 AND p.UseMultipleWarehouses = 0) OR   								(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE	[pwi].ProductId = p.Id	AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND p.UseMultipleWarehouses = 1)  							)  						)  					)  				)  				OR  				(p.Id IS NOT NULL AND   					(  						(cp.ManageInventoryMethodId = 0) OR  						(cp.ManageInventoryMethodId = 1 AND  							(  								(cp.StockQuantity > 0 AND cp.UseMultipleWarehouses = 0) OR   								(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = cp.Id	AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND cp.UseMultipleWarehouses = 1)  							)  						)  					)  				)  			)'
	END

	IF @ProductTypeId IS NOT NULL
	BEGIN
		SET @sql = @sql + '  		AND p.ProductTypeId = ' + CAST(@ProductTypeId AS NVARCHAR(max))
	END

	IF @VisibleIndividuallyOnly = 1
	BEGIN
		SET @sql = @sql + '  		AND p.VisibleIndividually = 1'
	END

	IF ISNULL(@ProductTagId, 0) != 0
	BEGIN
		SET @sql = @sql + '  		AND pptm.ProductTag_Id = ' + CAST(@ProductTagId AS NVARCHAR(max))
	END

	IF @ShowHidden = 0
	BEGIN
		SET @sql = @sql + '  		AND p.Published = 1  		AND p.Deleted = 0  		AND (getutcdate() BETWEEN ISNULL(p.AvailableStartDateTimeUtc, ''1/1/1900'') and ISNULL(p.AvailableEndDateTimeUtc, ''1/1/2999''))'
	END

	IF @PriceMin > 0
	BEGIN
		SET @sql = @sql + '  		AND (  				(  					cp.Id IS NULL  					  					AND  					  					(p.Price >= ' + CAST(@PriceMin AS NVARCHAR(max)) + ')  				)  				OR  				(	  					(cp.Price >= ' + CAST(@PriceMin AS NVARCHAR(max)) + ')  				)  			)'
	END

	IF @PriceMax > 0
	BEGIN
		SET @sql = @sql + '  		AND (  				(  					cp.Id IS NULL  					  					AND  					  					(p.Price <= ' + CAST(@PriceMax AS NVARCHAR(max)) + ')  				)  				OR  				(  					(cp.Price <= ' + CAST(@PriceMax AS NVARCHAR(max)) + ')  				)  			)'
	END

	IF @ShowHidden = 0
	BEGIN
		SET @sql = @sql + '  		AND (p.SubjectToAcl = 0 OR EXISTS (  			SELECT 1 FROM #FilteredCustomerRoleIds [fcr]  			WHERE  				[fcr].CustomerRoleId IN (  					SELECT [acl].CustomerRoleId  					FROM [AclRecord] acl with (NOLOCK)  					WHERE [acl].EntityId = p.Id AND [acl].EntityName = ''Product''  				)  			))'
	END

	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + '  		AND (p.LimitedToStores = 0 OR EXISTS (  			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)  			WHERE [sm].EntityId = p.Id AND [sm].EntityName = ''Product'' and [sm].StoreId=' + CAST(@StoreId AS NVARCHAR(max)) + '  			))'
	END

	SET @sqlWithoutFilters = @sql

	IF @SpecificationAttributesCount > 0
	BEGIN
		SET @sql = @sql + '  		AND (  				(SELECT AttributesCount FROM #FilteredSpecificationAttributesToProduct fsatp  				WHERE p.Id = fsatp.ProductId) = ' + CAST(@SpecificationAttributesCount AS NVARCHAR(max)) + ')'
	END

	IF @ProductAttributesCount > 0
	BEGIN
		SET @sql = @sql + '  				AND (  				(SELECT AttributesCount FROM #FilteredProductAttributesToProduct fpatp  				WHERE (cp.Id IS NULL AND p.Id = fpatp.ProductId) OR cp.Id = fpatp.ProductId) = ' + CAST(@ProductAttributesCount AS NVARCHAR(max)) + ')'
	END

	IF @ManufacturersCount > 0
	BEGIN
		SET @sql = @sql + '  		AND pmm.ManufacturerId IN (SELECT ManufacturerId FROM #FilteredManufacturers)'
	END

	IF @VendorsCount > 0
	BEGIN
		SET @sql = @sql + '   		AND p.VendorId IN (SELECT VendorId FROM #FilteredVendorIds)'
	END

	IF @FilteredRating > 0
	BEGIN
		SET @sql = @sql + '   		AND p.ApprovedRatingSum >= ' + CAST(@FilteredRating AS nvarchar(max))
	END


	SET @sql_orderby = [dbo].[seven_spikes_ajax_filters_product_sorting](@OrderBy, @CategoryIdsCount, @ManufacturerId, @ParentGroupedProductId)
	SET @sql = @sql + '  	ORDER BY' + @sql_orderby

	EXEC sp_executesql @sqlWithoutFilters

	CREATE TABLE #ProductIdsBeforeFiltersApplied (
		[ProductId] INT NOT NULL
		,[ChildProductId] INT
		)

	CREATE UNIQUE CLUSTERED INDEX IX_ProductIds_ProductId ON #ProductIdsBeforeFiltersApplied (
		ProductId
		,ChildProductId
		);

	INSERT INTO #ProductIdsBeforeFiltersApplied (
		[ProductId]
		,[ChildProductId]
		)
	SELECT ProductId
		,ChildProductId
	FROM #DisplayOrderTmp
	GROUP BY ProductId
		,ChildProductId
	ORDER BY MIN([Id])

	DELETE
	FROM #DisplayOrderTmp

	CREATE TABLE #FilteredSpecificationAttributesToProduct (
		ProductId INT NOT NULL
		,AttributesCount INT NOT NULL
		)

	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredSpecificationAttributesToProduct_ProductId ON #FilteredSpecificationAttributesToProduct (ProductId)

	IF @SpecificationAttributesCount > 0
	BEGIN
		IF @SpecificationAttributesCount > 1
		BEGIN
			INSERT INTO #FilteredSpecificationAttributesToProduct
			SELECT psm.ProductId
				,COUNT(DISTINCT sao.SpecificationAttributeId)
			FROM Product_SpecificationAttribute_Mapping psm
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = psm.ProductId
			INNER JOIN #FilteredSpecificationAttributeOptions fs ON fs.SpecificationAttributeOptionId = psm.SpecificationAttributeOptionId
			INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId
			GROUP BY psm.ProductId
			HAVING COUNT(DISTINCT sao.SpecificationAttributeId) >= @SpecificationAttributesCount - 1
		END

		IF @SpecificationAttributesCount = 1
		BEGIN
			INSERT INTO #FilteredSpecificationAttributesToProduct
			SELECT DISTINCT psm.ProductId
				,1
			FROM Product_SpecificationAttribute_Mapping psm
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = psm.ProductId
			INNER JOIN #FilteredSpecificationAttributeOptions fs ON fs.SpecificationAttributeOptionId = psm.SpecificationAttributeOptionId
				AND psm.AllowFiltering = 1

			INSERT INTO #FilteredSpecificationAttributesToProduct
			SELECT DISTINCT psm.ProductId
				,0
			FROM Product_SpecificationAttribute_Mapping psm
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = psm.ProductId
			INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId
			INNER JOIN #FilteredSpecificationAttributes fsa ON fsa.AttributeId = sao.SpecificationAttributeId
			WHERE NOT EXISTS (
					SELECT NULL
					FROM #FilteredSpecificationAttributesToProduct fsatp
					WHERE fsatp.ProductId = psm.ProductId
					)
				AND psm.AllowFiltering = 1
		END

		IF @SpecificationAttributesCount > 1
		BEGIN
			DELETE #FilteredSpecificationAttributesToProduct
			FROM #FilteredSpecificationAttributesToProduct fsatp
			WHERE (
					SELECT COUNT(DISTINCT sao.SpecificationAttributeId)
					FROM Product_SpecificationAttribute_Mapping psm
					INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId
					INNER JOIN #FilteredSpecificationAttributes fsa ON fsa.AttributeId = sao.SpecificationAttributeId
					WHERE psm.ProductId = fsatp.ProductId
					) < @SpecificationAttributesCount
		END
	END

	CREATE TABLE #FilteredProductAttributesToProduct (
		ProductId INT NOT NULL
		,AttributesCount INT NOT NULL
		)

	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredProductAttributesToProduct_ProductId ON #FilteredProductAttributesToProduct (ProductId)

	IF @ProductAttributesCount > 0
	BEGIN
		IF @ProductAttributesCount > 1
		BEGIN
			INSERT INTO #FilteredProductAttributesToProduct
			SELECT ppm.ProductId
				,COUNT(DISTINCT ppm.ProductAttributeId)
			FROM Product_ProductAttribute_Mapping ppm
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = ppm.ProductId
				OR p.ChildProductId = ppm.ProductId
			INNER JOIN #FilteredProductVariantAttributes fpva ON fpva.ProductVariantAttributeId = ppm.Id
			GROUP BY ppm.ProductId
			HAVING COUNT(DISTINCT ppm.ProductAttributeId) >= @ProductAttributesCount - 1
		END

		IF @ProductAttributesCount = 1
		BEGIN
			INSERT INTO #FilteredProductAttributesToProduct
			SELECT DISTINCT ppm.ProductId
				,1
			FROM Product_ProductAttribute_Mapping ppm
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = ppm.ProductId
				OR p.ChildProductId = ppm.ProductId
			INNER JOIN #FilteredProductVariantAttributes fpva ON fpva.ProductVariantAttributeId = ppm.Id

			INSERT INTO #FilteredProductAttributesToProduct
			SELECT DISTINCT ppm.ProductId
				,0
			FROM Product_ProductAttribute_Mapping ppm
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = ppm.ProductId
				OR p.ChildProductId = ppm.ProductId
			INNER JOIN #FilteredProductAttributes fa ON fa.AttributeId = ppm.ProductAttributeId
			WHERE ppm.ProductId NOT IN (
					SELECT ProductId
					FROM #FilteredProductAttributesToProduct
					)
		END

		IF @ProductAttributesCount > 1
		BEGIN
			DELETE #FilteredProductAttributesToProduct
			FROM #FilteredProductAttributesToProduct fpatp
			WHERE (
					SELECT COUNT(DISTINCT ppm.ProductAttributeId)
					FROM Product_ProductAttribute_Mapping ppm
					INNER JOIN #FilteredProductAttributes fa ON fa.AttributeId = ppm.ProductAttributeId
					WHERE ppm.ProductId = fpatp.ProductId
					) < @ProductAttributesCount
		END
	END

	EXEC sp_executesql @sql

	CREATE TABLE #PageIndex (
		[IndexId] INT IDENTITY(1, 1) NOT NULL
		,[ProductId] INT NOT NULL
		,[ChildProductId] INT
		)

	INSERT INTO #PageIndex (
		[ProductId]
		,[ChildProductId]
		)
	SELECT ProductId
		,ChildProductId
	FROM #DisplayOrderTmp
	GROUP BY ProductId
		,ChildProductId
	ORDER BY MIN([Id])

	SET @TotalRecords = @@rowcount
	IF @LoadAvailableFilters = 1
	BEGIN
	IF  @LoadFilterableSpecificationAttributeOptionIds = 1
	BEGIN
		CREATE TABLE #PotentialProductSpecificationAttributeIds (
			[ProductId] INT NOT NULL
			,[SpecificationAttributeOptionId] INT NOT NULL
			)
	
			INSERT INTO #PotentialProductSpecificationAttributeIds (
				[ProductId]
				,[SpecificationAttributeOptionId]
				)
			SELECT psm.ProductId
				,psm.SpecificationAttributeOptionId
			FROM Product_SpecificationAttribute_Mapping psm
			INNER JOIN #FilteredSpecificationAttributesToProduct fsatp ON fsatp.ProductId = psm.ProductId
			INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId
			INNER JOIN #FilteredSpecificationAttributes fsa ON fsa.AttributeId = sao.SpecificationAttributeId
			WHERE fsatp.AttributesCount = @SpecificationAttributesCount - 1
				AND sao.SpecificationAttributeId NOT IN (
					SELECT sao.SpecificationAttributeId
					FROM Product_SpecificationAttribute_Mapping psm1
					INNER JOIN SpecificationAttributeOption sao1 ON sao1.Id = psm1.SpecificationAttributeOptionId
					INNER JOIN #FilteredSpecificationAttributeOptions fs ON fs.SpecificationAttributeOptionId = sao.Id
					WHERE psm1.ProductId = psm.ProductId
					)
	
		IF @ProductAttributesCount > 0
		BEGIN
			DELETE #PotentialProductSpecificationAttributeIds
			FROM #PotentialProductSpecificationAttributeIds ppsa
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = ppsa.ProductId
			WHERE (
					pibfa.ChildProductId = 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							) != @ProductAttributesCount
						)
					)
				OR (
					pibfa.ChildProductId != 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							) != @ProductAttributesCount
						)
					)
		END

		IF @ManufacturersCount > 0
		BEGIN
			DELETE
			FROM #PotentialProductSpecificationAttributeIds
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product_Manufacturer_Mapping [pmm]
					INNER JOIN #FilteredManufacturers [fm] ON [fm].ManufacturerId = [pmm].ManufacturerId
					WHERE [pmm].ProductId = #PotentialProductSpecificationAttributeIds.ProductId
					)
		END



		IF @VendorsCount > 0
		BEGIN
			DELETE
			FROM #PotentialProductSpecificationAttributeIds
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product [p]
					INNER JOIN #FilteredVendorIds [fv] ON [fv].VendorId = [p].VendorId
					WHERE [p].Id = #PotentialProductSpecificationAttributeIds.ProductId
					)
		END;

		WITH FilterableSpecsCTE (ProductId, SpecificationAttributeOptionId) AS (
				SELECT	DISTINCT [psam].ProductId, [psam].SpecificationAttributeOptionId 
				FROM [Product_SpecificationAttribute_Mapping] [psam] WITH (NOLOCK)
				WHERE EXISTS (SELECT 1 FROM #PageIndex [pi] WHERE ([psam].[ProductId] = [pi].ProductId)) AND [psam].[AllowFiltering] = 1 
				UNION ALL 
				SELECT DISTINCT ProductId, SpecificationAttributeOptionId 
				FROM #PotentialProductSpecificationAttributeIds
			), FilterableSpecsDistinctCTE (SpecificationAttributeOptionId,FilterCount) AS 
			(
				SELECT DISTINCT FilterableSpecsCTE.SpecificationAttributeOptionId, COUNT(ProductId) FROM FilterableSpecsCTE  GROUP BY SpecificationAttributeOptionId
			)

		SELECT @FilterableSpecificationAttributeOptionIds = COALESCE(@FilterableSpecificationAttributeOptionIds + ',', '') + (CAST(SpecificationAttributeOptionId AS VARCHAR(4000)) + ':' +  CAST(FilterCount AS VARCHAR(4000)))
		FROM FilterableSpecsDistinctCTE

		DROP TABLE #PotentialProductSpecificationAttributeIds

	END
	IF  @LoadFilterableProductVariantAttributeIds = 1
	BEGIN
		CREATE TABLE #PotentialProductVariantAttributeIds (
			[ProductId] INT NOT NULL
			,[ProductVariantAttributeId] INT NOT NULL
			)

		CREATE INDEX IX_PotentialProductVariantAttributeIds_ProductId ON #PotentialProductVariantAttributeIds (ProductId);

		INSERT INTO #PotentialProductVariantAttributeIds (
			[ProductId]
			,[ProductVariantAttributeId]
			)
		SELECT [ppm].ProductId
			,[ppm].Id
		FROM Product_ProductAttribute_Mapping [ppm]
		INNER JOIN #FilteredProductAttributesToProduct fpatp ON fpatp.ProductId = [ppm].ProductId
		INNER JOIN #FilteredProductAttributes fa ON fa.AttributeId = ppm.ProductAttributeId
		WHERE fpatp.AttributesCount = @ProductAttributesCount - 1
			AND [ppm].Id NOT IN (
				SELECT ProductVariantAttributeId
				FROM #FilteredProductVariantAttributes
				)

		IF @SpecificationAttributesCount > 0
		BEGIN
			DELETE #PotentialProductVariantAttributeIds
			FROM #PotentialProductVariantAttributeIds ppva
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = ppva.ProductId
				OR pibfa.ChildProductId = ppva.ProductId
			WHERE (
					NOT EXISTS (
						SELECT NULL
						FROM #FilteredSpecificationAttributesToProduct
						WHERE ProductId = pibfa.ProductId
						)
					OR (
						SELECT AttributesCount
						FROM #FilteredSpecificationAttributesToProduct
						WHERE ProductId = pibfa.ProductId
						) != @SpecificationAttributesCount
					)
		END

		IF @ManufacturersCount > 0
		BEGIN
			DELETE
			FROM #PotentialProductVariantAttributeIds
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product_Manufacturer_Mapping pmm
					INNER JOIN #FilteredManufacturers fm ON fm.ManufacturerId = pmm.ManufacturerId
					INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = pmm.ProductId
					WHERE #PotentialProductVariantAttributeIds.ProductId = pibfa.ProductId
						OR #PotentialProductVariantAttributeIds.ProductId = pibfa.ChildProductId
					)
		END



		IF @VendorsCount > 0
		BEGIN
			DELETE
			FROM #PotentialProductVariantAttributeIds
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product [p]
					INNER JOIN #FilteredVendorIds [fv] ON [fv].VendorId = [p].VendorId
					INNER JOIN #ProductIdsBeforeFiltersApplied ON #PotentialProductVariantAttributeIds.ProductId = #ProductIdsBeforeFiltersApplied.ProductId
						OR #PotentialProductVariantAttributeIds.ProductId = #ProductIdsBeforeFiltersApplied.ChildProductId
					WHERE [p].Id = #ProductIdsBeforeFiltersApplied.ProductId
						OR [p].Id = #ProductIdsBeforeFiltersApplied.ChildProductId
					)
		END;


		--WITH FilterableProductVariantIdsCTE (ProductId, ProductVariantAttributeId) AS (
		--	SELECT DISTINCT [ppm].ProductId,[ppm].Id 
		--	FROM [Product_ProductAttribute_Mapping] [ppm] WITH (NOLOCK)
		--	WHERE(EXISTS (SELECT 1 FROM #PageIndex [pi] WHERE [pi].ProductId = [ppm].[ProductId] OR [pi].ChildProductId = [ppm].ProductId)) 
		--	UNION ALL
		--	SELECT DISTINCT #PotentialProductVariantAttributeIds.ProductId,#PotentialProductVariantAttributeIds.ProductVariantAttributeId 
		--	FROM #PotentialProductVariantAttributeIds
		--	), FilterableProductVariantIdsDistinctCTE (ProductVariantAttributeId,FIlterCount) AS (
		--	SELECT DISTINCT FilterableProductVariantIdsCTE.ProductVariantAttributeId , COUNT(ProductId)
		--	FROM FilterableProductVariantIdsCTE GROUP BY ProductVariantAttributeId
		--) 

		--SELECT @FilterableProductVariantAttributeIds = COALESCE(@FilterableProductVariantAttributeIds + ',', '') + (CAST(ProductVariantAttributeId AS VARCHAR(4000)) + ':' +  CAST(FIlterCount AS VARCHAR(4000)))
		--FROM FilterableProductVariantIdsDistinctCTE

	WITH FilterableProductVariantIdsCTE (ProductId, ProductVariantAttributeId) AS (
			SELECT DISTINCT [ppm].ProductId,[pav].Id 
			FROM [Product_ProductAttribute_Mapping] [ppm] WITH (NOLOCK)
			INNER JOIN [ProductAttributeValue] [pav] ON  [pav].ProductAttributeMappingId = [ppm].Id
			WHERE(EXISTS (SELECT 1 FROM #PageIndex [pi] WHERE [pi].ProductId = [ppm].[ProductId] OR [pi].ChildProductId = [ppm].ProductId)) 
			UNION ALL
			SELECT DISTINCT #PotentialProductVariantAttributeIds.ProductId,#PotentialProductVariantAttributeIds.ProductVariantAttributeId 
			FROM #PotentialProductVariantAttributeIds
			), FilterableProductVariantIdsDistinctCTE (ProductVariantAttributeId,FIlterCount) AS (
			SELECT DISTINCT FilterableProductVariantIdsCTE.ProductVariantAttributeId , COUNT(ProductId)
			FROM FilterableProductVariantIdsCTE GROUP BY ProductVariantAttributeId
		) 

		SELECT @FilterableProductVariantAttributeIds = COALESCE(@FilterableProductVariantAttributeIds + ',', '') + (CAST(ProductVariantAttributeId AS VARCHAR(4000)) + ':' +  CAST(FIlterCount AS VARCHAR(4000)))
		FROM FilterableProductVariantIdsDistinctCTE

		DROP TABLE #PotentialProductVariantAttributeIds
	END
	IF @LoadFilterableCategoryIds = 1
	BEGIN
		CREATE TABLE #FilterableCategories (
			[ProductId] INT NOT NULL
			,[CategoryId] INT NOT NULL
			)

		INSERT INTO #FilterableCategories (
			[ProductId]
			,[CategoryId]
			)
		SELECT DISTINCT [pcm].ProductId
			,[pcm].CategoryId
		FROM Product_Category_Mapping [pcm]
		INNER JOIN #ProductIdsBeforeFiltersApplied ON #ProductIdsBeforeFiltersApplied.ProductId = [pcm].ProductId

		IF @SpecificationAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableCategories
			FROM #FilterableCategories fm
			LEFT JOIN #FilteredSpecificationAttributesToProduct fsatp ON fsatp.ProductId = fm.ProductId
			WHERE fsatp.ProductId IS NULL
				OR fsatp.AttributesCount != @SpecificationAttributesCount
		END

		IF @ProductAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableCategories
			FROM #FilterableCategories fm
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = fm.ProductId
			WHERE (
					pibfa.ChildProductId = 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							) != @ProductAttributesCount
						)
					)
				OR (
					pibfa.ChildProductId != 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							) != @ProductAttributesCount
						)
					)
		END

		IF @ManufacturersCount > 0
		BEGIN
			DELETE
			FROM #FilterableCategories
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product_Manufacturer_Mapping [pmm]
					INNER JOIN #FilteredManufacturers [fm] ON [fm].ManufacturerId = [pmm].ManufacturerId
					WHERE [pmm].ProductId = #FilterableCategories.ProductId
					)
		END

		IF @VendorsCount > 0
		BEGIN
			DELETE
			FROM #FilterableCategories
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product [p]
					INNER JOIN #FilteredVendorIds [fv] ON fv.VendorId = [p].VendorId
					WHERE [p].Id = #FilterableCategories.ProductId
					)
		END

		SELECT @FilterableCategoryIds = COALESCE(@FilterableCategoryIds + ',', '') + (CAST(CategoryId AS VARCHAR(4000)) + ':' +  CAST(COUNT(ProductId) AS VARCHAR(4000)))
		FROM #FilterableCategories GROUP BY CategoryId
	END

	IF @LoadFilterableProductReviewIds = 1
	BEGIN
		CREATE TABLE #FilterableRating (
		[ProductId] INT NOT NULL
		,[ApprovedRatingSum] INT NOT NULL
		)

		INSERT INTO #FilterableRating (
			[ProductId]
			,[ApprovedRatingSum]
			)
		SELECT DISTINCT [p].Id,[p].ApprovedRatingSum
		FROM Product [p]
		INNER JOIN #ProductIdsBeforeFiltersApplied ON #ProductIdsBeforeFiltersApplied.ProductId = [p].Id

		IF @SpecificationAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableRating
			FROM #FilterableRating fm
			LEFT JOIN #FilteredSpecificationAttributesToProduct fsatp ON fsatp.ProductId = fm.ProductId
			WHERE fsatp.ProductId IS NULL
				OR fsatp.AttributesCount != @SpecificationAttributesCount
		END

		IF @ProductAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableRating
			FROM #FilterableRating fm
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = fm.ProductId
			WHERE (
					pibfa.ChildProductId = 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							) != @ProductAttributesCount
						)
					)
				OR (
					pibfa.ChildProductId != 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							) != @ProductAttributesCount
						)
					)
		END

		IF @ManufacturersCount > 0
		BEGIN
			DELETE
			FROM #FilterableRating
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product_Manufacturer_Mapping [pmm]
					INNER JOIN #FilteredManufacturers [fm] ON [fm].ManufacturerId = [pmm].ManufacturerId
					WHERE [pmm].ProductId = #FilterableRating.ProductId
					)
		END


		IF @VendorsCount > 0
		BEGIN
			DELETE
			FROM #FilterableRating
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product [p]
					INNER JOIN #FilteredVendorIds [fv] ON fv.VendorId = [p].VendorId
					WHERE [p].Id = #FilterableRating.ProductId
					)
		END

		SELECT @FilterableRatingIds = COALESCE(@FilterableRatingIds + ',', '') + (CAST(ApprovedRatingSum AS VARCHAR(4000)) + ':' +  CAST(COUNT(ProductId) AS VARCHAR(4000)))
		FROM #FilterableRating GROUP BY ApprovedRatingSum
	END

	IF  @LoadFilterableManufacturerIds = 1
	BEGIN
		CREATE TABLE #FilterableManufacturers (
			[ProductId] INT NOT NULL
			,[ManufacturerId] INT NOT NULL
			)
			
		INSERT INTO #FilterableManufacturers (
			[ProductId]
			,[ManufacturerId]
			)
		SELECT DISTINCT [pmm].ProductId
			,[pmm].ManufacturerId
		FROM Product_Manufacturer_Mapping [pmm]
		INNER JOIN #ProductIdsBeforeFiltersApplied ON #ProductIdsBeforeFiltersApplied.ProductId = [pmm].ProductId

		IF @SpecificationAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableManufacturers
			FROM #FilterableManufacturers fm
			LEFT JOIN #FilteredSpecificationAttributesToProduct fsatp ON fsatp.ProductId = fm.ProductId
			WHERE fsatp.ProductId IS NULL
				OR fsatp.AttributesCount != @SpecificationAttributesCount
		END

		IF @ProductAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableManufacturers
			FROM #FilterableManufacturers fm
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = fm.ProductId
			WHERE (
					pibfa.ChildProductId = 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							) != @ProductAttributesCount
						)
					)
				OR (
					pibfa.ChildProductId != 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							) != @ProductAttributesCount
						)
					)
		END

		IF @VendorsCount > 0
		BEGIN
			DELETE
			FROM #FilterableManufacturers
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product [p]
					INNER JOIN #FilteredVendorIds [fv] ON fv.VendorId = [p].VendorId
					WHERE [p].Id = #FilterableManufacturers.ProductId
					)
		END;

		With  FilterableManufacturersDistinctCTE(ManufacturerId,FilterCount) AS (
			SELECT DISTINCT ManufacturerId,COUNT(ProductId) From #FilterableManufacturers GROUP BY ManufacturerId
		)
		SELECT @FilterableManufacturerIds = COALESCE(@FilterableManufacturerIds + ',', '') + (CAST(ManufacturerId AS VARCHAR(4000)) + ':' +  CAST(FilterCount AS VARCHAR(4000)))
		FROM FilterableManufacturersDistinctCTE
		DROP TABLE #FilterableManufacturers
	END
	IF  @LoadFilterableVendorIds = 1
	BEGIN
		CREATE TABLE #FilterableVendors (
			[ProductId] INT NOT NULL
			,[VendorId] INT NOT NULL
			)

		INSERT INTO #FilterableVendors (
			[ProductId]
			,[VendorId]
			)
		SELECT DISTINCT [pv].Id
			,[pv].VendorId
		FROM Product [pv]
		INNER JOIN #ProductIdsBeforeFiltersApplied ON #ProductIdsBeforeFiltersApplied.ProductId = [pv].Id

		IF @SpecificationAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableVendors
			FROM #FilterableVendors fv
			LEFT JOIN #FilteredSpecificationAttributesToProduct fsatp ON fsatp.ProductId = fv.ProductId
			WHERE fsatp.ProductId IS NULL
				OR fsatp.AttributesCount != @SpecificationAttributesCount
		END

		IF @ProductAttributesCount > 0
		BEGIN
			DELETE
			FROM #FilterableVendors
			FROM #FilterableVendors fv
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = fv.ProductId
			WHERE (
					pibfa.ChildProductId = 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ProductId
							) != @ProductAttributesCount
						)
					)
				OR (
					pibfa.ChildProductId != 0
					AND (
						NOT EXISTS (
							SELECT NULL
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							)
						OR (
							SELECT AttributesCount
							FROM #FilteredProductAttributesToProduct
							WHERE ProductId = pibfa.ChildProductId
							) != @ProductAttributesCount
						)
					)
		END


		IF @ManufacturersCount > 0
		BEGIN
			DELETE
			FROM #FilterableVendors
			WHERE NOT EXISTS (
					SELECT NULL
					FROM Product_Manufacturer_Mapping [pmm]
					INNER JOIN #FilteredManufacturers [fm] ON [fm].ManufacturerId = [pmm].ManufacturerId
					WHERE [pmm].ProductId = #FilterableVendors.ProductId
					)
		END;
		
		With  FilterableVendorsDistinctCTE(VendorId,FilterCount) AS (
			SELECT DISTINCT VendorId,COUNT(ProductId) From #FilterableVendors GROUP BY VendorId
		)
		SELECT @FilterableVendorIds = COALESCE(@FilterableVendorIds + ',', '') + (CAST(VendorId AS VARCHAR(4000)) + ':' +  CAST(FilterCount AS VARCHAR(4000)))
		FROM FilterableVendorsDistinctCTE
		DROP TABLE #FilterableVendors
	END

		DROP TABLE #ProductIdsBeforeFiltersApplied
		DROP TABLE #FilteredSpecificationAttributeOptions
		DROP TABLE #FilteredSpecificationAttributes
		DROP TABLE #FilteredSpecificationAttributesToProduct
		
		DROP TABLE #FilteredProductVariantAttributes
		DROP TABLE #FilteredProductAttributes
		DROP TABLE #FilteredProductAttributesToProduct
		DROP TABLE #FilteredManufacturers
		
	END

	DELETE #PageIndex
	FROM #PageIndex
	LEFT OUTER JOIN (
		SELECT MIN(IndexId) AS RowId
			,ProductId
		FROM #PageIndex
		GROUP BY ProductId
		) AS KeepRows ON #PageIndex.IndexId = KeepRows.RowId
	WHERE KeepRows.RowId IS NULL

	SET @TotalRecords = @TotalRecords - @@rowcount

	CREATE TABLE #PageIndexDistinct (
		[IndexId] INT IDENTITY(1, 1) NOT NULL
		,[ProductId] INT NOT NULL
		)

	INSERT INTO #PageIndexDistinct ([ProductId])
	SELECT [ProductId]
	FROM #PageIndex
	ORDER BY [IndexId]

	IF @LoadFilterableOnSale = 1
	BEGIN
		SELECT @ProductsOnSale  = (
				SELECT COUNT(1)
				FROM Product p
				LEFT JOIN Product cp ON p.Id = cp.ParentGroupedProductId
				INNER JOIN #PageIndexDistinct [pid] ON [pid].ProductId = p.Id
				WHERE (
						(
							cp.Id IS NULL
							AND p.OldPrice > 0
							AND p.Price != p.OldPrice
							)
						OR (
							cp.Id IS NOT NULL
							AND cp.OldPrice > 0
							AND cp.OldPrice != cp.Price
							)
						)
				)
	END

	IF @LoadFilterableInStock = 1
	BEGIN
		SELECT @ProductsInStock  =  (
				SELECT COUNT(1)
				FROM Product p
				LEFT JOIN Product cp ON p.Id = cp.ParentGroupedProductId
				INNER JOIN #PageIndexDistinct [pid] ON [pid].ProductId = p.Id
				WHERE (
						(
							cp.ID IS NULL
							AND (
								(p.ManageInventoryMethodId = 0)
								OR (
									P.ManageInventoryMethodId = 1
									AND (
										(
											p.StockQuantity > 0
											AND p.UseMultipleWarehouses = 0
											)
										OR (
											EXISTS (
												SELECT 1
												FROM ProductWarehouseInventory [pwi]
												WHERE [pwi].ProductId = p.Id
													AND [pwi].StockQuantity > 0
													AND [pwi].StockQuantity > [pwi].ReservedQuantity
												)
											AND p.UseMultipleWarehouses = 1
											)
										)
									)
								)
							)
						OR (
							p.Id IS NOT NULL
							AND (
								(cp.ManageInventoryMethodId = 0)
								OR (
									cp.ManageInventoryMethodId = 1
									AND (
										(
											cp.StockQuantity > 0
											AND cp.UseMultipleWarehouses = 0
											)
										OR (
											EXISTS (
												SELECT 1
												FROM ProductWarehouseInventory [pwi]
												WHERE [pwi].ProductId = cp.Id
													AND [pwi].StockQuantity > 0
													AND [pwi].StockQuantity > [pwi].ReservedQuantity
												)
											AND cp.UseMultipleWarehouses = 1
											)
										)
									)
								)
							)
						)
				)
	END

	SELECT TOP (@RowsToReturn) p.*
	FROM #PageIndexDistinct [pi]
	INNER JOIN Product p WITH (NOLOCK) ON p.Id = [pi].[ProductId]
	WHERE [pi].IndexId > @PageLowerBound
		AND [pi].IndexId < @PageUpperBound
	ORDER BY [pi].IndexId

	DROP TABLE #KeywordProducts
	DROP TABLE #FilteredCategoryIds
	DROP TABLE #FilteredVendorIds
	DROP TABLE #FilteredCustomerRoleIds
	DROP TABLE #DisplayOrderTmp
	DROP TABLE #PageIndex
	DROP TABLE #PageIndexDistinct
END
GO
