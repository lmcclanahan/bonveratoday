using Common;
using Common.Api.ExigoOData;
using Common.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System;
using Dapper;

namespace ExigoService
{
    public static partial class Exigo
    {
        // Web Category Calls
        public static ItemCategory GetItemCategory(int itemCategoryID)
        {
            var context = Exigo.OData();


            // Get the nodes
            var categories = new List<ItemCategory>();
            var rowcount = 50;
            var lastResultCount = rowcount;
            var callsMade = 0;

            while (lastResultCount == rowcount)
            {
                // Get the data
                var results = context.WebCategories
                    .Where(c => c.WebID == 1)
                    .OrderBy(c => c.ParentID)
                    .OrderBy(c => c.SortOrder)
                    .Skip(callsMade * rowcount)
                    .Take(rowcount)
                    .Select(c => c)
                    .ToList();

                results.ForEach(c =>
                {
                    categories.Add((ItemCategory)c);
                });

                callsMade++;
                lastResultCount = results.Count;
            }


            // Recursively populate the children
            var category = categories.Where(c => c.ItemCategoryID == itemCategoryID).FirstOrDefault();
            if (category == null) return null;

            category.Subcategories = GetItemCategorySubcategories(category, categories);

            return category;
        }
        public static List<ItemCategory> GetItemCategories(int topCategoryID)
        {
            var categories = GetItemNestedCategoriesRecursively(topCategoryID);

            return categories;
        }
        public static List<ItemCategory> GetItemNestedCategoriesRecursively(int topCategoryID, bool getChildren = true)
        {
            // Get the nodes
            var rawcategories = new List<ItemCategory>();
            var categories = new List<ItemCategory>();

            using (var context = Exigo.Sql())
            {
                var results = context.Query(@"
                		            SELECT * FROM WebCategories
                                    Where WebID = 1 AND ParentID = @topCategoryID
                                    Order by ParentID
                    ", new
                     {
                         topCategoryID = topCategoryID
                     }).ToList();

                results.ForEach(c =>
                {
                    var category = new ItemCategory
                    {
                        ItemCategoryID = c.WebCategoryID,
                        ItemCategoryDescription = c.WebCategoryDescription,
                        ItemCategoryViewName = c.WebCategoryDescription.Replace(" ", "").Replace("&", "_").ToLower(),
                        SortOrder = c.SortOrder,
                        ParentItemCategoryID = c.ParentID
                    };

                    if (getChildren)
                    {
                        var childCats = GetItemNestedCategoriesRecursively(c.WebCategoryID, false);
                        var childCategories = new List<ItemCategory>();
                        childCategories.AddRange(childCats);

                        if (childCategories.Count > 0)
                        {
                            childCategories.ForEach(child =>
                            {
                                child.ParentItemCategoryDescription = category.ItemCategoryDescription;
                                child.ParentItemCategoryViewName = category.ItemCategoryViewName;
                            });

                            category.Subcategories.AddRange(childCategories);
                        }
                    }
                    rawcategories.Add(category);
                });
            }

            return rawcategories;
        }
        private static List<ItemCategory> GetItemCategorySubcategories(ItemCategory parentCategory, List<ItemCategory> categories)
        {
            var subCategories = categories.Where(c => c.ParentItemCategoryID == parentCategory.ItemCategoryID).ToList();

            foreach (var subCategory in subCategories)
            {
                subCategory.Subcategories = GetItemCategorySubcategories(subCategory, categories);
            }

            return subCategories;
        }

