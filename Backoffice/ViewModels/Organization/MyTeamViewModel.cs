using Backoffice.Models.Orginization;
using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels
{
    public class MyTeamViewModel
    {

        public MyTeamViewModel()
        {
            this.Teams = new List<Team>();
            this.TeamMembers = new List<TeamTableCustomer>();
        }
        public Customer CurrentTeamMember { get; set; }
        public List<Team> Teams { get; set; }
        public decimal CurrentTeamMemberPBV { get; set; }
        public decimal CurrentTeamMemberPCBV { get; set; }
        public decimal CurrentTeamMemberTGBV { get; set; }
        public List<Period> Periods { get; set; }
        public List<TeamTableCustomer> TeamMembers { get; set; }
        public int CurrentTeamMemberTeamCount { get; set; }
        public int CurrentPeriod { get; set; }
    }
}