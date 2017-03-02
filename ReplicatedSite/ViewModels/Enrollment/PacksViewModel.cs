using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels
{
    public class PacksViewModel
    {
        public IEnumerable<IItem> OrderItems { get; set; }
        public IEnumerable<IItem> AutoOrderItems { get; set; }

        public ShoppingCartItem SelectedOrderItem { get; set; }
        public ShoppingCartItem SelectedAutoOrderItem { get; set; }

        public int CustomerTypeID { get; set; }

        public string RequiredEnrollmentPackItemCode { get; set; }

        public IEnumerable<IItem> ReplinishPacks { get; set; }
    }
}