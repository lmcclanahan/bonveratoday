using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CustomerType
    {
        public int CustomerTypeID { get; set; }
        public string CustomerTypeDescription { get; set; }

        public PriceType PriceType { get; set; }
    }
}
