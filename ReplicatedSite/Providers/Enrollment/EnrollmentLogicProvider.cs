using ReplicatedSite.Models;
using Common.Providers;
using ExigoService;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ReplicatedSite.Providers
{
    public class EnrollmentLogicProvider : BaseLogicProvider
    {
        #region Constructors
        public EnrollmentLogicProvider() : base() { }
        public EnrollmentLogicProvider(Controller controller, ShoppingCartItemsPropertyBag cart, EnrollmentPropertyBag propertyBag)
        {
            Controller = controller;
            Cart = cart;
            PropertyBag = propertyBag;
        }
        #endregion

        #region Properties
        public ShoppingCartItemsPropertyBag Cart { get; set; }
        public EnrollmentPropertyBag PropertyBag { get; set; }
        #endregion

        
        public override CheckLogicResult CheckLogic()
        {
            if (!HasEnroller())
            {
                return CheckLogicResult.Failure(RedirectToAction("EnrollmentConfiguration"));
            }

            if (!HasEnrollmentPack(Cart))
            {
                return CheckLogicResult.Failure(RedirectToAction("Packs"));
            }

            if (!HasValidShippingAddress(PropertyBag.ShippingAddress))
            {
                return CheckLogicResult.Failure(RedirectToAction("EnrolleeInfo"));
            }

            if (!HasValidPaymentMethod(PropertyBag.PaymentMethod))
            {
                return CheckLogicResult.Failure(RedirectToAction("EnrolleeInfo"));
            }

            return CheckLogicResult.Success(RedirectToAction("Review"));
        }


        #region Logic
        public bool HasEnroller()
        {
            return PropertyBag.EnrollerID > 0;
        }
        public bool HasEnrollmentPack(ShoppingCartItemsPropertyBag cart)
        {
            return cart.Items.Any(c => c.Type == ShoppingCartItemType.EnrollmentPack);
        }
        public bool HasValidShippingAddress(ShippingAddress address)
        {
            return address != null && address.IsComplete;
        }
        public bool HasValidPaymentMethod(IPaymentMethod paymentMethod)
        {
            return paymentMethod != null &&
                (paymentMethod is CreditCard || paymentMethod is BankAccount);
        }
        #endregion
    }
}