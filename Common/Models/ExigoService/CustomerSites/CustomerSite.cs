namespace ExigoService
{
    public class CustomerSite : ICustomerSite
    {
        public CustomerSite()
        {
            this.Address = new Address();
        }

        public int CustomerID { get; set; }
        public string WebAlias { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }

        public string Email { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryPhone { get; set; }
        public string Fax { get; set; }

        public Address Address { get; set; }

        public string Notes1 { get; set; }
        public string Notes2 { get; set; }
        public string Notes3 { get; set; }
        public string Notes4 { get; set; }
    }
}