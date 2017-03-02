using ExigoService;

namespace Backoffice.ViewModels
{
    public class ManageLeadViewModel
    {
        public ManageLeadViewModel()
        {
            this.Lead = new Customer();
            this.Lead.MainAddress = new Address();
        }

        public Customer Lead { get; set; }
       
    }
}