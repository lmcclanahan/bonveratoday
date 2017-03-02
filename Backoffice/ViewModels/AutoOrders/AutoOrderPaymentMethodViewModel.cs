using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels
{
    public class AutoOrderPaymentMethodViewModel
    {
        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
    }
}