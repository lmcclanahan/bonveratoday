using ExigoService;

namespace ReplicatedSite.ViewModels
{
    public class EnrollmentCompleteViewModel
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int AutoOrderID { get; set; }

        public Order Order { get; set; }
        public AutoOrder AutoOrder { get; set; }
        public string Token { get; set; }

        public int WillCallShipMethodID { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsBackOfficeEnrollment { get; set; }
   }
}