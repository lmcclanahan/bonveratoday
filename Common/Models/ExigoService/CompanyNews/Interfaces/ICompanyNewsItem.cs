using System;

namespace ExigoService
{
    public interface ICompanyNewsItem
    {
        int NewsItemID { get; set; }
        string Title { get; set; }
        string Body { get; set; }
        DateTime CreatedDate { get; set; }
    }
}