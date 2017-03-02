using Backoffice.Exigo.WebService;
using Backoffice.Models;
using Backoffice.Services;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common.Api.ExigoWebService;
using ExigoService;

namespace Backoffice.Services
{
    public class ShoppingService
    {
        // Products
        public  GetProductsResponse GetProducts(IOrderConfiguration configuration, IWebCategoryConfiguration webcategoryConfiguration, GetProductsRequest request)
        {
            var response = new GetProductsResponse();


            // Set up ordering
            var orderByClause = "";
            switch (request.SortType)
            {
                case ProductListSortType.PriceAsc:
                    orderByClause = "ip.price";
                    break;
                case ProductListSortType.PriceDesc:
                    orderByClause = "ip.price DESC";
                    break;
                case ProductListSortType.NameAsc:
                    orderByClause = "COALESCE(il.itemdescription, i.itemdescription)";
                    break;
                case ProductListSortType.NameDesc:
                    orderByClause = "COALESCE(il.itemdescription, i.itemdescription) DESC";
                    break;
                default:
                    break;
            }

            // Set up where clause
            // This is dummied for now until we know what this filter is intended for
            string whereClause = string.Empty;

            if (request.CategoryID == 0)
            {
                var childCategoryIDs = GetAllWebCategoryIDChildren(configuration.CategoryID);
                whereClause += " WHERE w.WebCategoryID in ({0}) ".FormatWith(string.Join(",", childCategoryIDs));
            }
            else whereClause += " WHERE w.WebCategoryID = {0} ".FormatWith(request.CategoryID);



            if (request.SearchFilter.IsNotEmpty())
            {
                whereClause += @" 
                AND 
                    (
                        il.ItemDescription LIKE '%{0}%' 
                        OR i.ItemDescription LIKE '%{0}%'
                    )
                ".FormatWith(request.SearchFilter);
            }



            // Get the products
            var products = new List<Item>();
            using (var context = Exigo.Sql())
            {
                products = context.Query<Item>(@"
                    SELECT 
                            ItemCode = i.itemcode,
                            Quantity = @initialquantity,
                            TinyImageUrl = @imageprefix + i.tinyimagename, 
                            SmallImageUrl = @imageprefix + i.smallimagename, 
                            LargeImageUrl = @imageprefix + i.largeimagename, 
                            ItemDescription = COALESCE(il.itemdescription, i.itemdescription), 
                            ShortDetail1 = COALESCE(il.shortdetail, i.shortdetail), 
                            ShortDetail2 = COALESCE(il.shortdetail2, i.shortdetail2), 
                            ShortDetail3 = COALESCE(il.shortdetail3, i.shortdetail3), 
                            ShortDetail4 = COALESCE(il.shortdetail4, i.shortdetail4), 
                            LongDetail = COALESCE(il.longdetail, i.longdetail), 
                            LongDetail2 = COALESCE(il.longdetail2, i.longdetail2), 
                            LongDetail3 = COALESCE(il.longdetail3, i.longdetail3), 
                            LongDetail4 = COALESCE(il.longdetail4, i.longdetail4), 
                            Price = ip.price, 
                            Other3Price = ip.Other3Price,
                            BusinessVolume = ip.businessvolume, 
                            CommissionableVolume = ip.commissionablevolume,
                            CategoryID = w.WebCategoryID,
                            SortOrder = CASE WHEN TRY_PARSE(i.Field6 AS int) IS NULL THEN 99 ELSE PARSE(i.Field6 AS int) END,
                            AvailableQuantity = iw.StockLevel
                            --AvailableQuantity = case when i.ItemTypeID > 0 then 99999 else iw.StockLevel end
                            
                    FROM   items i 
                            INNER JOIN itemprices ip 
                                    ON i.itemid = ip.itemid 
                                        AND ip.pricetypeid = @pricetypeID 
                            LEFT JOIN itemlanguages il 
                                    ON i.itemid = il.itemid 
                                        AND il.languageid = @languageID 
                            LEFT JOIN itemwarehouses iw
                                ON i.itemid = iw.itemid
                            INNER JOIN webcategoryitems w 
                                    ON w.itemid = i.itemid 
                                        AND w.webid = @webID                    

                    " + whereClause + @"

                    ORDER BY " + orderByClause + @"

                    OFFSET @skip ROWS
                    FETCH NEXT @take ROWS ONLY
                ", new
                 {
                     imageprefix = GlobalSettings.ProductImagePath,
                     webID = 1,
                     pricetypeID = configuration.PriceTypeID,
                     languageID = GlobalUtilities.GetCurrentLanguageID(),
                     initialquantity = request.InitialQuantity,
                     skip = request.Skip,
                     take = request.Take
                 }).ToList();
            }

            // Run logic to set Item Type, for bundles
            products.ForEach(p => SetProductType(p, webcategoryConfiguration));

            // Now run logic to remove all bundles if the user has already purchased one
            if (products.Any(p => p.Type == ShoppingCartItemType.Bundle))
            {
                var hasPurchaseBundle = GlobalUtilities.HasPurchasedBundle(Identity.Current.CustomerID);

                if (hasPurchaseBundle)
                {
                    products.RemoveAll(p => p.Type == ShoppingCartItemType.Bundle);
                }
                else
                {
                    //var temp = new List<Product>();

                    //temp = products.Where(p => p.Type != ShoppingCartItemType.Bundle).ToList();
                    //products.RemoveAll(p => p.Type != ShoppingCartItemType.Bundle);

                    //products.AddRange(temp);
                }
            }

            response.Items = items;


            var itemcodelist = response.Products.Select(i => i.ItemCode).ToList();
            var memberlist = new List<Item>();
            var languageID = GlobalUtilities.GetCurrentLanguageID();

            var grouptask = Task.Factory.StartNew(() =>
            {
                var members = Exigo.WebService().GetItems(new GetItemsRequest
                {
                    WarehouseID = configuration.WarehouseID,
                    PriceType = configuration.PriceTypeID,
                    LanguageID = languageID,
                    CurrencyCode = configuration.CurrencyCode,
                    RestrictToWarehouse = true,
                    ReturnLongDetail = false,
                    ItemCodes = itemcodelist.Distinct().ToArray()
                }).Items;

                foreach (var item in members.Where(m => m.IsGroupMaster))
                {
                    foreach (var member in item.GroupMembers)
                    {
                        memberlist.Add(new Item
                        {
                            ItemCode = member.ItemCode,
                            ItemDescription = member.MemberDescription,
                            Price = item.Price,
                            Other3Price = item.Other3Price,
                            Type = ShoppingCartItemType.Default,
                            ShortDetail1 = item.ShortDetail,
                            ShortDetail2 = item.ShortDetail2,
                            LongDetail1 = item.LongDetail,
                            LongDetail2 = item.LongDetail2,
                            Category = item.Category,
                            SortOrder = (item.Field6.CanBeParsedAs<int>()) ? Convert.ToInt32(item.Field6) : 99,
                            BusinessVolume = item.BusinessVolume,
                            CommissionableVolume = item.CommissionableVolume,
                            GroupMasterItemCode = item.ItemCode
                        });
                    }
                }

                if (memberlist.Count > 0)
                {
                    foreach (var item in members.Where(i => i.IsGroupMaster))
                    {
                        var newprod = response.Products.Where(i => i.ItemCode == item.ItemCode).First();
                        newprod.IsGroupMaster = true;
                        newprod.GroupMembers = memberlist.Where(m => m.GroupMasterItemCode == item.ItemCode).ToList();
                    }
                }

            });


            if (request.ReturnTotalRowCount)
            {
                var rowCount = 0;
                using (var context = Exigo.Sql())
                {
                    rowCount = context.Query<int>(@"
                    SELECT
                        'RowCount' = COUNT(*)
                    FROM   items i 
                            INNER JOIN itemprices ip 
                                    ON i.itemid = ip.itemid 
                                        AND ip.pricetypeid = @pricetypeID 
                            LEFT JOIN itemlanguages il 
                                    ON i.itemid = il.itemid 
                                        AND il.languageid = @languageID 
                            INNER JOIN webcategoryitems w 
                                    ON w.itemid = i.itemid 
                                        AND w.webid = @webID

                    " + whereClause + @"
                ", new
                 {
                     webID = 1,
                     pricetypeID = configuration.PriceTypeID,
                     languageID = GlobalUtilities.GetCurrentLanguageID()
                 }).FirstOrDefault();
                }
                response.TotalRowCount = rowCount;
            }


            response.Page = request.Page;
            response.RowCount = response.Products.Count();

            Task.WaitAll(grouptask);

            return response;
        }

