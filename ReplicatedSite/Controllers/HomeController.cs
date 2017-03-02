using Common;
using ExigoService;
using ReplicatedSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Helpers;
using Common.HtmlHelpers;

namespace ReplicatedSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var sponsorID = Exigo.GetCustomersSponsorPreference(3);
            if (GlobalSettings.Globalization.HideForLive)
            {
                return View("PreLaunchIndex");
            }
            else
            {
                return View();
            }
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        public ActionResult CompanyOverview()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult OurMission()
        {
            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        [Route("distributorsearch")]
        public ActionResult DistributorSearch()
        {
            var model = new DistributorSearch();


            return View(model);
        }

        #region AJAX

        [AllowAnonymous]
        [HttpPost]
        public JsonNetResult GetDistributors(DistributorSearch model)
        {
            try
            {
                //Establish a search object on which to build
                var baseQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate);

                var customerQuery = baseQuery;

                // Check our Search Type to determine which variables to pass into search
                switch (model.SearchTypeID)
                {
                    case SearchType.webaddress:
                        customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.WebAlias.ToUpper() == model.WebAlias.ToUpper());
                        break;
                    case SearchType.distributorID:
                        customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.CustomerID == model.DistributorID);
                        break;
                    case SearchType.zipcode:
                        customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.Customer.MailZip == model.CustomerZipCode);
                        break;
                    case SearchType.distributorInfo:

                        #region DistributorInfoSearch Options and Combinations

                        #region Helpers

                        // Establish combinations of required and optional fields for Distributor Info Search
                        var requiredFieldsforInfoSearch = model.DistributorState != null && model.DistributorFirstName != null && model.DistributorLastName != null;
                        var infoSearchWithCity = requiredFieldsforInfoSearch && model.DistributorCity != null; // Required Fields plus City
                        var infoSearchWithZip = requiredFieldsforInfoSearch && model.DistributorZipCode != null; // Required Fields plus Zip
                        var allFieldsFromInfoSearch = requiredFieldsforInfoSearch && model.DistributorCity != null && model.DistributorZipCode != null; // All Fields
                        #endregion

                        if (allFieldsFromInfoSearch)
                        {
                            customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.FirstName == model.DistributorFirstName && c.LastName == model.DistributorLastName && c.Customer.MailState == model.DistributorState && c.Customer.MailCity == model.DistributorCity && c.Customer.MailZip == model.DistributorZipCode);

                            if (customerQuery.Count() == 0)
                            {
                                customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.FirstName == model.DistributorFirstName && c.LastName == model.DistributorLastName && c.Customer.MailState == model.DistributorState);
                            }
                        }

                        else if (infoSearchWithCity)
                        {
                            customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.FirstName == model.DistributorFirstName && c.LastName == model.DistributorLastName && c.Customer.MailState == model.DistributorState && c.Customer.MailCity == model.DistributorCity);
                        }

                        else if (infoSearchWithZip)
                        {
                            customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.FirstName == model.DistributorFirstName && c.LastName == model.DistributorLastName && c.Customer.MailState == model.DistributorState && c.Customer.MailZip == model.DistributorZipCode);
                        }

                        else
                        {
                            customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && c.FirstName == model.DistributorFirstName && c.LastName == model.DistributorLastName && c.Customer.MailState == model.DistributorState);
                        }

                        #endregion

                        break;
                    default:
                        customerQuery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate && (c.WebAlias.ToUpper() == model.WebAlias.ToUpper() || c.CustomerID == model.DistributorID));
                        break;
                }

                // Assemble a list for customers who match the search criteria
                var DistributorCollection = new List<SearchResult>();

                var urlHelper = new UrlHelper(Request.RequestContext);
                foreach (var item in DistributorCollection)
                {
                    item.AvatarURL = urlHelper.Avatar(item.CustomerID);
                    model.AvatarUrl = item.AvatarURL;
                }

                if (customerQuery.Count() > 0)
                {
                    DistributorCollection = customerQuery.Select(c => new SearchResult
                    {
                        CustomerID = c.CustomerID,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        MainCity = c.Customer.MailCity,
                        MainState = c.Customer.MailState,
                        MainCountry = c.Customer.MailCountry,
                        WebAlias = c.WebAlias
                    }).ToList();

                    foreach (var distributor in customerQuery)
                    {

                        model.BaseQuery.Add(distributor);

                    }
                }

                var html = this.RenderPartialViewToString("partials/_distributorSearchModal", model);

                return new JsonNetResult(new
                {
                    success = true,
                    distributors = DistributorCollection,
                    html = html
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonNetResult GetEvents(DistributorSearch model)
        {
            try
            {
                // assemble a list of customers who match the search criteria
                var enrollerCollection = new List<SearchResult>();

                var basequery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate);
                bool isNumber = model.EventCode.CanBeParsedAs<int>();

                var customerQuery = basequery;

                if (isNumber)
                {

                    customerQuery = basequery.Where(c => c.CustomerID == Convert.ToInt32(model.EventCode));

                }


                if (customerQuery.Count() > 0)
                {
                    enrollerCollection = customerQuery.Select(c => new SearchResult
                    {
                        CustomerID = c.CustomerID,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        MainCity = c.Customer.MainCity,
                        MainState = c.Customer.MainState,
                        MainCountry = c.Customer.MainCountry,
                        WebAlias = c.WebAlias
                    }).ToList();



                }

                var urlHelper = new UrlHelper(Request.RequestContext);
                foreach (var item in enrollerCollection)
                {
                    item.AvatarURL = urlHelper.Avatar(item.CustomerID);
                }

                return new JsonNetResult(new
                {
                    success = true,
                    enrollers = enrollerCollection
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region Models and Enums
        public class SearchResult
        {
            public int CustomerID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }
            public string AvatarURL { get; set; }
            public string WebAlias { get; set; }
            public string ReplicatedSiteUrl
            {
                get
                {
                    if (string.IsNullOrEmpty(this.WebAlias)) return "";
                    else return "" + this.WebAlias;
                }
            }
            public string MainState { get; set; }
            public string MainCity { get; set; }
            public string MainCountry { get; set; }
        }

        public class ProductSearchResult
        {
            public int ItemID { get; set; }
            public string ItemDescription { get; set; }
            public string ItemCode { get; set; }

            public string ItemImageURL { get; set; }
            public string ItemPageUrl
            {
                get
                {
                    if (string.IsNullOrEmpty(this.ItemCode)) return "";
                    else return "products/" + this.ItemCode;
                }
            }
            public decimal ItemPrice { get; set; }
            public Availability ItemAvailability { get; set; }
            public int Quantity { get; set; }

            public enum Availability
            {
                Available = 1,
                OutofStock = 2,
                Discontinued = 3,
                PendingRelease = 4
            }
        }
        #endregion
    }
}