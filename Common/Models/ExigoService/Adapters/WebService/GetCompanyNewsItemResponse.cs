namespace Common.Api.ExigoWebService
{
    public partial class GetCompanyNewsItemResponse
    {
        public static explicit operator ExigoService.CompanyNewsItem(GetCompanyNewsItemResponse newsItem)
        {
            var model = new ExigoService.CompanyNewsItem();
            if (newsItem == null) return model;

            model.NewsItemID  = newsItem.NewsID;
            model.Title       = newsItem.Description;
            model.Body        = newsItem.Content;
            model.CreatedDate = newsItem.CreatedDate;

            return model;
        }
    }
}