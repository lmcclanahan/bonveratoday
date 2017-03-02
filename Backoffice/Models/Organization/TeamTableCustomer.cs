using System;
using System.Collections.Generic;

namespace Backoffice.Models.Orginization
{
    public class TeamTableCustomer
    {


        public int CustomerID { get; set; }
        public int Team { get; set; }
        public bool IsTeamLead { get; set; }
        public decimal BV { get; set;}
        public decimal PBV { get; set; }
        public int SponsorID { get; set; }
        public decimal PCBV { get; set; }
        public decimal TGBV { get; set; }


    }
}
