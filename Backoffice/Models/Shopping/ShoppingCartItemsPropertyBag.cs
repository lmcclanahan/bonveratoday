using ExigoService;

using System.Collections;
using System.Linq;

namespace Backoffice.Models
{
    public class ShoppingCartItemsPropertyBag : BasePropertyBag, ICart
    {
        private string version = "2.0.1";
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
        #endregion
    }
}