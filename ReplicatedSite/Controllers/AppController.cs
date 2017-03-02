using Common;
using Common.Services;
using ExigoService;
using Newtonsoft.Json;
using ReplicatedSite.Services;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Dapper;
using System.Collections;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using ReplicatedSite.ViewModels;

namespace ReplicatedSite.Controllers
{
    public class AppController : Controller
    {
        #region Global & Local Resources
        [AllowAnonymous]
        public JavaScriptResult Resource(string name = "resources", string path = "Resources")
        {
            // Clean up any references to .resx - our code enters that automatically.
            if (path.Contains(".resx")) path = path.Replace(".resx", "");

            // Create our factory
            var service = new ClientResourceService();
            service.JavaScriptObjectName = name;
            service.GlobalResXFileName = path;

            // Write our javascript to the page.            
            return JavaScript(service.GetJavaScript());
        }
        #endregion

        #region Cultures
        [AllowAnonymous]
        public JavaScriptResult Culture()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            var adsfads = JsonConvert.SerializeObject(currentCulture.NumberFormat);

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
            Exigo.SetCustomerPreferredLanguage(Identity.Customer.CustomerID, id);
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
            return Json(Exigo.IsLoginNameAvailable(LoginName), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsWebAliasAvailable([Bind(Prefix = "Customer.LoginName")]string LoginName)
        {
            return Json(Exigo.IsWebAliasAvailable(LoginName), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsEmailAvailable([Bind(Prefix = "Customer.Email")]string Email)
        {
            return Json(Exigo.IsEmailAvailable(Identity.Owner.CustomerID, Email), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsTaxIDAvailable([Bind(Prefix = "Customer.TaxID")]string TaxID)
        {
            return Json(Exigo.IsTaxIDAvailable(TaxID), JsonRequestBehavior.AllowGet);
        }

        // Used for Retail Register page
        public JsonResult IsUserNameAvailable([Bind(Prefix = "UserName")]string UserName)
        {
            var isValid = true;

            var isEmailValid = Exigo.IsEmailAvailable(UserName);
            if (!isEmailValid)
            {
                isValid = false;
            }

            var isUserNameValid = Exigo.IsLoginNameAvailable(UserName);
            if (!isUserNameValid)
            {
                isValid = false;
            }

            return Json(isValid, JsonRequestBehavior.AllowGet);
        }


        // Used for just Enroller- For non soft launch
        public JsonResult IsEnrollerValid(string filter)
        {
            try
            {
                var enroller = new Customer();

                var where = "";

                if (filter.CanBeParsedAs<int>())
                {
                    where = "AND c.CustomerID = " + filter;
                }
                else
                {
                    where = "AND c.LoginName = '{0}'".FormatWith(filter);
                }

                using (var context = Exigo.Sql())
                {
                    var query = context.Query<Customer>(@"
                            Select c.CustomerID, 
                                c.FirstName, 
                                c.LastName, 
                                c.Company, 
                                c.Email, 
                                c.LoginName 
                            from Customers c
                            where c.CustomerStatusID = @activeCustomerStatus
                                AND c.CustomerTypeID = @distributorCustomerTypeID
                            " + where, new
                                {
                                    activeCustomerStatus = (int)CustomerStatuses.Active,
                                    distributorCustomerTypeID = (int)CustomerTypes.Associate
                                }).ToList();

                    if (query.Count() > 0)
                    {
                        enroller = query.FirstOrDefault();
                    }
                    else
                    {
                        throw new Exception("No Independent Associate was found with the filter you entered, please try again.");
                    }
                }

                var html = this.RenderPartialViewToString("../Account/Partials/_EnrollerResult", enroller);


                return Json(new
                {
                    success = true,
                    resultHtml = html,
                    enrollerID = enroller.CustomerID
                }, JsonRequestBehavior.AllowGet);

                // return Json(Exigo.VerifyEnroller(filter), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // For Soft Launch
        public ActionResult VerifyEnrollerSponsor(string input, bool isEnroller, int EnrollerID = 0)
        {
            if (input.IsNullOrEmpty())
            {
                return new JsonNetResult(new
                {
                    success = true,
                    valid = false
                });
            }
            // Check both login name and customer id even if int change how its in sql based on whether or not it can parse. Need to find out about what happens if login name = customerid. 
            var whereclause = "";
            int customerid = 0;
            var EnrollerString = "";
            dynamic User = null;
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
                using (var context = Exigo.Sql())
                {

                    User = context.Query(@" " + EnrollerString + @"

                    Select  c.CustomerID
                    from Customers c " + checkForWebAliasTableString + @" 
                    Where c.CustomerID > 5 and c.CustomerStatusID in @customerstatuses " + checkForWebAliasString + @" and c.CustomerTypeID = @customertype " + whereclause + @"
            ", new
             {
                 customerstatuses = CustomerStatusTypes.ActiveStatusTypes.ToList(),
                 customertype = (int)CustomerTypes.Associate
             }).FirstOrDefault();

                }
            }

            else
            {
                using (var context = Exigo.Sql())
                {

                    User = context.Query(@"

                    Select  c.CustomerID, c.FirstName, c.LastName, c.Company from Customers c
                    inner join UniLevelDownline ud
                    on c.CustomerID = ud.CustomerID
                    " + checkForWebAliasTableString + @" 
                    Where ud.DownlineCustomerID = @id  and c.CustomerID > 5 and c.CustomerStatusID in @customerstatuses  " + checkForWebAliasString + @" and c.CustomerTypeID = @customertype " + whereclause + @"
            ", new
                 {
                     id = EnrollerID,
                     customerstatuses = CustomerStatusTypes.ActiveStatusTypes.ToList(),
                     customertype = (int)CustomerTypes.Associate
                 }).FirstOrDefault();

                }
            }
            if (User != null)
            {
                try
                {
                    var customer = Exigo.GetCustomer(User.CustomerID);
                    var detailsModel = new CustomerDetailsViewModel();
                    detailsModel.Customer = customer;
                    var customerdetailshtml = "";
                    customerdetailshtml = this.RenderPartialViewToString("../Enrollment/_CustomerDetails", detailsModel);

                    var successhtml = this.RenderPartialViewToString("../Enrollment/_EnrollerResult", detailsModel.Customer);

                    return new JsonNetResult(new
                    {
                        success = true,
                        valid = true,
                        html = customerdetailshtml,
                        successhtml = successhtml,
                        customerid = detailsModel.Customer.CustomerID
                    });
                }
                catch (Exception ex)
                {
                    var failhtml = this.RenderPartialViewToString("../Enrollment/_EnrollerFail", 0);
                    return new JsonNetResult(new
                    {
                        success = true,
                        valid = false,
                        html = failhtml
                    });
                }
            }
            else
            {
                try
                {
                    var failhtml = this.RenderPartialViewToString("../Enrollment/_EnrollerFail", 0);
                    return new JsonNetResult(new
                    {
                        success = true,
                        valid = false,
                        html = failhtml
                    });
                }
                catch (Exception e)
                {
                    Console.Write(e);
                    return new JsonNetResult(new
                    {
                        success = true,
                        valid = false
                    });
                }

            }
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
            else return RedirectToAction("Index", "Home");
        }

        [Route("~/StopDebug")]
        public ActionResult StopDebug(string goTo = null)
        {
            GlobalUtilities.DeleteDebugCookie();
            if (Request.IsAjaxRequest()) return Json(true);
            else if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.AbsoluteUri);
            else if (goTo != null) return Redirect(goTo);
            else return RedirectToAction("Index", "Home");
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
        //Z.M. 76882 5-10-16 Added corporate blog link-
        public ActionResult CorporateBlog()
        {
            return Redirect("http://blog.bonvera.com");
        }

        #endregion
    }
}
