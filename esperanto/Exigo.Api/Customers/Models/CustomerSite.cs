using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CustomerSite
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
        public string FullName
        {
            get { return string.Format("{0} {1}", this.FirstName, this.LastName); }
        }
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Company)) return Company;
                else return this.FullName;
            }
        }
        public Address Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Notes1 { get; set; }
        public string Notes2 { get; set; }
        public string Notes3 { get; set; }
        public string Notes4 { get; set; }
    }
}
