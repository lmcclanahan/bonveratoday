using Common;
using System;

namespace ExigoService
{
    public class ShoppingCartItem : IShoppingCartItem
    {
        public ShoppingCartItem()
        {
            ID                  = Guid.NewGuid();
            ItemCode            = string.Empty;
            Quantity            = 0;
            ParentItemCode      = string.Empty;
            DynamicKitCategory  = string.Empty;
            GroupMasterItemCode = string.Empty;
            Type                = ShoppingCartItemType.Order;
            
        }
        public ShoppingCartItem(IShoppingCartItem item)
        {
            ID                  = (item.ID != Guid.Empty) ? item.ID : Guid.NewGuid();
            ItemCode            = GlobalUtilities.Coalesce(item.ItemCode);
            Quantity            = item.Quantity;
            ParentItemCode      = GlobalUtilities.Coalesce(item.ParentItemCode);
            DynamicKitCategory  = GlobalUtilities.Coalesce(item.DynamicKitCategory);
            GroupMasterItemCode = GlobalUtilities.Coalesce(item.GroupMasterItemCode);
            Type                = item.Type;
        }

        public Guid ID
        {
            get
            {
                if (_id == null)
                {
                    _id = Guid.NewGuid();
                }
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        private Guid _id;

        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string ParentItemCode { get; set; }
        public string GroupMasterItemCode { get; set; }
        public string DynamicKitCategory { get; set; }
        public ShoppingCartItemType Type { get; set; }
       

        public bool IsDynamicKitMember
        {
            get
            {
                return (!string.IsNullOrEmpty(this.ParentItemCode));
            }
        }
    }
}