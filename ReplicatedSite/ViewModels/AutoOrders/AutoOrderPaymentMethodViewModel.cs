using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels
{
    public class AutoOrderPaymentMethodViewModel
    {
        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
    }
}