using System;

namespace ExigoService
{
    public interface IDynamicKitCategoryItem : IShoppingCartItem
    {
        int ItemID { get; set; }
        string ItemDescription { get; set; }

        string TinyImageUrl { get; set; }
        string SmallImageUrl { get; set; }
        string LargeImageUrl { get; set; }



        new Guid ID { get; set; }
        new string ItemCode { get; set; }
        new decimal Quantity { get; set; }
        new string ParentItemCode { get; set; }
        new string GroupMasterItemCode { get; set; }
        new string DynamicKitCategory { get; set; }
        new ShoppingCartItemType Type { get; set; }
    }
}
