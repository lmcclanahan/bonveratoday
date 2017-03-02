using System;

namespace ExigoService
{
    public class GetCompanyNewsRequest : DataRequest
    {
        public int[] NewsItemIDs { get; set; }
        public int[] NewsDepartments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IncludeBody { get; set; }
    }
}