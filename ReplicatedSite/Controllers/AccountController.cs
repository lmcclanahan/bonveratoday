using Common;
using Common.Api.ExigoWebService;
using Common.HtmlHelpers;
using Common.Services;
using ExigoService;
using ReplicatedSite;
using ReplicatedSite.Controllers;
using ReplicatedSite.Models;
using ReplicatedSite.Services;
using ReplicatedSite.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Common.Filters;
using System.Threading.Tasks;
using ReplicatedSite.Filters;

namespace ReplicatedSite.Controllers
{
    [Authorize]
    [RoutePrefix("{webalias}")]
    public class AccountController : Controller
    {
        #region Properties
        public string ShoppingCartName = GlobalSettings.Company.Name + "ReplicatedSiteShopping";

        public ShoppingCartItemsPropertyBag ShoppingCart
        {
            get
            {
                if (_shoppingCart == null)
                {
                    _shoppingCart = Exigo.PropertyBags.Get<ShoppingCartItemsPropertyBag>(ShoppingCartName + "Cart");
                }
                return _shoppingCart;
            }
        }
        private ShoppingCartItemsPropertyBag _shoppingCart;

        public string SmartShopperSubscriptionItemCode = "SSR";
        public string FirstOrderPackItemCode = GlobalUtilities.GetCurrentMarket().SmartShopperFirstOrderPackItemCode;
        #endregion

        #region Overview
        [Route("settings")]
        public ActionResult Index(string ReturnUrl)
        {
            ReturnUrl = string.Empty;
            var model = new AccountSummaryViewModel();

            var customer = Exigo.GetCustomer(Identity.Customer.CustomerID);

            model.CustomerID = customer.CustomerID;
            model.FirstName = customer.FirstName;
            model.MiddleName = customer.MiddleName;
            model.LastName = customer.LastName;
            model.Email = customer.Email;
            model.LoginName = customer.LoginName;
            model.LanguageID = customer.LanguageID;

            model.PrimaryPhone = customer.PrimaryPhone;
            model.SecondaryPhone = customer.SecondaryPhone;
            model.MobilePhone = customer.MobilePhone;
            model.Fax = customer.Fax;
            model.Addresses = customer.Addresses;

            model.IsOptedIn = customer.IsOptedIn;

            model.Enroller = customer.Enroller;



            // Get the available languages
            model.Languages = Exigo.GetLanguages();

            var idservice = new IdentityService();
            idservice.RefreshIdentity();

            return View(model);
        }

