using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels.AutoOrders
{
    public class AutoOrderPaymentViewModel
    {
        public int AutoorderID { get; set; }

        public CreditCardType SelectedCardType { get; set; }
        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
    }
}