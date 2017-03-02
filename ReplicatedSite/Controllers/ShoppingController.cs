using Common;
using Common.Api.ExigoWebService;
using Common.Providers;
using Common.Services;
using ExigoService;
using ReplicatedSite.Factories;
using ReplicatedSite.Filters;
using ReplicatedSite.Models;
using ReplicatedSite.Providers;
using ReplicatedSite.Services;
using ReplicatedSite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ReplicatedSite.Controllers
{
    [RoutePrefix("{webalias}")]
    [PreLaunchHide(AllowPreLaunch = false)]
    public class ShoppingController : Controller
    {
        #region Properties
        public string ShoppingCartName = GlobalSettings.Company.Name + "ReplicatedSiteShopping";

        public IOrderConfiguration OrderConfiguration = (Identity.Customer == null) ? Identity.Owner.Market.Configuration.Orders : Identity.Customer.Market.Configuration.Orders;
        public IOrderConfiguration AutoOrderConfiguration = (Identity.Customer == null) ? Identity.Owner.Market.Configuration.AutoOrders : Identity.Customer.Market.Configuration.AutoOrders;

        public int OrderPriceTypeID
        {
            get
            {
                var priceTy = PriceTypes.Retail;

                if (Identity.Customer != null && Identity.Customer.CustomerTypeID == (int)CustomerTypes.SmartShopper)
                {
                    priceTy = PriceTypes.Wholesale;
                }
                else if (ShoppingCart.HasAutoOrderItems())
                {
                    priceTy = PriceTypes.Wholesale;
                }


                return priceTy;
            }
        }

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

        public ShoppingCartCheckoutPropertyBag PropertyBag
        {
            get
            {
                if (_propertyBag == null)
                {
                    _propertyBag = Exigo.PropertyBags.Get<ShoppingCartCheckoutPropertyBag>(ShoppingCartName + "PropertyBag");
                }
                return _propertyBag;
            }
        }
        private ShoppingCartCheckoutPropertyBag _propertyBag;

        public ILogicProvider LogicProvider
        {
            get
            {
                if (_logicProvider == null)
                {
                    _logicProvider = new ShoppingCartLogicProvider(this, ShoppingCart, PropertyBag);
                }
                return _logicProvider;
            }
        }
        private ILogicProvider _logicProvider;

        // Smart Shopper First Order Pack Item Code
        public string FirstOrderPackItemCode = GlobalUtilities.GetCurrentMarket().SmartShopperFirstOrderPackItemCode;
        #endregion

        #region Category Landing
        [Route("shopping")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OurProducts()
        {
            return View();
        }

        [Route("category")]
        public ActionResult Category()
        {
            return View();
        }
        #endregion

        #region Items/Cart
        [Route("products/{parentCategoryKey?}/{subCategoryKey?}")]
        public ActionResult ItemList(string parentCategoryKey = null, string subCategoryKey = null)
        {
            var model = ShoppingViewModelFactory.Create<ItemListViewModel>(PropertyBag);
            if (!parentCategoryKey.IsEmpty()) model.parentCategoryKey = parentCategoryKey;
            if (!subCategoryKey.IsEmpty()) model.subCategoryKey = subCategoryKey;

            model.OrderConfiguration = OrderConfiguration;
            var webCategories = Exigo.GetItemCategories(OrderConfiguration.CategoryID);
            model.CurrentCategory = new ItemCategory();

            model.CategoryID = OrderConfiguration.CategoryID;

            if (parentCategoryKey.IsEmpty() && subCategoryKey.IsEmpty())
            {
                model.CategoryID = OrderConfiguration.FeaturedCategoryID;
                model.CurrentCategory = webCategories.Where(c => c.ItemCategoryID == model.CategoryID).FirstOrDefault();
            }

            if (!parentCategoryKey.IsEmpty())
            {
                model.CurrentCategory = webCategories.Where(c => c.ItemCategoryViewName.Equals(parentCategoryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                model.CategoryID = model.CurrentCategory.ItemCategoryID;
            }

            if (!subCategoryKey.IsEmpty())
            {
                var subcategory = model.CurrentCategory.Subcategories.FirstOrDefault(s => s.ItemCategoryViewName.Equals(subCategoryKey, StringComparison.InvariantCultureIgnoreCase));
                if (subcategory != null)
                {
                    model.CategoryID = subcategory.ItemCategoryID;
                }
            }

            model.Categories = webCategories;

            if (!parentCategoryKey.IsEmpty())
            {
                var mainCategories = model.Categories.Where(c => c.ItemCategoryID == model.CategoryID);

                if (mainCategories.Count() > 0)
                {
                    model.CurrentCategory = mainCategories.FirstOrDefault();
                }
                else
                {
                    foreach (var cat in model.Categories)
                    {
                        if (cat.Subcategories != null && cat.Subcategories.Any(s => s.ItemCategoryID == model.CategoryID))
                        {
                            model.CurrentCategory = cat.Subcategories.FirstOrDefault(s => s.ItemCategoryID == model.CategoryID);
                            model.CurrentCategory.ParentItemCategoryDescription = cat.ItemCategoryDescription;
                            model.CurrentCategory.ParentItemCategoryViewName = cat.ItemCategoryViewName;
                            model.CurrentCategory.ParentItemCategoryID = cat.ItemCategoryID;
                        }
                    }
                }
            }


            return View(model);
        }

        // Method to get items via ajax, based on the category selected on the ItemList page
        [Route("GetItems")]
        public JsonNetResult GetItemList(int categoryId = 0, string search = "", SortByOptions sort = SortByOptions.AtoZ_Ascending)
        {
            var model = ShoppingViewModelFactory.Create<ItemListViewModel>(PropertyBag);
            var languageID = GlobalUtilities.GetSelectedExigoLanguageID();
            var hasSearchQuery = search.IsNotNullOrEmpty();

            model.OrderConfiguration = OrderConfiguration;

            //If the user is searching for something, then change the category id to "all products"
            categoryId = (hasSearchQuery) ? 6 : categoryId;

            //if its zero it means they havent landed on the page before, give em' the featured category (now Bonvera Private Label)
            if (categoryId == 0)
            {
                categoryId = model.OrderConfiguration.FeaturedCategoryID;
            }

            // Get the available products
            var itemsRequest = new ExigoService.GetItemsRequest
            {
                Configuration = OrderConfiguration,
                IncludeChildCategories = true,
                CategoryID = categoryId,
                LanguageID = languageID,
                PriceTypeID = OrderPriceTypeID
            };

            var itemsTask = Task.Factory.StartNew(() =>
            {
                model.Items = Exigo.GetItems(itemsRequest).ToList();
            });

            Task.WaitAll(itemsTask);
            //T.W. #84838 2/7/2017 IF othercheck is set to true than they are hidden/unavailable to purchase
            model.Items = model.Items.Where(c => c.OtherCheck1 == false).ToList();

            if (hasSearchQuery)
            {
                search = search.ToLower();
                var itemNameResults = model.Items.Where(c => c.ItemDescription.ToLower().Contains(search)).ToList();
                var shortDetailResults = model.Items = model.Items.Where(c => c.ShortDetail1.ToLower().Contains(search)).ToList();
                model.Items = itemNameResults.Union(shortDetailResults).ToList();
            }

            //sort the items gathered, by default alphabetically ascending
            switch (sort)
            {
                case SortByOptions.AtoZ_Descending:
                    model.Items = model.Items.OrderByDescending(x => x.ItemDescription).ToList();
                    break;
                case SortByOptions.BV_Ascending:
                    model.Items = model.Items.OrderBy(x => x.BV).ToList();
                    break;
                case SortByOptions.BV_Descending:
                    model.Items = model.Items.OrderByDescending(x => x.BV).ToList();
                    break;
                case SortByOptions.Price_Ascending:
                    model.Items = model.Items.OrderBy(x => x.Price).ToList();
                    break;
                case SortByOptions.Price_Descending:
                    model.Items = model.Items.OrderByDescending(x => x.Price).ToList();
                    break;
                default:
                    model.Items = model.Items.OrderBy(x => x.ItemDescription).ToList();
                    break;
            }

            // Here we run logic to check if a user has purchased their first order pack and if not, validate if they can if they are less than 30 days in the system - Mike M.
            // First thing we check is if they currently have one in the cart, if this is true we dont need to allow them to add it again
            var cartHasFirstOrderPack = ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0;
            if (!cartHasFirstOrderPack)
            {
                if (Identity.Customer != null && Identity.Customer.CustomerTypeID == (int)CustomerTypes.SmartShopper)
                {
                    var canPurchaseFirstOrderPack = GlobalUtilities.CanPurchaseFirstOrderPack(Identity.Customer.CustomerID, FirstOrderPackItemCode);

                    if (canPurchaseFirstOrderPack)
                    {
                        var firstOrderPack = Exigo.GetItems(new ExigoService.GetItemsRequest
                        {
                            Configuration = OrderConfiguration,
                            ItemCodes = new string[] { FirstOrderPackItemCode },
                            LanguageID = languageID,
                            PriceTypeID = OrderPriceTypeID
                        }).FirstOrDefault();

                        // We set this value to make sure this item is not included in the Tier 1 list because we have it displayed above everything in its own section.
                        firstOrderPack.Field2 = "99999";

                        model.Items.Add(firstOrderPack);
                    }
                }
            }


            model.HasAutoOrderItems = ShoppingCart.HasAutoOrderItems();
            model.CategoryID = categoryId;
            
            model.CurrentCategory = (Exigo.GetItemCategory(categoryId));
            //var webCategories = Exigo.GetItemCategories(OrderConfiguration.CategoryID);           
            //model.CurrentCategory = webCategories.Where(c => c.ItemCategoryID == model.CategoryID).FirstOrDefault();

            var html = this.RenderPartialViewToString("Partials/Items/_ItemListModule", model);

            return new JsonNetResult(new
            {
                success = true,
                html = html
            });
        }

        [Route("product/{itemcode}")]
        public ActionResult ItemDetail(string itemcode)
        {
            var model = ShoppingViewModelFactory.Create<ItemDetailViewModel>(PropertyBag);

            var languageID = GlobalUtilities.GetSelectedExigoLanguageID();

            model.Item = Exigo.GetItemDetail(new GetItemDetailRequest
            {
                Configuration = OrderConfiguration,
                ItemCode = itemcode,
                LanguageID = languageID,
                WarehouseID = OrderConfiguration.WarehouseID,
                CurrencyCode = OrderConfiguration.CurrencyCode,
                PriceTypeID = OrderPriceTypeID
            });

            if (model.Item != null)
            {
                model.Item.Quantity = 1;
            }

            #region Category Logic

            var categoryID = OrderConfiguration.CategoryID;
            var webCategories = Exigo.GetItemCategories(categoryID);
            model.Categories = webCategories;

            var parentCategory = webCategories.Where(c => c.ItemCategoryID == model.Item.CategoryID).FirstOrDefault();
            var parentCategoryKey = "";
            if (parentCategory != null)
            {
                parentCategoryKey = parentCategory.ItemCategoryViewName;
            }
            var subCategory = webCategories.Where(c => c.ItemCategoryID == model.Item.CategoryID).FirstOrDefault();
            var subCategoryKey = "";
            if (subCategory != null)
            {
                subCategoryKey = subCategory.ItemCategoryViewName;
            }

            if (!parentCategoryKey.IsEmpty()) model.parentCategoryKey = parentCategoryKey;
            if (!subCategoryKey.IsEmpty()) model.subCategoryKey = subCategoryKey;


            model.OrderConfiguration = OrderConfiguration;
            model.CurrentCategory = new ItemCategory();

            model.CategoryID = OrderConfiguration.CategoryID;

            if (parentCategoryKey.IsEmpty() && subCategoryKey.IsEmpty())
            {
                model.CategoryID = OrderConfiguration.FeaturedCategoryID;
                model.CurrentCategory = webCategories.Where(c => c.ItemCategoryID == model.CategoryID).FirstOrDefault();
            }

            if (!parentCategoryKey.IsEmpty())
            {
                model.CurrentCategory = webCategories.Where(c => c.ItemCategoryViewName.Equals(parentCategoryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                model.CategoryID = model.CurrentCategory.ItemCategoryID;
            }

            if (!subCategoryKey.IsEmpty())
            {
                var subcategory = model.CurrentCategory.Subcategories.FirstOrDefault(s => s.ItemCategoryViewName.Equals(subCategoryKey, StringComparison.InvariantCultureIgnoreCase));
                if (subcategory != null)
                {
                    model.CategoryID = subcategory.ItemCategoryID;
                }
            }

            model.Categories = webCategories;

            if (!parentCategoryKey.IsEmpty())
            {
                var mainCategories = model.Categories.Where(c => c.ItemCategoryID == model.CategoryID);

                if (mainCategories.Count() > 0)
                {
                    model.CurrentCategory = mainCategories.FirstOrDefault();
                }
                else
                {
                    foreach (var cat in model.Categories)
                    {
                        if (cat.Subcategories != null && cat.Subcategories.Any(s => s.ItemCategoryID == model.CategoryID))
                        {
                            model.CurrentCategory = cat.Subcategories.FirstOrDefault(s => s.ItemCategoryID == model.CategoryID);
                            model.CurrentCategory.ParentItemCategoryDescription = cat.ItemCategoryDescription;
                            model.CurrentCategory.ParentItemCategoryViewName = cat.ItemCategoryViewName;
                            model.CurrentCategory.ParentItemCategoryID = cat.ItemCategoryID;
                        }
                    }
                }
            }
            #endregion

            // Logic for First Order Packs, we use this in the view to determine if we already have the First Order Item in the cart - Mike M.
            ViewBag.IsFirstOrderPack = itemcode == FirstOrderPackItemCode;
            ViewBag.HasFirstOrderPack = ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0;
            ViewBag.CanPurchaseFirstOrderPack = Identity.Customer != null && GlobalUtilities.CanPurchaseFirstOrderPack(Identity.Customer.CustomerID, FirstOrderPackItemCode);


            return View(model);
        }

        [Route("partnerstores")]
        public ActionResult PartnerStores()
        {
            return View();

            //var model = ShoppingViewModelFactory.Create<ItemListViewModel>(PropertyBag);

            //model.OrderConfiguration = OrderConfiguration;
            //var webCategories = Exigo.GetItemCategories(OrderConfiguration.CategoryID);
            //model.CurrentCategory = new ItemCategory();
            //model.Categories = webCategories;

            //return View(model);
        }

        /// <summary>
        /// 2016-10-12 80760 DV. This is a client requested "secret" test page for banners not yet ready for live viewing.  It is nothing more than a clone of [Route("partnerstores")]
        /// </summary>
        /// <returns></returns>
        [PreLaunchHide(AllowPreLaunch = true)]
        [Route("bv5partnerpages")]
        public ActionResult BV5PartnerPages()
        {
            return View();
        }

        [Route("affiliatestores")]
        public ActionResult AffiliateStores()
        {
            return View();

            //var model = ShoppingViewModelFactory.Create<ItemListViewModel>(PropertyBag);

            ////model.OrderConfiguration = OrderConfiguration;
            ////var webCategories = Exigo.GetItemCategories(OrderConfiguration.CategoryID);
            ////model.CurrentCategory = new ItemCategory();
            ////model.Categories = webCategories;

            //return View(model);
        }

        [Route("cart")]
        public ActionResult Cart()
        {
            // Check to make sure that a Customer has not bought the First Order Pack yet, if it is currently in the shopping cart. 
            // If they have and it is in the current cart, we need to remove it and let the user know why - Mike M.
            ViewBag.FirstOrderPackItemCode = FirstOrderPackItemCode;


            if (ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0)
            {
                var customerID = (Identity.Customer != null) ? Identity.Customer.CustomerID : 0;
                var canPurchaseFirstOrderPack = GlobalUtilities.CanPurchaseFirstOrderPack(customerID, FirstOrderPackItemCode);

                // At this point, if we find they can't purchase this again, we remove the item from the cart and add something to the ViewBag so we can let the user know what happened.
                if (canPurchaseFirstOrderPack == false)
                {
                    ShoppingCart.Items.Remove(FirstOrderPackItemCode);
                    Exigo.PropertyBags.Update(ShoppingCart);

                    ViewBag.FirstOrderItemRemoved = true;
                }
            }

            var model = this.GetCartViewModel();

            return View(model);
        }

        /// <summary>
        /// Get our cart view model for the Cart view and AJAX calls to modify cart items
        /// </summary>
        /// <returns>CartViewModel</returns>
        public CartViewModel GetCartViewModel()
        {
            var model = ShoppingViewModelFactory.Create<CartViewModel>(PropertyBag);

            // Get the cart items
            var cartItems = ShoppingCart.Items.ToList();
            var tasks = new List<Task>();

            var _orderItems = cartItems.Where(i => i.Type == ShoppingCartItemType.Order);
            var _autoorderItems = cartItems.Where(i => i.Type == ShoppingCartItemType.AutoOrder);
            var languageID = GlobalUtilities.GetSelectedExigoLanguageID();

            // Ensure auto order items have matching order items in the cart
            if (ShoppingCart.HasAutoOrderItems())
            {
                var needsUpdate = ShoppingCart.Items.EnsureMatchingItems();

                if (needsUpdate)
                {
                    Exigo.PropertyBags.Update(ShoppingCart);
                }
            }

            var list = new List<Item>();
            var priceTypeID = OrderPriceTypeID;

            if (_orderItems.Any())
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var orderItems = Exigo.GetItems(_orderItems, OrderConfiguration, languageID, priceTypeID).ToList();
                    list.AddRange(orderItems);
                }));
            }
            if (_autoorderItems.Any())
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var autoOrderItems = Exigo.GetItems(_autoorderItems, AutoOrderConfiguration, languageID).ToList();
                    list.AddRange(autoOrderItems);
                }));

                if (Identity.Customer != null)
                {
                    var autoorders = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
                    {
                        CustomerID = Identity.Customer.CustomerID,
                        IncludeDetails = false,
                        IncludePaymentMethods = false
                    });

                    if (autoorders.Any(a => a.AutoOrderStatusID == (int)AutoOrderStatusType.Active))
                    {
                        PropertyBag.HasActiveAutoOrder = true;
                        Exigo.PropertyBags.Update(PropertyBag);

                        model.AutoOrderCount = autoorders.Where(a => a.AutoOrderStatusID == (int)AutoOrderStatusType.Active).Count();
                    }
                }
            }
            else
            {
                PropertyBag.HasActiveAutoOrder = false;
            }


            model.HasActiveAutoOrder = PropertyBag.HasActiveAutoOrder;

            Task.WaitAll(tasks.ToArray());

            model.Items = list;

            return model;
        }
        #endregion

        #region Shipping
        [Route("checkout/shipping")]
        public ActionResult Shipping()
        {
            var model = ShoppingViewModelFactory.Create<ShippingAddressesViewModel>(PropertyBag);
            model.SaveAddress = true;

            if (Identity.Customer != null)
            {
                model.Addresses = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID)
                    .Where(c => c.IsComplete)
                    .Select(c => c as ShippingAddress)
                    .Where(c => c.IsNotPoBox);
            }
            return View(model);
        }

        [HttpPost]
        [Route("checkout/shipping")]
        public ActionResult Shipping(ShippingAddress address, bool saveAddress = false)
        {
            PropertyBag.ShippingAddress = address;

            // Autoorder logic
            if (ShoppingCart.HasAutoOrderItems())
            {
                PropertyBag.AutoOrderShippingAddress = address;
            }

            // Save the address
            if (saveAddress)
            {
                Exigo.SetCustomerAddressOnFile(Identity.Customer.CustomerID, address);
            }

            Exigo.PropertyBags.Update(PropertyBag);

            return LogicProvider.GetNextAction();
        }

        // OLD VERSION
        //[HttpPost]
        //[Route("checkout/shipping")]
        //public ActionResult Shipping(ShippingAddress address)
        //{
        //    // Validate the address
        //    var response = Exigo.VerifyAddress(address as Address);

        //    if (response.IsValid)
        //    {
        //        address.Address1 = response.VerifiedAddress.Address1;
        //        address.Address2 = response.VerifiedAddress.Address2;
        //        address.City = response.VerifiedAddress.City;
        //        address.State = response.VerifiedAddress.State;
        //        address.Zip = response.VerifiedAddress.Zip;
        //        address.Country = response.VerifiedAddress.Country;

        //        // Save the address to the customer's account if applicable
        //        if (Request.IsAuthenticated && address.AddressType == AddressType.New)
        //        {
        //            Exigo.SetCustomerAddressOnFile(Identity.Customer.CustomerID, address as Address);
        //        }

        //        PropertyBag.ShippingAddress = address;
        //        Exigo.PropertyBags.Update(PropertyBag);

        //        return LogicProvider.GetNextAction();
        //    }
        //    else
        //    {
        //        return RedirectToAction("Shipping", new { error = "Unable to verify address" });
        //    }
        //}
        #endregion

        #region AutoOrders
        [Route("checkout/autoorder")]
        public ActionResult AutoOrder()
        {
            // Ensure we have a valid frequency type
            if (!GlobalSettings.AutoOrders.AvailableFrequencyTypes.Contains(PropertyBag.AutoOrderFrequencyType))
            {
                PropertyBag.AutoOrderFrequencyType = GlobalSettings.AutoOrders.AvailableFrequencyTypes.FirstOrDefault();
            }

            // Ensure we have a valid start date based on the frequency
            if (PropertyBag.AutoOrderStartDate == DateTime.MinValue)
            {
                PropertyBag.AutoOrderStartDate = GlobalUtilities.GetAutoOrderStartDate(FrequencyType.Monthly);
            }

            Exigo.PropertyBags.Update(PropertyBag);

            return RedirectToAction("checkout");
        }

        // Old Version not being used - Mike M.
        //[Route("checkout/autoorder")]
        //public ActionResult AutoOrder()
        //{
        //    var model = ShoppingViewModelFactory.Create<AutoOrderSettingsViewModel>(PropertyBag);

        //    // Ensure we have a valid frequency type
        //    if (!GlobalSettings.AutoOrders.AvailableFrequencyTypes.Contains(PropertyBag.AutoOrderFrequencyType))
        //    {
        //        PropertyBag.AutoOrderFrequencyType = GlobalSettings.AutoOrders.AvailableFrequencyTypes.FirstOrDefault();
        //    }

        //    // Ensure we have a valid start date based on the frequency
        //    if (PropertyBag.AutoOrderStartDate == DateTime.MinValue)
        //    {
        //        PropertyBag.AutoOrderStartDate = GlobalUtilities.GetAutoOrderStartDate(FrequencyType.Monthly);
        //    }

        //    Exigo.PropertyBags.Update(PropertyBag);

        //    // Set our model
        //    model.AutoOrderStartDate = PropertyBag.AutoOrderStartDate;
        //    model.AutoOrderFrequencyType = PropertyBag.AutoOrderFrequencyType;

        //    return View(model);
        //}

        [HttpPost]
        [Route("checkout/autoorder")]
        public ActionResult AutoOrder(DateTime autoOrderStartDate, FrequencyType autoOrderFrequencyType)
        {
            PropertyBag.AutoOrderStartDate = autoOrderStartDate;
            PropertyBag.AutoOrderFrequencyType = autoOrderFrequencyType;
            Exigo.PropertyBags.Update(PropertyBag);

            return LogicProvider.GetNextAction();
        }
        #endregion

        #region Payments
        [Route("checkout/payment")]
        public ActionResult Payment()
        {
            var model = ShoppingViewModelFactory.Create<PaymentMethodsViewModel>(PropertyBag);

            if (Identity.Customer != null)
            {
                model.PaymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                {
                    CustomerID = Identity.Customer.CustomerID,
                    ExcludeIncompleteMethods = true,
                    ExcludeInvalidMethods = true
                });

                model.Addresses = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID)
                    .Where(c => c.IsComplete)
                    .Select(c => c as ShippingAddress);
            }

            if (ShoppingCart.Items.Any(c => c.Type == ShoppingCartItemType.AutoOrder))
            {
                model.HasAutoOrderItems = true;
            }

            model.PaymentMethods.ToList();
            Exigo.PropertyBags.Update(PropertyBag);

            return View("Payment", model);
        }

        [Route("checkout/autoorderpayment")]
        public ActionResult AutoOrderPayment(CreditCard newCard, bool billingSameAsShipping = false)
        {
            var model = ShoppingViewModelFactory.Create<PaymentMethodsViewModel>(PropertyBag);

            model.IsAutoOrder = true;

            if (Identity.Customer != null)
            {
                model.PaymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                {
                    CustomerID = Identity.Customer.CustomerID,
                    ExcludeIncompleteMethods = true,
                    ExcludeInvalidMethods = true
                });

                model.Addresses = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID)
                    .Where(c => c.IsComplete)
                    .Select(c => c as ShippingAddress);
            }

            Exigo.PropertyBags.Update(PropertyBag);
            return View("Payment", model);
        }

        [HttpPost]
        [Route("UseCreditCardOnFile")]
        public ActionResult UseCreditCardOnFile(CreditCard card, CreditCardType type, bool UsePaymentForAutoOrders = false)
        {

            var paymentMethod = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                ExcludeIncompleteMethods = true,
                ExcludeInvalidMethods = true
            }).Where(c => c is CreditCard && ((CreditCard)c).Type == type).FirstOrDefault();

            var selectedCard = paymentMethod.As<CreditCard>();
            selectedCard.CVV = card.CVV;
            Exigo.PropertyBags.Update(PropertyBag);

            return UsePaymentMethod(paymentMethod, UsePaymentForAutoOrders);

        }

        [HttpPost]
        [Route("UseBankAccountOnFile")]
        public ActionResult UseBankAccountOnFile(ExigoService.BankAccountType type, bool UsePaymentForAutoOrders = false)
        {
            var paymentMethod = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                ExcludeIncompleteMethods = true,
                ExcludeInvalidMethods = true
            }).Where(c => c is BankAccount && ((BankAccount)c).Type == type).FirstOrDefault();

            return UsePaymentMethod(paymentMethod, UsePaymentForAutoOrders);
        }

        [HttpPost]
        [Route("UseCreditCard")]
        public ActionResult UseCreditCard(CreditCard newCard, bool billingSameAsShipping = false, bool UsePaymentForAutoOrders = true)
        {
            if (billingSameAsShipping)
            {
                var address = PropertyBag.ShippingAddress;

                newCard.BillingAddress = new Address
                {
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    City = address.City,
                    State = address.State,
                    Zip = address.Zip,
                    Country = address.Country
                };
            }

            Exigo.PropertyBags.Update(PropertyBag);


            // Verify that the card is valid
            if (!newCard.IsValid)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = "This card is invalid, please try again"
                });
            }
            else
            {
                // Save the credit card to the customer's account if applicable
                if (LogicProvider.IsAuthenticated())
                {
                    var paymentMethodsOnFile = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                    {
                        CustomerID = Identity.Customer.CustomerID,
                        ExcludeIncompleteMethods = true,
                        ExcludeInvalidMethods = true
                    }).Where(c => c is CreditCard).Select(c => c as CreditCard);

                    if (paymentMethodsOnFile.Where(c => c.Type == CreditCardType.Primary).FirstOrDefault() == null)
                    {
                        newCard.Type = CreditCardType.Primary;
                        Exigo.SetCustomerCreditCard(Identity.Customer.CustomerID, newCard);
                    }
                    else if (paymentMethodsOnFile.Where(c => c.Type == CreditCardType.Secondary).FirstOrDefault() == null)
                    {
                        newCard.Type = CreditCardType.Secondary;
                        Exigo.SetCustomerCreditCard(Identity.Customer.CustomerID, newCard);
                    }
                }


                return UsePaymentMethod(newCard, UsePaymentForAutoOrders);

            }
        }

        [HttpPost]
        [Route("UseBankAccount")]
        public ActionResult UseBankAccount(BankAccount newBankAccount, bool billingSameAsShipping = false, bool UsePaymentForAutoOrders = false)
        {
            if (billingSameAsShipping)
            {
                var address = PropertyBag.ShippingAddress;

                newBankAccount.BillingAddress = new Address
                {
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    City = address.City,
                    State = address.State,
                    Zip = address.Zip,
                    Country = address.Country
                };
            }


            // Verify that the card is valid
            if (!newBankAccount.IsValid)
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
            else
            {
                // Save the bank account to the customer's account if applicable
                if (LogicProvider.IsAuthenticated())
                {
                    var paymentMethodsOnFile = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                    {
                        CustomerID = Identity.Customer.CustomerID,
                        ExcludeIncompleteMethods = true,
                        ExcludeInvalidMethods = true,
                        ExcludeNonAutoOrderPaymentMethods = true
                    }).Where(c => c is BankAccount).Select(c => c as BankAccount);

                    if (paymentMethodsOnFile.FirstOrDefault() == null)
                    {
                        Exigo.SetCustomerBankAccount(Identity.Customer.CustomerID, newBankAccount);
                    }
                }


                return UsePaymentMethod(newBankAccount, UsePaymentForAutoOrders);
            }
        }

        [HttpPost]
        [Route("UsePaymentMethod")]
        public ActionResult UsePaymentMethod(IPaymentMethod paymentMethod, bool UsePaymentForAutoOrders = false)
        {
            var isAutoOrder = Request.Url.AbsoluteUri.Contains("autoorder");

            if (isAutoOrder)
            {
                PropertyBag.AutoOrderPaymentMethod = paymentMethod;
            }
            else
            {
                PropertyBag.PaymentMethod = paymentMethod;

                if (UsePaymentForAutoOrders)
                {
                    PropertyBag.AutoOrderPaymentMethod = paymentMethod;
                }
            }

            Exigo.PropertyBags.Update(PropertyBag);

            return new JsonNetResult(new
            {
                success = true
            });
        }
        #endregion

        #region Review/Checkout
        [Route("checkout/review")]
        public ActionResult Review()
        {
            var languageID = GlobalUtilities.GetSelectedExigoLanguageID();
            var logicResult = LogicProvider.CheckLogic();
            if (!logicResult.IsValid) return logicResult.NextAction;


            // If an auto order address/payment method has not been selected, then we set it to match the order's here first thing
            if (ShoppingCart.HasAutoOrderItems())
            {
                if (PropertyBag.AutoOrderShippingAddress == null || PropertyBag.AutoOrderPaymentMethod == null)
                {
                    PropertyBag.AutoOrderShippingAddress = PropertyBag.ShippingAddress;
                    PropertyBag.AutoOrderPaymentMethod = PropertyBag.PaymentMethod;
                    Exigo.PropertyBags.Update(PropertyBag);
                }
            }

            var model = ShoppingViewModelFactory.Create<OrderReviewViewModel>(PropertyBag);

            #region First Order Pack Logic
            // Check to make sure that a Customer has not bought the First Order Pack yet, if it is currently in the shopping cart. 
            // If they have and it is in the current cart, we need to remove it and let the user know why - Mike M.
            ViewBag.FirstOrderPackItemCode = FirstOrderPackItemCode;
            if (ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0)
            {
                var canPurchaseFirstOrderPack = GlobalUtilities.CanPurchaseFirstOrderPack(Identity.Customer.CustomerID, FirstOrderPackItemCode);
                // At this point, if we find they can't purchase this again, we remove the item from the cart and add something to the ViewBag so we can let the user know what happened.
                if (canPurchaseFirstOrderPack == false)
                {
                    if (ShoppingCart.Items.Count > 1)
                    {
                        ShoppingCart.Items.Remove(FirstOrderPackItemCode);
                        Exigo.PropertyBags.Update(ShoppingCart);
                        ViewBag.FirstOrderItemRemoved = true;
                    }
                    else
                    {
                        // If we are going to have a cleared out cart after removing the First Order Pack, we need to just redirect the user to the Cart and the removal will occur there.
                        return RedirectToAction("cart");
                    }
                }
                else
                {
                    ViewBag.CartHasFirstOrderItem = true;
                }
            }
            #endregion



            // Get the cart items
            var cartItems = ShoppingCart.Items.ToList();

            // Make sure we are using the appropriate price type depending on if the customer is a Smart Shopper or if there is auto order items in the cart - Mike M.
            OrderConfiguration.PriceTypeID = OrderPriceTypeID;

            // Calculate the order if applicable
            var orderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.Order).ToList();
            //var shippingDiscountID = "";

            if (orderItems.Count > 0)
            {            
                model.OrderItems = Exigo.GetItems(orderItems, OrderConfiguration, languageID).ToList();

                //TW 7/8/2016 77999 Adds all tier one items' BV and conditionally flags shipping discount id
                //var tierOneItems = model.OrderItems.Where(c =>c.Field2=="1");
                //if (tierOneItems.Sum(c => c.BV * c.Quantity) >= 75)
                //{
                //    shippingDiscountID = "1";
                //    PropertyBag.ShippingDiscountID = shippingDiscountID;                   
                //}

                model.OrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                    Configuration = OrderConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = true,
                    //Other15 = shippingDiscountID,
                    
                });
                model.ShipMethods = model.OrderTotals.ShipMethods;
            }       

            // Calculate the autoorder if applicable
            var autoOrderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.AutoOrder).ToList();
            if (autoOrderItems.Count > 0)
            {
                model.AutoOrderItems = Exigo.GetItems(autoOrderItems, OrderConfiguration, languageID, PriceTypes.Wholesale).ToList();
                model.AutoOrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                    Configuration = AutoOrderConfiguration,
                    Items = autoOrderItems,
                    Address = PropertyBag.AutoOrderShippingAddress,
                    ShipMethodID = PropertyBag.AutoOrderShipMethodID,
                    ReturnShipMethods = true
                });

                model.AutoOrderShipMethods = model.AutoOrderTotals.ShipMethods;
            }


            // Set the default ship method
            if (orderItems.Count != 0)
            {
                if (PropertyBag.ShipMethodID == 0)
                {
                    PropertyBag.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
                    Exigo.PropertyBags.Update(PropertyBag);
                }
                if (model.ShipMethods.Any(c => c.ShipMethodID == PropertyBag.ShipMethodID))
                {
                    model.ShipMethods.First(c => c.ShipMethodID == PropertyBag.ShipMethodID).Selected = true;
                }
                else if (model.ShipMethods.Count() <= 0)
                {
                    model.Errors = new string[1];
                    model.Errors[0] = "We are having trouble calculating shipping for this order. Please double-check your shipping address or contact support@bonvera.com for assistance";
                }
                else
                {
                    // If we don't have the ship method we're supposed to select, 
                    // check the first one, save the selection and recalculate
                    model.ShipMethods.First().Selected = true;

                    PropertyBag.ShipMethodID = model.ShipMethods.First().ShipMethodID;
                    Exigo.PropertyBags.Update(PropertyBag);

                    var newCalculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                        Configuration = OrderConfiguration,
                        Items = orderItems,
                        Address = PropertyBag.ShippingAddress,
                        ShipMethodID = PropertyBag.ShipMethodID,
                        ReturnShipMethods = false,
                        //Other15 = shippingDiscountID                    
                    });

                    model.OrderTotals = newCalculationResult;

                }
                PropertyBag.ShipMethodID = (model.ShipMethods.Count() > 0) ? model.ShipMethods.Where(c => c.Selected == true).Select(c => c.ShipMethodID).First() : OrderConfiguration.DefaultShipMethodID;
                Exigo.PropertyBags.Update(PropertyBag);

            }
            if (autoOrderItems.Count != 0)
            {
                if (PropertyBag.AutoOrderShipMethodID == 0)
                {
                    PropertyBag.AutoOrderShipMethodID = AutoOrderConfiguration.DefaultShipMethodID;
                    Exigo.PropertyBags.Update(PropertyBag);
                }
                if (model.AutoOrderShipMethods.Any(c => c.ShipMethodID == PropertyBag.AutoOrderShipMethodID))
                {
                    model.AutoOrderShipMethods.First(c => c.ShipMethodID == PropertyBag.AutoOrderShipMethodID).Selected = true;
                }
                else if (model.AutoOrderShipMethods.Count() <= 0)
                {
                    model.Errors = new string[1];
                    model.Errors[0] = "We are having trouble calculating shipping for this order. Please double-check your shipping address or contact support@bonvera.com for assistance";
                }
                else
                {
                    // If we don't have the ship method we're supposed to select, 
                    // check the first one, save the selection and recalculate
                    model.AutoOrderShipMethods.First().Selected = true;

                    PropertyBag.AutoOrderShipMethodID = model.AutoOrderShipMethods.First().ShipMethodID;
                    Exigo.PropertyBags.Update(PropertyBag);

                    var newCalculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                        Configuration = AutoOrderConfiguration,
                        Items = autoOrderItems,
                        Address = PropertyBag.AutoOrderShippingAddress,
                        ShipMethodID = PropertyBag.AutoOrderShipMethodID,
                        ReturnShipMethods = false
                    });

                    model.AutoOrderTotals = newCalculationResult;
                }
                model.AutoOrderFrequencyTypes = Common.GlobalUtilities.GetCurrentMarket().AvailableAutoOrderFrequencyTypes;
                PropertyBag.AutoOrderShipMethodID = (model.AutoOrderShipMethods.Count() > 0) ? model.AutoOrderShipMethods.Where(c => c.Selected == true).Select(c => c.ShipMethodID).First() : AutoOrderConfiguration.DefaultShipMethodID;
                Exigo.PropertyBags.Update(PropertyBag);
            }

            #region Will Call Logic
            // Will Call Ship Method Check
            model.WillCallShipMethodID = GlobalUtilities.GetCurrentMarket().WillCallShipMethodID;
            var hasWillCallAvailable = false;
            var userSelectedWillCall = false;

            if (model.OrderTotals.ShipMethods.Any(s => s.ShipMethodID == model.WillCallShipMethodID))
            {
                hasWillCallAvailable = true;

                if (PropertyBag.ShipMethodID == model.WillCallShipMethodID)
                {
                    userSelectedWillCall = true;
                }
            }

            if (ShoppingCart.HasAutoOrderItems() && model.AutoOrderTotals.ShipMethods.Any(s => s.ShipMethodID == model.WillCallShipMethodID))
            {
                hasWillCallAvailable = true;

                if (PropertyBag.AutoOrderShipMethodID == model.WillCallShipMethodID)
                {
                    userSelectedWillCall = true;
                }
            }

            ViewBag.HasWillCallAvailable = hasWillCallAvailable;
            ViewBag.UserSelectedWillCall = userSelectedWillCall;
            #endregion

            return View(model);
        }

        private OrderCalculationResponse GetCartTotals(ShoppingCartItemType ItemType)
        {
            var languageID = GlobalUtilities.GetSelectedExigoLanguageID();
            OrderConfiguration.PriceTypeID = OrderPriceTypeID;
            // Get the cart items
            var cartItems = ShoppingCart.Items.ToList();
            var Items = Exigo.GetItems(cartItems, OrderConfiguration, languageID).ToList();
            
            OrderCalculationResponse OrderTotals;

            // Calculate the order if applicable
            var orderItems = cartItems.Where(c => c.Type == ItemType).ToList();
            
            if (orderItems.Count > 0)
            {
                OrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                    Configuration = OrderConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = true,
                    //Other15 = PropertyBag.ShippingDiscountID,
                });
                return OrderTotals;
            }
            else
                return new OrderCalculationResponse();
        }

        [HttpPost]
        public ActionResult SubmitCheckout()
        {
            // Testing
            //return new JsonNetResult(new { success = false, message = "here" });


            // Set the price type appropriately for order creation
            OrderConfiguration.PriceTypeID = OrderPriceTypeID;

            // A variable that is set which determines when a user is attempting to purchase a First Order Pack when they have already purchased it. 
            // This will tell the javascript we need to redirect somewhere else after this occurs.
            var redirectUrl = "";

            #region First Order Pack Logic
            // Check to make sure that a Customer has not bought the First Order Pack yet, if it is currently in the shopping cart. 
            // If they have and it is in the current cart, we need to remove it and let the user know why - Mike M.
            if (ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0)
            {
                var canPurchaseFirstOrderPack = GlobalUtilities.CanPurchaseFirstOrderPack(Identity.Customer.CustomerID, FirstOrderPackItemCode);
                // At this point, if we find they can't purchase this again, we remove the item from the cart and add something to the ViewBag so we can let the user know what happened.
                if (canPurchaseFirstOrderPack == false)
                {
                    var urlHelper = new UrlHelper(Request.RequestContext);

                    // Here we check to ensure if the cart is going to be empty after we remove this item, we need to redirect the user to the cart page and let them know what went wrong. 
                    // By redirecting to the Cart page, we are not going to get errors for no items in the cart that would cause order calculation exceptions. - Mike M.
                    if (ShoppingCart.Items.Count() > 1)
                    {
                        redirectUrl = urlHelper.Action("review");
                    }
                    else
                    {
                        redirectUrl = urlHelper.Action("cart");
                    }

                    return new JsonNetResult(new
                    {
                        success = false,
                        redirectUrl
                    });
                }
            }
            #endregion

            try
            {
                // Set up our guest customer & testing variables
                var isGuestCheckout = Identity.Customer == null;

                var isLocal = Request.IsLocal;


                // Start creating the API requests
                var details = new List<ApiRequest>();


                // If we are dealing with a Retail Customer, then we need to make sure that if they are creating an Auto Order, their customer type is updated as well
                if (Identity.Customer.CustomerTypeID == CustomerTypes.RetailCustomer && ShoppingCart.HasAutoOrderItems())
                {
                    UpdateCustomerRequest request = new UpdateCustomerRequest();

                    request.CustomerID = Identity.Customer.CustomerID;
                    request.CustomerType = CustomerTypes.SmartShopper;

                    details.Add(request);
                }


                // Create the order request, if applicable
                var orderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order);
                var hasOrder = orderItems.Count() > 0;

                if (hasOrder)
                {
                    var orderRequest = new CreateOrderRequest(OrderConfiguration, PropertyBag.ShipMethodID, orderItems, PropertyBag.ShippingAddress);
                    //orderRequest.Other15 = PropertyBag.ShippingDiscountID;
                    //orderRequest.Other16 = PropertyBag.ShippingDiscountDifference;
                    if (!isGuestCheckout)
                    {
                        orderRequest.CustomerID = Identity.Customer.CustomerID;
                    }
                    details.Add(orderRequest);
                }


                // Create the autoorder request, if applicable
                var autoOrderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder);
                var hasAutoOrder = autoOrderItems.Count() > 0;

                if (hasAutoOrder)
                {
                    var autoOrderRequest = new CreateAutoOrderRequest(AutoOrderConfiguration, Exigo.GetAutoOrderPaymentType(PropertyBag.AutoOrderPaymentMethod), PropertyBag.AutoOrderStartDate, PropertyBag.AutoOrderShipMethodID, autoOrderItems, PropertyBag.AutoOrderShippingAddress);

                    if (!isGuestCheckout)
                    {
                        autoOrderRequest.CustomerID = Identity.Customer.CustomerID;
                    }
                    autoOrderRequest.Frequency = PropertyBag.AutoOrderFrequencyType;
                    details.Add(autoOrderRequest);
                }


                // Create the payment request
                if (PropertyBag.PaymentMethod is CreditCard)
                {
                    var card = PropertyBag.PaymentMethod as CreditCard;
                    if (card.Type == CreditCardType.New)
                    {
                        if (hasAutoOrder)
                        {
                            if (!isGuestCheckout)
                            {
                                if (!card.IsTestCreditCard)
                                {
                                    card = Exigo.SaveNewCustomerCreditCard(Identity.Customer.CustomerID, card);
                                }
                                ((CreateAutoOrderRequest)details.Where(c => c is CreateAutoOrderRequest).FirstOrDefault()).PaymentType = Exigo.GetAutoOrderPaymentType(card);
                            }
                            else
                            {
                                // Add logic if guest checkout is allowed : constructor for SetAccountCreditCardTokenRequest

                            }
                        }
                        if (hasOrder)
                        {
                            if (card.IsTestCreditCard)
                            {
                                // no need to charge card
                                ((CreateOrderRequest)details.Where(c => c is CreateOrderRequest).FirstOrDefault()).OrderStatus = OrderStatusType.Accepted;
                            }
                            else
                            {
                                if (!isLocal)
                                {
                                    details.Add(new ChargeCreditCardTokenRequest(card));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (hasOrder)
                        {
                            if (card.IsTestCreditCard)
                            {
                                // no need to charge card

                            }
                            else
                            {
                                if (!isLocal)
                                {
                                    var cctype = (card.Type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary;
                                    details.Add(new ChargeCreditCardTokenOnFileRequest { CreditCardAccountType = cctype });
                                }
                            }
                        }
                    }
                }



                // Process the transaction
                var transactionRequest = new TransactionalRequest();
                transactionRequest.TransactionRequests = details.ToArray();
                var transactionResponse = Exigo.WebService().ProcessTransaction(transactionRequest);


                var newOrderID = 1;
                var customerID = 0;
                if (transactionResponse.Result.Status == ResultStatus.Success)
                {
                    foreach (var response in transactionResponse.TransactionResponses)
                    {
                        if (response is CreateOrderResponse) newOrderID = ((CreateOrderResponse)response).OrderID;
                        if (response is CreateCustomerResponse) customerID = ((CreateCustomerResponse)response).CustomerID;
                    }
                }



                // Ensure if we are creating a new customer, they will get logged in
                // Will not apply if they are not required to enter their password
                customerID = Identity.Customer.CustomerID;

                var token = Security.Encrypt(newOrderID, customerID);

                return new JsonNetResult(new
                {
                    success = true,
                    token = token
                });
            }
            catch (Exception exception)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = exception.Message
                });
            }
        }

        [Route("thank-you")]
        public ActionResult OrderComplete()
        {


            var model = new OrderCompleteViewModel();
            model.WillCallShipMethodID = GlobalUtilities.GetCurrentMarket().WillCallShipMethodID;
            Exigo.PropertyBags.Delete(PropertyBag);
            Exigo.PropertyBags.Delete(ShoppingCart);

            var customerID = Identity.Customer.CustomerID;


            var orderID = Security.Decrypt(Request.QueryString["token"], customerID);

            model.OrderID = Convert.ToInt32(orderID);
            model.Order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest { CustomerID = customerID, IncludeOrderDetails = true, IncludePayments = true, OrderID = model.OrderID }).FirstOrDefault();

            return View(model);
        }

        [Route("checkout")]
        public ActionResult Checkout()
        {
            return LogicProvider.GetNextAction();
        }
        #endregion

        #region Ajax
        // Get Quantity of total items in cart, used on _MasterLayout and called when page loads
        [HttpPost]
        public ActionResult GetCartItemQuantity()
        {
            var count = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order && c.ParentItemCode == null).Sum(c => c.Quantity);

            return new JsonNetResult(new
            {
                success = true,
                count = count
            });

        }

        // Cart preview found in header of site. Called via pubsub : window.trigger("update.cartpreview");
        public ActionResult FetchCartPreview()
        {
            try
            {
                var languageID = GlobalUtilities.GetSelectedExigoLanguageID();
                var address = (PropertyBag.ShippingAddress != null && PropertyBag.ShippingAddress.IsComplete) ? PropertyBag.ShippingAddress : null;
                var calculateOrderResponse = new OrderCalculationResponse();
                var subtotal = "";
                var qty = 0;
                OrderConfiguration.PriceTypeID = OrderPriceTypeID;

                var items = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order), OrderConfiguration, languageID).ToList();

                qty = (int)items.Sum(i => i.Quantity);

                if (address != null)
                {
                    calculateOrderResponse = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                        Address = address,
                        Configuration = OrderConfiguration,
                        Items = ShoppingCart.Items,
                        ReturnShipMethods = false
                    });


                    subtotal = calculateOrderResponse.Subtotal.ToString("C");
                }
                else
                {
                    subtotal = items.Sum(i => i.Quantity * i.Price).ToString("C");
                }


                var cartHtml = this.RenderPartialViewToString("partials/cart/cartpreview", new CartViewModel
                {
                    Items = items,
                    PropertyBag = PropertyBag
                });

                return new JsonNetResult(new
                {
                    success = true,
                    total = subtotal,
                    cart = cartHtml,
                    quantity = qty
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

        // Quick Shop Modal that is triggered from Item List module
        public ActionResult QuickShopModal(string itemcode)
        {
            OrderConfiguration.PriceTypeID = OrderPriceTypeID;

            try
            {
                var model = ShoppingViewModelFactory.Create<ItemDetailViewModel>(PropertyBag);

                model.Item = Exigo.GetItemDetail(new GetItemDetailRequest
                {
                    Configuration = OrderConfiguration,
                    ItemCode = itemcode
                });

                if (model.Item != null)
                {
                    model.Item.Quantity = 1;
                }

                var customerID = Identity.Customer != null ? Identity.Customer.CustomerID : 0;

                // Logic for First Order Packs, we use this in the view to determine if we already have the First Order Item in the cart - Mike M.
                ViewBag.IsFirstOrderPack = itemcode == FirstOrderPackItemCode;
                ViewBag.HasFirstOrderPack = ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0;
                ViewBag.CanPurchaseFirstOrderPack = GlobalUtilities.CanPurchaseFirstOrderPack(customerID, FirstOrderPackItemCode);


                var html = this.RenderPartialViewToString("partials/items/quickshopmodal", model);

                return new JsonNetResult(new
                {
                    success = true,
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

        // Cart Modification Methods
        [HttpPost]
        [Route("AddItemToCart")]
        public ActionResult AddItemToCart(ShoppingCartItem item)
        {
            var model = ShoppingViewModelFactory.Create<ItemDetailViewModel>(PropertyBag);
            OrderConfiguration.PriceTypeID = OrderPriceTypeID;

            try
            {
                var languageID = GlobalUtilities.GetSelectedExigoLanguageID();
                // Conditionally calculate the totals - we don't need to calculate them every time
                OrderCalculationResponse totals = null;
                var address = (PropertyBag.ShippingAddress != null && PropertyBag.ShippingAddress.IsComplete) ? PropertyBag.ShippingAddress : null;
                var autoOrderAddress = (PropertyBag.AutoOrderShippingAddress != null && PropertyBag.AutoOrderShippingAddress.IsComplete) ? PropertyBag.AutoOrderShippingAddress : null;
                var subtotal = "";

                // First Order Pack variables
                var isFirstOrderPack = item.ItemCode == FirstOrderPackItemCode;
                var cartHasFirstOrderPack = ShoppingCart.Items.Where(c => c.ItemCode == FirstOrderPackItemCode).Count() > 0;


                // If this is an order item, we don't need to run any logic, just add the normal way
                if (item.Type == ShoppingCartItemType.Order)
                {
                    if (isFirstOrderPack)
                    {
                        // Ensure that the quantity of the First Order Pack is restricted to 1, if somehow the user hacks the input and increases the quantity - Mike M.
                        item.Quantity = 1;

                        if (cartHasFirstOrderPack)
                        {
                            throw new Exception("You already have a first order pack in your cart, you can only purchase one of these packs.");
                        }
                    }

                    ShoppingCart.Items.Add(item);
                    Exigo.PropertyBags.Update(ShoppingCart);

                    var items = Exigo.GetItems(ShoppingCart.Items, OrderConfiguration, languageID).ToList();

                    if (address == null)
                    {
                        subtotal = items.Where(i => i.Type == ShoppingCartItemType.Order).Sum(i => i.Quantity * i.Price).ToString("C");
                    }
                    else
                    {
                        totals = Exigo.CalculateOrder(new OrderCalculationRequest
                        {
                            CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                            Configuration = OrderConfiguration,
                            Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order),
                            Address = address,
                            ShipMethodID = PropertyBag.ShipMethodID
                        });

                        subtotal = totals.Subtotal.ToString("C");
                    }

                    var cartHtml = this.RenderPartialViewToString("partials/cart/cartpreview", new CartViewModel
                    {
                        Items = items,
                        PropertyBag = PropertyBag
                    });


                    return new JsonNetResult(new
                    {
                        success = true,
                        carttype = "order",
                        item = item,
                        items = ShoppingCart.Items,
                        totals = totals,
                        totalPrice = subtotal,
                        cart = cartHtml,
                        isFirstOrderPack
                    });

                }
                // When adding product to the auto order cart, first we need to check and see if there are any auto orders that are currently active
                else
                {
                    // Add the auto order item to the cart, but then we need to run some logic on to pull their active auto orders, if they have a current active auto order
                    ShoppingCart.Items.Add(item);

                    // Add this item to the Order shopping cart since it is required to purchase this item today
                    if (ShoppingCart.HasAutoOrderItems())
                    {
                        var needsUpdate = ShoppingCart.Items.EnsureMatchingItems();

                        // Run some additional logic here if needed, this determines if a change in the cart occurred
                        if (needsUpdate)
                        {
                        }
                    }

                    Exigo.PropertyBags.Update(ShoppingCart);

                    if (Identity.Customer != null)
                    {
                        var autoorders = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
                        {
                            CustomerID = Identity.Customer.CustomerID,
                            IncludeInactiveAutoOrders = false,
                            IncludeCancelledAutoOrders = false
                        });

                        // If we find that the user is adding an item to auto order and they have auto orders on file, then we need to show them the 
                        // choice modal to allow them the option to save to an existing auto order.
                        if (autoorders.Count() > 0)
                        {
                            return GetAutoOrderChoiceModal();
                        }
                    }

                    var items = Exigo.GetItems(ShoppingCart.Items, OrderConfiguration, languageID).ToList();

                    if (address == null)
                    {
                        subtotal = items.Where(i => i.Type == ShoppingCartItemType.AutoOrder).Sum(i => i.Quantity * i.Price).ToString("C");
                    }
                    else
                    {
                        totals = Exigo.CalculateOrder(new OrderCalculationRequest
                        {
                            CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                            Configuration = AutoOrderConfiguration,
                            Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder),
                            Address = autoOrderAddress,
                            ShipMethodID = PropertyBag.AutoOrderShipMethodID
                        });

                        subtotal = totals.Subtotal.ToString("C");
                    }

                    var cartHtml = this.RenderPartialViewToString("partials/cart/cartpreview", new CartViewModel
                    {
                        Items = items,
                        PropertyBag = PropertyBag
                    });

                    return new JsonNetResult(new
                    {
                        success = true,
                        carttype = "order",
                        item = item,
                        items = ShoppingCart.Items,
                        totals = totals,
                        totalPrice = subtotal,
                        cart = cartHtml,
                        isFirstOrderPack
                    });
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

        /// <summary>
        /// Method used on the Cart view to update cart quantities. This Action also returns html for the cart view.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateItemQuantity(Guid id, decimal quantity)
        {
            var hasCartChanged = false;
            OrderConfiguration.PriceTypeID = OrderPriceTypeID;

            var item = ShoppingCart.Items.Where(c => c.ID == id).FirstOrDefault();

            // If somehow the user got to the update method and the item is the First Order Pack item, we need to make sure that they can't up the quantity past 1. 
            var isFirstOrderPack = item.ItemCode == FirstOrderPackItemCode;
            if (isFirstOrderPack && quantity > 1)
            {
                quantity = 1;
            }

            ShoppingCart.Items.Update(id, quantity);

            // Check and see if we have a matching auto order item and if the quantity decresed from the today's order item but not the auto order item, we need to also adjust the auto order item's quantity - Mike M.
            if (item.Type == ShoppingCartItemType.Order)
            {
                var autoOrderItem = ShoppingCart.Items.Where(c => c.ItemCode == item.ItemCode && c.Type == ShoppingCartItemType.AutoOrder).FirstOrDefault();

                if (autoOrderItem != null && autoOrderItem.Quantity > item.Quantity)
                {
                    hasCartChanged = true;
                    autoOrderItem.Quantity = item.Quantity;

                    ShoppingCart.Items.Update(autoOrderItem.ID, autoOrderItem.Quantity);
                }
            }

            // Validate that if auto order items have changed quantity, the Order items should match
            var cartRequiresChange = ShoppingCart.Items.EnsureMatchingItems();
            if (cartRequiresChange)
            {
                hasCartChanged = true;
            }

            Exigo.PropertyBags.Update(ShoppingCart);


            // Conditionally calculate the totals - we don't need to calculate them every time
            OrderCalculationResponse totals = null;
            // Logic to only use GetItems and not Calculate Order on the cart page - Mike M.
            totals = GetCartTotals_NoCalculation(item.Type);
            // OLD CODE FOR ABOVE
            #region totals calculations
            //switch (item.Type)
            //{
            //    case ShoppingCartItemType.Order:
            //        totals = Exigo.CalculateOrder(new OrderCalculationRequest
            //        {
            //            Configuration = OrderConfiguration,
            //            Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order),
            //            Address = PropertyBag.ShippingAddress,
            //            ShipMethodID = PropertyBag.ShipMethodID
            //        });
            //        break;

            //    case ShoppingCartItemType.AutoOrder:
            //        totals = Exigo.CalculateOrder(new OrderCalculationRequest
            //        {
            //            Configuration = AutoOrderConfiguration,
            //            Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder),
            //            Address = PropertyBag.ShippingAddress,
            //            ShipMethodID = PropertyBag.ShipMethodID
            //        });
            //        break;
            //}
            #endregion

            // Get the cart view model and render the view so we have the updated cart html
            var model = this.GetCartViewModel();

            ViewBag.FirstOrderPackItemCode = FirstOrderPackItemCode;

            var carthtml = this.RenderPartialViewToString("_Cart", model);

            if (Request.IsAjaxRequest())
            {
                return new JsonNetResult(new
                {
                    success = true,
                    item = item,
                    items = ShoppingCart.Items,
                    totals = totals,
                    carthtml = carthtml,
                    message = (hasCartChanged) ? "We have made an adjustment to your Replenish cart due to the last change. You are required to purchase the items today that you would like to have on your Replenish." : ""
                });
            }
            else
            {
                return RedirectToAction("Cart");
            }
        }

        public ActionResult RemoveItemFromCart(Guid id)
        {
            var hasCartChanged = false;
            var item = ShoppingCart.Items.Where(c => c.ID == id).FirstOrDefault();

            ShoppingCart.Items.Remove(id);

            // Here we need to run some logic to ensure that if an auto order item matching this item exists, that we remove that as well and let the user know something changed beyond their control. - Mike M.
            if (item.Type == ShoppingCartItemType.Order)
            {
                var autoOrderItem = ShoppingCart.Items.Where(c => c.ItemCode == item.ItemCode && c.Type == ShoppingCartItemType.AutoOrder).FirstOrDefault();

                if (autoOrderItem != null)
                {
                    ShoppingCart.Items.Remove(autoOrderItem.ID);
                    hasCartChanged = true;
                }
            }

            Exigo.PropertyBags.Update(ShoppingCart);

            // Conditionally calculate the totals - we don't need to calculate them every time
            OrderCalculationResponse totals = null;
            // Logic to only use GetItems and not Calculate Order on the cart page - Mike M.
            totals = GetCartTotals_NoCalculation(item.Type);
            // OLD CODE FOR ABOVE
            #region totals calculations
            //switch (item.Type)
            //{
            //    case ShoppingCartItemType.Order:
            //        totals = Exigo.CalculateOrder(new OrderCalculationRequest
            //        {
            //            Configuration = OrderConfiguration,
            //            Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order),
            //            Address = PropertyBag.ShippingAddress,
            //            ShipMethodID = PropertyBag.ShipMethodID
            //        });
            //        break;

            //    case ShoppingCartItemType.AutoOrder:
            //        totals = Exigo.CalculateOrder(new OrderCalculationRequest
            //        {
            //            Configuration = AutoOrderConfiguration,
            //            Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder),
            //            Address = PropertyBag.ShippingAddress,
            //            ShipMethodID = PropertyBag.ShipMethodID
            //        });
            //        break;
            //}
            #endregion

            // Get the cart view model and render the view so we have the updated cart html
            var model = this.GetCartViewModel();

            ViewBag.FirstOrderPackItemCode = FirstOrderPackItemCode;

            var carthtml = this.RenderPartialViewToString("_Cart", model);


            if (Request.IsAjaxRequest())
            {
                return new JsonNetResult(new
                {
                    success = true,
                    item = item,
                    items = ShoppingCart.Items,
                    totals = totals,
                    carthtml = carthtml,
                    message = (hasCartChanged) ? "An item has been removed from your Replenish cart because you removed this item from Today's Order. You must purchase the items you wish to put on Replenish today as well in order to create your Replenish." : ""
                });
            }
            else
            {
                return RedirectToAction("ItemList");
            }
        }

        // This just simply gets the cart totals, using GetItems, that are used for the Update and Delete functions on the Cart view - Mike M.
        public OrderCalculationResponse GetCartTotals_NoCalculation(ShoppingCartItemType type)
        {
            OrderCalculationResponse totals = null;
            var languageID = GlobalUtilities.GetSelectedExigoLanguageID();
            var _items = new List<ShoppingCartItem>();

            switch (type)
            {
                case ShoppingCartItemType.Order:
                    _items = ShoppingCart.Items.OrderItems;
                    break;

                case ShoppingCartItemType.AutoOrder:
                    _items = ShoppingCart.Items.AutoOrderItems;
                    break;
            }

            var items = Exigo.GetItems(_items, OrderConfiguration, languageID, OrderPriceTypeID);
            totals = new OrderCalculationResponse { Subtotal = items.Sum(c => c.Quantity * c.Price) };

            return totals;
        }

        [HttpPost]
        public ActionResult SetShipMethodID(int shipMethodID)
        {
            PropertyBag.ShipMethodID = shipMethodID;
            Exigo.PropertyBags.Update(PropertyBag);

            return RedirectToAction("Review");
        }
        [HttpPost]
        public ActionResult SetAutoOrderShipMethodID(int shipMethodID)
        {
            PropertyBag.AutoOrderShipMethodID = shipMethodID;
            Exigo.PropertyBags.Update(PropertyBag);

            return RedirectToAction("Review");
        }
        
        

        [HttpPost]
        public JsonNetResult CalculateOrder(ShippingAddress shippingAddress, List<ShoppingCartItem> items, int shipMethodID = 0)
        {
            var configuration = (Identity.Customer != null) ? Identity.Customer.Market.Configuration.Orders : Identity.Owner.Market.Configuration.Orders;

            var response = Exigo.CalculateOrder(new OrderCalculationRequest
            {
                CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                Address = shippingAddress,
                Configuration = configuration,
                Items = items,
                ShipMethodID = shipMethodID,
                ReturnShipMethods = true
            });

            return new JsonNetResult(response);
        }


        /// <summary>
        /// This method is used with the Smart Shopper Register to set a value on the cart property bag signifying they are a potential Smart Shopper 
        /// and they should see a specific message on the Login page when they go to check out - Mike M.
        /// </summary>
        /// <returns></returns>
        public JsonNetResult FlagAsSmartShopperCheckout()
        {
            try
            {
                ShoppingCart.IsSmartShopperCheckout = true;
                Exigo.PropertyBags.Update(ShoppingCart);

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

        #region Auto Order Modal
        public JsonNetResult GetAutoOrderModal(int autoshipID = 0)
        {
            try
            {
                // First, we need to handle the generic call to get the auto order modal, if no autoshipID is provided
                if (autoshipID == 0)
                {
                    return GetAutoOrderChoiceModal();
                }

                var html = this.RenderPartialViewToString("partials/autoorder/autoorderpopupmodal", autoshipID);

                return new JsonNetResult(new
                {
                    carttype = "autoorder",
                    success = true,
                    html = html
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    carttype = "autoorder",
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("GetAutoOrderDetails")]
        public JsonNetResult GetAutoOrderDetails(int autoshipID)
        {
            var isExisting = false;
            try
            {
                var referrerUrl = Request.UrlReferrer.ToString();
                var isAutoOrderManager = referrerUrl.ToLower().Contains("replenishments");

                var model = new AutoOrderCartReviewViewModel();

                var autoorders = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
                {
                    CustomerID = Identity.Customer.CustomerID,
                    IncludeDetails = true,
                    IncludeCancelledAutoOrders = false
                });

                // If we have an existing auto order, we then load it into our model
                if (autoorders.Any())
                {
                    isExisting = true;
                    model.ActiveAutoOrder = autoorders.Where(a => a.AutoOrderID == autoshipID).FirstOrDefault();
                }

                if (ShoppingCart.Items.Any(i => i.Type == ShoppingCartItemType.AutoOrder))
                {
                    var autoorderCartItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.AutoOrder).ToList();

                    if (isAutoOrderManager)
                    {
                        autoorderCartItems = new List<ShoppingCartItem>();
                    }

                    if (isExisting)
                    {
                        var activeAutoOrderDetails = model.ActiveAutoOrder.Details.Where(d => d.PriceTotal > 0).Select(d => new ShoppingCartItem
                        {
                            ItemCode = d.ItemCode,
                            Type = ShoppingCartItemType.AutoOrder,
                            Quantity = d.Quantity
                        });

                        foreach (var item in activeAutoOrderDetails)
                        {
                            // Check to see if we are adding new items from the active auto ship or if we are just updating quantities to display to the user
                            var cartItem = autoorderCartItems.Where(i => i.ItemCode == item.ItemCode);
                            if (cartItem.Count() > 0)
                            {
                                var newQuantity = cartItem.FirstOrDefault().Quantity + item.Quantity;
                                autoorderCartItems.Where(c => c.ItemCode == item.ItemCode).First().Quantity = newQuantity;
                            }
                            else
                            {
                                autoorderCartItems.Add(item);
                            }
                        }

                    }

                    var itemcodes = autoorderCartItems.Select(i => i.ItemCode).ToArray();
                    var autoorderItems = Exigo.GetItems(new ExigoService.GetItemsRequest { Configuration = OrderConfiguration, ItemCodes = itemcodes, PriceTypeID = PriceTypes.Wholesale });
                    if (autoorderItems != null && autoorderItems.Count() > 0)
                    {
                        var rawItems = autoorderItems.ToList();
                        rawItems.ForEach(i =>
                        {
                            var cartItem = autoorderCartItems.Where(a => a.ItemCode == i.ItemCode).FirstOrDefault();
                            var activeQuantity = 0m;


                            i.Quantity = cartItem.Quantity + activeQuantity;
                            i.ID = cartItem.ID;
                        });
                        model.AutoOrderCartItems = rawItems;

                        // Calculate Auto Order
                        var address = model.ActiveAutoOrder.Recipient;
                        var shipMethodID = model.ActiveAutoOrder.ShipMethodID;

                        if (address != null)
                        {
                            var orderCalcResponse = Exigo.CalculateOrder(new OrderCalculationRequest
                            {
                                CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                                Address = address,
                                ShipMethodID = shipMethodID,
                                Configuration = AutoOrderConfiguration,
                                Items = autoorderCartItems
                            });

                            model.CalculatedAutoOrder = orderCalcResponse;
                        }

                    }
                }
                else
                {
                    // If we are just loading an auto order, we need to get the order totals as well
                    if (isExisting)
                    {
                        var activeAutoOrderDetails = model.ActiveAutoOrder.Details.Where(d => d.PriceTotal > 0).Select(d => new ShoppingCartItem
                        {
                            ItemCode = d.ItemCode,
                            Type = ShoppingCartItemType.AutoOrder,
                            Quantity = d.Quantity
                        });

                        var itemcodes = activeAutoOrderDetails.Select(i => i.ItemCode).ToArray();

                        var rawItems = Exigo.GetItems(new ExigoService.GetItemsRequest { Configuration = AutoOrderConfiguration, ItemCodes = itemcodes }).ToList();
                        rawItems.ForEach(i =>
                        {
                            var cartItem = activeAutoOrderDetails.Where(a => a.ItemCode == i.ItemCode).FirstOrDefault();
                            i.Quantity = cartItem.Quantity;
                            i.ID = cartItem.ID;
                        });
                        model.AutoOrderCartItems = rawItems;


                        // Calculate Auto Order
                        var address = model.ActiveAutoOrder.Recipient;
                        var shipMethodID = model.ActiveAutoOrder.ShipMethodID;
                        var orderCalcResponse = Exigo.CalculateOrder(new OrderCalculationRequest
                        {
                            CustomerID = Utilities.GetCustomerID(), //20161129 #82854 DV. For ReplicatedSite there could be guest portions of the shopping experience.  This modification explicitly handles when a customer is logged in or not
                            Address = address,
                            ShipMethodID = shipMethodID,
                            Configuration = AutoOrderConfiguration,
                            Items = model.AutoOrderCartItems
                        });

                        model.CalculatedAutoOrder = orderCalcResponse;
                    }
                }


                var html = this.RenderPartialViewToString("partials/autoorder/activeautoorder", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = html,
                    isExistingAutoOrder = isExisting
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

        public JsonNetResult RefreshCart()
        {
            try
            {
                // Get the cart view model and render the view so we have the updated cart html
                var model = this.GetCartViewModel();
                var carthtml = this.RenderPartialViewToString("_Cart", model);

                return new JsonNetResult(new
                {
                    success = true,
                    carthtml = carthtml
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

        // Call to pull in the Autoship choice screen to let the user choose to add to a new autoship or update an existing autoship
        public JsonNetResult GetAutoOrderChoiceModal()
        {
            try
            {
                var html = this.RenderPartialViewToString("partials/autoorder/autoorderchoicemodal", null);

                return new JsonNetResult(new
                {
                    carttype = "autoorder",
                    success = true,
                    html = html
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    carttype = "autoorder",
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("GetAutoOrderChoiceDetails")]
        public JsonNetResult GetAutoOrderChoiceDetails()
        {
            try
            {
                var model = new AutoOrderChoiceViewModel();

                var autoorders = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
                {
                    CustomerID = Identity.Customer.CustomerID,
                    IncludeDetails = true,
                    IncludeCancelledAutoOrders = false
                });

                // If we have existing auto orders, we then load it into our model
                if (autoorders.Any())
                {
                    model.AutoOrders.AddRange(autoorders);
                }

                if (ShoppingCart.Items.Any(i => i.Type == ShoppingCartItemType.AutoOrder))
                {
                    var autoorderCartItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.AutoOrder).ToList();

                    var items = Exigo.GetItems(autoorderCartItems, AutoOrderConfiguration, GlobalUtilities.GetSelectedExigoLanguageID());

                    model.AutoOrderItems.AddRange(items);
                }

                var html = this.RenderPartialViewToString("partials/autoorder/_autoorderchoicecontent", model);

                return new JsonNetResult(new
                {
                    success = true,
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



        [HttpPost]
        [Route("UpdateAutoOrder")]
        public JsonNetResult UpdateAutoOrder(List<ShoppingCartItem> items, int autoorderID = 0)
        {
            try
            {
                var existingAutoOrder = Exigo.OData().AutoOrders.Expand("Details")
                .Where(c => c.CustomerID == Identity.Customer.CustomerID)
                .Where(c => c.AutoOrderID == autoorderID)
                .FirstOrDefault();

                // Re-create the autoorder
                var request = new CreateAutoOrderRequest(existingAutoOrder);

                var details = new List<OrderDetailRequest>();

                foreach (var item in items.Where(i => i.Quantity > 0))
                {
                    details.Add(new OrderDetailRequest
                    {
                        ItemCode = item.ItemCode,
                        Quantity = item.Quantity,
                        Type = ShoppingCartItemType.AutoOrder
                    });
                }
                request.Details = details.ToArray();

                var response = Exigo.WebService().CreateAutoOrder(request);

                if (response.Result.Status == ResultStatus.Success)
                {
                    // Get original list of item codes
                    var originalItemCodeList = existingAutoOrder.Details.Select(d => d.ItemCode).ToList();

                    // Loop through the new itemcodes and remove the corresponding items from the cart                    
                    foreach (var item in items)
                    {
                        if (!originalItemCodeList.Contains(item.ItemCode))
                        {
                            var shoppingCartItem = ShoppingCart.Items.Where(c => c.ItemCode == item.ItemCode && c.Type == ShoppingCartItemType.Order).FirstOrDefault();
                            if (shoppingCartItem != null)
                            {
                                ShoppingCart.Items.Remove(shoppingCartItem);
                            }
                        }
                    }

                    // Clear out the auto order cart items since we have updated our auto order at this point
                    ShoppingCart.Items.Remove(ShoppingCartItemType.AutoOrder);
                    PropertyBag.HasActiveAutoOrder = false;
                    Exigo.PropertyBags.Update(PropertyBag);
                    Exigo.PropertyBags.Update(ShoppingCart);
                }

                return GetAutoOrderDetails(autoorderID);
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

        [Route("RemoveAutoOrdersItems")]
        public JsonNetResult RemoveAutoOrdersItems()
        {
            if (ShoppingCart.Items.Any(s => s.Type == ShoppingCartItemType.AutoOrder))
            {
                ShoppingCart.Items.Remove(ShoppingCartItemType.AutoOrder);
                Exigo.PropertyBags.Update(ShoppingCart);
            }

            return new JsonNetResult(new
            {
                success = true
            });
        }

        [Route("CancelAutoOrderUpdate")]
        public JsonNetResult CancelAutoOrderUpdate()
        {
            ShoppingCart.Items.Remove(ShoppingCartItemType.AutoOrder);
            Exigo.PropertyBags.Update(ShoppingCart);

            return new JsonNetResult(new
            {
                success = true
            });
        }
        #endregion
    }
}