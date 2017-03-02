using System;

namespace ExigoService
{
    public class DynamicKitCategoryItem : IDynamicKitCategoryItem
    {
        public int ItemID { get; set; }
        public string ItemDescription { get; set; }

        public string TinyImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string LargeImageUrl { get; set; }

        

        // IShoppingCartItem Properties
        public Guid ID { get; set; }
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string ParentItemCode { get; set; }
        public string GroupMasterItemCode { get; set; }
        public string DynamicKitCategory { get; set; }
        public ShoppingCartItemType Type { get; set; }
    }
}
