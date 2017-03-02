using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Api.ExigoWebService
{
    public partial class CreateOrderRequest
    {
        public CreateOrderRequest() { }
        public CreateOrderRequest(IOrderConfiguration configuration, int shipMethodID, IEnumerable<IShoppingCartItem> items, ShippingAddress address)
        {
            WarehouseID  = configuration.WarehouseID;
            PriceType    = configuration.PriceTypeID;
            CurrencyCode = configuration.CurrencyCode;
            OrderDate    = DateTime.Now;
            OrderType    = ExigoWebService.OrderType.ShoppingCart;
            OrderStatus  = OrderStatusType.Incomplete;
            ShipMethodID = shipMethodID;
            

            Details      = items.Select(c => (OrderDetailRequest)(c as ShoppingCartItem)).ToArray();

            FirstName    = address.FirstName;
            MiddleName   = address.MiddleName;
            LastName     = address.LastName;
            Email        = address.Email;
            Phone        = address.Phone;
            Address1     = address.Address1;
            Address2     = address.Address2;
            City         = address.City;
            State        = address.State;
            Zip          = address.Zip;
            Country      = address.Country;
        }
    }
}