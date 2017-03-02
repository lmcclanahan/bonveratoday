using Backoffice.Models;
using Common;
using Common.Providers;
using Common.Services;
using ExigoService;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Backoffice.Services
{
    public class IdentityService
    {
        IIdentityAuthenticationProvider authProvider = new WebServiceIdentityAuthenticationProvider();

        public IdentityService() { }
        public IdentityService(IIdentityAuthenticationProvider provider)
        {
            authProvider = provider;
        }

        public LoginResponse SignIn(string loginname, string password)
        {
            var response = new LoginResponse();

            try
            {
                // Authenticate the customer
                var customerID = authProvider.AuthenticateCustomer(loginname, password);
                if (customerID == 0)
                {
                    response.Fail("Unable to authenticate");
                    return response;
                }

                return AuthorizeCustomer(customerID);
            }
            catch (Exception ex)
            {
                response.Fail(ex.Message);
            }

            return response;
        }
        public LoginResponse SignIn(int customerid)
        {
            var response = new LoginResponse();

            try
            {
                // Authenticate the customer
                var customerID = authProvider.AuthenticateCustomer(customerid);
                if (customerID == 0)
                {
                    response.Fail("Unable to authenticate");
                    return response;
                }

                return AuthorizeCustomer(customerID);
            }
            catch (Exception ex)
            {
                response.Fail(ex.Message);
            }

            return response;
        }
        public LoginResponse SignIn(string silentLoginToken)
        {
            var response = new LoginResponse();

            try
            {
                // Decrypt the token
                var token = Security.Decrypt(silentLoginToken);

                // Split the value and get the values

                // Return the expiration status of the token and the sign in response
                if (token.ExpirationDate < DateTime.Now)
                {
                    response.Fail("Token expired");
                    return response;
                }

                // Sign the customer in with their customer ID
                response = SignIn((int)token.CustomerID);
            }
            catch (Exception ex)
            {
                response.Fail(ex.Message);
            }

            return response;
        }

        protected LoginResponse AuthorizeCustomer(int customerID)
        {
            var response = new LoginResponse();


            // Get the customer's identity
            var identity = GetIdentity(customerID);


            // Get the redirect URL (for silent logins) or create the forms ticket
            response.RedirectUrl = GetSilentLoginRedirect(identity);


            // Continue to authorize this customer if they signed in to the right site
            if (response.RedirectUrl.IsEmpty())
            {
                CreateFormsAuthenticationTicket(identity);

                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                    response.LandingUrl = urlHelper.AbsoluteAction("Index", "Dashboard");

                    // subscription logic
                    if (!identity.IsSubscribedMonthly)
                    {
                        response.LandingUrl = urlHelper.AbsoluteAction("ItemList", "Shopping");
                    }
                }
                else
                {
                    response.LandingUrl = "/";
                }
            }


            // Mark the response as successful
            response.Success();


            return response;
        }

        public LoginResponse AdminSilentLogin(string token)
        {
            var response = new LoginResponse();

            try
            {
                // Decrypt the token
                var IV = GlobalSettings.EncryptionKeys.SilentLogins.IV;
                var key = GlobalSettings.EncryptionKeys.SilentLogins.Key;
                var decryptedToken = Security.AESDecrypt(token, key, IV);

                // Split the value and get the values
                var splitToken = decryptedToken.Split('|');
                var customerID = Convert.ToInt32(splitToken[0]);
                var tokenExpirationDate = Convert.ToDateTime(splitToken[1]);

                // Return the expiration status of the token and the sign in response
                //if (tokenExpirationDate < DateTime.Now)
                //{
                //    response.Fail("Token expired");
                //    return response;
                //}

                // Sign the customer in with their customer ID
                response = SignIn(customerID);

                // Mark the response as successful
                response.Success();
            }
            catch (Exception ex)
            {
                response.Fail(ex.Message);
            }

            return response;
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public UserIdentity RefreshIdentity()
        {
            return CreateFormsAuthenticationTicket(Identity.Current.CustomerID);
        }

        public UserIdentity GetIdentity(int customerID)
        {
            var identity = Exigo.OData().Customers
                .Where(c => c.CustomerID == customerID)
                .Select(c => new UserIdentity()
                {
                    CustomerID = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Company = c.Company,
                    LoginName = c.LoginName,
                    Country = c.MainCountry,
                    CustomerTypeID = c.CustomerTypeID,
                    CustomerStatusID = c.CustomerStatusID,
                    LanguageID = c.LanguageID,
                    DefaultWarehouseID = c.DefaultWarehouseID,
                    CurrencyCode = c.CurrencyCode
                })
                .FirstOrDefault();

            // Get the web alias and set it
            //identity.WebAlias = Exigo.OData().CustomerSites.Where(c => c.CustomerID == customerID).FirstOrDefault().WebAlias;

            return identity;
        }

        public string GetSilentLoginRedirect(UserIdentity identity)
        {
            System.Collections.Generic.List<int> retailCustomerTypes = new System.Collections.Generic.List<int>
	        {
		        CustomerTypes.RetailCustomer,
                CustomerTypes.SmartShopper
	        };

            if (retailCustomerTypes.Contains(identity.CustomerTypeID))
            {
                var token = Security.Encrypt(new
                {
                    CustomerID = identity.CustomerID,
                    ExpirationDate = DateTime.Now.AddHours(24)
                });

                return GlobalSettings.Backoffices.SilentLogins.RetailCustomerBackofficeUrl.FormatWith(token);
            }

            return string.Empty;
        }

        public UserIdentity CreateFormsAuthenticationTicket(int customerID)
        {
            // If we got here, we are authorized. Let's attempt to get the identity.
            var identity = GetIdentity(customerID);
            if (identity == null) return null;

            return CreateFormsAuthenticationTicket(identity);
        }
        public UserIdentity CreateFormsAuthenticationTicket(UserIdentity identity)
        {
            // Ensure any defaults that the customer must have
            Task.Run(() =>
            {
                EnsureIdentityDefaults(identity.CustomerID);
            });

            // Create the ticket
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                identity.CustomerID.ToString(),
                DateTime.Now,
                DateTime.Now.AddMinutes(GlobalSettings.Backoffices.SessionTimeout),
                false,
                identity.SerializeProperties());


            // Encrypt the ticket
            string encTicket = FormsAuthentication.Encrypt(ticket);


            // Create the cookie.
            HttpCookie cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName]; //saved user
            if (cookie == null)
            {
                cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                cookie.HttpOnly = true;

                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else
            {
                cookie.Value = encTicket;
                HttpContext.Current.Response.Cookies.Set(cookie);
            }


            // Add the customer ID to the items in case we need this in the same request later on.
            // We need this because we don't have access to the Identity.Current in this same request later on.
            HttpContext.Current.Items.Add("CustomerID", identity.CustomerID);


            return identity;
        }

        public void EnsureIdentityDefaults(int customerID)
        {
            try
            {
                Exigo.EnsureCalendar(customerID);
            }
            catch { }
        }
    }
}