        public IEnumerable<IProduct> GetProducts(IOrderConfiguration configuration, int categoryID = 0, bool returnLongDetails = true)
        {
            if (categoryID == 0) categoryID = configuration.CategoryID;

            // Get the products
            var response = Exigo.WebService().GetItems(new GetItemsRequest
            {
                WarehouseID = configuration.WarehouseID,
                PriceType = configuration.PriceTypeID,
                LanguageID = GlobalUtilities.GetCurrentLanguageID(),
                CurrencyCode = configuration.CurrencyCode,
                RestrictToWarehouse = true,
                ReturnLongDetail = returnLongDetails,
                WebID = 1,
                WebCategoryID = categoryID
            });
            

            foreach (var item in response.Items)
            {
                yield return (Item)item;
            }
        }
        public IEnumerable<IItem> GetProducts(IOrderConfiguration configuration, string[] itemcodes)
        {
            var results = new List<IItem>();
            if (itemcodes.Length == 0) yield break;

            // Get the products
            var response = Exigo.WebService().GetItems(new GetItemsRequest
            {
                WarehouseID = configuration.WarehouseID,
                PriceType = configuration.PriceTypeID,
                LanguageID = GlobalUtilities.GetCurrentLanguageID(),
                CurrencyCode = configuration.CurrencyCode,
                RestrictToWarehouse = true,
                ReturnLongDetail = true,
                ItemCodes = itemcodes.Distinct().ToArray()
            });


            foreach (var item in response.Items)
            {
                yield return (Item)item;
            }
        }
        public IEnumerable<IItem> GetProducts<T>(IOrderConfiguration configuration, List<T> cartItems) where T : IItem
        {
            if (cartItems.Count == 0) yield break;

            // Get the products
            var results = GetProducts(configuration, cartItems.Select(c => c.ItemCode).Distinct().ToArray()).ToList();

            for (int p = 0; p < results.Count; p++)
            {
                var result  = results[p];
                var cartItem = cartItems.Where(c => c.ItemCode == result.ItemCode).FirstOrDefault();
                if (cartItem == null) continue;

                result.ID                  = cartItem.ID;
                result.Quantity            = cartItem.Quantity;
                result.ParentItemCode      = cartItem.ParentItemCode;
                result.GroupMasterItemCode = cartItem.GroupMasterItemCode;
                result.DynamicKitCategory  = cartItem.DynamicKitCategory;
                result.Type                = cartItem.Type;
                result.IsRequired          = cartItem.IsRequired;
                

                yield return result;
            }
        }
        public IItem GetProduct(IOrderConfiguration configuration, string itemcode)
        {
            return GetProducts(configuration, new string[] { itemcode }).FirstOrDefault();
        }

