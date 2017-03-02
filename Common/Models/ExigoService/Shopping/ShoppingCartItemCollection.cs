using System;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public class ShoppingCartItemCollection : List<ShoppingCartItem>, IShoppingCartItemCollection
    {
        public void Add(IShoppingCartItem item)
        {
            var newItem = new ShoppingCartItem(item);

            // Don't process items with no quantities
            if (newItem.Quantity == 0) return;

            // Get a list of all items that have the same item code and type.
            var preExistingItems = this.FindAll(i => 
                  i.ItemCode == newItem.ItemCode
               && i.Type.Equals(newItem.Type)
               && i.DynamicKitCategory == newItem.DynamicKitCategory
               && i.ParentItemCode == newItem.ParentItemCode);

            // If we returned any existing items that match the item code and type, we need to add to those existing items.
            if (preExistingItems.Count() > 0)
            {
                // Loop through each item found.
                preExistingItems.ForEach(i =>
                {
                    // Add the new quantity to the existing item code.
                    // Note that the only thing we are adding to the existing item code is the new quantity.
                    i.Quantity = i.Quantity + newItem.Quantity;
                });
            }

            // If we didn't find any existing items matching the item code or type, let's add it to the ShoppingBasketItemsCollection
            else
            {
                base.Add(newItem);
            }
        }
        
        /// <summary>
        /// Short cut add which will handle adding one of a specific item type to the cart collection
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="type"></param>
        public void Add(string itemCode, ShoppingCartItemType type)
        {
            var item = new ShoppingCartItem();
            item.ItemCode = itemCode;
            item.Type = type;
            item.Quantity = 1;
            

            this.Add(item);
        }

        public void Update(Guid id, decimal quantity)
        {
            var item = this.Where(c => c.ID == id).FirstOrDefault();
            if (item == null) return;

            // Remove the item if it is an invalid quantity
            if (quantity > 0)
            {
                item.Quantity = quantity;
            }
            else
            {
                this.Remove(item.ID);
            }
        }
        public void Update(IShoppingCartItem item)
        {
            var cartitem = new ShoppingCartItem(item);
            var oldItem = this.Where(c => c.ID == cartitem.ID).FirstOrDefault();
            if (oldItem == null) return;

            // Remove the old item
            this.Remove(oldItem.ID);

            // If we have a valid quantity, add the new item
            if (item.Quantity > 0)
            {
                this.Add(item);
            }
        }

        public void Remove(Guid id)
        {
            var matchingItems = this.Where(item => item.ID == id).ToList();
            foreach (var item in matchingItems)
            {
                base.Remove(item);
            }

            DeleteOrphanDynamicKitMembers();
        }
        public void Remove(string itemcode, ShoppingCartItemType type = ShoppingCartItemType.Order)
        {
            var matchingItems = this.Where(item => item.ItemCode == itemcode && item.Type == type).ToList();
            foreach (var item in matchingItems)
            {
                base.Remove(item);
            }

            DeleteOrphanDynamicKitMembers();
        }
        public void Remove(ShoppingCartItemType type)
        {
            var matchingItems = this.Where(item => item.Type.Equals(type)).ToList();
            foreach (var item in matchingItems)
            {
                base.Remove(item);
            }

            DeleteOrphanDynamicKitMembers();
        }

        public void DeleteOrphanDynamicKitMembers()
        {
            List<ShoppingCartItem> itemsToDelete = new List<ShoppingCartItem>();
            foreach (var item in this)
            {
                if (item.IsDynamicKitMember)
                {
                    var existingParents = this.FindAll(i => i.ItemCode == item.ParentItemCode && !i.IsDynamicKitMember && i.Type == item.Type);
                    if (existingParents.Count == 0)
                    {
                        itemsToDelete.Add(item);
                    }
                }
            }

            foreach (var item in itemsToDelete)
            {
                base.Remove(item);
            }
        }

        // Quick check to ensure that a required item is included in the cart, looking at all items in cart - Mike M.
        public bool Contains(string itemcode)
        {
            return this.Any(i => i.ItemCode == itemcode);
        }

        public List<ShoppingCartItem> OrderItems
        {
            get
            {
                var orderItems = new List<ShoppingCartItem>();

                if (this.Any(c => c.Type == ShoppingCartItemType.Order))
                {
                    orderItems = this.Where(c => c.Type == ShoppingCartItemType.Order).ToList();
                }

                return orderItems;
            }
        }
        public List<ShoppingCartItem> AutoOrderItems
        {
            get 
            {
                var autoOrderItems = new List<ShoppingCartItem>();

                if (this.Any(c => c.Type == ShoppingCartItemType.AutoOrder))
                {
                    autoOrderItems = this.Where(c => c.Type == ShoppingCartItemType.AutoOrder).ToList();
                }

                return autoOrderItems;
            }
        }


        /// <summary>
        /// Make sure the cart has matching order items for each auto order item
        /// </summary>
        /// <returns>bool that informs us there was a change needed</returns>
        public bool EnsureMatchingItems(DateTime? startDate = null)
        {
            var hasUpdate = false;
            if (this.AutoOrderItems.Count > 0)
            {
                // Here we need to do a check to ensure that the start date of the auto order is today, if not then we do not need to add additional items when dealing with distributors - per ticket # 75485
                if (startDate != null)
                {
                    var sDate = Convert.ToDateTime(startDate);
                    var today = DateTime.Today;

                    if (sDate.Date != today.Date)
                    {
                        return false;
                    }
                }

                foreach (var item in this.AutoOrderItems)
                {
                    var needsUpdate = this.EnsureMatchingItems(item.ID);

                    if (needsUpdate)
                    {
                        hasUpdate = true;
                    }
                }
            }

            return hasUpdate;
        }


        /// <summary>
        /// Use this to ensure that all auto order items have matching order items in the cart collection. 
        /// </summary>
        /// <param name="id">ID of an Auto Order Item type.</param>
        /// <returns>bool that informs us there was a change needed</returns>
        public bool EnsureMatchingItems(Guid id)
        {
            var autoOrderItem = this.FirstOrDefault(i => i.ID == id && i.Type == ShoppingCartItemType.AutoOrder);
            var orderItem = this.FirstOrDefault(i => i.ItemCode == autoOrderItem.ItemCode && i.Type == ShoppingCartItemType.Order);
            var hasUpdate = false;

            if (orderItem == null)
            {
                var _orderItem = new ShoppingCartItem(autoOrderItem);

                _orderItem.Type = ShoppingCartItemType.Order;
                _orderItem.ID = Guid.NewGuid();

                this.Add(_orderItem);
                hasUpdate = true;
            }
            else
            {
                if (orderItem.Quantity < autoOrderItem.Quantity)
                {
                    this.Update(orderItem.ID, autoOrderItem.Quantity);
                    hasUpdate = true;
                }
            }

            return hasUpdate;
        }
    }
}