using Backoffice.Models;
using Common;
using Common.Providers;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ExigoService;
using Dapper;


namespace Backoffice.Providers
{
    public class ShoppingCartLogicProvider : BaseLogicProvider
    {
        #region Constructors
        public ShoppingCartLogicProvider() : base() {  }
        public ShoppingCartLogicProvider(Controller controller, ShoppingCartItemsPropertyBag cart, ShoppingCartCheckoutPropertyBag propertyBag)
        {
            Controller  = controller;
            Cart        = cart;
            PropertyBag = propertyBag;
        }
        #endregion

        #region Properties
        public ShoppingCartItemsPropertyBag Cart { get; set; }
        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        #endregion 

        #region Logic
        public override CheckLogicResult CheckLogic()
        {
            if (!HasValidOrderDetails(Cart.Items))
            {
                return CheckLogicResult.Failure(RedirectToAction("Cart"));
            }

            if (!IsAuthenticated())
            {
                var helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                return CheckLogicResult.Failure(RedirectToAction("Login", "Account", new RouteValueDictionary
                {
                    { "ReturnUrl",  HttpContext.Current.Request.Url }
                }));
            }

            if (HasAutoOrderItems(Cart.Items))
            {
                if (HasMaxAutoOrders())
                {
                    return CheckLogicResult.Failure(RedirectToAction("Cart"));
                }


                if (!HasValidAutoOrderDetails(Cart.Items))
                {
                    return CheckLogicResult.Failure(RedirectToAction("AutoOrder"));
                }
            }

            if (!HasValidShippingAddress(PropertyBag.ShippingAddress))
            {
                return CheckLogicResult.Failure(RedirectToAction("Shipping"));
            }

            if (!HasValidPaymentMethod(PropertyBag.PaymentMethod))
            {
                return CheckLogicResult.Failure(RedirectToAction("Payment"));
            }

            return CheckLogicResult.Success(RedirectToAction("Review"));
        }
        public bool HasMaxAutoOrders()
        {
            using (var context = Exigo.Sql())
            {

                var autoOrders = context.Query<AutoOrder>(@"
                                select * from AutoOrders
                                where CustomerID = @customerID
                                and AutoOrderStatusID = 0
                                ", new { customerID = Identity.Current.CustomerID }).ToList();

                if (autoOrders.Count() == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public override bool IsAuthenticated()
        {
            return HttpContext.Current.Request.IsAuthenticated;
        }
        public bool HasValidOrderDetails(IEnumerable<IShoppingCartItem> items)
        {
            return items.Count() > 0;
        }
        public bool HasOrderItems(IEnumerable<IShoppingCartItem> items)
        {
            return items.Any(c => c.Type == ShoppingCartItemType.Order);
        }

        public bool HasAutoOrderItems(IEnumerable<IShoppingCartItem> items)
        {
            return items.Any(c => c.Type == ShoppingCartItemType.AutoOrder);
        }
        public bool HasValidAutoOrderDetails(IEnumerable<IShoppingCartItem> items)
        {
            if (!HasAutoOrderItems(items)) return true;

            return PropertyBag.AutoOrderStartDate >= DateTime.Now.BeginningOfDay();
        }

        public bool HasValidShippingAddress(ShippingAddress address)
        {
            return address != null && address.IsComplete;
        }
        public bool HasValidShipMethodID(int shipMethodID)
        {
            return shipMethodID != 0;
        }
        public bool HasValidPaymentMethod(IPaymentMethod paymentMethod)
        {
            return paymentMethod != null &&
                (paymentMethod is CreditCard || paymentMethod is BankAccount);
        }
        #endregion
    }
}