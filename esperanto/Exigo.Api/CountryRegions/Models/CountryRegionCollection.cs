using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CountryRegionCollection
    {
        public CountryRegionCollection()
        {
            this.Countries = new List<Country>();
            this.Regions = new List<Region>();
        }

        public List<Country> Countries { get; set; }
        public List<Region> Regions { get; set; }
    }
}
