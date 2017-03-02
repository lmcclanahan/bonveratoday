using Backoffice.Factories;
using Backoffice.Filters;
using Backoffice.Models;
using Backoffice.Providers;
using Backoffice.ViewModels;
using Common;
using Common.Api.ExigoWebService;
using Common.Providers;
using Common.Services;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Backoffice.Controllers
{
    [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = false)]
    public class SubscriptionsController : Controller
    {
        public SubscriptionsController()
        {
            //We create this object to retrieve the user's currently selected market
            var CurrentMarket = GlobalUtilities.GetCurrentMarket();
            //We create an auto order configuration based off of their currently selected market
            this.OrderConfiguration = GlobalUtilities.GetMarketConfiguration(CurrentMarket.Name).BackOfficeOrders;
        }

        #region Properties
        public IOrderConfiguration OrderConfiguration { get; set; }

        public int CurrentCustomerID
        {
            get
            {
                var customerID = Identity.Current.CustomerID;

                return customerID;
            }
        }
        #endregion

        public ActionResult Index()
        {          
            var model = new BackofficeSubscriptionsViewModel();

            // Get the customer's subscriptions
            model.Subscriptions = Exigo.GetCustomerSubscriptions(CurrentCustomerID).ToList();

            
            // If we have any expired subscriptions, we need to calculate how much they are going to cost to catch them up today
            if (model.Subscriptions.Where(c => c.SubscriptionID == Subscriptions.BackofficeFeatures).FirstOrDefault() == null || model.Subscriptions.Where(c => c.SubscriptionID == Subscriptions.BackofficeFeatures).Any(s => s.IsExpired))
            {

                var customer = Exigo.GetCustomer(Identity.Current.CustomerID);
                if (customer.CreatedDate >= DateTimeExtensions.ToCST(DateTime.Now.AddMinutes(-30)) && customer.CustomerTypeID == CustomerTypes.Associate)
                {
                    var cookie = new HttpCookie(GlobalSettings.Backoffices.MonthlySubscriptionCookieName);
                    cookie.Value = "true";
                    cookie.Expires = DateTime.Now.AddMinutes(15);
                    HttpContext.Response.Cookies.Add(cookie);

                    return RedirectToAction("Index", "Dashboard");
                }
                // Set up our request to get the order totals on the subscriptions that are required
                var request = new OrderCalculationRequest();

                // We pull the customer's addresses first, if there are none we calculate with the corp address
                var customerAddresses = Exigo.GetCustomerAddresses(CurrentCustomerID);
                var address = (customerAddresses.Count() > 0 && customerAddresses.Where(c => c.IsComplete == true).Count() > 0) ? customerAddresses.Where(c => c.IsComplete == true).FirstOrDefault() : GlobalSettings.Company.Address;

                // Find which subscriptions are expired and add the appropriate items to those where needed
                var itemsToCalculate = new List<ShoppingCartItem>();


                itemsToCalculate.Add(new ShoppingCartItem { ItemCode = GlobalSettings.Backoffices.MonthlySubscriptionItemCode, Quantity = 1 });


                model.PaymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                {
                    CustomerID = CurrentCustomerID,
                    ExcludeIncompleteMethods = true,
                    ExcludeInvalidMethods = true
                });
                request.Configuration = OrderConfiguration;
                request.Address = address;
                request.Items = itemsToCalculate;

                request.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
                //82774 Ivan S. 11/24/2016
                //The Commissions page was displaying an OOPS error when clicking on it, because
                //we were not passing in the CustomerID, and it was generating a null exception
                //Therefore I added this line of code:
                request.CustomerID = Identity.Current.CustomerID;
                model.OrderCalcResponse = Exigo.CalculateOrder(request);
            }
            
            return View(model);
        }

        [HttpPost]
        public ActionResult RenewSubscription(CreditCard creditcard)
        {
            var shipMethodID = OrderConfiguration.DefaultShipMethodID; // Will call or free option, since this is a virtual order
            var customerID = CurrentCustomerID;
            var requests = new List<ApiRequest>();
            var items = new List<ShoppingCartItem>();
            var paymonthly = true;
            var card = new CreditCard();

            
            if (creditcard.Type != CreditCardType.New)
            {
                card = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                  {
                      CustomerID = Identity.Current.CustomerID,
                      ExcludeIncompleteMethods = true,
                      ExcludeInvalidMethods = true
                  }).Where(c => c is CreditCard && ((CreditCard)c).Type == creditcard.Type).FirstOrDefault().As<CreditCard>();
            }
            else
            {
                card = creditcard;
            }
                      

            try
            {
                // Determine which items we need to add to the order we are creating and assemble our order request
                if (paymonthly)
                {
                    items.Add(new ShoppingCartItem { ItemCode = GlobalSettings.Backoffices.MonthlySubscriptionItemCode, Quantity = 1 });
                }


                // We pull the customer's addresses first, if there are none we calculate with the corp address
                var customerAddresses = Exigo.GetCustomerAddresses(CurrentCustomerID);
                var address = (customerAddresses.Count() > 0 && customerAddresses.Where(c => c.IsComplete).Count() > 0) ? customerAddresses.Where(c => c.IsComplete).FirstOrDefault().As<ShippingAddress>() : new ShippingAddress(GlobalSettings.Company.Address);


                var OrderRequest = new CreateOrderRequest(OrderConfiguration, shipMethodID, items, address)
                {
                    CustomerID = customerID
                };

                requests.Add(OrderRequest);

                // Then next step is adding the credit card payment and account update request     
                if (!card.IsTestCreditCard && !Request.IsLocal)
                {
                    if (card.Type == CreditCardType.New)
                    {
                        requests.Add(new ChargeCreditCardTokenRequest(card));
                        requests.Add(new SetAccountCreditCardTokenRequest(card) { CustomerID = customerID, CreditCardAccountType = AccountCreditCardType.Primary });
                    }
                    else
                    {
                        requests.Add(new ChargeCreditCardTokenOnFileRequest(card));
                    }
                }
                else
                {
                    OrderRequest.OrderStatus = OrderStatusType.Shipped;
                }

                // Process the transaction
                var transactionRequest = new TransactionalRequest();
                transactionRequest.TransactionRequests = requests.ToArray();
                var transactionResponse = Exigo.WebService().ProcessTransaction(transactionRequest);

                if (transactionResponse.Result.Status != ResultStatus.Success)
                {
                    //throw new Exception(transactionResponse.Result.Errors.ToString());
                    throw new Exception("Transaction failed");
                }
                else
                {

                    var httpContext = System.Web.HttpContext.Current;
                    var cookie = httpContext.Request.Cookies[GlobalSettings.Backoffices.MonthlySubscriptionCookieName];
                    if (cookie != null)
                    {
                        cookie.Expires = DateTime.Now.AddMinutes(30);
                        Response.Cookies.Set(cookie);
                    }
                    else
                    {
                        cookie = new HttpCookie(GlobalSettings.Backoffices.MonthlySubscriptionCookieName);
                        cookie.Value = "true";
                        cookie.Expires = DateTime.Now.AddMinutes(30);
                        Response.Cookies.Add(cookie);
                        httpContext.Response.Cookies.Add(cookie);
                        //Invalidate the customer subscription cache
                        HttpRuntime.Cache.Remove(string.Format("exigo.customersubscriptions.{0}-{1}", customerID, Subscriptions.BackofficeFeatures));

                    }
                }


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
        
        //20161110 81686 CC. Client requested a thank you page with additional information 
        public ActionResult ThankYou()
        {
            return View();
        }

    }
}
