using Common.Api.ExigoWebService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<AutoOrder> GetCustomerAutoOrders(GetCustomerAutoOrdersRequest request)
        {
            var context = Exigo.OData();

            // Setup the base query
            var basequery = context.AutoOrders;
            if (request.IncludeDetails) basequery = basequery.Expand("Details");


            // Setup the query
            var query = basequery.Where(c => c.CustomerID == request.CustomerID);

            // Filters
            if (!request.IncludeCancelledAutoOrders && !request.IncludeInactiveAutoOrders)
            {
                query = query.Where(c => c.AutoOrderStatusID == 0 || c.AutoOrderStatusID ==1); //82854 20161130 DV. Per client request allow Inactive auto order status 1 to be included in result
            }
            else if (!request.IncludeCancelledAutoOrders && request.IncludeInactiveAutoOrders)
            {
                query = query.Where(c => c.AutoOrderStatusID < 2);
            }
            else if (request.IncludeCancelledAutoOrders && !request.IncludeInactiveAutoOrders)
            {
                query = query.Where(c => c.AutoOrderStatusID == 0 || c.AutoOrderStatusID == 2);
            }

            // Get the data
            var autoOrderData = query.Select(c => c);

            var autoOrders = new List<AutoOrder>();
            foreach (var autoOrder in autoOrderData)
            {
                autoOrders.Add((AutoOrder)autoOrder);
            }


            // If we don't have any autoOrder, stop here.
            if (autoOrders.Count == 0) yield break;


            // Get the payment methods if applicable
            if (request.IncludePaymentMethods)
            {
                var paymentMethods = GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                {
                    CustomerID = request.CustomerID
                });
                foreach (var autoOrder in autoOrders)
                {
                    IPaymentMethod paymentMethod;
                    switch (autoOrder.AutoOrderPaymentTypeID)
                    {
                        case 1: paymentMethod = paymentMethods.Where(c => c is CreditCard && ((CreditCard)c).Type == CreditCardType.Primary).FirstOrDefault(); break;
                        case 2: paymentMethod = paymentMethods.Where(c => c is CreditCard && ((CreditCard)c).Type == CreditCardType.Secondary).FirstOrDefault(); break;
                        case 3: paymentMethod = paymentMethods.Where(c => c is BankAccount && ((BankAccount)c).Type == ExigoService.BankAccountType.Primary).FirstOrDefault(); break;
                        default: paymentMethod = null; break;
                    }
                    autoOrder.PaymentMethod = paymentMethod;
                }
            }


            // Determine if each autoOrder contains only virtual items if applicable
            if (request.IncludeDetails)
            {
                var itemIDs = new List<int>();
                autoOrders.ForEach(a =>
                {
                    if (a.Details != null && a.Details.Count() > 0)
                    {
                        itemIDs.AddRange(a.Details.Select(c => c.ItemID));
                    }
                });
                itemIDs = itemIDs.Distinct().ToList();

                // Determine which item IDs are virtual
                var virtualItems = context.Items
                    .Where(c => c.IsVirtual)
                    .Where(itemIDs.ToOrExpression<Common.Api.ExigoOData.Item, int>("ItemID"))
                    .Select(c => new { c.ItemID })
                    .ToList();
                var virtualItemIDs = virtualItems.Select(c => c.ItemID).ToList();

                // Loop through each auto order, setting the details
                foreach (var autoOrder in autoOrders)
                {
                    foreach (var detail in autoOrder.Details)
                    {
                        if (virtualItemIDs.Contains(detail.ItemID))
                        {
                            detail.IsVirtual = true;
                        }
                    }
                }
            }


            // Get the images
            if (request.IncludeDetails)
            {
                var itemcodes = new List<string>();
                foreach (var order in autoOrders)
                {
                    itemcodes.AddRange(order.Details.Select(c => c.ItemCode));
                }

                var images = new List<Common.Api.ExigoOData.Item>();
                if (itemcodes.Count > 0)
                {
                    images = context.Items
                        .Where(itemcodes.ToOrExpression<Common.Api.ExigoOData.Item, string>("ItemCode"))
                        .Select(c => new Common.Api.ExigoOData.Item
                        {
                            ItemCode = c.ItemCode,
                            SmallImageUrl = c.SmallImageUrl
                        })
                        .ToList();
                }


                // Loop through each auto order, setting the details
                foreach (var autoOrder in autoOrders)
                {
                    foreach (var detail in autoOrder.Details)
                    {
                        var image = images.Where(c => c.ItemCode == detail.ItemCode).FirstOrDefault();
                        if (image != null) detail.ImageUrl = image.SmallImageUrl;
                    }
                }
            }


            // Return the results
            foreach (var autoOrder in autoOrders)
            {
                yield return autoOrder;
            }
        }
        public static AutoOrder GetCustomerAutoOrder(int customerID, int autoOrderID)
        {
            var context = Exigo.OData();

            // Setup the base query
            var autoOrderData = context.AutoOrders.Expand("Details")
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.AutoOrderID == autoOrderID)
                .FirstOrDefault();
            if (autoOrderData == null) return null;

            var autoOrder = (AutoOrder)autoOrderData;


            // Get the payment methods if applicable
            var paymentMethods = GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = customerID
            });

            IPaymentMethod paymentMethod;
            switch (autoOrder.AutoOrderPaymentTypeID)
            {
                case 1: paymentMethod = paymentMethods.Where(c => c is CreditCard && ((CreditCard)c).Type == CreditCardType.Primary).FirstOrDefault(); break;
                case 2: paymentMethod = paymentMethods.Where(c => c is CreditCard && ((CreditCard)c).Type == CreditCardType.Secondary).FirstOrDefault(); break;
                case 3: paymentMethod = paymentMethods.Where(c => c is BankAccount && ((BankAccount)c).Type == ExigoService.BankAccountType.Primary).FirstOrDefault(); break;
                default: paymentMethod = null; break;
            }
            autoOrder.PaymentMethod = paymentMethod;


            // Get the images
            var itemcodes = new List<string>();
            itemcodes.AddRange(autoOrder.Details.Select(c => c.ItemCode));


            var tempList = new List<Common.Api.ExigoOData.Item>();
            if (itemcodes.Count > 0)
            {
                tempList = context.Items
                    .Where(itemcodes.ToOrExpression<Common.Api.ExigoOData.Item, string>("ItemCode"))
                    .Select(c => new Common.Api.ExigoOData.Item
                    {
                        ItemCode = c.ItemCode,
                        TinyImageUrl = c.TinyImageUrl,
                        IsVirtual = c.IsVirtual
                    })
                    .ToList();

                // Loop through each auto order detail, setting the image and virtual flags on each
                foreach (var detail in autoOrder.Details)
                {
                    var additionalDetail = tempList.Where(c => c.ItemCode == detail.ItemCode).FirstOrDefault();
                    if (additionalDetail != null)
                    {
                        detail.ImageUrl = additionalDetail.TinyImageUrl;
                        detail.IsVirtual = additionalDetail.IsVirtual;
                    }
                }
            }

            // Return the results
            return autoOrder;
        }

        // Auto Order Manager
        public static void UpdateCustomerAutoOrderShippingAddress(int customerID, int autoOrderID, ShippingAddress address)
        {
            var context = Exigo.OData();


            // Get the autoorder
            var autoOrder = context.AutoOrders
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.AutoOrderID == autoOrderID)
                .FirstOrDefault();
            if (autoOrder == null) return;


            // Re-create the autoorder
            var request = GetCreateOverridenAutoOrderRequest(customerID, autoOrderID);
            request.FirstName = address.FirstName;
            request.LastName = address.LastName;
            request.Company = address.Company;
            request.Address1 = address.Address1;
            request.Address2 = address.Address2;
            request.City = address.City;
            request.State = address.State;
            request.Zip = address.Zip;
            request.Country = address.Country;
            request.Email = address.Email;
            request.Phone = address.Phone;

            request.Frequency = FrequencyType.Monthly;


            // Update the autoorder
            var response = Exigo.WebService().CreateAutoOrder(request);
        }
        public static void UpdateCustomerAutoOrderShipMethod(int customerID, int autoOrderID, int shipMethodID)
        {
            // Get the autoorder
            var autoOrder = Exigo.WebService().GetAutoOrders(new GetAutoOrdersRequest
            {
                CustomerID = customerID,
                AutoOrderID = autoOrderID
            }).AutoOrders[0];
            if (autoOrder == null) return;


            // Re-create the autoorder
            var request = GetCreateOverridenAutoOrderRequest(customerID, autoOrderID);
            request.ShipMethodID = shipMethodID;

            request.Frequency = FrequencyType.Monthly;

            // Update the autoorder
            var response = Exigo.WebService().CreateAutoOrder(request);
        }
        public static void UpdateCustomerAutoOrderPaymentMethod(int customerID, int autoOrderID, AutoOrderPaymentType type)
        {
            // Re-create the autoorder
            var request = GetCreateOverridenAutoOrderRequest(customerID, autoOrderID);
            if (request == null) return;

            request.PaymentType = type;

            request.Frequency = FrequencyType.Monthly;

            // Update the autoorder
            var response = Exigo.WebService().CreateAutoOrder(request);
        }
        public static void UpdateCustomerAutoOrderRunDate(int customerID, int autoOrderID, DateTime rundate, FrequencyType frequency)
        {
            // Get the autoorder
            var autoOrder = Exigo.GetCustomerAutoOrder(customerID, autoOrderID);

            if (autoOrder == null) return;

            // Re-create the autoorder
            var request = GetCreateOverridenAutoOrderRequest(customerID, autoOrderID);
            request.StartDate = rundate;

            request.Frequency = FrequencyType.Monthly;

            // Update the autoorder
            var response = Exigo.WebService().CreateAutoOrder(request);
        }
        public static void UpdateCustomerAutoOrderItems(int customerID, int autoOrderID, List<Item> products)
        {
            // Re-create the autoorder
            var request = GetCreateOverridenAutoOrderRequest(customerID, autoOrderID);

            var details = new List<OrderDetailRequest>();

            foreach (var item in products.Where(i => i.Quantity > 0))
            {
                details.Add(new OrderDetailRequest
                {
                    ItemCode = item.ItemCode,
                    Quantity = item.Quantity,
                    Type = ShoppingCartItemType.AutoOrder
                });
            }

            request.Details = details.ToArray();

            request.Frequency = FrequencyType.Monthly;

            // Update the autoorder
            var response = Exigo.WebService().CreateAutoOrder(request);
        }


        public static void DeleteCustomerAutoOrder(int customerID, int autoOrderID)
        {
            // Get the autoorder
            var context = Exigo.OData();
            var autoOrder = context.AutoOrders
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.AutoOrderID == autoOrderID);
            if (autoOrder == null) return;


            // Cancel the autoorder
            var response = Exigo.WebService().ChangeAutoOrderStatus(new ChangeAutoOrderStatusRequest
            {
                AutoOrderID = autoOrderID,
                AutoOrderStatus = AutoOrderStatusType.Deleted
            });
        }

        public static void ModifyCustomerAutoOrder(int customerID, int autoOrderID, AutoOrderStatusType status)
        {
            // Get the autoorder
            var context = Exigo.OData();
            var autoOrder = context.AutoOrders
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.AutoOrderID == autoOrderID);
            if (autoOrder == null) return;

            // Change the autoorder status
            var response = Exigo.WebService().ChangeAutoOrderStatus(new ChangeAutoOrderStatusRequest
            {
                AutoOrderID = autoOrderID,
                AutoOrderStatus = status
            });
        }

        public static bool IsValidAutoOrderID(int customerID, int autoOrderID, bool includeCancelledAutoOrders = false)
        {
            // Get the autoorder
            var context = Exigo.OData();
            var query = context.AutoOrders
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.AutoOrderID == autoOrderID);

            // Only pull active auto orders if applicable
            if (!includeCancelledAutoOrders) query = query.Where(c => c.AutoOrderStatusID == 1);

            var autoOrder = query.FirstOrDefault();

            return autoOrder != null;
        }

        public static DateTime GetNewAutoOrderStartDate(FrequencyType frequency)
        {
            DateTime autoOrderStartDate = new DateTime();
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            switch (frequency)
            {
                case FrequencyType.Weekly: autoOrderStartDate = now.AddDays(7); break;
                case FrequencyType.BiWeekly: autoOrderStartDate = now.AddDays(14); break;
                case FrequencyType.EveryFourWeeks: autoOrderStartDate = now.AddDays(28); break;
                case FrequencyType.Monthly: autoOrderStartDate = now.AddMonths(1); break;
                case FrequencyType.BiMonthly: autoOrderStartDate = now.AddMonths(2); break;
                case FrequencyType.Quarterly: autoOrderStartDate = now.AddMonths(3); break;
                case FrequencyType.SemiYearly: autoOrderStartDate = now.AddMonths(6); break;
                case FrequencyType.Yearly: autoOrderStartDate = now.AddYears(1); break;
                default: autoOrderStartDate = now; break;
            }

            // Ensure we are not returning a day of 29, 30 or 31.
            autoOrderStartDate = GetNextAvailableAutoOrderStartDate(autoOrderStartDate);

            return autoOrderStartDate;
        }
        public static DateTime GetNextAvailableAutoOrderStartDate(DateTime date)
        {
            // Ensure we are not returning a day of 29, 30 or 31.
            if (date.Day > 28)
            {
                date = new DateTime(date.AddMonths(1).Year, date.AddMonths(1).Month, 1).Date;
            }

            return date;
        }

        public static AutoOrderPaymentType GetAutoOrderPaymentType(int autoOrderPaymentTypeID)
        {
            switch (autoOrderPaymentTypeID)
            {
                case 1: return AutoOrderPaymentType.PrimaryCreditCard;
                case 2: return AutoOrderPaymentType.SecondaryCreditCard;
                case 3: return AutoOrderPaymentType.CheckingAccount;
                case 4: return AutoOrderPaymentType.WillSendPayment;
                case 6: return AutoOrderPaymentType.PrimaryWalletAccount;
                case 7: return AutoOrderPaymentType.SecondaryWalletAccount;

                default: return AutoOrderPaymentType.PrimaryCreditCard;
            }
        }
        public static AutoOrderPaymentType GetAutoOrderPaymentType(IPaymentMethod paymentMethod)
        {
            if (!(paymentMethod is IAutoOrderPaymentMethod)) throw new Exception("The provided payment method does not implement IAutoOrderPaymentMethod.");

            if (paymentMethod is CreditCard) return ((CreditCard)paymentMethod).AutoOrderPaymentType;
            if (paymentMethod is BankAccount) return ((BankAccount)paymentMethod).AutoOrderPaymentType;

            return AutoOrderPaymentType.WillSendPayment;
        }
        public static AutoOrderProcessType GetAutoOrderProcessType(int autoOrderProcessTypeID)
        {
            switch (autoOrderProcessTypeID)
            {
                case 1: return AutoOrderProcessType.AlwaysProcess;
                case 2: return AutoOrderProcessType.Conditional;

                default: return AutoOrderProcessType.AlwaysProcess;
            }
        }

        private static CreateAutoOrderRequest GetCreateOverridenAutoOrderRequest(int customerID, int autoOrderID)
        {
            // Get the autoorder
            var context = Exigo.OData();
            var autoOrder = context.AutoOrders.Expand("Details")
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.AutoOrderID == autoOrderID)
                .FirstOrDefault();

            // Re-create the autoorder
            return new CreateAutoOrderRequest(autoOrder);
        }

    }
}