        // Item call for product lists
        public static IEnumerable<Item> GetItems(GetItemsRequest request)
        {
            // If we don't have what we need to make this call, stop here.
            if (request.Configuration == null)
                throw new InvalidRequestException("ExigoService.GetItems() requires an OrderConfiguration.");

            if (request.Configuration.CategoryID == 0 && request.CategoryID == null && request.ItemCodes.Length == 0)
                throw new InvalidRequestException("ExigoService.GetItems() requires either a CategoryID or a collection of item codes."); ;


            // Set some defaults
            if (request.CategoryID == null && request.ItemCodes.Length == 0)
            {
                request.CategoryID = request.Configuration.CategoryID;
            }


            var tempCategoryIDs = new List<int>();
            var categoryIDs = new List<int>();
            if (request.CategoryID != null)
            {
                // Get all category ids underneath the request's category id
                if (request.IncludeChildCategories)
                {
                    using (var context = Exigo.Sql())
                    {
                        categoryIDs.AddRange(context.Query<int>(@"
                            WITH webcat (WebCategoryID, WebCategoryDescription, ParentID, NestedLevel) 
                                 AS (SELECT WebCategoryID, 
                                            WebCategoryDescription, 
                                            ParentID, 
                                            NestedLevel 
                                     FROM   WebCategories 
                                     WHERE  WebCategoryID = @masterCategoryID
                                            AND WebID = 1
                                     UNION ALL 
                                     SELECT w.WebCategoryID, 
                                            w.WebCategoryDescription, 
                                            w.ParentID, 
                                            w.NestedLevel 
                                     FROM   WebCategories w 
                                            INNER JOIN webcat c 
                                                    ON c.WebCategoryID = w.ParentID) 
                            SELECT WebCategoryID
                            FROM   webcat
                        ", new
                         {
                             masterCategoryID = request.CategoryID
                         }).ToList());
                    }
                }
                else
                {
                    categoryIDs.Add(Convert.ToInt32(request.CategoryID));
                }
            }

            // If we requested specific categories, get the item codes in the categories
            if (categoryIDs.Count > 0)
            {
                var categoryItemCodes = new List<string>();

                using (var context = Exigo.Sql())
                {
                    categoryItemCodes = context.Query<string>(@"
                        select distinct
	                        i.ItemCode
                        from WebCategoryItems c
	                        inner join Items i
		                        on c.ItemID = i.ItemID
	                        inner join WebCategories w
		                        on w.WebID = c.WebID
		                        and w.WebCategoryID = c.WebCategoryID
                        where 
	                        c.WebID = 1
	                        and c.WebCategoryID in @webcategoryids
                    ", new
                     {
                         webcategoryids = categoryIDs
                     }).ToList();
                }

                var existingItemCodes = request.ItemCodes.ToList();
                existingItemCodes.AddRange(categoryItemCodes);
                request.ItemCodes = existingItemCodes.ToArray();
            }

            // Do a final check to ensure if the category we are looking at does not contain a item directly nested within it, we pull back the first child category
            if (request.ItemCodes.Length == 0 && request.CategoryID != null)
            {
                var tempItemCodeList = new List<string>();
                using (var context = Exigo.Sql())
                {
                    tempItemCodeList = context.Query<string>(@"
                
                                        ;WITH webcat (WebCategoryID, WebCategoryDescription, ParentID, NestedLevel) 
				                            AS (SELECT WebCategoryID, 
						                            WebCategoryDescription, 
						                            ParentID, 
						                            NestedLevel 
					                            FROM   WebCategories 
					                            WHERE  WebCategoryID = @masterCategoryID
						                            AND WebID = 1
					                            UNION ALL 
					                            SELECT w.WebCategoryID, 
						                            w.WebCategoryDescription, 
						                            w.ParentID, 
						                            w.NestedLevel 
					                            FROM   WebCategories w 
					                            INNER JOIN webcat c 
						                            ON c.WebCategoryID = w.ParentID) 
                                        select i.ItemCode
                                        from WebCategoryItems c
	                                        inner join Items i
		                                        on c.ItemID = i.ItemID
                                        where c.WebCategoryID = (select top 1 WebCategoryID 
						                from webcat where ParentID = @masterCategoryID 
						                ORDER BY CASE WHEN WebCategoryDescription like 'Special%' THEN 0 ELSE 1 END asc)
                                        ", new
                                         {
                                             masterCategoryID = request.CategoryID
                                         }).ToList();
                }

                request.ItemCodes = tempItemCodeList.ToArray();
            }


            // If we don't have any items, stop here.
            if (request.ItemCodes.Length == 0) yield break;
            // get the item information
            var items = new List<ExigoService.Item>();

            var priceTypeID = (request.PriceTypeID > 0) ? request.PriceTypeID : request.Configuration.PriceTypeID;


            try
            {
                using (var context = Exigo.Sql())
                {
                    items = context.Query<ExigoService.Item>(@"
                			SELECT
	                            ItemID = i.ItemID,
	                            ItemCode = i.ItemCode,
	                            ItemDescription = 
		                            case 
			                            when i.IsGroupMaster = 1 then COALESCE(i.GroupDescription, il.ItemDescription, i.ItemDescription)
			                            when il.ItemDescription != '' then COALESCE(il.ItemDescription, i.ItemDescription)
							            else i.ItemDescription
		                            end,
	                            Weight = i.Weight,
	                            ItemTypeID = i.ItemTypeID,
	                            TinyImageUrl = i.TinyImageName,
	                            SmallImageUrl = i.SmallImageName,
	                            LargeImageUrl = i.LargeImageName,
	                            ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
	                            ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
	                            ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
	                            ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),
	                            LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
	                            LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
	                            LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
	                            LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),
	                            IsVirtual = i.IsVirtual,
	                            AllowOnAutoOrder = i.AllowOnAutoOrder,
	                            IsGroupMaster = i.IsGroupMaster,
	                            IsDynamicKitMaster = cast(case when i.ItemTypeID = 2 then 1 else 0 end as bit),
	                            GroupMasterItemDescription = i.GroupDescription,
	                            GroupMembersDescription = i.GroupMembersDescription,
	                            Field1 = i.Field1,
	                            Field2 = i.Field2,
	                            Field3 = i.Field3,
	                            Field4 = i.Field4,
	                            Field5 = i.Field5,
	                            Field6 = i.Field6,
	                            Field7 = i.Field7,
	                            Field8 = i.Field8,
	                            Field9 = i.Field9,
	                            Field10 = i.Field10,
	                            OtherCheck1 = i.OtherCheck1,
	                            OtherCheck2 = i.OtherCheck2,
	                            OtherCheck3 = i.OtherCheck3,
	                            OtherCheck4 = i.OtherCheck4,
	                            OtherCheck5 = i.OtherCheck5,
	                            Price = ip.Price,
	                            CurrencyCode = ip.CurrencyCode,
	                            BV = ip.BusinessVolume,
	                            CV = ip.CommissionableVolume,
	                            OtherPrice1 = ip.Other1Price,
	                            OtherPrice2 = ip.Other2Price,
	                            OtherPrice3 = ip.Other3Price,
	                            OtherPrice4 = ip.Other4Price,
	                            OtherPrice5 = ip.Other5Price,
	                            OtherPrice6 = ip.Other6Price,
	                            OtherPrice7 = ip.Other7Price,
	                            OtherPrice8 = ip.Other8Price,
	                            OtherPrice9 = ip.Other9Price,
	                            OtherPrice10 = ip.Other10Price,
					            a.Price AS RetailPrice,
					            a.PriceTypeID AS PriceTypeID1,
	                            b.Price AS SmartShopperPrice,
					            b.PriceTypeID AS PriceTypeID2
                            FROM Items i
	                            INNER JOIN ItemPrices ip
		                            ON ip.ItemID = i.ItemID
		                            AND ip.PriceTypeID = @priceTypeID
						            AND ip.CurrencyCode = @currencyCode
                                INNER JOIN ItemPrices a
		                            ON a.ItemID = ip.ItemID
                                    AND a.CurrencyCode = @currencyCode
                                    AND a.PriceTypeID = @FirstPriceTypeID
					            INNER JOIN ItemPrices b					
					                ON b.ItemID = ip.ItemID  
						            AND b.CurrencyCode = @currencyCode
						            AND b.PriceTypeID = @SecondPriceTypeID
	                            INNER JOIN ItemWarehouses iw
		                            ON iw.ItemID = i.ItemID
		                            AND iw.WarehouseID = @warehouse
						            LEFT JOIN ItemLanguages il
		                            ON il.ItemID = i.ItemID
						            AND il.LanguageID = @languageID
					            WHERE i.ItemCode in @itemCodes
                                ", new
                                 {
                                     warehouse = request.Configuration.WarehouseID,
                                     currencyCode = request.Configuration.CurrencyCode,
                                     languageID = request.LanguageID,
                                     itemCodes = request.ItemCodes,
                                     priceTypeID = priceTypeID,
                                     FirstPriceTypeID = PriceTypes.Retail,
                                     SecondPriceTypeID = PriceTypes.Wholesale
                                 }).ToList();
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }

