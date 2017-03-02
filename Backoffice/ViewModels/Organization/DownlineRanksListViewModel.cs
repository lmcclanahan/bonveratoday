using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels
{
    public class DownlineRanksListViewModel
    {
        public List<DownlineRankCountViewModel> DownlineRanks { get; set; }
        public int TeamCount { get; set; }
    }
}