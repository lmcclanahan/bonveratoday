using ExigoService;
using System;

namespace Common.Api.ExigoWebService
{
    public partial class OrderDetailRequest
    {
        public static explicit operator OrderDetailRequest(ShoppingCartItem item)
        {
            var model = new OrderDetailRequest();
            if (item == null) return model;

            model.ItemCode       = item.ItemCode;
            model.ParentItemCode = item.ParentItemCode;
            model.Quantity       = item.Quantity;

            return model;
        }
    }

    public partial class OrderDetailRequest : IShoppingCartItem
    {
        public OrderDetailRequest() { }
        public OrderDetailRequest(IShoppingCartItem item)
        {
            this.ItemCode       = item.ItemCode;
            this.Quantity       = item.Quantity;
            this.ParentItemCode = item.ParentItemCode;
            this.Type           = item.Type;
        }

        public Guid ID { get; set; }
        public string GroupMasterItemCode { get; set; }
        public string DynamicKitCategory { get; set; }
        public ShoppingCartItemType Type { get; set; }
    }
}