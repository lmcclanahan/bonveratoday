using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels.AutoOrders
{
    public class AutoOrderPaymentViewModel
    {
        public int AutoorderID { get; set; }
        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
        public bool PayWithAvailablePoints { get; set; }
        public CreditCardType SelectedCardType { get; set; }
    }
}