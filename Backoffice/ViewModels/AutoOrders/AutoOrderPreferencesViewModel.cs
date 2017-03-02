using ExigoService;
using System.Collections.Generic;


namespace Backoffice.ViewModels.AutoOrders
{
    public class AutoOrderPreferencesViewModel
    {
        public AutoOrderPreferencesViewModel()
        {
            this.AutoOrders = new List<AutoOrder>();
        }

        public List<AutoOrder> AutoOrders { get; set; }
    }
}