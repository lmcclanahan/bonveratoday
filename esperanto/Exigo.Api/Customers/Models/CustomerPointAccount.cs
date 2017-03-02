using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CustomerPointAccount
    {
        public int PointAccountID { get; set; }
        public int CustomerID { get; set; }
        public decimal Balance { get; set; }
    }
}
