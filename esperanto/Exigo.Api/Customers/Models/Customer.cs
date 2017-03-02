using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class Customer
    {
        public int CustomerID { get; set; }
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
        public Address MainAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public CustomerType Type { get; set; }
        public CustomerStatus Status { get; set; }

        
        /// <summary>
        /// Represents user-defined field 1.
        /// </summary>
        public DateTime MoveDate { get; set; }

        /// <summary>
        /// Represents user-defined field 2.
        /// </summary>
        public int SkinType { get; set; }

        /// <summary>
        /// Represents user-defined field 3.
        /// </summary>
        public int IncomeLevel { get; set; }

        /// <summary>
        /// Represents user-defined field 4.
        /// </summary>
        public bool W9Received { get; set; }
    }
}