        [HttpParamAction]
        public JsonNetResult UpdateEmailAddress(string email)
        {
            //ensure there are no customers with the username or email chosen
            var emailIsAvailable = Exigo.IsEmailAvailable(Identity.Customer.CustomerID, email);
            var loginAvailable = Exigo.IsLoginNameAvailable(email, Identity.Customer.CustomerID);
            var canUse = (emailIsAvailable && loginAvailable);

            if (canUse)
            {
                Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
                {
                    CustomerID = Identity.Customer.CustomerID,
                    LoginName = email,
                    Email = email
                });

                Exigo.SendEmailVerification(Identity.Customer.CustomerID, email);

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
                    message = "This email/username is unavailable."
                });
            }
        }

        [HttpParamAction]
        public JsonNetResult UpdateNotifications(bool isOptedIn, string email)
        {
            var model = new AccountSummaryViewModel();
            var html = string.Empty;

            try
            {
                var token = Security.Encrypt(new
                {
                    CustomerID = Identity.Customer.CustomerID,
                    Email = email
                });

                if (isOptedIn)
                {
                    Exigo.OptInCustomer(token);
                    Exigo.SendEmailVerification(Identity.Customer.CustomerID, email);
                    html = string.Format("{0}", Resources.Common.OptedInStatus);
                }
                else
                {
                    Exigo.OptOutCustomer(Identity.Customer.CustomerID);
                    html = string.Format("{0}", Resources.Common.OptedOutStatus);
                }
            }
            catch
            {
                throw new HttpException(404, "Invalid token");
            }

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateNotifications",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateName(string firstname, string middlename, string lastname)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                FirstName = firstname,
                MiddleName = middlename,
                LastName = lastname
            });

            var html = string.Format("{0} {1} {2}, {4}# {3}", firstname, middlename, lastname, Identity.Customer.CustomerID, Resources.Common.ID);

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
                CustomerID = Identity.Customer.CustomerID,
                WebAlias = webalias
            });

            var html = string.Format("<a href='http://exigoreplicated.azurewebsites.net/{0}'>exigoreplicated.azurewebsites.net/<strong>{0}</strong></a>", webalias);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebAlias",
                html = html
            });
        }
        public JsonResult IsValidWebAlias(string webalias)
        {
            var isValid = Exigo.IsWebAliasAvailable(Identity.Customer.CustomerID, webalias);

            if (isValid) return Json(true, JsonRequestBehavior.AllowGet);
            else return Json(string.Format(Resources.Common.PasswordNotAvailable, webalias), JsonRequestBehavior.AllowGet);
        }

        [HttpParamAction]
        public JsonNetResult UpdatePassword(string password)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
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
                CustomerID = Identity.Customer.CustomerID,
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
                CustomerID = Identity.Customer.CustomerID,
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
                CustomerID = Identity.Customer.CustomerID,
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
                CustomerID = Identity.Customer.CustomerID,
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
        #endregion

        #region Creating an account
        [AllowAnonymous]
        [Route("register")]
        public ActionResult Register(string ReturnUrl, string ErrorMessage)
        {
            var model = new AccountRegistrationViewModel();

            try
            {
                model.EnrollerID = Identity.Owner.CustomerID;

                if (Identity.Owner.WebAlias == GlobalSettings.ReplicatedSites.DefaultWebAlias)
                {
                    model.IsOrphan = true;
                }

                // If customer has been returned to page because of error, display the explanation
                if (ErrorMessage != null)
                {
                    model.ReturnedError = true;
                    ViewBag.Error = ErrorMessage;
                }

                model.ShoppingCart = ShoppingCart;

                return View(model);
            }
            catch (Exception exception)
            {
                ViewBag.Error = "We are currently expereriencing intermittent technical isues with our Customer Registration Form. We apologize for any inconvience. Please contact our customer service at" + GlobalSettings.Company.Phone + " or by email at " + GlobalSettings.Emails.SupportEmail + " for assisance.";

                var mailConfig = Common.GlobalSettings.Emails.SMTPConfigurations.Default;

                MailMessage mail = new MailMessage();
                mail.CC.Add(GlobalSettings.ErrorLogging.EmailRecipients.ToString());
                mail.From = new MailAddress(GlobalSettings.Emails.NoReplyEmail);
                mail.Subject = "Error" + exception.Source.ToString(); // Customize "Error" string if neccessary

                mail.IsBodyHtml = true;
                mail.Body = exception.ToString();
                SmtpClient smtp = new SmtpClient();
                smtp.Host = mailConfig.Server;
                smtp.Port = mailConfig.Port;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                (mailConfig.Username, mailConfig.Password);
                smtp.EnableSsl = mailConfig.EnableSSL;
                smtp.Send(mail);

                return View(model);
            }

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public ActionResult Register(string ReturnUrl, string ErrorMessage, AccountRegistrationViewModel model)
        {
            try
            {
                var orderConfiguration = Utilities.GetCurrentMarket().Configuration.Orders;
                var checkEmail = Exigo.WebService().GetCustomers(new GetCustomersRequest()
                {
                    Email = model.Username,
                    LoginName = model.Username
                }).Customers.ToList();

                foreach (var check in checkEmail)
                {
                    if (check.Email == model.Username)
                    {
                        ErrorMessage = "That email address is not available. Please try another.";
                        return new JsonNetResult(new
                        {
                            success = false,
                            message = ErrorMessage
                        });
                    }
                    else
                    {
                        model.ReturnedError = false;
                    }
                }

                // Save the customer
                var request                = new CreateCustomerRequest();
                request.FirstName          = model.FirstName;
                request.MiddleName         = model.MiddleName;
                request.LastName           = model.LastName;
                request.Email              = model.Username;
                request.CanLogin           = true;
                request.LoginName          = model.Username;
                request.LoginPassword      = model.Password;
                request.CustomerType       = CustomerTypes.RetailCustomer;
                request.EnrollerID         = model.EnrollerID;
                request.InsertEnrollerTree = true;
                request.CustomerStatus     = CustomerStatuses.Active;

                request.InsertUnilevelTree = true;
                request.SponsorID = request.EnrollerID;
                
                request.EntryDate = DateTime.Now;
                request.DefaultWarehouseID = orderConfiguration.WarehouseID;
                request.CurrencyCode = orderConfiguration.CurrencyCode;
                request.LanguageID = orderConfiguration.LanguageID;

                var response = Exigo.WebService().CreateCustomer(request);

                var token = Security.Encrypt(new
                {
                    CustomerID = response.CustomerID,
                    Email = model.Username
                });

                if (model.IsOptedIn)
                {

                    // This will be formatted in the send email verification method. 
                    var body = @"
                    <p>
                        {1} has received a request to enable this email account to receive email notifications from {1}.
                    </p>

                    <p> 
                        To confirm this email account, please click the following link:<br />
                        <a href='{0}'>{0}</a>
                    </p>

                    <p>
                        If you did not request email notifications from {1}, or believe you have received this email in error, please contact {1} customer service.
                    </p>

                    <p>
                        Sincerely, <br />
                        {1} Customer Service
                    </p>";
                    Exigo.SendEmailVerification(response.CustomerID, request.Email, body);
                }


                // Sign the customer into their backoffice
                var service = new IdentityService();
                service.SignIn(model.Username, model.Password);

                model.ReturnedError = false;

                if (ReturnUrl.IsNotEmpty())
                {
                    return new JsonNetResult(new
                    {
                        success = true,
                        action = ReturnUrl
                    });
                }
                else
                {
                    return new JsonNetResult(new
                    {
                        success = true,
                        action = "complete",
                        token = token
                    });
                }
            }
            catch (Exception e)
            {
                // Return customer to the form and display an error message for the. Technical error (exception) will be emailed to support contact
                model.ReturnedError = true;
                ViewBag.Error = e.Message;
                ErrorMessage = e.Message;

                return new JsonNetResult(new
                {
                    success = false,
                    message = ErrorMessage
                });
            }

        }

        /// <summary>
        /// This is the Register view, but with Smart Shopper specific logic displayed. The Smart Shopper will be able to pay 
        /// a subscription fee else they will be required to check out with an Auto Order
        /// </summary>
        /// <param name="ReturnUrl"></param>
        /// <param name="ErrorMessage"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("smartshopper/register")]
        public ActionResult SmartShopperRegister(string ReturnUrl, string ErrorMessage)
        {
            var model = new AccountRegistrationViewModel();

            model.EnrollerID = Identity.Owner.CustomerID;

            if (Identity.Owner.WebAlias == GlobalSettings.ReplicatedSites.DefaultWebAlias)
            {
                model.IsOrphan = true;
            }

            // If customer has been returned to page because of error, display the explanation
            if (ErrorMessage != null)
            {
                model.ReturnedError = true;
                ViewBag.Error = ErrorMessage;
            }

            model.ShoppingCart = ShoppingCart;

            model.HasAutoOrderItems = ShoppingCart.HasAutoOrderItems();
            var orderConfiguration = Utilities.GetCurrentMarket().Configuration.Orders;

            // Smart Shopper Pack population
            var smartShopperItems = Exigo.GetItems(new ExigoService.GetItemsRequest { Configuration = orderConfiguration, ItemCodes = new string[] { SmartShopperSubscriptionItemCode, FirstOrderPackItemCode }, PriceTypeID = PriceTypes.Wholesale });

            model.SmartShopperSubscriptionItem = smartShopperItems.Where(i => i.ItemCode == SmartShopperSubscriptionItemCode).FirstOrDefault();
            model.FirstOrderPack = smartShopperItems.Where(i => i.ItemCode == FirstOrderPackItemCode).FirstOrDefault();
            
            // Will Call Ship Method 
            model.WillCallShipMethodID = GlobalUtilities.GetCurrentMarket().WillCallShipMethodID;

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("smartshopper/register")]
        public ActionResult SmartShopperRegister(string ReturnUrl, string ErrorMessage, AccountRegistrationViewModel model)
        {
            var customerID = 0;
            var newOrderID = 0;
            try
            {
                var isCheckoutWithReplenish = model.SmartShopperOption == SmartShopperOption.CreateReplenishment;
                var hasAutoOrderItems = ShoppingCart.HasAutoOrderItems();
                var token = "";

                var orderConfiguration = Utilities.GetCurrentMarket().Configuration.Orders;
                var checkEmail = Exigo.WebService().GetCustomers(new GetCustomersRequest()
                {
                    Email = model.Username,
                    LoginName = model.Username
                }).Customers.ToList();

                foreach (var check in checkEmail)
                {
                    if (check.Email == model.Username)
                    {
                        ErrorMessage = "That email address is not available. Please try another.";
                        return new JsonNetResult(new
                        {
                            success = false,
                            message = ErrorMessage
                        });
                    }
                    else
                    {
                        model.ReturnedError = false;
                    }
                }

                // Save the customer
                var request = new CreateCustomerRequest();
                request.FirstName = model.FirstName;
                request.MiddleName = model.MiddleName;
                request.LastName = model.LastName;
                request.Email = model.Username;
                request.CanLogin = true;
                request.LoginName = model.Username;
                request.LoginPassword = model.Password;
                request.CustomerType = (isCheckoutWithReplenish) ? CustomerTypes.RetailCustomer : CustomerTypes.SmartShopper;
                request.EnrollerID = model.EnrollerID;
                request.CustomerStatus = CustomerStatuses.Active;
                request.InsertEnrollerTree = true;

                var address = model.ShippingAddress;
                request.MainAddress1 = address.Address1;
                //request.MainAddress2 = address.Address2;
                request.MainCity = address.City;
                request.MainState = address.State;
                request.MainZip = address.Zip;
                request.MainCountry = address.Country;


                // IF ADDITIONAL LOGIC IS REQUIRED FOR SPONSOR VS. ENROLLERS, IT SHOULD RESIDE HERE
                request.InsertUnilevelTree = true;
                request.SponsorID = request.EnrollerID;

                request.EntryDate = DateTime.Now;
                request.DefaultWarehouseID = orderConfiguration.WarehouseID;
                request.CurrencyCode = orderConfiguration.CurrencyCode;
                request.LanguageID = orderConfiguration.LanguageID;



                // If we have a Credit Card in our model, we know that we are dealing with a Customer that wishes to purchase their Smart Shopper subscription now. 
                // This needs to take place in a transactional request
                if (model.SmartShopperOption == SmartShopperOption.PurchaseSubscription)
                {
                    if (model.CreditCard.IsValid || model.CreditCard.IsTestCreditCard)
                    {
                        var requests = new List<ApiRequest>();

                        // Add our Create Customer request
                        requests.Add(request);

                        // Add our Create Order Request. We first have to assemble our "Shipping Address". 
                        // Since this is a virtual item that is not actually shipped, I set this using their Credit Card billing info and their name/email entered in the Register form.
                        var shipAddress = model.ShippingAddress;

                        // Create our item collection and make sure that our smart shopper subscription is always included
                        var items = new List<ShoppingCartItem> { new ShoppingCartItem { ItemCode = model.SmartShopperItemCode, Quantity = 1 } };
                        if (model.SmartShopperItemCode != SmartShopperSubscriptionItemCode)
                        {
                            items.Add(new ShoppingCartItem { ItemCode = SmartShopperSubscriptionItemCode, Quantity = 1 });
                        }

                        var shipMethodID = (model.ShipMethodID > 0) ? model.ShipMethodID : orderConfiguration.DefaultShipMethodID;
                        var orderRequest = new CreateOrderRequest(orderConfiguration, shipMethodID, items, shipAddress);
                        orderRequest.PriceType = (int)PriceTypes.Wholesale;

                        // We need to override the status of the order if we are only shipping a virtual item, the SmartShopperSubscriptionItemCode
                        if (model.SmartShopperItemCode == SmartShopperSubscriptionItemCode)
                        {
                            orderRequest.OrderStatus = OrderStatusType.Shipped;
                        }

                        requests.Add(orderRequest);

                        // Lastly, we need to add our payment, if we are not using a test card.
                        if (!model.CreditCard.IsTestCreditCard)
                        {
                            requests.Add(new ChargeCreditCardTokenRequest(model.CreditCard));
                        }
                        else
                        {
                            // no need to charge card
                            ((CreateOrderRequest)requests.Where(c => c is CreateOrderRequest).FirstOrDefault()).OrderStatus = OrderStatusType.Shipped;
                        }


                        //// Now we process our transaction
                        var transactionResponse = Exigo.WebService().ProcessTransaction(new TransactionalRequest { TransactionRequests = requests.ToArray() });
                        if (transactionResponse.Result.Status == ResultStatus.Success)
                        {
                            foreach (var response in transactionResponse.TransactionResponses)
                            {
                                if (response is CreateCustomerResponse) customerID = ((CreateCustomerResponse)response).CustomerID;
                                if (response is CreateOrderResponse) newOrderID = ((CreateOrderResponse)response).OrderID;
                            }

                        token = Security.Encrypt(new
                        {
                            CustomerID = customerID,
                            ShipMethodID = orderRequest.ShipMethodID
                        });
                        }
                    }
                    else
                    {
                        throw new Exception("Please enter a valid credit card");
                    }
                }
                else
                {
                    var response = Exigo.WebService().CreateCustomer(request);
                    customerID = response.CustomerID;
                }

                if (model.IsOptedIn)
                {
                    // This will be formatted in the send email verification method. 
                    var body = @"
                    <p>
                        {1} has received a request to enable this email account to receive email notifications from {1}.
                    </p>

                    <p> 
                        To confirm this email account, please click the following link:<br />
                        <a href='{0}'>{0}</a>
                    </p>

                    <p>
                        If you did not request email notifications from {1}, or believe you have received this email in error, please contact {1} customer service.
                    </p>

                    <p>
                        Sincerely, <br />
                        {1} Customer Service
                    </p>";
                    Exigo.SendEmailVerification(customerID, request.Email,body);
                }


                //// Sign the customer into their backoffice
                var service = new IdentityService();
                service.SignIn(model.Username, model.Password);

                model.ReturnedError = false;

                if (ReturnUrl.IsNotEmpty())
                {
                    return new JsonNetResult(new
                    {
                        success = true,
                        action = ReturnUrl
                    });
                }
                else
                {
                    if (isCheckoutWithReplenish)
                    {
                        if (hasAutoOrderItems)
                        {

                            return new JsonNetResult(new
                            {
                               success = true,
                               action = "checkout"
                            });
                        }
                        else
                        {
                            return new JsonNetResult(new
                            {
                                success = true,
                                action = "itemlist"
                            });
                        }
                    }
                    else
                    {

                        return new JsonNetResult(new
                        {
                            success = true,
                            action = "complete",
                            token = token
                        });
                    }
                }
            }
            catch (Exception e)
            {
                // Return customer to the form and display an error message for the. Technical error (exception) will be emailed to support contact
                model.ReturnedError = true;
                ViewBag.Error = e.Message;
                ErrorMessage = e.Message;
                return new JsonNetResult(new
                {
                    success = false,
                    message = ErrorMessage
                });
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("getsmartshopperconfirmmodal")]
        public JsonNetResult GetSmartShopperConfirmModal(AccountRegistrationViewModel model)
        {
            try
            {
                var orderConfiguration = Utilities.GetCurrentMarket().Configuration.Orders;
                var items = new List<ShoppingCartItem> { new ShoppingCartItem { ItemCode = model.SmartShopperItemCode, Quantity = 1 } };

                if (model.SmartShopperItemCode != SmartShopperSubscriptionItemCode)
                {
                    items.Add(new ShoppingCartItem { ItemCode = SmartShopperSubscriptionItemCode, Quantity = 1 });
                }

                var shipMethodID = (model.ShipMethodID > 0) ? model.ShipMethodID : orderConfiguration.DefaultShipMethodID;

                // We need to ensure that we are using a valid address when calculating our order, else we show an error message 
                var validationResult = Exigo.VerifyAddress(model.ShippingAddress);

                if (validationResult.IsValid)
                {
                    orderConfiguration.PriceTypeID = (int)PriceTypes.Wholesale;
                    model.CalculatedOrder = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                        Address = model.ShippingAddress,
                        Configuration = orderConfiguration,
                        Items = items,
                        ReturnShipMethods = true,
                        ShipMethodID = shipMethodID
                    });

                    if (model.CalculatedOrder.ShipMethods.Count() == 0)
                    {
                        throw new Exception("We are having trouble calculating shipping for this order. Please double-check your shipping address or contact support@bonvera.com for assistance");
                    }

                    var hasDiscount = model.CalculatedOrder.Details.Any(d => d.PriceTotal < 0);

                    if (hasDiscount)
                    {
                        var discountItems = model.CalculatedOrder.Details.Where(d => d.PriceTotal < 0);
                        model.Discount = discountItems.Sum(d => d.PriceTotal * -1);
                    }

                    // Will Call Ship Method 
                    model.WillCallShipMethodID = GlobalUtilities.GetCurrentMarket().WillCallShipMethodID;

                    var html = this.RenderPartialViewToString("partials/_registerconfirmmodal", model);

                    return new JsonNetResult(new
                    {
                        success = true,
                        html
                    });
                }
                else
                {
                    throw new Exception("We are having trouble calculating shipping for this order. Please double-check your shipping address or contact support@bonvera.com for assistance");   
                }
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

        [Route("registrationcomplete")]
        public ActionResult RegistrationComplete(string token)
        {
            try{
            var decryptedToken = Security.Decrypt(token);
            var shipMethodID = Convert.ToInt32(decryptedToken.ShipMethodID);
                
            var WillCallShipMethodID = GlobalUtilities.GetCurrentMarket().WillCallShipMethodID;
            ViewBag.IsWillCall = (shipMethodID == WillCallShipMethodID);
            }
            catch (Exception e)
            {
                ViewBag.IsWillCall = false;
            }
            return View();
        }
        #endregion

        #region Addresses
        [Route("addresses")]
        public ActionResult AddressList()
        {
            var model = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID).Where(c => c.IsComplete).ToList();

            return View(model);
        }

        [Route("addresses/edit/{type:alpha}")]
        public ActionResult ManageAddress(AddressType type)
        {
            var model = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID).Where(c => c.AddressType == type).FirstOrDefault();

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

        [Route("deleteaddress")]
        public ActionResult DeleteAddress(AddressType type)
        {
            Exigo.DeleteCustomerAddress(Identity.Customer.CustomerID, type);

            return RedirectToAction("AddressList");
        }

        [Route("setprimaryaddress")]
        public ActionResult SetPrimaryAddress(AddressType type)
        {
            Exigo.SetCustomerPrimaryAddress(Identity.Customer.CustomerID, type);

            return RedirectToAction("AddressList");
        }

        [HttpPost]
        [Route("saveaddress")]
        public ActionResult SaveAddress(Address address, bool? makePrimary)
        {
            // Verify the address            
            var verifyAddressResponse = Exigo.VerifyAddress(address);
            if (verifyAddressResponse.IsValid)
            {
                address = (verifyAddressResponse.IsValid) ? verifyAddressResponse.VerifiedAddress as Address : address;
                address = Exigo.SetCustomerAddressOnFile(Identity.Customer.CustomerID, address);

                if (makePrimary != null && ((bool)makePrimary) == true)
                {
                    Exigo.SetCustomerPrimaryAddress(Identity.Customer.CustomerID, address.AddressType);
                }

                return RedirectToAction("AddressList");
            }
            else
            {
                return RedirectToAction("ManageAddress", new { type = address.AddressType, error = "notvalid" });
            }
        }
        #endregion

        #region Payment Methods
        [Route("paymentmethods")]
        public ActionResult PaymentMethodList()
        {
            var model = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                ExcludeIncompleteMethods = true
            });

            return View(model);
        }

        #region Credit Cards
        [Route("paymentmethods/creditcards/edit/{type:alpha}")]
        public ActionResult ManageCreditCard(CreditCardType type)
        {
            var model = Exigo.GetCustomerPaymentMethods(Identity.Customer.CustomerID)
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

        [Route("deletecreditcard")]
        public ActionResult DeleteCreditCard(CreditCardType type)
        {
            Exigo.DeleteCustomerCreditCard(Identity.Customer.CustomerID, type);

            return RedirectToAction("PaymentMethodList");
        }

        [HttpPost]
        [Route("savecreditcard")]
        public ActionResult SaveCreditCard(CreditCard card)
        {
            card = Exigo.SetCustomerCreditCard(Identity.Customer.CustomerID, card);

            return RedirectToAction("PaymentMethodList");
        }
        #endregion

        #region Bank Accounts
        [Route("paymentmethods/bankaccounts/edit/{type:alpha}")]
        public ActionResult ManageBankAccount(ExigoService.BankAccountType type)
        {
            var model = Exigo.GetCustomerPaymentMethods(Identity.Customer.CustomerID)
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
            Exigo.DeleteCustomerBankAccount(Identity.Customer.CustomerID, type);

            return RedirectToAction("PaymentMethodList");
        }

        [HttpPost]
        public ActionResult SaveBankAccount(BankAccount account)
        {
            account = Exigo.SetCustomerBankAccount(Identity.Customer.CustomerID, account);

            return RedirectToAction("PaymentMethodList");
        }
        #endregion

        #endregion

        #region Order History
        [Route("orders/{page:int:min(1)=1}")]
        public ActionResult OrderList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                IncludeOrderDetails = true,
                RowCount = 10
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }
        [Route("orders/partnerstore/{page:int:min(1)=1}")]
        public ActionResult PartnerStoreOrdersList(int page)
        {

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                IncludeOrderDetails = true
            }).Where(c => !c.Other11.IsNullOrEmpty()).ToList();
            ViewBag.isPartnerStoreOrders = true;
            return View("OrderList", model);
        }
        [Route("orders/cancelled/{page:int:min(1)=1}")]
        public ActionResult CancelledOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                IncludeOrderDetails = true,
                OrderStatuses = new int[] { 4 }
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("orders/open/{page:int:min(1)=1}")]
        public ActionResult OpenOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                IncludeOrderDetails = true,
                OrderStatuses = new int[] { 0, 1, 2, 3, 5, 6, 10 }
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("orders/shipped/{page:int:min(1)=1}")]
        public ActionResult ShippedOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                IncludeOrderDetails = true,
                OrderStatuses = new int[] { 9 }
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("orders/declined/{page:int:min(1)=1}")]
        public ActionResult DeclinedOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                IncludeOrderDetails = true,
                OrderStatuses = new int[] { 0, 2, 3 }
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("orders/search/{id:int}")]
        public ActionResult SearchOrdersList(int id)
        {
            ViewBag.IsSearch = true;

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                OrderID = id,
                IncludeOrderDetails = true,
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("order/cancel")]
        public ActionResult CancelOrder(string token)
        {
            var orderID = Convert.ToInt32(Security.Decrypt(token, Identity.Customer.CustomerID));

            Exigo.CancelOrder(orderID);

            return Redirect(Request.UrlReferrer.ToString());
        }
        #endregion

        #region Invoices
        [Route("invoice")]
        public ActionResult OrderInvoice(string token)
        {
            var orderID = Convert.ToInt32(Security.Decrypt(token, Identity.Customer.CustomerID));

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                OrderID = orderID,
                IncludeOrderDetails = true,
            }).FirstOrDefault();

            return View("OrderInvoice", model);
        }
        #endregion

        #region Signing in
        [AllowAnonymous]
        [Route("login")]
        public ActionResult Login()
        {
            var model = new LoginViewModel();

            // Populate our model with a flag to determine if we need to show the user a message since they came from the Smart Shopper reigster flow - Mike M.
            if (Identity.Customer == null && ShoppingCart.IsSmartShopperCheckout && ShoppingCart.HasAutoOrderItems())
            {
                model.IsSmartShopperRegistration = true;
            }

            return View(model);
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public JsonNetResult Login(LoginViewModel model)
        {
            var service = new IdentityService();
            var response = service.SignIn(model.LoginName, model.Password);

            return new JsonNetResult(response);
        }

        [AllowAnonymous]
        public ActionResult SilentLogin(string token)
        {
            var service = new IdentityService();
            var response = service.SignIn(token);

            if (response.Status)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        #endregion

        #region Signing Out
        [Route("logout")]
        public ActionResult LogOut()
        {
            var cartHasFirstOrderPack = ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0;

            if (cartHasFirstOrderPack)
            {
                ShoppingCart.Items.Remove(FirstOrderPackItemCode);
                Exigo.PropertyBags.Update(ShoppingCart);
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        #endregion

        #region ForgotPassword
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public string DistibutorForgotPassword(DistributorForgotPasswordViewModel model)
        {
            //Search if email exists
            var request = new GetCustomersRequest();
            request.Email = model.Email;
            var response = Exigo.WebService().GetCustomers(request);

            if (response.Customers.Count() == 0)
                return "<strong>There is no customer associated with the email you provided. Please contact customer Service.</strong>";

            //Generate Link to reset password
            var customer = (Customer)response.Customers.FirstOrDefault();

            //var token = Security.Encrypt(customer.CustomerID);
            var token = Security.Encrypt(new { CustomerID = customer.CustomerID, Date = DateTime.Now });

            var url = Url.Action("ResetPassword", "account", new { token = token }, HttpContext.Request.Url.Scheme);

            SendEmail(url, customer.CustomerID, customer.Email, token);

            return "<strong>We've sent you an email with instructions on how to reset your password</strong>";
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string token)
        {
            //Created Model
            var model = new ResetPasswordViewModel();

            //Find Customer By hash
            var query = Exigo.OData().Customers.Where(c => c.Field2 == token);

            if (query != null && query.Count() > 0)
            {
                var decryptedToken = Security.Decrypt(token);
                var date = Convert.ToDateTime(decryptedToken.Date);
                var customer = query.FirstOrDefault();
                var dateExpiration = date.AddMinutes(30);

                if (DateTime.Now > dateExpiration)
                {
                    model.IsExpiredLink = true;
                }

                model.CustomerID = customer.CustomerID;
                model.CustomerType = customer.CustomerTypeID;

                return View(model);
            }
            else
            {
                if (GlobalSettings.Globalization.HideForLive)
                { 
                    // Needs to redirect to back office login page if this fails and we are in 'Soft Launch' mode - Mike M.
                    return Redirect(GlobalSettings.Company.BaseBackofficeUrl + "?error=invalidtoken");
                }
                else
                {
                    return RedirectToAction("Login", new { error = "invalidtoken" });
                }
                
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest()
                {
                    CustomerID = model.CustomerID,
                    LoginPassword = model.Password,
                    Field2 = string.Empty
                });


                var urlHelper = new UrlHelper(Request.RequestContext);
                var url = ((GlobalSettings.Globalization.HideForLive) ? GlobalSettings.Company.BaseBackofficeUrl + "/login" : urlHelper.Action("login")) + "?p=r";

                return new JsonNetResult(new
                {
                    success = true,
                    url
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

        public void SendEmail(string url, int customerID, string emailAddress, string token)
        {
            try
            {
                //Send Email with Reset instructions
                var email = new MailMessage();

                email.From = new MailAddress(GlobalSettings.Emails.NoReplyEmail);
                email.To.Add(emailAddress);
                email.Subject = "Password Reset";
                email.Body = "<p>You forgot your password?  Don't worry, just click the link below to return to your account and reset it.</p>" + url;
                email.IsBodyHtml = true;

                var SmtpServer = new SmtpClient();
                SmtpServer.Host = GlobalSettings.Emails.SMTPConfigurations.Default.Server;
                SmtpServer.Port = GlobalSettings.Emails.SMTPConfigurations.Default.Port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(GlobalSettings.Emails.SMTPConfigurations.Default.Username, GlobalSettings.Emails.SMTPConfigurations.Default.Password);
                SmtpServer.EnableSsl = GlobalSettings.Emails.SMTPConfigurations.Default.EnableSSL;

                Task.Factory.StartNew(() =>
                {
                    SmtpServer.Send(email);
                });

                var request = new UpdateCustomerRequest();
                request.CustomerID = customerID;
                request.Field2 = token;
                Exigo.WebService().UpdateCustomer(request);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region AJAX
        [AllowAnonymous]
        [HttpPost]
        public JsonNetResult GetDistributors(string query)
        {
            try
            {
                // assemble a list of customers who match the search criteria
                var enrollerCollection = new List<SearchResult>();

                var basequery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.Associate);
                var isCustomerID = query.CanBeParsedAs<int>();

                if (isCustomerID)
                {
                    var customerQuery = basequery.Where(c => c.CustomerID == Convert.ToInt32(query));

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
                }
                else
                {
                    var customerQuery = basequery.Where(c => c.FirstName.Contains(query) || c.LastName.Contains(query));

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
                    else return "http://google.com/" + this.WebAlias;
                }
            }

            public string MainState { get; set; }
            public string MainCity { get; set; }
            public string MainCountry { get; set; }
        }

        #endregion
    }
}