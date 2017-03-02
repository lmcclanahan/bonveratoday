using Common.Api.ExigoWebService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static List<CompanyNewsItem> GetCompanyNews(GetCompanyNewsRequest request)
        {
            var results = new List<CompanyNewsItem>();
            var exigoWebService = Exigo.WebService();

            // Ensure that we have the minimum requirements
            var hasRequestedDepartments = (request.NewsDepartments != null && request.NewsDepartments.Length > 0);
            var hasRequestedNewsItems = (request.NewsItemIDs != null && request.NewsItemIDs.Length > 0);

            if (!hasRequestedDepartments && !hasRequestedNewsItems)
            {
                return results;
            }
                        

            // Get the news items based on the requested settings
            var tasks = new List<Task>();

            if (hasRequestedDepartments)
            {
                var apiItems = new List<CompanyNewsResponse>();

                foreach (var department in request.NewsDepartments)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        apiItems.AddRange(exigoWebService.GetCompanyNews(new Common.Api.ExigoWebService.GetCompanyNewsRequest
                        {
                            DepartmentType = department,
                            StartDate      = request.StartDate,
                            EndDate        = request.EndDate
                        }).CompanyNews);
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                // Convert the news items to our model
                results = apiItems.Select(apiItem => (CompanyNewsItem)apiItem).ToList();

            }
            else if (hasRequestedNewsItems)
            {
                var apiItems = new List<GetCompanyNewsItemResponse>();

                foreach (var newsItemID in request.NewsItemIDs)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        apiItems.Add(exigoWebService.GetCompanyNewsItem(new GetCompanyNewsItemRequest()
                        {
                            NewsID = newsItemID
                        }));
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                // Convert the news items to our model
                results = apiItems.Select(apiItem => (CompanyNewsItem)apiItem).ToList();
            }


            // Now that we have the results, order them and filter out the ones we don't need
            results = results.OrderByDescending(c => c.CreatedDate).ToList();
            if(hasRequestedDepartments && request.RowCount > 0)
            {
                results = results.Skip(request.Skip).Take(request.Take).ToList();
            }



            // Fetch the bodies of each news item if applicable
            if (hasRequestedDepartments && request.IncludeBody)
            {
                foreach (var result in results)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        var apiItemDetail = exigoWebService.GetCompanyNewsItem(new GetCompanyNewsItemRequest()
                        {
                            NewsID = result.NewsItemID
                        });

                        result.Body = apiItemDetail.Content;
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
            }

            return results;
        }
    }
}