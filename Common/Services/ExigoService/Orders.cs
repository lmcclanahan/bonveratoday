using Common;
using Common.Api.ExigoOData;
using Common.Api.ExigoWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;


namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<Order> GetCustomerOrders(GetCustomerOrdersRequest request)
        {
            /* 20161222 80967 DV.  Business rules have changed that will affect search.
             * 1. Remove filter using Other11 field.
             * 2. Instead of using ItemCode, use ItemID to identify PartnerStore and AffiliateStore items in an order, because BA and client believe that the ItemCode COULD change.  
             * That is, if an order has ItemID 58 or 627 in the corresponding orderdetails table then simply filter out the entire order from the search results.
             * Note: For the tab called Partners & Affiliates, only include orders that contain ItemCode 58 or 627 specifically.
             * Filter out all orders using Order Type "Import" which is value 5. 
             */
            if (request.CustomerID == 0)
            {
                throw new ArgumentException("CustomerID is required.");
            }

            var context = Exigo.CreateODataContext<ExigoContext>(GlobalSettings.Exigo.Api.SandboxID);

            //Clear the OrderDetailModels
            if (OrderDetailModels != null) OrderDetailModels = new List<ExigoService.OrderDetail>();
            var orders = new List<Order>();

            // Setup the base orders query
            var ordersBaseQuery = context.Orders;

            if (request.IncludePayments) ordersBaseQuery = ordersBaseQuery.Expand("Payments");

            var ordersQuery = ordersBaseQuery.Where(c => c.CustomerID == request.CustomerID);

            //20161222 80967 DV. Per item 3 of GetCustomerOrders summary, filter out all orders using Order Type "Import", which is value 5
            ordersQuery.Where(c => c.OrderTypeID != 5);

            // Apply the request variables
            if (request.OrderID != null)
            {
                ordersQuery = ordersQuery.Where(c => c.OrderID == ((int)request.OrderID));
            }
            if (request.OrderStatuses.Length > 0)
            {
                ordersQuery = ordersQuery.Where(request.OrderStatuses.ToList().ToOrExpression<Common.Api.ExigoOData.Order, int>("OrderStatusID"));
            }
            if (request.OrderTypes.Length > 0)
            {
                ordersQuery = ordersQuery.Where(request.OrderTypes.ToList().ToOrExpression<Common.Api.ExigoOData.Order, int>("OrderTypeID"));
            }
            if (request.StartDate != null)
            {
                ordersQuery = ordersQuery.Where(c => c.OrderDate >= (DateTime)request.StartDate);
            }
            if (request.EndDate != null) //20161212 80967 DV. Add ability for user to control both StartDate and EndDate
            {
                ordersQuery = ordersQuery.Where(c => c.OrderDate <= Convert.ToDateTime(request.EndDate).AddDays(1)); //20161216 #80967 DV. Like in commissions you need to add 1 to the end date in order to included all data up until 11:59:59:59 PM 
            }

            //I.M. 11/29/2016 #80967 Add SQL query for grabbing all Order information
            //            var orderSql = new List<ExigoService.Order>();
            //            try
            //            {
            //                using (var context = Exigo.Sql())
            //                {
            //                    orderSql = context.Query<ExigoService.Order>(@"
            //                			   SELECT
            //                                    o.OrderID, 
            //                                    o.OrderDate, 
            //                                    o.OrderStatusID, 
            //                                    o.OrderTypeID, 
            //                                    o.Total, 
            //                                    o.FirstName, 
            //                                    o.LastName, 
            //                                    o.Address1, 
            //                                    o.City, 
            //                                    o.State, 
            //                                    o.Zip, 
            //                                    o.Country, 
            //                                    o.Phone,
            //                                    o.Email, 
            //                                    od.ItemDescription, 
            //                                    od.ItemCode                                  
            //                                FROM 
            //                                    order o INNER JOIN orderdetails od
            //                                ON 
            //                                    o.OrderID = od.OrderID
            //                                WHERE 
            //                                    
            //                          ", new
            //                         {
            //                            OrderID = request.OrderID,
            //                         }).ToList();
            //                }
            //            }
            //            catch (Exception e)
            //            {
            //                Console.Write(e);
            //            }
            //            return orderSql;
            //            }

            // Get the orders
            var odataOrders = ordersQuery
                .OrderByDescending(c => c.OrderDate)
                .Skip(request.Skip)
                //.Take(20) //.Take(request.Take) 20161226 80967 DV. 
                .Select(c => c)
                .ToList();


            //Begin flagged block.  20161230 DV. This block of code will be examined for performance analysis. 

            // If we don't have any orders, stop here.
            if (odataOrders.Count == 0) yield break;


            // Collect our orders together
            foreach (var order in odataOrders)
            {
                var model = (Order)order;
                orders.Add(model);
            }


            // Get the order details if applicable
            if (request.IncludeOrderDetails)
            {
                // Get the order IDs
                var orderIDs = orders.Select(c => c.OrderID).Distinct().ToList();


                // Get the order details (Results are saved via the ReadingEntity delegate to the private OrderDetailModels property.
                context.ReadingEntity += context_ReadingEntity;
                context.OrderDetails
                    .Where(orderIDs.ToOrExpression<Common.Api.ExigoOData.OrderDetail, int>("OrderID"))
                    .ToList();

                // Get a unique list of item IDs in the orders
                var itemIDs = OrderDetailModels.Select(c => c.ItemID).Distinct().ToList();


                // Get the extra data we need for each detail
                var apiItems = new List<Common.Api.ExigoOData.Item>();
                if (itemIDs.Count > 0)
                {
                    apiItems = context.Items
                        .Where(itemIDs.ToOrExpression<Common.Api.ExigoOData.Item, int>("ItemID"))
                        .Select(c => new Common.Api.ExigoOData.Item
                        {
                            ItemCode = c.ItemCode,
                            SmallImageUrl = c.SmallImageUrl,
                            IsVirtual = c.IsVirtual
                        })
                        .ToList();
                }


                // Format the data to our models
                foreach (var order in orders)
                {
                    // Get the order details
                    var details = OrderDetailModels.Where(c => c.OrderID == order.OrderID);
                    foreach (var detail in details)
                    {
                        var apiItem = apiItems.Where(c => c.ItemCode == detail.ItemCode).FirstOrDefault();
                        if (apiItem != null)
                        {
                            detail.ImageUrl = apiItem.SmallImageUrl;
                            detail.IsVirtual = apiItem.IsVirtual;
                        }
                    }
                    order.Details = details;
                }
            }
            //End of flagged block.  

            //20161222 80967 DV. Per item 2 of GetCustomerOrders summary, filter out any orderID's that has ItemID 58 or 627 in a corresponding orderdetail table
            foreach (var order in orders.ToList())
            {               
                var orderID = order.OrderID;

                var orderdetails = order.Details.Where(c => c.OrderID == order.OrderID);
                bool removecurrentorder = false;

                foreach (var item in orderdetails)
                {
                    if (request.ShowOnlyPartnerAffiliateOrders)
                    {
                        if (!(item.ItemID == 58 || item.ItemID == 627))
                        {
                            //orders.Remove(order);
                            removecurrentorder = true;
                        }
                    }
                    else if (request.ShowOnlyFeesAndServicesOrders) //20161229 80697 DV. Add filter to display orders with fees and services
                    {
                        if (!(item.ItemID == 45 || item.ItemID == 289 || item.ItemID == 290))
                        {
                            //orders.Remove(order);
                            removecurrentorder = true;
                        }
                    }
                    else //Show all orders except orders that contain partner and affiliate items as well as fees and services
                    {
                        if (item.ItemID == 58 || item.ItemID == 627 || item.ItemID == 45 || item.ItemID == 289 || item.ItemID == 290)
                        {
                            orders.Remove(order);
                        }
                    }
                }
                if (removecurrentorder)
                {
                    orders.Remove(order);
                    //removecurrentorder = false; //reset bool
                }
            }


            // Format the data to our models
            foreach (var order in orders)
            {
                yield return order;
            }
        }
        private static List<ExigoService.OrderDetail> OrderDetailModels { get; set; }
































        private static void context_ReadingEntity(object sender, System.Data.Services.Client.ReadingWritingEntityEventArgs e)
        {
            if (OrderDetailModels == null) OrderDetailModels = new List<ExigoService.OrderDetail>();

            var orderDetailModel = ((ExigoService.OrderDetail)((Common.Api.ExigoOData.OrderDetail)e.Entity));

            OrderDetailModels.Add(orderDetailModel);
        }

        public static void CancelOrder(int orderID)
        {
            Exigo.WebService().ChangeOrderStatus(new ChangeOrderStatusRequest
            {
                OrderID = orderID,
                OrderStatus = OrderStatusType.Canceled
            });
        }

        public static OrderCalculationResponse CalculateOrder(OrderCalculationRequest request)
        {
            var result = new OrderCalculationResponse();
            if (request.Items.Count() == 0) return result;
            if (request.Address == null) request.Address = GlobalSettings.Company.Address;
            if (request.ShipMethodID == 0) request.ShipMethodID = request.Configuration.DefaultShipMethodID;


            var apirequest = new CalculateOrderRequest();

            apirequest.CustomerID = (int)request.CustomerID;
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
            apirequest.Other15 = request.Other15;
            apirequest.OrderType = Common.Api.ExigoWebService.OrderType.ShoppingCart;


            var apiresponse = Exigo.WebService().CalculateOrder(apirequest);

            result.Subtotal = apiresponse.SubTotal;
            result.Shipping = apiresponse.ShippingTotal;
            result.Tax = apiresponse.TaxTotal;
            result.Discount = apiresponse.DiscountTotal;
            result.Total = apiresponse.Total;
            result.Details = apiresponse.Details;
            result.Other16 = apiresponse.Other16;

            // Assemble the ship methods
            var shipMethods = new List<ShipMethod>();
            if (apiresponse.ShipMethods != null && apiresponse.ShipMethods.Length > 0)
            {
                foreach (var shipMethod in apiresponse.ShipMethods)
                {
                    shipMethods.Add((ShipMethod)shipMethod);
                }

                // Ensure that at least one ship method is selected
                var shipMethodID = (request.ShipMethodID != 0) ? request.ShipMethodID : request.Configuration.DefaultShipMethodID;
                if (shipMethods.Any(c => c.ShipMethodID == (int)shipMethodID))
                {
                    shipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
                }
                else
                {
                    shipMethods.First().Selected = true;
                }
            }
            result.ShipMethods = shipMethods.AsEnumerable();

            return result;
        }
    }
}