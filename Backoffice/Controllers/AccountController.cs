using Backoffice.Filters;
using Backoffice.Models.CommissionPayout;
using Backoffice.Services;
using Backoffice.ViewModels;
using Common;
using Common.Api.ExigoWebService;
using Common.Filters;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backoffice.Controllers
{
    [RoutePrefix("account")]
    [Route("{action=index}")]
    [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = false)]
    public class AccountController : Controller
    {
        #region Overview
        [Route("settings")]
        public ActionResult Index()
        {
            var model = new AccountOverviewViewModel();
            var customer = Exigo.GetCustomer(Identity.Current.CustomerID);
            model.Enroller = customer.Enroller;
            model.Sponsor = customer.Sponsor;
            var website = Exigo.GetCustomerSite(Identity.Current.CustomerID);
            var socialNetworksResponse = Exigo.WebService().GetCustomerSocialNetworks(new GetCustomerSocialNetworksRequest()
            {
                CustomerID = Identity.Current.CustomerID
            });

            // Social NetWorks
            foreach (var network in socialNetworksResponse.CustomerSocialNetwork)
            {
                switch (network.SocialNetworkID)
                {
                    case (int)SocialNetworks.Facebook: model.FacebookUrl = network.Url; break;
                    case (int)SocialNetworks.Twitter: model.TwitterUrl = network.Url; break;
                    case (int)SocialNetworks.YouTube: model.YouTubeUrl = network.Url; break;
                    case (int)SocialNetworks.Blog: model.BlogUrl = network.Url; break;
                }
            }


            //Basic Info
            model.CustomerID = customer.CustomerID;
            model.FirstName = customer.FirstName;
            model.LastName = customer.LastName;
            model.Email = customer.Email;
            model.WebAlias = website.WebAlias;
            model.LoginName = customer.LoginName;
            model.LanguageID = customer.LanguageID;
            model.CreatedDate = customer.CreatedDate;


            // Team Placement
            var currentPeriod = Exigo.GetCurrentPeriod(PeriodTypes.Default);
            var placementOptions = new List<SelectListItem>();
            var volumes = Exigo.GetCustomerVolumes(new GetCustomerVolumesRequest()
            {
                CustomerID = Identity.Current.CustomerID,
                PeriodID = currentPeriod.PeriodID,
                PeriodTypeID = PeriodTypes.Default
            });
            var canSeeTeamOne = (volumes.Volume50 > 0);
            var canSeeTeamTwo = (volumes.Volume51 > 0);
            var canSeeTeamThree = (volumes.Volume52 > 0);
            var canSeeTeamFour = (volumes.Volume53 > 0);
            var canSeeTeamFive = (volumes.Volume54 > 0);
            model.TeamPlacementPreferenceID = customer.Field1;

            // Only show available teams. If none available, default to team one
            if (canSeeTeamOne || (canSeeTeamOne == false && canSeeTeamTwo == false && canSeeTeamThree == false && canSeeTeamFour == false && canSeeTeamFive == false))
            {
                placementOptions.Add(new SelectListItem { Text = Resources.Common.Team + " 1", Value = "1" });
            }
            if (canSeeTeamTwo)
            {
                placementOptions.Add(new SelectListItem { Text = Resources.Common.Team + " 2", Value = "2" });
            }
            if (canSeeTeamThree)
            {
                placementOptions.Add(new SelectListItem { Text = Resources.Common.Team + " 3", Value = "3" });
            }
            if (canSeeTeamFour)
            {
                placementOptions.Add(new SelectListItem { Text = Resources.Common.Team + " 4", Value = "4" });
            }
            if (canSeeTeamFive)
            {
                placementOptions.Add(new SelectListItem { Text = Resources.Common.Team + " 5", Value = "5" });
            }
            model.TeamPlacementPreferenceOptions = placementOptions;

            // Set the description for the user's team to be displayed
            if (model.TeamPlacementPreferenceID != "")
            {
                model.TeamPlacementPreference = Resources.Common.Team + " " + model.TeamPlacementPreferenceID;
            }
            // If somehow the customer does not have a value in thier Field1, default the description to the first available option
            else
            {
                var firstAvailableTeamNumber = placementOptions.OrderBy(c => c.Value).FirstOrDefault().Value;
                model.TeamPlacementPreference = Resources.Common.Team + " " + firstAvailableTeamNumber;
            }


            // Tax ID - Added try catch around this because it fails from time to time and kills the entire page - Mike M.           
            try
            {
                var request = new Common.Api.ExigoWebService.GetCustomReportRequest();
                request.ReportID = 3;
                request.Parameters = new List<ParameterRequest>
            {
                new ParameterRequest { ParameterName = "CustomerID", Value = Identity.Current.CustomerID }                        
            }.ToArray();
                var taxIDResponse = Exigo.WebService().GetCustomReport(request);
                var taxId = taxIDResponse.ReportData.Tables[0].Rows[0][0].ToString();
                model.TaxIDIsSet = (taxId != "");
                model.MaskedTaxID = taxId;
            }
            catch (Exception ex)
            {

            }

            // Contact
            model.PrimaryPhone = customer.PrimaryPhone;
            model.SecondaryPhone = customer.SecondaryPhone;
            model.MobilePhone = customer.MobilePhone;
            model.Fax = customer.Fax;
            model.Addresses = customer.Addresses;


            // Customer Site
            model.CustomerSite.FirstName = website.FirstName;
            model.CustomerSite.LastName = website.LastName;
            model.CustomerSite.Email = website.Email;
            model.CustomerSite.PrimaryPhone = website.PrimaryPhone;
            model.CustomerSite.SecondaryPhone = website.SecondaryPhone;
            model.CustomerSite.Fax = website.Fax;

            model.CustomerSite.Notes1 = website.Notes1;
            model.CustomerSite.Notes2 = website.Notes2;
            model.CustomerSite.Notes3 = website.Notes3;
            model.CustomerSite.Notes4 = website.Notes4;

            model.CustomerSite.Address.Address1 = website.Address.Address1;
            model.CustomerSite.Address.Address2 = website.Address.Address2;
            model.CustomerSite.Address.Country = website.Address.Country;
            model.CustomerSite.Address.City = website.Address.City;
            model.CustomerSite.Address.State = website.Address.State;
            model.CustomerSite.Address.Zip = website.Address.Zip;


            // Opt in
            model.IsOptedIn = customer.IsOptedIn;

            // Annual Membership
            model.Membership = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                IncludeDetails = true,
                IncludePaymentMethods = true,
                IncludeInactiveAutoOrders = true
            }).Where(v => v.Details.Any(d => d.ItemCode == "IAANNUALRENEWAL")).FirstOrDefault();

            model.ActiveMembership = model.Membership != null ? "Scheduled" + @model.Membership.NextRunDate : "No Renewal Scheduled";

            // Get the available languages
            model.Languages = Exigo.GetLanguages();


            return View(model);
        }

        // Account settings
        [HttpParamAction]
        public JsonNetResult UpdateTeamPlacement(string teamPlacementPreferenceID)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Field1 = teamPlacementPreferenceID
            });
            var html = string.Format("Team {0}", teamPlacementPreferenceID);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateTeamPlacement",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateEmailAddress(string email)
        {
            var customerID = Identity.Current.CustomerID;
            var emailIsAvailable = Exigo.IsEmailAvailable(customerID, email);

            if (emailIsAvailable)
            {
                Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
                {
                    CustomerID = customerID,
                    Email = email
                });

                Exigo.SendEmailVerification(customerID, email);

                var html = string.Format("{0}", email);

                return new JsonNetResult(new
                {
                    success = true,
                    action = "UpdateEmailAddress",
                    html = html
                });
            }
            else
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = Resources.Common.EmailNotAvailable
                });
            }
        }

        [HttpParamAction]
        public JsonNetResult UpdateName(string firstname, string lastname)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                FirstName = firstname,
                LastName = lastname
            });

            var html = string.Format("{0} {1}, {3}# {2}", firstname, lastname, Identity.Current.CustomerID, Resources.Common.ID);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateName",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebAlias(string webalias)
        {
            Exigo.WebService().SetCustomerSite(new SetCustomerSiteRequest
            {
                CustomerID = Identity.Current.CustomerID,
                WebAlias = webalias
            });

            var html = string.Format("<a href='" + GlobalSettings.Company.BaseReplicatedeUrl + "/" + webalias + "'>" + GlobalSettings.Company.BaseReplicatedeUrl + "/{0}</a>", webalias);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebAlias",
                html = html
            });
        }
        public JsonResult IsValidWebAlias(string webalias)
        {
            var isValid = Exigo.IsWebAliasAvailable(Identity.Current.CustomerID, webalias);

            if (isValid) return Json(true, JsonRequestBehavior.AllowGet);
            else return Json(string.Format(Resources.Common.PasswordNotAvailable, webalias), JsonRequestBehavior.AllowGet);
        }

        [HttpParamAction]
        public JsonNetResult UpdateLoginName(string loginname)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                LoginName = loginname
            });

            var html = string.Format("{0}", loginname);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateLoginName",
                html = html
            });
        }
        public JsonResult IsValidLoginName(string loginname)
        {
            var isValid = Exigo.IsLoginNameAvailable(loginname, Identity.Current.CustomerID);

            if (isValid) return Json(true, JsonRequestBehavior.AllowGet);
            else return Json(string.Format(Resources.Common.LoginNameNotAvailable, loginname), JsonRequestBehavior.AllowGet);
        }

        [HttpParamAction]
        public JsonNetResult UpdateTaxID(string taxId, TaxIDType taxIdType)
        {
            try
            {
                var customerID = Identity.Current.CustomerID;

                // Double check one last time to make sure a Tax ID is available
                var isValid = Exigo.IsTaxIDAvailable(taxId, customerID: customerID);
                if (!isValid)
                {
                    throw new Exception("Tax ID not valid");
                }
                // 79630 09/01/2016 PM Added TaxIDType to UpdateCustomerRequest
                Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
                {
                    CustomerID = Identity.Current.CustomerID,
                    TaxID = taxId,
                    TaxIDType = taxIdType
                });

                var html = string.Format("{0}", "<p>Please contact Cusomter Support at <a href='mailto:support@bonvera.com'>support@bonvera.com</a> to change your SSN.</p>");

                return new JsonNetResult(new
                {
                    success = true,
                    action = "UpdateTaxID",
                    html = html,
                    hideEdit = true
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    error = "This SSN is not available."
                });
            }
        }

        [HttpParamAction]
        public JsonNetResult UpdatePassword(string password)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                LoginPassword = password
            });

            var html = "********";

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdatePassword",
                message = "Your password has been updated",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateLanguagePreference(int languageid)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                LanguageID = languageid
            });


            var language = Exigo.GetLanguage(languageid);
            var html = language.LanguageDescription;

            // Refresh the identity in case the country changed
            new IdentityService().RefreshIdentity();

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateLanguagePreference",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdatePhoneNumbers(string primaryphone, string secondaryphone)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Phone = primaryphone,
                Phone2 = secondaryphone
            });

            var html = string.Format(@"
                " + Resources.Common.Primary + @": <strong>{0}</strong><br />
                " + Resources.Common.Secondary + @": <strong>{1}</strong>
                ", primaryphone, secondaryphone);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdatePhoneNumbers",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateMobilePhone(string mobilephone)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                MobilePhone = mobilephone
            });

            var html = string.Format("{1}: <strong>{0}</strong>", mobilephone, Resources.Common.SendTextsTo);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateMobilePhone",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateFaxNumber(string fax)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Fax = fax
            });

            var html = string.Format("{1}: <strong>{0}</strong>", fax, Resources.Common.SendFaxesTo);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateFaxNumber",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult AddAnnualMembershipAutoOrder()
        {
            try
            {
                var customer = Exigo.GetCustomer(Identity.Current.CustomerID);

                var newDate = new DateTime(DateTime.Now.Year, customer.CreatedDate.Month, customer.CreatedDate.Day) <= DateTime.Now ? new DateTime(DateTime.Now.AddYears(1).Year, customer.CreatedDate.Month, customer.CreatedDate.Day) : new DateTime(DateTime.Now.Year, customer.CreatedDate.Month, customer.CreatedDate.Day);           

                CreateAutoOrderRequest autoOrderRequest = new CreateAutoOrderRequest()
                {
                    Frequency = FrequencyType.Yearly,
                    CustomerID = customer.CustomerID,
                    StartDate = newDate,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Address1 = customer.MainAddress.Address1,
                    City = customer.MainAddress.City,
                    State = customer.MainAddress.State,
                    Country = customer.MainAddress.Country,
                    Zip = customer.MainAddress.Zip,
                    CurrencyCode = CurrencyCodes.DollarsUS,
                    WarehouseID = Warehouses.Default,
                    PriceType = PriceTypes.Wholesale,
                    ShipMethodID = 8
                };
                OrderDetailRequest det = null;

                List<OrderDetailRequest> details = new List<OrderDetailRequest>();
                det = new OrderDetailRequest() 
                {
                    ItemCode = "IAANNUALRENEWAL",
                    Quantity = 1,
                };              
                details.Add(det);
                autoOrderRequest.Details = details.ToArray();

                Exigo.WebService().CreateAutoOrder(autoOrderRequest);

                return new JsonNetResult(new
                {
                    success = true,
                    action = "AddAnnualMembershipAutoOrder"
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
        [HttpParamAction]
        public JsonNetResult DeleteAnnualMembershipAutoOrder(int autoOrderID)
        {
            try
            {
                var customerID = Identity.Current.CustomerID;

                Exigo.DeleteCustomerAutoOrder(customerID, autoOrderID);

                return new JsonNetResult(new
                {
                    success = true,
                    action = "DeleteAnnualMembershipAutoOrder"
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

        // Website settings
        [HttpParamAction]
        public JsonNetResult UpdateWebsiteName(CustomerSite customersite)
        {
            Exigo.UpdateCustomerSite(new CustomerSite
            {
                CustomerID = Identity.Current.CustomerID,
                FirstName = customersite.FirstName,
                LastName = customersite.LastName
            });

            var html = string.Format("{0} {1}", customersite.FirstName, customersite.LastName);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsiteName",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebsiteEmail(CustomerSite customersite)
        {
            Exigo.UpdateCustomerSite(new CustomerSite
            {
                CustomerID = Identity.Current.CustomerID,
                Email = customersite.Email
            });

            var html = string.Format("{0}", customersite.Email);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsiteEmail",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebsitePhoneNumbers(CustomerSite customersite)
        {

            var testa = Exigo.GetCustomerSite(Identity.Current.CustomerID);

            Exigo.UpdateCustomerSite(new CustomerSite
            {
                CustomerID = Identity.Current.CustomerID,
                PrimaryPhone = customersite.PrimaryPhone,
                SecondaryPhone = customersite.SecondaryPhone
            });

            var testb = Exigo.GetCustomerSite(Identity.Current.CustomerID);

            var html = string.Format(@"
                " + Resources.Common.Primary + @": <strong>{0}</strong><br />
                " + Resources.Common.Secondary + @": <strong>{1}</strong>
                ", customersite.PrimaryPhone, customersite.SecondaryPhone);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsitePhoneNumbers",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebsiteFax(CustomerSite customersite)
        {
            Exigo.UpdateCustomerSite(new CustomerSite
            {
                CustomerID = Identity.Current.CustomerID,
                Fax = customersite.Fax
            });

            var html = string.Format("{0}", customersite.Fax);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsiteFax",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebsiteAddress(CustomerSite customersite)
        {
            Exigo.UpdateCustomerSite(new CustomerSite
            {
                CustomerID = Identity.Current.CustomerID,
                Address = customersite.Address
            });


            var html = customersite.Address.AddressDisplay + "<br />" + customersite.Address.City + ", " + customersite.Address.State + " " + customersite.Address.Zip + ", " + customersite.Address.Country;

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsiteAddress",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebsiteMessage(CustomerSite customersite)
        {
            Exigo.UpdateCustomerSite(new CustomerSite
            {
                CustomerID = Identity.Current.CustomerID,
                Notes1 = customersite.Notes1
            });

            var html = "<p>" + customersite.Notes1 + "</p>";

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsiteMessage",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebsiteSocialMediaLinks(string facebookurl, string twitterurl, string youtubeurl, string blogurl)
        {

            facebookurl = FormatUrl(facebookurl);
            twitterurl = FormatUrl(twitterurl);
            youtubeurl = FormatUrl(youtubeurl);
            blogurl = FormatUrl(blogurl);

            var socialrequest = new SetCustomerSocialNetworksRequest();
            socialrequest.CustomerID = Identity.Current.CustomerID;

            var urls = new List<CustomerSocialNetworkRequest>();
            if (!string.IsNullOrEmpty(facebookurl)) urls.Add(new CustomerSocialNetworkRequest() { SocialNetworkID = (int)SocialNetworks.Facebook, Url = facebookurl });
            if (!string.IsNullOrEmpty(twitterurl)) urls.Add(new CustomerSocialNetworkRequest() { SocialNetworkID = (int)SocialNetworks.Twitter, Url = twitterurl });
            if (!string.IsNullOrEmpty(youtubeurl)) urls.Add(new CustomerSocialNetworkRequest() { SocialNetworkID = (int)SocialNetworks.YouTube, Url = youtubeurl });
            if (!string.IsNullOrEmpty(blogurl)) urls.Add(new CustomerSocialNetworkRequest() { SocialNetworkID = (int)SocialNetworks.Blog, Url = blogurl });
            socialrequest.CustomerSocialNetworks = urls.ToArray();

            if (socialrequest.CustomerSocialNetworks.Length > 0)
            {
                Exigo.WebService().SetCustomerSocialNetworks(socialrequest);
            }

            var html = string.Format(@"
                <dl class='dl-metric'>
                    <dt>" + Resources.Common.Facebook + @":</dt>
                    <dd><a href='{0}' target='_blank'><strong>" + facebookurl + @"</strong></a></dd>
                    <dt>" + Resources.Common.Twitter + @":</dt>
                    <dd><a href='{2}' target='_blank'><strong>" + twitterurl + @"</strong></a></dd>
                    <dt>" + Resources.Common.YouTube + @":</dt>
                    <dd><a href='{2}' target='_blank'><strong>" + youtubeurl + @"</strong></a></dd>
                    <dt>" + Resources.Common.Blog + @":</dt>
                    <dd><a href='{2}' target='_blank'><strong>" + blogurl + @"</strong></a></dd>
                </dl>
                ", facebookurl, twitterurl, youtubeurl, blogurl);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsiteSocialMediaLinks",
                html = html
            });
        }

        public string FormatUrl(string url)
        {
            if (url.Contains("http"))
            {
                return url;
            }
            else
            {
                url = "http://" + url;

                return url;
            }
        }
        #endregion

        #region Addresses
        [Route("addresses")]
        public ActionResult AddressList()
        {
            var model = Exigo.GetCustomerAddresses(Identity.Current.CustomerID).Where(c => c.IsComplete).ToList();

            return View(model);
        }

        [Route("addresses/edit/{type:alpha}")]
        public ActionResult ManageAddress(AddressType type)
        {
            var model = Exigo.GetCustomerAddresses(Identity.Current.CustomerID).Where(c => c.AddressType == type).FirstOrDefault();

            return View("ManageAddress", model);
        }

        [Route("addresses/new")]
        public ActionResult AddAddress()
        {
            var model = new Address();
            model.AddressType = AddressType.New;
            model.Country = GlobalSettings.Company.Address.Country;

            return View("ManageAddress", model);
        }

        public ActionResult DeleteAddress(AddressType type)
        {
            Exigo.DeleteCustomerAddress(Identity.Current.CustomerID, type);

            return RedirectToAction("AddressList");
        }

        public ActionResult SetPrimaryAddress(AddressType type)
        {
            Exigo.SetCustomerPrimaryAddress(Identity.Current.CustomerID, type);

            return RedirectToAction("AddressList");
        }

        [HttpPost]
        public ActionResult SaveAddress(Address address, bool? makePrimary)
        {
            // Verify the address            
            if (address.Country == "US") {  //20170118 82825 DV. Only validate address for US addresses
                var verifyAddressResponse = Exigo.VerifyAddress(address);
                if (verifyAddressResponse.IsValid)
                {
                    address = Exigo.SetCustomerAddressOnFile(Identity.Current.CustomerID, address);

                    if (makePrimary != null && ((bool)makePrimary) == true)
                    {
                        Exigo.SetCustomerPrimaryAddress(Identity.Current.CustomerID, address.AddressType);
                    }

                    return RedirectToAction("AddressList");
                }
                else
                {
                    return RedirectToAction("ManageAddress", new { type = address.AddressType, error = "notvalid" });
                }
            }
            else
            {
                address = Exigo.SetCustomerAddressOnFile(Identity.Current.CustomerID, address);

                if (makePrimary != null && ((bool)makePrimary) == true)
                {
                    Exigo.SetCustomerPrimaryAddress(Identity.Current.CustomerID, address.AddressType);
                }

                return RedirectToAction("AddressList");
            }
        }
        #endregion

        #region Payment Methods
        [Route("paymentmethods")]
        public ActionResult PaymentMethodList()
        {
            var model = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Current.CustomerID,
                ExcludeIncompleteMethods = true
            });

            return View(model);
        }

        #region Credit Cards
        [Route("paymentmethods/creditcards/edit/{type:alpha}")]
        public ActionResult ManageCreditCard(CreditCardType type)
        {
            var model = Exigo.GetCustomerPaymentMethods(Identity.Current.CustomerID)
                .Where(c => c is CreditCard && ((CreditCard)c).Type == type)
                .FirstOrDefault();

            // Clear out the card number
            ((CreditCard)model).CardNumber = "";

            return View("ManageCreditCard", model);
        }

        [Route("paymentmethods/creditcards/new")]
        public ActionResult AddCreditCard()
        {
            var model = new CreditCard();
            model.Type = CreditCardType.New;
            model.BillingAddress = new Address()
            {
                Country = GlobalSettings.Company.Address.Country
            };

            return View("ManageCreditCard", model);
        }

        public ActionResult DeleteCreditCard(CreditCardType type)
        {
            Exigo.DeleteCustomerCreditCard(Identity.Current.CustomerID, type);

            return RedirectToAction("PaymentMethodList");
        }

        [HttpPost]
        public ActionResult SaveCreditCard(CreditCard card)
        {
            try
            {
                card = Exigo.SetCustomerCreditCard(Identity.Current.CustomerID, card);

                return RedirectToAction("PaymentMethodList");
            }
            catch (Exception ex)
            {

                return RedirectToAction("PaymentMethodList", new { error = ex.Message.ToString() });
            }
        }
        #endregion

        #region Bank Accounts
        [Route("paymentmethods/bankaccounts/edit/{type:alpha}")]
        public ActionResult ManageBankAccount(ExigoService.BankAccountType type)
        {
            var model = Exigo.GetCustomerPaymentMethods(Identity.Current.CustomerID)
                .Where(c => c is BankAccount && ((BankAccount)c).Type == type)
                .FirstOrDefault();


            // Clear out the account number
            ((BankAccount)model).AccountNumber = "";


            return View("ManageBankAccount", model);
        }

        [Route("paymentmethods/bankaccounts/new")]
        public ActionResult AddBankAccount()
        {
            var model = new BankAccount();
            model.Type = ExigoService.BankAccountType.New;
            model.BillingAddress = new Address()
            {
                Country = GlobalSettings.Company.Address.Country
            };

            return View("ManageBankAccount", model);
        }

        public ActionResult DeleteBankAccount(ExigoService.BankAccountType type)
        {
            Exigo.DeleteCustomerBankAccount(Identity.Current.CustomerID, type);

            return RedirectToAction("PaymentMethodList");
        }

        [HttpPost]
        public ActionResult SaveBankAccount(BankAccount account)
        {
            account = Exigo.SetCustomerBankAccount(Identity.Current.CustomerID, account);

            return RedirectToAction("PaymentMethodList");
        }
        #endregion

        #endregion

        #region Commission Payout
        public ActionResult CommissionPayout()
        {
            var bankaccount = Commissions.GetDirectDeposit();

            return View(bankaccount);
        }

        public ActionResult ManageCommissionPayout()
        {
            var bankaccount = Commissions.GetDirectDeposit();

            bankaccount.AccountNumber = "";

            return View(bankaccount);
        }

        [HttpPost]
        public ActionResult UpdateDirectDeposit(CommissionPayout account)
        {
            if (Commissions.SetDirectDeposit(account))
            {
                return RedirectToAction("commissionpayout", "account", new { success = true });
            }
            else
            {
                return RedirectToAction("managecommissionpayout", "account", new { success = false });
            }
        }
        #endregion

        #region Avatars
        public List<string> AllowableImageFormats = new List<string>()
        {
            "image/jpg",
            "image/jpeg",
            "image/pjpeg",
            "image/gif",
            "image/x-png",
            "image/png"
        };

        [Route("avatar")]
        public ActionResult ManageAvatar()
        {
            // Get all the avatars the customer has used in the past
            var historicalAvatars = Exigo.Images().GetCustomerAvatarHistory(Identity.Current.CustomerID);

            // Get the default avatars from the folder in this website
            try
            {
                var defaultAvatarPath = "~/Content/images/avatars";
                var baseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(defaultAvatarPath));
                var defaultAvatarFiles = Directory.GetFiles(Server.MapPath(defaultAvatarPath));
                if (defaultAvatarFiles.Count() > 0)
                {
                    foreach (var url in defaultAvatarFiles)
                    {
                        var urlParts = url.Split('\\');
                        var filename = urlParts[urlParts.Length - 1];
                        historicalAvatars.Add(baseUrl + "/" + filename);
                    }
                }
            }
            catch { }

            ViewBag.AvatarHistoryItems = historicalAvatars;

            return View();
        }

        public ActionResult SetHistoricalAvatar(string url)
        {
            // Get the bytes from the provided location
            var bytes = GlobalUtilities.GetExternalImageBytes(url);
            Exigo.Images().SetCustomerAvatar(Identity.Current.CustomerID, bytes);

            return RedirectToAction("ManageAvatar");
        }

        [HttpPost]
        public JsonNetResult SetAvatarFromFile(HttpPostedFileBase file)
        {
            // Validate that the file is valid
            var isValidImage = (file != null && file.ContentLength > 0 && AllowableImageFormats.Contains(file.ContentType.ToLower()));

            if (isValidImage)
            {
                // Save the image
                var bytes = GlobalUtilities.GetBytesFromStream(file.InputStream);
                Exigo.Images().SaveUncroppedCustomerAvatar(Identity.Current.CustomerID, bytes);
            }

            return new JsonNetResult(new
            {
                success = isValidImage,
                length = file.ContentLength
            });
        }

        [HttpPost]
        public JsonNetResult SetAvatarFromUrl(string url)
        {
            // Get the bytes from the provided location
            var bytes = GlobalUtilities.GetExternalImageBytes(url);

            // Determine if the image is valid
            var isValidImage = false;
            try
            {
                var stream = new MemoryStream(bytes);
                var bitmap = Image.FromStream(stream);
                isValidImage = (bytes != null && bytes.Length > 0 && bitmap != null);
            }
            catch { }


            if (isValidImage)
            {
                // Save the image
                Exigo.Images().SaveUncroppedCustomerAvatar(Identity.Current.CustomerID, bytes);
            }

            return new JsonNetResult(new
            {
                success = isValidImage,
                length = bytes.Length
            });
        }

        [HttpPost]
        public JsonNetResult SetAvatarFromGravatar(string email)
        {
            var url = GlobalUtilities.GetGravatarUrl(email, 300);
            return SetAvatarFromUrl(url);
        }

        [HttpPost]
        public JsonNetResult SetAvatarFromFacebook(string username)
        {
            var url = "http://graph.facebook.com/{0}/picture?width=300&height=300".FormatWith(username);
            return SetAvatarFromUrl(url);
        }


        [Route("avatar/crop")]
        public ActionResult CropAvatar()
        {
            return View();
        }

        [HttpPost]
        [Route("avatar/crop")]
        public JsonNetResult CropAvatar(int width, int height, int x, int y)
        {
            var bytes = GlobalUtilities.GetExternalImageBytes(GlobalUtilities.GetUncroppedCustomerAvatarUrl(Identity.Current.CustomerID));
            var croppedBytes = GlobalUtilities.Crop(bytes, width, height, x, y);

            Exigo.Images().SetCustomerAvatar(Identity.Current.CustomerID, croppedBytes, true);

            return new JsonNetResult(new
            {
                success = true
            });
        }
        #endregion

        #region Email Notifications
        [Route("notifications")]
        public ActionResult Notifications()
        {
            var model = new AccountNotificationsViewModel();

            var customer = Exigo.GetCustomer(Identity.Current.CustomerID);
            model.Email = customer.Email;
            model.IsOptedIn = customer.IsOptedIn;

            return View(model);
        }

        [Route("notifications/unsubscribe")]
        public ActionResult Unsubscribe()
        {
            Exigo.OptOutCustomer(Identity.Current.CustomerID);

            return RedirectToAction("Notifications");
        }

        [HttpPost]
        public JsonNetResult SendEmailVerification(string email)
        {
            try
            {
                Exigo.SendEmailVerification(Identity.Current.CustomerID, email);

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
                    error = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [Route("~/verifyemail")]
        public ActionResult VerifyEmail(string token)
        {
            try
            {
                Exigo.OptInCustomer(token);

                return View();
            }
            catch
            {
                throw new HttpException(404, "Invalid token");
            }
        }
        #endregion
    }
}