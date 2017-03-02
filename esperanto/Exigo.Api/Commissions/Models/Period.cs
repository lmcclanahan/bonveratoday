using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class Period
    {
        public int PeriodID { get; set; }
        public string PeriodDescription { get; set; }
        public PeriodType PeriodType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }        

        public bool IsCurrent
        {
            get { return (DateTime.Now > this.StartDate && DateTime.Now <= this.EndDate); }
        }
    }
}
