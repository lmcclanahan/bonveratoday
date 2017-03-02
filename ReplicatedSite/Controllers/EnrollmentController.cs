using Common;
using Common.Api.ExigoWebService;
using Common.Providers;
using Common.HtmlHelpers;
using Common.Services;
using ExigoService;
using ReplicatedSite.Factories;
using ReplicatedSite.Models;
using ReplicatedSite.Providers;
using ReplicatedSite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;

namespace ReplicatedSite.Controllers
{
    public class EnrollmentController : Controller
    {
        #region Constructor
        public EnrollmentController()
        {
            this.OrderConfiguration = Common.GlobalUtilities.GetMarketConfiguration(PropertyBag.SelectedMarket).Orders;
            this.AutoOrderConfiguration = Common.GlobalUtilities.GetMarketConfiguration(PropertyBag.SelectedMarket).AutoOrders;
            this.OrderPacksConfiguration = Common.GlobalUtilities.GetMarketConfiguration(PropertyBag.SelectedMarket).EnrollmentKits;
        }
        #endregion

        #region Properties
        public string ApplicationName = "BonveraEnrollment";

        public IOrderConfiguration OrderConfiguration { get; set; }
        public IOrderConfiguration AutoOrderConfiguration { get; set; }
        public IOrderConfiguration OrderPacksConfiguration { get; set; }

        public ShoppingCartItemsPropertyBag ShoppingCart
        {
            get
            {
                if (_shoppingCart == null)
                {
                    _shoppingCart = Exigo.PropertyBags.Get<ShoppingCartItemsPropertyBag>(ApplicationName + "Cart");
                }
                return _shoppingCart;
            }
        }
        private ShoppingCartItemsPropertyBag _shoppingCart;

        public EnrollmentPropertyBag PropertyBag
        {
            get
            {
                if (_propertyBag == null)
                {
                    _propertyBag = Exigo.PropertyBags.Get<EnrollmentPropertyBag>(ApplicationName + "PropertyBag");
                }
                return _propertyBag;
            }
        }
        private EnrollmentPropertyBag _propertyBag;

        public ILogicProvider LogicProvider
        {
            get
            {
                if (_logicProvider == null)
                {
                    _logicProvider = new EnrollmentLogicProvider(this, ShoppingCart, PropertyBag);
                }
                return _logicProvider;
            }
        }
        private ILogicProvider _logicProvider;

        public string FirstOrderPackItemCode = GlobalUtilities.GetCurrentMarket().FirstOrderPackItemCode;
        #endregion

        #region Step 1 - Enrollment Configuration - Enroller Validate/Country Select
        public ActionResult Index()
        {
            ShoppingCart.Items.Clear();
            Exigo.PropertyBags.Update(ShoppingCart);
            return View();
        }

        [Route("{webalias}/choose-registration")]
        public ActionResult RegistrationLanding(string message = "")
        {
            ViewBag.Message = message;

            return View();
        }

        // If you have any issues with this, the web alias version needs to be used for UAT only and the Route without the webalias is for soft launch and live as of 2/18 - Mike M.
        [Route("backofficeenrollmentlanding")] // For soft launch
        [Route("{webalias}/backofficeenrollmentlanding")] // For live launch
        public ActionResult BackofficeEnrollmentLanding(int ownerID, string enroller, string sponsor)
        {
            dynamic Enroller;
            dynamic Sponsor;

            var enrollerwhereclause = "";
            var enrollerID = 0;
            if (int.TryParse(enroller, out enrollerID))
            {
                enrollerwhereclause = "AND c.CustomerID = " + enrollerID;
            }
            else
            {
                enrollerwhereclause = "AND c.LoginName = '" + enroller + "'";
            }
            using (var context = Exigo.Sql())
            {

                Enroller = context.Query(@"
                            Select c.CustomerID, c.FirstName, c.LastName, c.Company from Customers c
                            inner join UniLevelUpline uu
                            on c.CustomerID = uu.CustomerID
                            Where uu.UplineCustomerID = @id " + enrollerwhereclause + @"
                            and c.CustomerTypeID = @customerTypeID

                            union

                            Select  c.CustomerID, c.FirstName, c.LastName, c.Company from Customers c
                            inner join UniLevelDownline ud
                            on c.CustomerID = ud.CustomerID
                            Where ud.DownlineCustomerID = @id " + enrollerwhereclause + @"
                            and c.CustomerTypeID = @customerTypeID
                        ", new
                         {
                             id = ownerID,
                             customerTypeID = (int)CustomerTypes.Associate
                         }).FirstOrDefault();

                if (Enroller != null)
                {
                    if (enrollerID == 0)
                    {
                        enrollerID = Enroller.CustomerID;
                    }
                }
                else
                {
                    return RedirectToAction("EnrollmentConfiguration", new { message = "Enroller is invalid" });

                }

            }

            var sponsorrwhereclause = "";
            var sponsorID = 0;
            if (int.TryParse(sponsor, out sponsorID))
            {
                sponsorrwhereclause = "AND c.CustomerID = " + sponsorID;
            }
            else
            {
                sponsorrwhereclause = "AND c.LoginName = '" + sponsor + "'";
            }
            using (var context = Exigo.Sql())
            {
                Sponsor = context.Query(@"
                            Select  c.CustomerID, c.FirstName, c.LastName, c.Company from Customers c
                            inner join UniLevelDownline ud
                            on c.CustomerID = ud.CustomerID
                            Where ud.DownlineCustomerID = @id " + sponsorrwhereclause + @"
                            and c.CustomerTypeID = @customerTypeID
                        ", new
                        {
                            id = ownerID,
                            customerTypeID = (int)CustomerTypes.Associate
                        }).FirstOrDefault();

                if (Sponsor != null)
                {
                    if (sponsorID == 0)
                    {
                        sponsorID = Sponsor.CustomerID;

                    }
                }
                else
                {
                    return RedirectToAction("EnrollmentConfiguration", new { message = "Sponsor is invalid" });

                }
            }
            PropertyBag.SponsorID = sponsorID;
            PropertyBag.EnrollerID = enrollerID;

            // Set a value on the Property Bag to ensure that we know this was a back office user that started the Enrollment
            PropertyBag.IsBackofficeEnrollment = true;


            Exigo.PropertyBags.Update(PropertyBag);

            return RedirectToAction("Packs");
        }

