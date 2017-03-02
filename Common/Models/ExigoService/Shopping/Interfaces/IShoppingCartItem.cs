using System;

namespace ExigoService
{
    public interface IShoppingCartItem
    {
        Guid ID { get; set; }
        string ItemCode { get; set; }
        decimal Quantity { get; set; }
        string ParentItemCode { get; set; }
        string GroupMasterItemCode { get; set; }
        string DynamicKitCategory { get; set; }
        ShoppingCartItemType Type { get; set; }
    }
}