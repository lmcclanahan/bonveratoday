using ExigoService;
using System.Collections;
using System.Linq;

namespace ReplicatedSite.Models
{
    public class ShoppingCartItemsPropertyBag : BasePropertyBag, ICart
    {
        private string version = "3.0.0";
        private int expires = 1440; // 1 day in minutes



        #region Constructors
        public ShoppingCartItemsPropertyBag()
        {
            this.Expires = expires;
            this.Items = new ShoppingCartItemCollection();
        }
        #endregion

        #region Properties
        public int CustomerID { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string Domain { get; set; }
        public ShoppingCartItemCollection Items { get; set; }

        public bool IsSmartShopperCheckout { get; set; }
        #endregion

        #region Methods
        public override T OnBeforeUpdate<T>(T propertyBag)
        {
            propertyBag.Version = version;

            return propertyBag;
        }
        public override bool IsValid()
        {
            return this.Version == version;
        }
        public bool HasOrderItems()
        {
            return this.Items.Where(i => i.Type == ShoppingCartItemType.Order).Count() > 0;
        }
        public bool HasAutoOrderItems()
        {
            return this.Items.Where(i => i.Type == ShoppingCartItemType.AutoOrder).Count() > 0;
        }
        public bool HasPackAutoOrderItems()
        {
            return this.Items.Where(i => i.Type == ShoppingCartItemType.EnrollmentAutoOrderPack).Count() > 0;
        }

        #endregion
    }
}