        public Item SetProductType(Item item, IWebCategoryConfiguration configuration)
        {

            if (item.CategoryID == configuration.BusinessBuilderCategoryID || item.CategoryID == configuration.EssentialCategoryID)
            {
                item.AllowedOnAutoship = false;
                item.Type = ShoppingCartItemType.Bundle;
            }
            else if (item.CategoryID == configuration.SalesToolsCategoryID)
            {
                item.AllowedOnAutoship = false;
            }

            return item;
        }

        // Order Calculation
        public OrderCalculationResult CalculateOrder(OrderCalculationRequest request)
        {
            var result = new OrderCalculationResult();
            if (request.Items.Count() == 0) return result;
            if (request.Address == null) request.Address = GlobalSettings.Company.DefaultCalculationAddress;
            if (request.ShipMethodID == 0) request.ShipMethodID = request.Configuration.DefaultShipMethodID;


            var apirequest = new CalculateOrderRequest();

            apirequest.WarehouseID = request.Configuration.WarehouseID;
            apirequest.CurrencyCode = request.Configuration.CurrencyCode;
            apirequest.PriceType = request.Configuration.PriceTypeID;
            apirequest.ShipMethodID = request.ShipMethodID;
            apirequest.ReturnShipMethods = request.ReturnShipMethods;
            apirequest.City = request.Address.City;
            apirequest.State = request.Address.State;
            apirequest.Zip = request.Address.Zip;
            apirequest.Country = request.Address.Country;
            apirequest.Details = request.Items.Select(c => new OrderDetailRequest(c)).ToArray();

            var apiresponse = Exigo.WebService().CalculateOrder(apirequest);

            result.Subtotal = apiresponse.SubTotal;
            result.Shipping = apiresponse.ShippingTotal;
            result.Tax = apiresponse.TaxTotal;
            result.Discount = apiresponse.DiscountTotal;
            result.Total = apiresponse.Total;


            // Assemble the ship methods
            var shipMethods = new List<IShipMethod>();
            if (apiresponse.ShipMethods != null)
            {
                foreach (var shipMethod in apiresponse.ShipMethods.Where(s => request.Configuration.AvailableShipMethods.Contains(s.ShipMethodID)))
                {
                    shipMethods.Add(GlobalUtilities.TranslateShipMethods((ShipMethod)shipMethod));
                }

                // Ensure that at least one ship method is selected
                var shipMethodID = (request.ShipMethodID != 0) ? request.ShipMethodID : request.Configuration.DefaultShipMethodID;
                if (shipMethods.Any(c => c.ShipMethodID == shipMethodID))
                {
                    shipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
                }
                else
                {
                    shipMethods.First().Selected = true;
                }
            }

            result.ShipMethods = shipMethods.AsEnumerable();

            var test = "";
            return result;
        }

        // Web Categories
        public int[] GetAllWebCategoryIDChildren(int webCategoryID)
        {
            var results = new int[0];


            using (var context = Exigo.Sql())
            {
                results = context.Query<int>(@"
                    WITH webcat (WebCategoryID, WebCategoryDescription, ParentID, NestedLevel) 
                         AS (SELECT WebCategoryID, 
                                    WebCategoryDescription, 
                                    ParentID, 
                                    NestedLevel 
                             FROM   WebCategories 
                             WHERE  WebCategoryID = @webcategoryid
                                    AND WebID = @webid 
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
                     webid = 1,
                     webcategoryid = webCategoryID
                 }).ToArray();
            }


            return results;
        }
    }
}