            if (items.Any())
            {
                // Populate the group members and dynamic kits
                PopulateAdditionalItemData(items, request.LanguageID, request.Configuration.CurrencyCode, request.Configuration.WarehouseID);
            }

            // Return the data
            foreach (var item in items)
            {
                yield return item;
            }
        }

        // Item call for shopping cart items
        public static List<Item> GetItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IOrderConfiguration configuration, int languageID, int _priceTypeID = 0)
        {
            var results = new List<Item>();

            // If we don't have what we need to make this call, stop here.
            if (configuration == null)
                throw new InvalidRequestException("ExigoService.GetItems() requires an OrderConfiguration.");

            if (shoppingCartItems.Count() == 0)
                return results;


            // Create the contexts we will use
            var priceTypeID = (_priceTypeID > 0) ? _priceTypeID : configuration.PriceTypeID;

            var itemcodes = new List<string>();

            shoppingCartItems.ToList().ForEach(c => itemcodes.Add(c.ItemCode));

            var apiItems = new List<ExigoService.Item>();
            try
            {
                using (var context = Exigo.Sql())
                {
                    apiItems = context.Query<ExigoService.Item>(@"
                			SELECT
	                        ItemID = i.ItemID,
	                        ItemCode = i.ItemCode,
	                        ItemDescription = 
		                        case 
			                        when i.IsGroupMaster = 1 then COALESCE(i.GroupDescription, il.ItemDescription, i.ItemDescription)
			                        when il.ItemDescription != '' then COALESCE(il.ItemDescription, i.ItemDescription)
							        else i.ItemDescription
		                        end,
	                        Weight = i.Weight,
	                        ItemTypeID = i.ItemTypeID,
	                        TinyImageUrl = i.TinyImageName,
	                        SmallImageUrl = i.SmallImageName,
	                        LargeImageUrl = i.LargeImageName,
	                        ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
	                        ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
	                        ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
	                        ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),
	                        LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
	                        LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
	                        LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
	                        LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),
	                        IsVirtual = i.IsVirtual,
	                        AllowOnAutoOrder = i.AllowOnAutoOrder,
	                        IsGroupMaster = i.IsGroupMaster,
	                        IsDynamicKitMaster = cast(case when i.ItemTypeID = 2 then 1 else 0 end as bit),
	                        GroupMasterItemDescription = i.GroupDescription,
	                        GroupMembersDescription = i.GroupMembersDescription,
	                        Field1 = i.Field1,
	                        Field2 = i.Field2,
	                        Field3 = i.Field3,
	                        Field4 = i.Field4,
	                        Field5 = i.Field5,
	                        Field6 = i.Field6,
	                        Field7 = i.Field7,
	                        Field8 = i.Field8,
	                        Field9 = i.Field9,
	                        Field10 = i.Field10,
	                        OtherCheck1 = i.OtherCheck1,
	                        OtherCheck2 = i.OtherCheck2,
	                        OtherCheck3 = i.OtherCheck3,
	                        OtherCheck4 = i.OtherCheck4,
	                        OtherCheck5 = i.OtherCheck5,
	                        Auto1 = i.Auto1,
	                        Auto2 = i.Auto2,
	                        Auto3 = i.Auto3,
	                        Price = ip.Price,
	                        CurrencyCode = ip.CurrencyCode,
	                        BV = ip.BusinessVolume,
	                        CV = ip.CommissionableVolume,
	                        OtherPrice1 = ip.Other1Price,
	                        OtherPrice2 = ip.Other2Price,
	                        OtherPrice3 = ip.Other3Price,
	                        OtherPrice4 = ip.Other4Price,
	                        OtherPrice5 = ip.Other5Price,
	                        OtherPrice6 = ip.Other6Price,
	                        OtherPrice7 = ip.Other7Price,
	                        OtherPrice8 = ip.Other8Price,
	                        OtherPrice9 = ip.Other9Price,
	                        OtherPrice10 = ip.Other10Price,
					        a.Price AS RetailPrice,
					        a.PriceTypeID AS PriceTypeID1,
	                        b.Price AS SmartShopperPrice,
					        b.PriceTypeID AS PriceTypeID2
                        FROM Items i
	                        INNER JOIN ItemPrices ip
		                        ON ip.ItemID = i.ItemID
		                        AND ip.PriceTypeID = @priceTypeID
						        AND ip.CurrencyCode = @currencyCode
                        INNER JOIN ItemPrices a
		                        ON a.ItemID = ip.ItemID
                                AND a.CurrencyCode = @currencyCode
                                AND a.PriceTypeID = @FirstPriceTypeID
					        INNER JOIN ItemPrices b					
					            ON b.ItemID = ip.ItemID
						        AND b.CurrencyCode = @currencyCode
						        AND b.PriceTypeID = @SecondPriceTypeID
	                        INNER JOIN ItemWarehouses iw
		                        ON iw.ItemID = i.ItemID
		                        AND iw.WarehouseID = @warehouse
						        LEFT JOIN ItemLanguages il
		                        ON il.ItemID = i.ItemID
						        AND il.LanguageID = @languageID
					        WHERE i.ItemCode in @itemCodes
                        ", new
                            {
                                warehouse = configuration.WarehouseID,
                                currencyCode = configuration.CurrencyCode,
                                languageID = languageID,
                                itemCodes = itemcodes,
                                priceTypeID = priceTypeID,
                                FirstPriceTypeID = PriceTypes.Retail,
                                SecondPriceTypeID = PriceTypes.Wholesale
                            }).ToList();
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }

            if (apiItems.Any())
            {
                // Populate the group members and dynamic kits
                PopulateAdditionalItemData(apiItems, languageID, configuration.CurrencyCode, configuration.WarehouseID);
            }

            foreach (var apiItem in apiItems)
            {
                var cartItems = shoppingCartItems.Where(c => c.ItemCode == apiItem.ItemCode).ToList();
                foreach (var cartItem in cartItems)
                {
                    var newItem = apiItem;
                    newItem.ID = cartItem.ID;
                    newItem.Quantity = cartItem.Quantity;
                    newItem.ParentItemCode = cartItem.ParentItemCode;
                    newItem.GroupMasterItemCode = cartItem.GroupMasterItemCode;
                    newItem.DynamicKitCategory = cartItem.DynamicKitCategory;
                    newItem.Type = cartItem.Type;
                    results.Add(newItem);
                }
            }
            // Return the data
            return results;
        }

        // Item Detail page
        public static Item GetItemDetail(GetItemDetailRequest request)
        {
            // If we don't have what we need to make this call, stop here.
            if (request.Configuration == null)
                throw new InvalidRequestException("ExigoService.GetItemDetail() requires an OrderConfiguration.");

            if (request.ItemCode.IsNullOrEmpty())
                throw new InvalidRequestException("ExigoService.GetItemDetail() requires an item code."); ;

            if (request.PriceTypeID > 0)
            {
                request.Configuration.PriceTypeID = request.PriceTypeID;
            }

            // Get the item details
            var query = GetItemsQueryable(request.Configuration, new[] { request.ItemCode }, "Item");
            var apiItem = query.Select(c => c).FirstOrDefault();
            var item = (ExigoService.Item)apiItem;


            // Populate the group members and dynamic kits
            PopulateAdditionalItemData(new[] { item }, request.LanguageID, request.CurrencyCode, request.WarehouseID);


            // Return the converted item
            return item;
        }
        private static IQueryable<ItemWarehousePrice> GetItemsQueryable(IOrderConfiguration configuration, IEnumerable<string> itemCodes, params string[] expansions)
        {
            var query = Exigo.OData().ItemWarehousePrices;
            if (expansions != null && expansions.Length > 0)
            {
                query = query.Expand(string.Join(",", expansions));
            }

            return query
                    .Where(c => c.WarehouseID == configuration.WarehouseID)
                    .Where(c => c.PriceTypeID == configuration.PriceTypeID)
                    .Where(c => c.CurrencyCode == configuration.CurrencyCode)
                    .Where(itemCodes.ToList().ToOrExpression<ItemWarehousePrice, string>("Item.ItemCode"))
                    .AsQueryable();
        }



        // Calls to populate additional data
        private static void PopulateAdditionalItemData(IEnumerable<Item> items, int languageID, string currencyCode, int warehouseID)
        {
            GlobalUtilities.RunAsyncTasks(
                () => { PopulateItemImages(items); },
                () => { PopulateGroupMembers(items, languageID, currencyCode, warehouseID); },
                () => { PopulateDynamicKitMembers(items); }
                //,
                //() => { PopulateAdditionalPrices(items, currencyCode); }
            );
        }
        private static void PopulateItemImages(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.TinyImageUrl = GlobalUtilities.GetProductImagePath(item.TinyImageUrl);
                item.SmallImageUrl = GlobalUtilities.GetProductImagePath(item.SmallImageUrl);
                item.LargeImageUrl = GlobalUtilities.GetProductImagePath(item.LargeImageUrl);
            }
        }
        private static void PopulateGroupMembers(IEnumerable<Item> items, int languageID, string currencyCode, int warehouseID)
        {
            try
            {
                // Determine if we have any group master items
                var groupMasterItemIDs = items.Where(c => c.IsGroupMaster).Select(c => c.ItemID).ToList();
                if (groupMasterItemIDs.Count == 0) return;

                // Get a list of group member items for all the group master items
                var itemGroupMembers = new List<ItemGroupMember>();


                using (var context = Exigo.Sql())
                {
                    context.Open();
                    itemGroupMembers = context.Query<Item, ItemGroupMember, ItemGroupMember>(@"
                SELECT
	                ItemCode = i.ItemCode,
	                ItemDescription = i.ItemDescription,
	                Weight = i.Weight,
	                ItemTypeID = i.ItemTypeID,
	                TinyImageUrl = i.TinyImageName,
	                SmallImageUrl = i.SmallImageName,
	                LargeImageUrl = i.LargeImageName,
	                ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
	                ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
	                ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
	                ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),
	                LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
	                LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
	                LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
	                LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),
	                IsVirtual = i.IsVirtual,
	                AllowOnAutoOrder = i.AllowOnAutoOrder,
	                IsGroupMaster = i.IsGroupMaster,
	                IsDynamicKitMaster = cast(case when i.ItemTypeID = 2 then 1 else 0 end as bit),
	                GroupMasterItemDescription = i.GroupDescription,
	                GroupMembersDescription = i.GroupMembersDescription,
	                Field1 = i.Field1,
	                Field2 = i.Field2,
	                Field3 = i.Field3,
	                Field4 = i.Field4,
	                Field5 = i.Field5,
	                Field6 = i.Field6,
	                Field7 = i.Field7,
	                Field8 = i.Field8,
	                Field9 = i.Field9,
	                Field10 = i.Field10,
	                OtherCheck1 = i.OtherCheck1,
	                OtherCheck2 = i.OtherCheck2,
	                OtherCheck3 = i.OtherCheck3,
	                OtherCheck4 = i.OtherCheck4,
	                OtherCheck5 = i.OtherCheck5,
	                Auto1 = i.Auto1,
	                Auto2 = i.Auto2,
	                Auto3 = i.Auto3,
	                Price = ip.Price,
	                CurrencyCode = ip.CurrencyCode,
	                BV = ip.BusinessVolume,
	                CV = ip.CommissionableVolume,
	                OtherPrice1 = ip.Other1Price,
	                OtherPrice2 = ip.Other2Price,
	                OtherPrice3 = ip.Other3Price,
	                OtherPrice4 = ip.Other4Price,
	                OtherPrice5 = ip.Other5Price,
	                OtherPrice6 = ip.Other6Price,
	                OtherPrice7 = ip.Other7Price,
	                OtherPrice8 = ip.Other8Price,
	                OtherPrice9 = ip.Other9Price,
	                OtherPrice10 = ip.Other10Price,
					PreferredPrice = a.Price,
					PriceTypeID1 = a.PriceTypeID,
	                PremierPrice = b.Price ,
					PriceTypeID2 = b.PriceTypeID,
					RetailPrice = c.Price ,
					PriceTypeID3 = c.PriceTypeID,
                    MasterItemID = im.MasterItemID,
                    MemberDescription = im.GroupMemberDescription,
                    SortOrder = im.Priority,
                    ItemID = i.ItemID
                    FROM ItemGroupMembers im
	                inner join Items i
		                on i.ItemID = im.ItemID
	                INNER JOIN ItemPrices ip
		                ON ip.ItemID = i.ItemID
		                AND ip.PriceTypeID = @ThirdPriceTypeID
                    INNER JOIN ItemPrices a
		                ON a.ItemID = ip.ItemID
					INNER JOIN ItemPrices b					
					    ON b.ItemID = ip.ItemID

					INNER JOIN ItemPrices c					
					    ON c.ItemID = ip.ItemID      
	                INNER JOIN ItemWarehouses iw
		                ON iw.ItemID = i.ItemID
		                AND iw.WarehouseID = @warehouse
						LEFT JOIN ItemLanguages il
		                ON il.ItemID = i.ItemID
						AND il.LanguageID = @languageID
						  WHERE  c.CurrencyCode = @currencyCode
						  AND ip.CurrencyCode = @currencyCode
                          AND a.CurrencyCode = @currencyCode
                          AND im.ItemID != im.MasterItemID
						  AND b.CurrencyCode = @currencyCode
                        AND a.PriceTypeID = @FirstPriceTypeID
						AND b.PriceTypeID = @SecondPriceTypeID
						AND c.PriceTypeID = @ThirdPriceTypeID
                        AND im.MasterItemID in @groupMasterItemIDs
                                                
                ", (Item, ItemGroupMember) =>
                 {
                     ItemGroupMember.Item = Item;
                     ItemGroupMember.ItemCode = Item.ItemCode;
                     return ItemGroupMember;
                 }, new
                 {
                     warehouse = warehouseID,
                     currencyCode = currencyCode,
                     languageID = languageID,
                     FirstPriceTypeID = PriceTypes.Retail,
                     SecondPriceTypeID = PriceTypes.Wholesale,
                     groupMasterItemIDs = groupMasterItemIDs
                 }, splitOn: "MasterItemID").ToList();

                    context.Close();
                }


                //bind the item group members to the group master items               
                foreach (var groupmasteritemid in groupMasterItemIDs)
                {
                    var masteritem = items.Where(c => c.ItemID == groupmasteritemid).FirstOrDefault();
                    if (masteritem == null) continue;

                    masteritem.GroupMembers = itemGroupMembers
                        .Where(c => c.MasterItemID == groupmasteritemid)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

                    // populate the master item's basic details for cart purposes
                    foreach (var groupmember in masteritem.GroupMembers)
                    {
                        groupmember.Item = groupmember.Item ?? new Item();
                        groupmember.Item.ItemCode = groupmember.ItemCode;
                        groupmember.Item.GroupMasterItemCode = masteritem.ItemCode;
                    }
                }
            }
            catch { }
        }
        private static void PopulateDynamicKitMembers(IEnumerable<Item> items)
        {
            try
            {
                // Determine if we have any dynamic kit items
                var dynamicKitMasterItemCodes = items.Where(c => c.IsDynamicKitMaster).Select(c => c.ItemCode).ToList();
                if (dynamicKitMasterItemCodes.Count == 0) return;

                // Get the dynamic kit data
                var context = Exigo.OData();
                var apiItemDynamicKitCagtegoryMembers = context.ItemDynamicKitCategoryMembers.Expand("MasterItem,DynamicKitCategory/DynamicKitCategoryItemMembers/DynamicKitCategory,DynamicKitCategory/DynamicKitCategoryItemMembers/Item")
                    .Where(dynamicKitMasterItemCodes.ToOrExpression<Common.Api.ExigoOData.ItemDynamicKitCategoryMember, string>("MasterItem.ItemCode"))
                    .ToList();

                // Bind the item group members to the items
                foreach (var dynamicKitMasterItemCode in dynamicKitMasterItemCodes)
                {
                    var item = items.Where(c => c.ItemCode == dynamicKitMasterItemCode).FirstOrDefault();
                    if (item == null) continue;

                    var apiCategories = apiItemDynamicKitCagtegoryMembers.Where(c => c.MasterItem.ItemCode == dynamicKitMasterItemCode).ToList();
                    item.DynamicKitCategories = apiCategories.Select(c => (DynamicKitCategory)c).ToList();

                    foreach (var category in item.DynamicKitCategories)
                    {
                        foreach (var categoryItem in category.Items)
                        {
                            categoryItem.ParentItemCode = dynamicKitMasterItemCode;
                        }
                    }
                }
            }
            catch { }
        }
    }
}