        public ActionResult EnrollmentConfiguration(EnrollmentConfigurationViewModel enroller = null)
        {
            var model = new EnrollmentConfigurationViewModel();
            model.EnrollerID = Identity.Owner.CustomerID;
            model.SponsorID = GlobalSettings.ReplicatedSites.OrphanCustomerID;
            if (Request.QueryString["message"] != null)
            {
                ViewBag.Message = Request.QueryString["message"].ToString();
            }

            if (Request.HttpMethod == "GET")
            {
                if (!GlobalSettings.Globalization.HideForLive)
                {
                    // In this case, we already have an enroller set, so we are good to proceed to the next steps. This will need to change when multiple markets are available - Mike M.
                    if (PropertyBag.EnrollerID > 0 && PropertyBag.EnrollerID != GlobalSettings.ReplicatedSites.OrphanCustomerID)
                    {
                        return RedirectToAction("Packs");
                    }

                    // This will need to be removed when we introduce more markets than just US - Mike M.
                    if (model.EnrollerID != GlobalSettings.ReplicatedSites.OrphanCustomerID)
                    {
                        PropertyBag.EnrollerID = model.EnrollerID;
                        Exigo.PropertyBags.Update(PropertyBag);

                        return RedirectToAction("Packs");
                    }
                }

                return View(model);
            }
            else
            {
                // If the user has not chosen an Enroller, we need to return the view back to them so they can see an error message
                if (enroller.EnrollerID == GlobalSettings.ReplicatedSites.OrphanCustomerID)
                {
                    ViewBag.Error = ("Please choose an enroller before you continue your registration.");
                    return View(model);
                }
                if (enroller.SponsorID == GlobalSettings.ReplicatedSites.OrphanCustomerID)
                {
                    ViewBag.Error = ("Please choose a sponsor before you continue your registration.");
                    return View(model);
                }
                // FOR SOFTLAUNCH

                PropertyBag.SponsorID = enroller.SponsorID;
                PropertyBag.EnrollerID = enroller.EnrollerID;
                PropertyBag.SelectedMarket = (enroller.MarketName != null) ? enroller.MarketName : MarketName.UnitedStates;
                PropertyBag.EnrollmentType = enroller.SelectedEnrollmentType;
                Exigo.PropertyBags.Update(PropertyBag);


                // Remove if and only use the logic contained within when soft launch is over - Mike M.
                if (!GlobalSettings.Globalization.HideForLive)
                {
                    // We need to do a check to make sure that the selected enroller is now the website we are on
                    var webalias = Identity.Owner.WebAlias;
                    if (Identity.Owner.CustomerID != enroller.EnrollerID)
                    {
                        try
                        {
                            webalias = Exigo.GetCustomerSite(enroller.EnrollerID).WebAlias;
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    return RedirectToAction("Packs", new { webalias = webalias });
                }
                else
                {
                    return RedirectToAction("Packs");
                }
            }
        }
        #endregion

        #region Step 2 - Packs
        public ActionResult Packs()
        {
            var model = new PacksViewModel();
            model.RequiredEnrollmentPackItemCode = GlobalUtilities.GetCurrentMarket().RequiredEnrollmentPackItemCode;

            if (ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack).Count() > 0)
            {
                // If we have another pack in the cart besides the required kit, we need to set it as the active kit so that the UI will show it as 'Added' to the cart
                if (ShoppingCart.Items.Any(c => c.ItemCode != model.RequiredEnrollmentPackItemCode))
                {
                    model.SelectedOrderItem = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack && c.ItemCode != model.RequiredEnrollmentPackItemCode).FirstOrDefault();
                }
            }

            // We need to make sure that we have our default kit
            if (!ShoppingCart.Items.Any(c => c.ItemCode == model.RequiredEnrollmentPackItemCode))
            {
                // Add the required item since no packs are present in the cart yet
                ShoppingCart.Items.Add(model.RequiredEnrollmentPackItemCode, ShoppingCartItemType.EnrollmentPack);
                Exigo.PropertyBags.Update(ShoppingCart);
            }

            model.CustomerTypeID = (PropertyBag.Customer != null) ? PropertyBag.Customer.CustomerTypeID : 1;

            model.OrderItems = (!GlobalSettings.Globalization.HideForLive) ? Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderPacksConfiguration,
                IncludeChildCategories = true
            }).ToList() :

            Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderPacksConfiguration,
                IncludeChildCategories = true
            }).Where(c => c.ItemCode == model.RequiredEnrollmentPackItemCode).ToList();

            foreach (var item in model.OrderItems)
            {
                item.Type = ShoppingCartItemType.EnrollmentPack;
            }

            model.ReplinishPacks = Exigo.GetItems(new ExigoService.GetItemsRequest() { Configuration = AutoOrderConfiguration, ItemCodes = ReplinishPacks.PackItemCodes.ToArray() });

            return View(model);
        }

        public ActionResult ProductList()
        {
            var model = new ProductListViewModel();

            model.OrderItems = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderConfiguration
            }).ToList();

            var autoOrderItems = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = AutoOrderConfiguration
            }).ToList();
            autoOrderItems.ForEach(c => c.Type = ShoppingCartItemType.AutoOrder);
            model.AutoOrderItems = autoOrderItems;

            return View(model);
        }
        #endregion

        #region Step 3 - Enrollee Info
        // DEVELOPER NOTE: Had to add PaymentMethod here because the ModelBindingContext does not work correctly when you are updating the Credit Card with a new one - Mike M.
        public ActionResult EnrolleeInfo(FormCollection form = null, CreditCard PaymentMethod = null)
        {
            if (Request.HttpMethod == "GET")
            {
                if (PropertyBag.EnrollerID == 0)
                {
                    return RedirectToAction("checkout");
                }

                var requiredItemCode = GlobalUtilities.GetCurrentMarket().RequiredEnrollmentPackItemCode;
               
                // Ensure we only have one First Order Pack in the cart, in case a user attempted to add more than one on the packs page
                if (ShoppingCart.Items.Contains(FirstOrderPackItemCode))
                {
                    if (ShoppingCart.Items.Any(c => c.ItemCode == FirstOrderPackItemCode && c.Quantity > 1))
                    {
                        ShoppingCart.Items.Update(ShoppingCart.Items.FirstOrDefault(c => c.ItemCode == FirstOrderPackItemCode).ID, 1);
                        Exigo.PropertyBags.Update(ShoppingCart);
                    }
                }

                var enrollmentPacks = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.EnrollmentPack || i.Type == ShoppingCartItemType.Order).ToList();
                var enrollmentAutoOrderPacks = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.EnrollmentAutoOrderPack).ToList();
                if (enrollmentPacks.Count() > 0)
                {
                    var order = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                        Configuration = OrderPacksConfiguration,
                        Items = enrollmentPacks,
                        Address = PropertyBag.ShippingAddress,
                        ShipMethodID = PropertyBag.ShipMethodID,
                        ReturnShipMethods = true
                    });

                    PropertyBag.ShowBirthday = (PropertyBag.HasChosenNoBirthday) ? false : true;
                    PropertyBag.ShipMethods = order.ShipMethods;
                }

                if (enrollmentAutoOrderPacks.Count() > 0)
                {
                    var order = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                        Configuration = OrderPacksConfiguration,
                        Items = enrollmentAutoOrderPacks,
                        Address = PropertyBag.ShippingAddress,
                        ShipMethodID = PropertyBag.ShipMethodID,
                        ReturnShipMethods = false
                    });                   
                }

                var enroller = Exigo.GetCustomer(PropertyBag.EnrollerID);

                ViewBag.EnrollerPhone = enroller.PrimaryPhone;
                ViewBag.EnrollerEmail = enroller.Email;
                ViewBag.EnrollerName = enroller.FullName;

                // Populate our sponsor info panel too if Sponsor ID has been set explicitly - Mike M.
                if (PropertyBag.SponsorID > 0)
                {
                    var sponsor = Exigo.GetCustomer(PropertyBag.SponsorID);

                    ViewBag.SponsorPhone = sponsor.PrimaryPhone;
                    ViewBag.SponsorEmail = sponsor.Email;
                    ViewBag.SponsorName = sponsor.FullName;
                }


                return View(PropertyBag);
            }
            else
            {

                var type = typeof(EnrollmentPropertyBag);
                var binder = Binders.GetBinder(type);
                var bindingContext = new ModelBindingContext()
                {
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => PropertyBag, type),
                    ModelState = ModelState,
                    ValueProvider = form
                };
                binder.BindModel(ControllerContext, bindingContext);

                PropertyBag.HasChosenNoBirthday = (PropertyBag.ShowBirthday) ? false : true;

                // Had to manually set this because the ModelBindingContext is not setting the Payment Method correctly - Mike M.
                PropertyBag.PaymentMethod = PaymentMethod;

                // Set Annual Auto Renewal if wanted
                
                if (PropertyBag.WantAnnualRenewal)
                {
                    if (!ShoppingCart.Items.Where(s => s.ItemCode == "IAANNUALRENEWAL").Any())
                    {
                        var renewalItem = Exigo.GetItemDetail(new GetItemDetailRequest() { ItemCode = "IAANNUALRENEWAL", Configuration = OrderPacksConfiguration });
                        renewalItem.Type = ShoppingCartItemType.AutoOrder;
                        renewalItem.Quantity = 1;
                        ShoppingCart.Items.Add(renewalItem);
                    }
                }

                // Set ICAA Membership if wanted 
                if (PropertyBag.WantICAAMembership)
                {
                    if (!ShoppingCart.Items.Where(s => s.ItemCode == "ICAAANNUALFEE").Any())
                    {
                        var membershipitem = Exigo.GetItemDetail(new GetItemDetailRequest() { ItemCode = "ICAAANNUALFEE", Configuration = OrderPacksConfiguration });
                        membershipitem.Type = ShoppingCartItemType.Order;
                        membershipitem.Quantity = 1;
                        ShoppingCart.Items.Add(membershipitem);
                    }
                }
                Exigo.PropertyBags.Update(ShoppingCart);

                // Set shipping address
                if (PropertyBag.UseSameShippingAddress)
                {
                    PropertyBag.ShippingAddress = new ShippingAddress(PropertyBag.Customer.MainAddress);
                    PropertyBag.ShippingAddress.FirstName = PropertyBag.Customer.FirstName;
                    PropertyBag.ShippingAddress.LastName = PropertyBag.Customer.LastName;
                    PropertyBag.ShippingAddress.Phone = PropertyBag.Customer.PrimaryPhone;
                    PropertyBag.ShippingAddress.Email = PropertyBag.Customer.Email;
                }

                // Set billing address
                if (PropertyBag.UseSameBillingAddress)
                {
                    var creditCard = PropertyBag.PaymentMethod as CreditCard;

                    creditCard.BillingAddress.Address1 = PropertyBag.Customer.MainAddress.Address1;
                    creditCard.BillingAddress.Address2 = PropertyBag.Customer.MainAddress.Address2;
                    creditCard.BillingAddress.City = PropertyBag.Customer.MainAddress.City;
                    creditCard.BillingAddress.State = PropertyBag.Customer.MainAddress.State;
                    creditCard.BillingAddress.Country = PropertyBag.Customer.MainAddress.Country;
                    creditCard.BillingAddress.Zip = PropertyBag.Customer.MainAddress.Zip;
                }

                Exigo.PropertyBags.Update(PropertyBag);

                return new JsonNetResult(new
                {
                    success = false
                });
            }

        }
        #endregion

        #region Step 4 - Review and Submit
        public ActionResult Review()
        {
            var logicCheck = LogicProvider.CheckLogic();
            if (!logicCheck.IsValid)
            {
                return logicCheck.NextAction;
            }

            var model = EnrollmentViewModelFactory.Create<EnrollmentReviewViewModel>(PropertyBag);
            var languageID = GlobalUtilities.GetSelectedExigoLanguageID();

            // Ensure we only have one First Order Pack in the cart, in case a user attempted to add more than one on the packs page
            if (ShoppingCart.Items.Contains(FirstOrderPackItemCode))
            {
                if (ShoppingCart.Items.Any(c => c.ItemCode == FirstOrderPackItemCode && c.Quantity > 1))
                {
                    ShoppingCart.Items.Update(ShoppingCart.Items.FirstOrDefault(c => c.ItemCode == FirstOrderPackItemCode).ID, 1);
                    Exigo.PropertyBags.Update(ShoppingCart);
                }
            }

            // Get the cart items
            var cartItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.EnrollmentPack).ToList();
            model.Items = Exigo.GetItems(ShoppingCart.Items, OrderPacksConfiguration, languageID).ToList();

            var calculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
            {
                CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                Configuration = OrderPacksConfiguration,
                Items = cartItems,
                Address = PropertyBag.ShippingAddress,
                ShipMethodID = PropertyBag.ShipMethodID,
                ReturnShipMethods = true
            });


            model.Totals = calculationResult;
            model.ShipMethods = calculationResult.ShipMethods;


            // Set the default ship method
            var shipMethodID = 0;
            if (PropertyBag.ShipMethodID != 0)
            {
                shipMethodID = PropertyBag.ShipMethodID;
            }
            else
            {
                if (calculationResult.ShipMethods.Where(s => s.ShipMethodID == 5).Count() > 0) // Check for Smart Post ship method
                {
                    shipMethodID = 5;
                }
                else
                {
                    shipMethodID = OrderPacksConfiguration.DefaultShipMethodID;
                }

                PropertyBag.ShipMethodID = shipMethodID;
                Exigo.PropertyBags.Update(PropertyBag);
            }

            if (model.ShipMethods != null && model.ShipMethods.Count() > 0)
            {
                if (model.ShipMethods.Any(c => c.ShipMethodID == shipMethodID))
                {
                    // Set all other ship methods to not be selected
                    model.ShipMethods.ToList().ForEach(c =>
                    {
                        c.Selected = false;
                    });
                    // Then we ensure the choice or the cheapest option is selected first
                    model.ShipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
                }
                else
                {
                    // If we don't have the ship method we're supposed to select, 
                    // check the first one, save the selection and recalculate
                    model.ShipMethods.OrderBy(c => c.Price).First().Selected = true;

                    PropertyBag.ShipMethodID = model.ShipMethods.OrderBy(c => c.Price).First().ShipMethodID;
                    Exigo.PropertyBags.Update(PropertyBag);

                    var newCalculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                        Configuration = OrderPacksConfiguration,
                        Items = cartItems,
                        Address = PropertyBag.ShippingAddress,
                        ShipMethodID = PropertyBag.ShipMethodID,
                        ReturnShipMethods = false
                    });

                    model.Totals = newCalculationResult;
                }
            }
            else
            {
                model.Errors = new string[1];
                model.Errors[0] = "We are having trouble calculating shipping for this order. Please double-check your shipping address or contact support@bonvera.com for assistance";
            }


            // Logic to handle showing First Order Pack discount total
            if (model.Totals.Details.Where(d => d.PriceTotal < 0).Count() > 0)
            {
                var discountedItems = model.Totals.Details.Where(d => d.PriceTotal < 0).ToList();

                model.Discount = discountedItems.Sum(d => d.PriceTotal);
            }

            model.Enroller = Exigo.GetCustomer(PropertyBag.EnrollerID);

            if (PropertyBag.SponsorID != 0)
            {
                model.Sponsor = Exigo.GetCustomer(PropertyBag.SponsorID);
            }

            #region Will Call Logic
            // Will Call Ship Method Check
            model.WillCallShipMethodID = GlobalUtilities.GetCurrentMarket().WillCallShipMethodID;
            var hasWillCallAvailable = false;
            var userSelectedWillCall = false;

            if (model.Totals.ShipMethods.Any(s => s.ShipMethodID == model.WillCallShipMethodID))
            {
                hasWillCallAvailable = true;

                if (PropertyBag.ShipMethodID == model.WillCallShipMethodID)
                {
                    userSelectedWillCall = true;
                }
            }

            ViewBag.HasWillCallAvailable = hasWillCallAvailable;
            ViewBag.UserSelectedWillCall = userSelectedWillCall;
            #endregion


            return View(model);
        }

        [HttpPost]
        public ActionResult SubmitCheckout()
        {
            try
            {

                // Start creating the API requests
                var apiRequests = new List<ApiRequest>();

                // Create the customer
                if (PropertyBag.HasChosenNoBirthday)
                {
                    PropertyBag.Customer.BirthDate = DateTime.MinValue;
                }
                var customerRequest = new CreateCustomerRequest(PropertyBag.Customer);
                // Use the web alias as the login name                
                customerRequest.InsertEnrollerTree = true;
                customerRequest.CustomerType = CustomerTypes.Associate;
                customerRequest.EnrollerID = PropertyBag.EnrollerID;
                customerRequest.EntryDate = DateTime.Now;
                customerRequest.CustomerStatus = CustomerStatuses.Active;
                customerRequest.CanLogin = true;
                customerRequest.Notes = "Independent Associate was entered by Independent Associate #{0}. Created by the API Enrollment at ".FormatWith(Identity.Owner.CustomerID) + HttpContext.Request.Url.Host + HttpContext.Request.Url.LocalPath + " on " + DateTime.Now.ToString("dddd, MMMM d, yyyy h:mmtt") + " CST at IP " + Common.GlobalUtilities.GetClientIP() + " using " + HttpContext.Request.Browser.Browser + " " + HttpContext.Request.Browser.Version + " (" + HttpContext.Request.Browser.Platform + ").";



                // Here we need to determine if the Sponsor ID has been set at this point or if we need to look up the Enroller's selected preference
                if (PropertyBag.SponsorID > 0)
                {
                    customerRequest.SponsorID = PropertyBag.SponsorID;
                }
                else
                {
                    // Not using placement preferences for now, per ticket # 75086 - Mike M.
                    var sponsorID = Exigo.GetCustomersSponsorPreference(PropertyBag.EnrollerID);
                    //var sponsorID = PropertyBag.EnrollerID;
                    
                    customerRequest.SponsorID = sponsorID;
                }
                customerRequest.InsertUnilevelTree = true;

                apiRequests.Add(customerRequest);


                // Set a few variables up for our shippping address, order/auto order items and the default auto order payment type
                var shippingAddress = PropertyBag.ShippingAddress;
                var orderItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.Order || i.Type == ShoppingCartItemType.EnrollmentPack).ToList();
                var autoOrderPackItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.EnrollmentAutoOrderPack).ToList();
                var autoOrderItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.AutoOrder).ToList();
                var autoOrderPaymentType = AutoOrderPaymentType.PrimaryCreditCard;

                // Create initial order
                var orderRequest = new CreateOrderRequest(OrderPacksConfiguration, PropertyBag.ShipMethodID, orderItems, shippingAddress);

                // Add the new credit card to the customer's record and charge it for the current order
                if (PropertyBag.PaymentMethod.CanBeParsedAs<CreditCard>())
                {
                    var creditCard = PropertyBag.PaymentMethod.As<CreditCard>();

                    // If we are dealing with a test credit card, then we set the order as accepted to simulate an 'Accepted' order
                    if (!creditCard.IsTestCreditCard)
                    {
                        var chargeCCRequest = new ChargeCreditCardTokenRequest(creditCard);
                        apiRequests.Add(chargeCCRequest);

                        var saveCCRequest = new SetAccountCreditCardTokenRequest(creditCard);
                        apiRequests.Add(saveCCRequest);
                    }
                    else
                    {
                        orderRequest.OrderStatus = OrderStatusType.Shipped;
                    }
                }

                // Add order request now if we need to do any testing with the accepted functionality
                apiRequests.Add(orderRequest);

                // Create subscription autoorder if an autoorder has been chosen
                if (autoOrderPackItems != null && autoOrderPackItems.Count() > 0)
                {
                    var autoOrderRequest = new CreateAutoOrderRequest(AutoOrderConfiguration, autoOrderPaymentType, DateTime.Now.AddMonths(1), PropertyBag.ShipMethodID, autoOrderPackItems, shippingAddress);
                    autoOrderRequest.Frequency = FrequencyType.Monthly;
                    apiRequests.Add(autoOrderRequest);
                }

                // Create customer site
                var customerSiteRequest = new SetCustomerSiteRequest(PropertyBag.Customer);
                apiRequests.Add(customerSiteRequest);


                // Process the transaction
                var transaction = new TransactionalRequest { TransactionRequests = apiRequests.ToArray() };
                var response = Exigo.WebService().ProcessTransaction(transaction);

                var newcustomerid = 0;
                var neworderid = 0;
                var newautoorderid = 0;

                if (response.Result.Status == ResultStatus.Success)
                {
                    foreach (var apiresponse in response.TransactionResponses)
                    {
                        if (apiresponse.CanBeParsedAs<CreateCustomerResponse>()) newcustomerid = apiresponse.As<CreateCustomerResponse>().CustomerID;
                        if (apiresponse.CanBeParsedAs<CreateOrderResponse>()) neworderid = apiresponse.As<CreateOrderResponse>().OrderID;
                        if (apiresponse.CanBeParsedAs<CreateAutoOrderResponse>()) newautoorderid = apiresponse.As<CreateAutoOrderResponse>().AutoOrderID;
                    }
                }

                if (autoOrderItems != null && autoOrderItems.Count > 0)
                {
                    var autoOrderRequest = new CreateAutoOrderRequest(AutoOrderConfiguration, autoOrderPaymentType, DateTime.Now.AddYears(1), 8, autoOrderItems, shippingAddress);
                    autoOrderRequest.CustomerID = newcustomerid;
                    autoOrderRequest.Frequency = FrequencyType.Yearly;
                    Exigo.WebService().CreateAutoOrder(autoOrderRequest);
                }

                var firstName = PropertyBag.Customer.FirstName;
                var lastName = PropertyBag.Customer.LastName;
                var email = PropertyBag.Customer.Email;


                // Handle email opt in logic
                if (PropertyBag.Customer.IsOptedIn)
                {
                    Exigo.SendEmailVerification(newcustomerid, email);
                }

                var isBackofficeEnrollment = PropertyBag.IsBackofficeEnrollment;

                var token = Security.Encrypt(new
                {
                    CustomerID = newcustomerid,
                    OrderID = neworderid,
                    IsBackofficeEnrollment = isBackofficeEnrollment
                });

                // Enrollment complete, now delete the Property Bag
                Exigo.PropertyBags.Delete(PropertyBag);
                Exigo.PropertyBags.Delete(ShoppingCart);

                return RedirectToAction("EnrollmentComplete", new { token = token });
            }
            catch (Exception exception)
            {
                return RedirectToAction("Review", new { error = exception.Message });
            }
        }

        public ActionResult EnrollmentComplete(string token)
        {
            var model = new EnrollmentCompleteViewModel();
            model.WillCallShipMethodID = GlobalUtilities.GetCurrentMarket().WillCallShipMethodID;

            var args = Security.Decrypt(token);
            var hasOrder = args["OrderID"] != null;
            model.Token = token;
            model.CustomerID = Convert.ToInt32(args["CustomerID"]);

            model.IsBackOfficeEnrollment = Convert.ToBoolean(args["IsBackofficeEnrollment"]);

            if (hasOrder)
            {
                model.OrderID = Convert.ToInt32(args["OrderID"]);
            }
            if (hasOrder)
            {
                model.Order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
                {
                    CustomerID = model.CustomerID,
                    OrderID = model.OrderID,
                    IncludeOrderDetails = true,
                    IncludePayments = true
                }).FirstOrDefault();
            }

            var credentials = Exigo.GetCustomerCredentials(model.CustomerID);

            model.Username = credentials.UserName;
            model.Password = credentials.Password;


            return View(model);
        }

        [Route("enrollmentcheckout")]
        public ActionResult Checkout()
        {
            return LogicProvider.GetNextAction();
        }
        #endregion

        #region General Ajax Actions
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

        [HttpPost]
        public JsonNetResult UpdatePackItems(string itemcode, string action, int packType)
        {
            try
            {
                if (packType == (int)ShoppingCartItemType.EnrollmentPack)
                {
                    //ShoppingCart.Items.Remove(ShoppingCartItemType.EnrollmentPack);

                    if (action == "add")
                    {
                        //TODO: need to add logic here to remove the standard item(added if no pack was chosen)

                        ShoppingCart.Items.Add(new ShoppingCartItem()
                        {
                            ItemCode = itemcode,
                            Quantity = 1,
                            Type = ShoppingCartItemType.EnrollmentPack
                        });
                    }
                    else if (action == "remove") 
                    {
                        var requiredItemCode = GlobalUtilities.GetCurrentMarket().RequiredEnrollmentPackItemCode;
                        // Ensure we are not removing the required pack
                        if (itemcode != requiredItemCode)
                        { 
                            ShoppingCart.Items.Remove(itemcode, ShoppingCartItemType.EnrollmentPack);
                        }
                    }

                    Exigo.PropertyBags.Update(ShoppingCart);

                    return new JsonNetResult(new
                    {
                        success = true
                    });
                }

                else if (packType == (int)ShoppingCartItemType.EnrollmentAutoOrderPack)
                {
                    //ShoppingCart.Items.Remove(ShoppingCartItemType.EnrollmentPack);

                    if (action == "add")
                    {
                        //We only allow 1 of the 3 packs during the enrollment process
                        if (ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.EnrollmentAutoOrderPack).Any())
                        {
                            //If any exist remove them
                            var removeItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.EnrollmentAutoOrderPack).ToList();
                            foreach (var item in removeItems)
                            {
                                ShoppingCart.Items.Remove(item.ID);
                            }
                        }
                        //Add the one we want
                        ShoppingCart.Items.Add(new ShoppingCartItem()
                        {
                            ItemCode = itemcode,
                            Quantity = 1,
                            Type = ShoppingCartItemType.EnrollmentAutoOrderPack
                        });
                    }
                    else if (action == "remove")
                    {                   
                            ShoppingCart.Items.Remove(itemcode, ShoppingCartItemType.EnrollmentAutoOrderPack);
                    }

                    Exigo.PropertyBags.Update(ShoppingCart);

                    return new JsonNetResult(new
                    {
                        success = true
                    });
                }
                else
                {
                    return new JsonNetResult(new
                    {
                        success = false
                    });
                }

            }
            catch (Exception)
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        public JsonNetResult UpdateItemSummary(int shipMethodID = 0, bool hideShippingAndTax = false)
        {
            try
            {
                var model = new EnrollmentSummaryViewModel();
                var languageID = GlobalUtilities.GetSelectedExigoLanguageID();
                var orderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.EnrollmentPack).ToList();

                if (shipMethodID == 0)
                {
                    shipMethodID = PropertyBag.ShipMethodID;
                }

                var order = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not 
                    Configuration = OrderPacksConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = shipMethodID,
                    ReturnShipMethods = true
                });


                model.OrderEnrollmentPacks = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack), OrderPacksConfiguration, languageID);
                model.EnrollmentAutoOrderPack = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentAutoOrderPack), OrderPacksConfiguration, languageID);
                model.AutoOrderItems = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder), OrderConfiguration, languageID);
                model.OrderItems = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order), OrderConfiguration, languageID);
                model.OrderSubtotal = order.Subtotal;
                model.Total = order.Total;
                model.Shipping = order.Shipping;
                model.Tax = order.Tax;

                if (order.Details.Where(d => d.PriceTotal < 0).Count() > 0)
                {
                    var discountedItems = order.Details.Where(d => d.PriceTotal < 0).ToList();

                    model.Discount = discountedItems.Sum(d => d.PriceTotal);
                }

                model.HideShippingAndTax = hideShippingAndTax;
                return new JsonNetResult(new
                {
                    success = true,
                    orderitems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.AutoOrder),
                    html = this.RenderPartialViewToString("_EnrollmentSummary", model)
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

        public JsonNetResult AddItemToCart(Item item)
        {
            try
            {
                ShoppingCart.Items.Add(item);
                Exigo.PropertyBags.Update(ShoppingCart);

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception)
            {

                return new JsonNetResult(new
                {
                    success = false
                });
            }

        }

        public ActionResult UpdateItemQuantity(string itemcode, string type, decimal quantity)
        {
            try
            {
                var itemType = ConvertItemType(type);
                var item = ShoppingCart.Items.Where(c => c.ItemCode == itemcode && c.Type == itemType).FirstOrDefault();

                ShoppingCart.Items.Update(item.ID, quantity);
                Exigo.PropertyBags.Update(ShoppingCart);

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception)
            {

                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        public JsonNetResult DeleteItemFromCart(string itemcode, string type)
        {
            try
            {
                var itemType = ConvertItemType(type);

                ShoppingCart.Items.Remove(itemcode, itemType);
                Exigo.PropertyBags.Update(ShoppingCart);

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception)
            {

                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        [HttpPost]
        public ActionResult SetShipMethodID(int shipMethodID)
        {
            PropertyBag.ShipMethodID = shipMethodID;
            Exigo.PropertyBags.Update(PropertyBag);

            return RedirectToAction("Review");
        }

        [OutputCache(Duration = 600, VaryByParam = "itemcategoryid")]
        public JsonNetResult GetItems(int itemcategoryid)
        {
            var items = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderPacksConfiguration,
                CategoryID = itemcategoryid
            }).ToList();

            ViewBag.Category = Exigo.GetItemCategory(itemcategoryid);


            var html = this.RenderPartialViewToString("_ProductList", items);


            return new JsonNetResult(new
            {
                success = true,
                items = items,
                html = html
            });
        }
        #endregion

        #region Helpers
        public ShoppingCartItemType ConvertItemType(string type)
        {
            var itemType = new ShoppingCartItemType();
            switch (type)
            {
                case "Order":
                    itemType = ShoppingCartItemType.Order;
                    break;
                case "AutoOrder":
                    itemType = ShoppingCartItemType.AutoOrder;
                    break;
                case "EnrollmentPack":
                    itemType = ShoppingCartItemType.EnrollmentPack;
                    break;
                case "EnrollmentAutoOrderPack":
                    itemType = ShoppingCartItemType.EnrollmentAutoOrderPack;
                    break;
                default:
                    break;
            }
            return itemType;
        }

        public ActionResult ResetEnrollment()
        {
            var isBackofficeEnrollment = PropertyBag.IsBackofficeEnrollment;

            Exigo.PropertyBags.Delete(PropertyBag);
            Exigo.PropertyBags.Delete(ShoppingCart);

            var resetUrl = isBackofficeEnrollment ? GlobalSettings.Backoffices.EnrollNowLandingPageUrl : Url.Action("index", "home");

            return Redirect(resetUrl);
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
                    else return "http://exigodemo.azurewebsites.net/" + this.WebAlias;
                }
            }
            public string MainState { get; set; }
            public string MainCity { get; set; }
            public string MainCountry { get; set; }
        }
        #endregion
    }
}