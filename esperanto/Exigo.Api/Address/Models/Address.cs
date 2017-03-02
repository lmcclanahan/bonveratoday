using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class Address
    {
        public Address()
        {
            this.IsVerified = false;
        }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string FullAddress
        {
            get 
            { 
                if(!string.IsNullOrEmpty(this.Address2)) return string.Format("{0}, {1}", this.Address1, this.Address2);
                else return this.Address1;
            }
        }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        public string CompleteAddressDisplay
        {
            get { return string.Format("{0}, {1}, {2} {3}, {4}", 
                this.FullAddress,
                this.City,
                this.State,
                this.Zip,
                this.Country); }
        }

        public bool IsVerified { get; set; }
    }
}
