
namespace Common.Api.ExigoOData
{
    public partial class Payment
    {
        public static explicit operator ExigoService.Payment(Payment payment)
        {
            var model = new ExigoService.Payment();
            if (payment == null) return model;

            model.PaymentID         = payment.PaymentID;
            model.CustomerID        = payment.CustomerID;
            model.OrderID           = payment.OrderID;
            model.PaymentTypeID     = payment.PaymentTypeID;
            model.PaymentDate       = payment.PaymentDate;
            model.Amount            = payment.Amount;
            model.CurrencyCode      = payment.CurrencyCode;
            model.WarehouseID       = payment.WarehouseID;
            model.BillingName       = payment.BillingName;
            model.CreditCardTypeID  = payment.CreditCardTypeID;
            model.CreditCardNumber  = payment.CreditCardNumber;
            model.AuthorizationCode = payment.AuthorizationCode;
            model.Memo              = payment.Memo;

            if (payment.PaymentType != null)
            {
                model.PaymentType   = (ExigoService.PaymentType)payment.PaymentType;
            }

            return model;
        }
    }
}