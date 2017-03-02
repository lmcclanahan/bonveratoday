using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels
{
    public class ProfileViewModel
    {
        public Customer Customer { get; set; }
        public IEnumerable<CustomerWallItem> RecentActivity { get; set; }
        public VolumeCollection Volumes { get; set; }
    }
}