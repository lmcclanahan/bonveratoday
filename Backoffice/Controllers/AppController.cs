using Backoffice.Services;
using Common;
using Common.Services;
using ExigoService;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using Dapper;
using System;
using Backoffice.ViewModels;
using Backoffice.Filters;
using System.Text.RegularExpressions;

namespace Backoffice.Controllers
{
    [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = false)]
    public class AppController : Controller
    {
        #region Warm-up
        /// <summary>
        /// Calls a short series of erroneous calls to ensure the site is warmed up and will perform at optimum speeds. Usually called during the sign-in process.
        /// </summary>
        [AllowAnonymous]
        public JsonNetResult WarmUp()
        {
            try
            {
                var tasks = new List<Task>();


                // Call the web service first, as its the slowest the first time through
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    Exigo.WebService().GetCustomers(new Common.Api.ExigoWebService.GetCustomersRequest
                    {
                        CustomerID = 1
                    });
                }));


                // Call the SQL via a Dapper call
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    using (var context = Exigo.Sql())
                    {
                        context.Query("select CustomerID from Customers where CustomerID = @customerid", new { customerid = 1 });
                    }
                }));


                // Perform each task at the same time
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
            }
            catch { }


            // Let the browser know we're good
            return new JsonNetResult(new
            {
                success = true
            });
        }
        #endregion

        #region Keeping Sessions Alive
        public JsonResult KeepAlive()
        {
            return Json("OK", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Cultures
        [AllowAnonymous]
        public JavaScriptResult Culture()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            var result = new StringBuilder();
            result.AppendFormat(@"
                CultureInfo = function (c, b, a) {{
                    this.name = c;
                    this.numberFormat = b;
                    this.dateTimeFormat = a
                }};

                CultureInfo.prototype = {{
                    _getDateTimeFormats: function () {{
                        if (!this._dateTimeFormats) {{
                            var a = this.dateTimeFormat;
                            this._dateTimeFormats = [a.MonthDayPattern, a.YearMonthPattern, a.ShortDatePattern, a.ShortTimePattern, a.LongDatePattern, a.LongTimePattern, a.FullDateTimePattern, a.RFC1123Pattern, a.SortableDateTimePattern, a.UniversalSortableDateTimePattern]
                        }}
                        return this._dateTimeFormats
                    }},
                    _getIndex: function (c, d, e) {{
                        var b = this._toUpper(c),
                            a = Array.indexOf(d, b);
                        if (a === -1) a = Array.indexOf(e, b);
                        return a
                    }},
                    _getMonthIndex: function (a) {{
                        if (!this._upperMonths) {{
                            this._upperMonths = this._toUpperArray(this.dateTimeFormat.MonthNames);
                            this._upperMonthsGenitive = this._toUpperArray(this.dateTimeFormat.MonthGenitiveNames)
                        }}
                        return this._getIndex(a, this._upperMonths, this._upperMonthsGenitive)
                    }},
                    _getAbbrMonthIndex: function (a) {{
                        if (!this._upperAbbrMonths) {{
                            this._upperAbbrMonths = this._toUpperArray(this.dateTimeFormat.AbbreviatedMonthNames);
                            this._upperAbbrMonthsGenitive = this._toUpperArray(this.dateTimeFormat.AbbreviatedMonthGenitiveNames)
                        }}
                        return this._getIndex(a, this._upperAbbrMonths, this._upperAbbrMonthsGenitive)
                    }},
                    _getDayIndex: function (a) {{
                        if (!this._upperDays) this._upperDays = this._toUpperArray(this.dateTimeFormat.DayNames);
                        return Array.indexOf(this._upperDays, this._toUpper(a))
                    }},
                    _getAbbrDayIndex: function (a) {{
                        if (!this._upperAbbrDays) this._upperAbbrDays = this._toUpperArray(this.dateTimeFormat.AbbreviatedDayNames);
                        return Array.indexOf(this._upperAbbrDays, this._toUpper(a))
                    }},
                    _toUpperArray: function (c) {{
                        var b = [];
                        for (var a = 0, d = c.length; a < d; a++) b[a] = this._toUpper(c[a]);
                        return b
                    }},
                    _toUpper: function (a) {{
                        return a.split(""\u00a0"").join("" "").toUpperCase()
                    }}
                }};
                CultureInfo._parse = function (a) {{
                    var b = a.dateTimeFormat;
                    if (b && !b.eras) b.eras = a.eras;
                    return new CultureInfo(a.name, a.numberFormat, b)
                }};


                CultureInfo.InvariantCulture = CultureInfo._parse({{
                    {0}
                }});

                CultureInfo.CurrentCulture = CultureInfo._parse({{
                    {1}
                }});

            ", GetCultureInfoJson(currentCulture), GetCultureInfoJson(currentUICulture));


            return JavaScript(result.ToString());
        }
        private string GetCultureInfoJson(CultureInfo cultureInfo)
        {
            var result = new StringBuilder();

            result.AppendFormat(@"
                ""name"": ""{0}"",
                ""numberFormat"": {1},
                ""dateTimeFormat"": {2},
                ""eras"": [1, ""A.D."", null, 0]
            ",
                cultureInfo.Name,
                JsonConvert.SerializeObject(cultureInfo.NumberFormat),
                JsonConvert.SerializeObject(cultureInfo.DateTimeFormat));

            return result.ToString();
        }
        #endregion

        #region Countries & Regions
        [OutputCache(Duration = 86400)]
        public JsonNetResult GetCountries()
        {
            var countries = Exigo.GetCountries();

            return new JsonNetResult(new
            {
                success = true,
                countries = countries
            });
        }

        [OutputCache(VaryByParam = "id", Duration = 86400)]
        public JsonNetResult GetRegions(string id)
        {
            var regions = Exigo.GetRegions(id);

            return new JsonNetResult(new
            {
                success = true,
                regions = regions
            });
        }
        #endregion

        #region Avatars
        [Route("~/profiles/avatar/{id:int}/{type?}/{cache?}")]
        public FileResult Avatar(int id, AvatarType type = AvatarType.Default, bool cache = true)
        {
            var bytes = Exigo.Images().GetCustomerAvatar(id, type, cache);

            // Return the image
            return File(bytes, "application/png", "{0}.png".FormatWith(id));
        }
        #endregion

        #region Globalization
        public ActionResult SetLanguagePreference(int id)
        {
            Exigo.SetCustomerPreferredLanguage(Identity.Current.CustomerID, id);
            new IdentityService().RefreshIdentity();

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        #endregion

        #region Validation
        public JsonResult VerifyAddress(Address address)
        {
            return Json(Exigo.VerifyAddress(address), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsLoginNameAvailable([Bind(Prefix = "Customer.LoginName")]string LoginName)
        {
            return Json(Exigo.IsLoginNameAvailable(LoginName, Identity.Current.CustomerID), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsTaxIDAvailable([Bind(Prefix = "Customer.TaxID")]string TaxID)
        {
            return Json(Exigo.IsTaxIDAvailable(TaxID), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsTaxIDAvailable_Account([Bind(Prefix = "TaxID")]string TaxID)
        {
            return Json(Exigo.IsTaxIDAvailable(TaxID), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Debug

        [Route("~/StartDebug")]
        public ActionResult StartDebug(string goTo = null)
        {
            GlobalUtilities.SetDebugCookie();
            if (Request.IsAjaxRequest()) return Json(true);
            else if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.AbsoluteUri);
            else if (goTo != null) return Redirect(goTo);
            else return RedirectToAction("Index", "Dashboard");
        }

        [Route("~/StopDebug")]
        public ActionResult StopDebug(string goTo = null)
        {
            GlobalUtilities.DeleteDebugCookie();
            if (Request.IsAjaxRequest()) return Json(true);
            else if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.AbsoluteUri);
            else if (goTo != null) return Redirect(goTo);
            else return RedirectToAction("Index", "Dashboard");
        }

        #endregion

        #region Enrollment Redirect
        [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = true)]
        public ActionResult EnrollmentRedirect()
        {
            return View();
        }


        [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = true)]
        public ActionResult RetailEnrollmentRedirect()
        {
            var webAlias = GlobalSettings.ReplicatedSites.DefaultWebAlias;
            try
            {
                var customerID = Identity.Current.CustomerID;

                using (var context = Exigo.Sql())
                {
                    var _webAlias = context.Query<string>(@"
                                    select top 1 WebAlias from CustomerSites where CustomerID = @customerID
                                    ", new { customerID }).FirstOrDefault();

                    if (_webAlias.IsNotNullOrEmpty())
                    {
                        webAlias = _webAlias;
                    }
                }
            }
            catch { }

            var url = GlobalSettings.ReplicatedSites.FormattedBaseUrl.FormatWith(webAlias) + "/register";

            return Redirect(url);
        }

        [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = true)]
        public ActionResult SmartShopperEnrollmentRedirect()
        {
            var webAlias = GlobalSettings.ReplicatedSites.DefaultWebAlias;
            try
            {
                var customerID = Identity.Current.CustomerID;

                using (var context = Exigo.Sql())
                {
                    var _webAlias = context.Query<string>(@"
                                    select top 1 WebAlias from CustomerSites where CustomerID = @customerID
                                    ", new { customerID }).FirstOrDefault();

                    if (_webAlias.IsNotNullOrEmpty())
                    {
                        webAlias = _webAlias;
                    }
                }
            }
            catch { }

            var url = GlobalSettings.ReplicatedSites.FormattedBaseUrl.FormatWith(webAlias) + "/smartshopper/register";

            return Redirect(url);
        }

        /// <summary>
        /// This action is used by the Enrollment Redirect view to send a user to the correct enrollment page with enroller and sponsor customer id information as well
        /// </summary>
        /// <param name="enroller">Enroller of new enrollment</param>
        /// <param name="sponsor">Sponsor of new enrollment</param>
        /// <returns>Redirects to replicated site back office enrollment landing page</returns>
        [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = true)]
        public ActionResult CustomEnrollmentRedirect(int enroller, int sponsor)
        {
            var webAlias = GlobalSettings.ReplicatedSites.DefaultWebAlias;
            var customerSite = Exigo.GetCustomerSite(enroller);
            if (!GlobalSettings.Globalization.HideForLive)
            {
                if (customerSite != null && customerSite.WebAlias.IsNotNullOrEmpty())
                {
                    webAlias = customerSite.WebAlias;
                }

                var url = GlobalSettings.ReplicatedSites.FormattedBaseUrl.FormatWith(webAlias) + "/backofficeenrollmentlanding?ownerID=" + Identity.Current.CustomerID + "&enroller=" + enroller + "&sponsor=" + sponsor;

                return Redirect(url);
            }
            else
            {
                var url = GlobalSettings.Company.BaseReplicatedeUrl + "/backofficeenrollmentlanding?ownerID=" + Identity.Current.CustomerID + "&enroller=" + enroller + "&sponsor=" + sponsor;
                return Redirect(url);

            }
        }

        [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = true)]
        public ActionResult VerifyEnrollerSponsor(string input, bool isEnroller)
        {
            // Check both login name and customer id even if int change how its in sql based on whether or not it can parse. Need to find out about what happens if login name = customerid. 
            var whereclause = "";
            int customerid = 0;
            var EnrollerString = "";
            dynamic User = null;
            if (input.IsNullOrEmpty())
            {
                return new JsonNetResult(new
                {
                    success = true,
                    valid = false
                });
            }
            var checkForWebAliasString = (!GlobalSettings.Globalization.HideForLive) ? "and cs.WebAlias != '' and cs.WebAlias IS NOT NULL" : "";
            var checkForWebAliasTableString = (!GlobalSettings.Globalization.HideForLive) ? "left join CustomerSites cs on cs.CustomerID = c.CustomerID" : "";

            if (int.TryParse(input, out customerid))
            {
                whereclause = "AND c.CustomerID = " + customerid;
            }
            else
            {
                whereclause = "AND c.LoginName = '" + input + "'";
            }
            if (isEnroller)
            {
                EnrollerString = "Select c.CustomerID, c.FirstName, c.LastName, c.Company from Customers c " +
                                 " inner join UniLevelUpline uu " +
                                     " on c.CustomerID = uu.CustomerID " +
                                     checkForWebAliasTableString +
                                     " Where uu.UplineCustomerID = @id " + checkForWebAliasString +
                                     " and c.CustomerID > 5  and c.CustomerTypeID = @associateCustomerType and c.CustomerStatusID in @customerstatuses " + whereclause + @"
                                        
                                     union ";
            }

            using (var context = Exigo.Sql())
            {
                User = context.Query(@" " + EnrollerString + @"
                Select  c.CustomerID, c.FirstName, c.LastName, c.Company from Customers c
                inner join UniLevelDownline ud
                on c.CustomerID = ud.CustomerID
                 " + checkForWebAliasTableString + @"
                Where ud.DownlineCustomerID = @id " + checkForWebAliasString + @" and c.CustomerID > 5 and c.CustomerTypeID = @associateCustomerType and c.CustomerStatusID in @customerstatuses " + whereclause + @"
            ", new
         {
             id = Identity.Current.CustomerID,
             associateCustomerType = (int)CustomerTypes.Associate,
             customerstatuses = CustomerStatusTypes.ActiveStatusTypes.ToList()
         }).FirstOrDefault();
            }


            if (User != null)
            {
                var customer = Exigo.GetCustomer(User.CustomerID);
                var detailsModel = new CustomerDetailsViewModel();
                detailsModel.Customer = customer;
                var customerdetailshtml = "";

                customerdetailshtml = this.RenderPartialViewToString("_CustomerDetails", detailsModel);

                var successhtml = this.RenderPartialViewToString("_EnrollerResult", detailsModel.Customer);

                return new JsonNetResult(new
                {
                    success = true,
                    valid = true,
                    html = customerdetailshtml,
                    successhtml = successhtml,
                    customerid = detailsModel.Customer.CustomerID
                });
            }
            else
            {
                var failhtml = this.RenderPartialViewToString("_EnrollerFail", 0);
                return new JsonNetResult(new
                {
                    success = true,
                    valid = false,
                    html = failhtml
                });

            }
        }

        #endregion

        #region Contact Us
        [HttpPost]
        public JsonNetResult MessageUS(string fromEmail, string subject, string body)
        {
            try
            {
                // Validate that the email is valid
                var emailRegex = new Regex(GlobalSettings.RegularExpressions.EmailAddresses);
                var isVailidEmail = emailRegex.IsMatch(fromEmail);
                var bodyPrefix = "<b>Customer ID:</b> " + Identity.Current.CustomerID + "<br/>";
                bodyPrefix += "<b>Name:</b> " + Identity.Current.FullName + "<br/><br/>";

                body = bodyPrefix + body;

                if (!isVailidEmail)
                {
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "The email entered appears to be invalid, please try again"
                    });
                }

                Exigo.SendEmail(new SendEmailRequest()
                {
                    To = new[] { GlobalSettings.Emails.SupportEmail },
                    From = fromEmail,
                    ReplyTo = new[] { fromEmail },
                    SMTPConfiguration = GlobalSettings.Emails.SMTPConfigurations.Default,
                    Subject = subject,
                    Body = body
                });

                return new JsonNetResult(new
                {
                    success = true
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

        #region Corporate Blog
        //@*Z.M. 76882 5-10-16 Added corporate blog link-
        public ActionResult CorporateBlog()
        {
            return Redirect("http://blog.bonvera.com");
        }

        #endregion

        #region Order Business Cards
        // P.M. 78700 7/21/2016 Added order business cards link.
        public ActionResult OrderBusinessCards()
        {
            return Redirect("http://www.docuplex.com/customer_portal/login.html?ut=b983e98e-a2a6-4755-a6f1-6b1e0e0d4259");
        }
        #endregion
    }
}

