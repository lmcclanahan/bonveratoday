using Exigo.Api;
using Exigo.Api.Base;
using Exigo.Api.OData;
using Exigo.Api.WebService;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CompanyNewsService : ICompanyNewsService
    {
        public ICompanyNewsService Provider { get; set; }
        public CompanyNewsService(ICompanyNewsService Provider = null)
        {
            if (Provider != null)
            {
                this.Provider = Provider;
            }
            else
            {
                this.Provider = new WebServiceCompanyNewsProvider(new DefaultApiSettings());
            }
        }

        public List<CompanyNewsStory> GetCompanyNewsStories(int DepartmentID, int Page = 1, int RecordCount = 10)
        {
            return Provider.GetCompanyNewsStories(DepartmentID, Page, RecordCount);
        }
        public List<CompanyNewsStory> GetCompanyNewsStories(int DepartmentID, DateTime StartDate, DateTime EndDate, int Page = 1, int RecordCount = 10)
        {
            return Provider.GetCompanyNewsStories(DepartmentID, StartDate, EndDate, Page, RecordCount);
        }    
    }

    public interface ICompanyNewsService
    {
        List<CompanyNewsStory> GetCompanyNewsStories(int DepartmentID, int Page = 1, int RecordCount = 10);
        List<CompanyNewsStory> GetCompanyNewsStories(int DepartmentID, DateTime StartDate, DateTime EndDate, int Page = 1, int RecordCount = 10);
    }

    #region Web Service
    public class WebServiceCompanyNewsProvider : BaseWebServiceProvider, ICompanyNewsService
    {
        public WebServiceCompanyNewsProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public List<CompanyNewsStory> GetCompanyNewsStories(int DepartmentID, int Page = 1, int RecordCount = 10)
        {
            var result = new List<CompanyNewsStory>();

            var response = GetContext().GetCompanyNews(new GetCompanyNewsRequest()
            {
                DepartmentType = DepartmentID,
                StartDate = new DateTime(),
                EndDate = DateTime.Now
            });

            result = response.CompanyNews.ToList()
                .Select(c => new CompanyNewsStory()
                {
                    CompanyNewsStoryID = c.NewsID,
                    Title = c.Description,
                     CreatedDate = c.CreatedDate
                })
                .OrderByDescending(c => c.CreatedDate)
                .Skip((Page - 1) * RecordCount)
                .Take(RecordCount)
                .ToList();


            // Fetch the contents of each news article in tasks so we can get this data faster.
            var tasks = new List<Task>();
            foreach(var story in result)
            {
                tasks.Add(Task.Factory.StartNew(() => {
                    var detailresponse = GetContext().GetCompanyNewsItem(new GetCompanyNewsItemRequest()
                    {
                        NewsID = story.CompanyNewsStoryID
                    });
                    story.Body = detailresponse.Content;
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());
            tasks.Clear();


            return result;
        }
        public List<CompanyNewsStory> GetCompanyNewsStories(int DepartmentID, DateTime StartDate, DateTime EndDate, int Page = 1, int RecordCount = 10)
        {
            var result = new List<CompanyNewsStory>();

            var response = GetContext().GetCompanyNews(new GetCompanyNewsRequest()
            {
                DepartmentType = DepartmentID,
                StartDate = StartDate,
                EndDate = EndDate
            });

            result = response.CompanyNews.ToList()
                .Select(c => new CompanyNewsStory()
                {
                    CompanyNewsStoryID = c.NewsID,
                    Title = c.Description,
                     CreatedDate = c.CreatedDate
                })
                .OrderByDescending(c => c.CreatedDate)
                .Skip((Page - 1) * RecordCount)
                .Take(RecordCount)
                .ToList();


            // Fetch the contents of each news article in tasks so we can get this data faster.
            var tasks = new List<Task>();
            foreach(var story in result)
            {
                tasks.Add(Task.Factory.StartNew(() => {
                    var detailresponse = GetContext().GetCompanyNewsItem(new GetCompanyNewsItemRequest()
                    {
                        NewsID = story.CompanyNewsStoryID
                    });
                    story.Body = detailresponse.Content;
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());
            tasks.Clear();


            return result;
        }

    }
    #endregion
}
