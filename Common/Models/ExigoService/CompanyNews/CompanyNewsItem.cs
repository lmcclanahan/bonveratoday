using System;

namespace ExigoService
{
    public class CompanyNewsItem : ICompanyNewsItem
    {
        public int NewsItemID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }       
        public DateTime CreatedDate { get; set; }
    }
}