using Backoffice.ViewModels.AutoOrders;
using Common.Api.ExigoWebService;
using ExigoService;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Dapper;
using System.Threading.Tasks;
using Common;
using Backoffice.Filters;


namespace Backoffice.Controllers
{
    [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = false)]
    [PreLaunchHide(AllowPreLaunch = false)]
    public class AutoOrdersController : Controller
    {
        [Route("~/replenishments")]
        public ActionResult AutoOrderPreferences()
        {
            var context = Exigo.OData();

            var model = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                IncludeDetails = true,
                IncludePaymentMethods = true,
                IncludeInactiveAutoOrders = true
            }).Where(v => !v.Details.Any(d => d.ItemCode == "IAANNUALRENEWAL"));

           
            return View(model);
        }

        public AutoOrderResponse GetAutoOrder(int autoorderid)
        {
            var context = Exigo.WebService();

            var autoOrder = context.GetAutoOrders(new GetAutoOrdersRequest
            {
                AutoOrderID = autoorderid,
                CustomerID = Identity.Current.CustomerID
            });

            return autoOrder.AutoOrders[0];
        }

        public ActionResult UpdateAutoOrderDate(AutoOrderDateViewModel dateVM)
        {
            if (dateVM.NextDate > dateVM.CreatedDate)
            {
                try
                {
                    var autoorderid = dateVM.AutoorderID;
                    var frequencyType = FrequencyType.Monthly;

                    Exigo.UpdateCustomerAutoOrderRunDate(Identity.Current.CustomerID, autoorderid, dateVM.NextDate, frequencyType);


                    var model = Exigo.GetCustomerAutoOrder(Identity.Current.CustomerID, autoorderid);
                    var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                    return new JsonNetResult(new
                    {
                        success = true,
                        html = partial,
                        autoorderid = autoorderid
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
            else
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = "Please Select A Valid Date"
                });
            }
        }

        public ActionResult UpdateAutoOrderShippingAddress(ShippingAddress recipient)
        {
            if (!recipient.IsComplete)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = "Please Enter A CompleteAddress"
                    //message = Resources.Account.PleaseEnterACompleteAddress
                });
            }

            try
            {
                var autoorderid = Convert.ToInt32(Request.Form["autoorderid"]);

                Exigo.UpdateCustomerAutoOrderShippingAddress(Identity.Current.CustomerID, autoorderid, recipient);

                // Get Partial to update the AutoOrder
                var model = Exigo.GetCustomerAutoOrder(Identity.Current.CustomerID, autoorderid);
                var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = partial,
                    autoorderid = autoorderid
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

        public ActionResult UpdateAutoOrderShipMethod(int shipMethodID)
        {
            try
            {
                var autoorderid = Convert.ToInt32(Request.Form["autoorderid"]);

                Exigo.UpdateCustomerAutoOrderShipMethod(Identity.Current.CustomerID, autoorderid, shipMethodID);

                // Get Partial to update the AutoOrder 
                var model = Exigo.GetCustomerAutoOrder(Identity.Current.CustomerID, autoorderid);
                var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = partial,
                    autoorderid = autoorderid
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
        public JsonNetResult SetAutoOrderPaymentMethodPreference(int autoorderid, AutoOrderPaymentType type)
        {
            try
            {
                Exigo.UpdateCustomerAutoOrderPaymentMethod(Identity.Current.CustomerID, autoorderid, type);

                var model = Exigo.GetCustomerAutoOrder(Identity.Current.CustomerID, autoorderid);
                var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = partial,
                    autoorderid = autoorderid
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
        public JsonNetResult UpdateAutoOrderItems(AutoOrderAddEditCartViewModel model)
        {
            try
            {
                Exigo.UpdateCustomerAutoOrderItems(Identity.Current.CustomerID, model.AutoOrder.AutoOrderID, model.ProductsList);

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

        [HttpPost]
        public ActionResult FetchAutoOrderModule(int autoorderid, string module)
        {
            try
            {
                switch (module)
                {
                    case ".auto-order-shipping":
                        return FetchAutoOrderShippingModule(autoorderid);
                    case ".auto-order-shipmethod":
                        return FetchAutoOrderShipMethodModule(autoorderid);
                    case ".auto-order-cart":
                        return FetchEditAutoOrderOrderModule(autoorderid);
                    case ".auto-order-payment":
                        return FetchAutoOrderEditPaymentMethodModule(autoorderid);
                    case ".auto-order-date":
                        return FetchAutoOrderEditDateModule(autoorderid);
                    default:
                        return FetchAutoOrderShippingModule(autoorderid);
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

        public ActionResult FetchEditAutoOrderOrderModule(int autoorderid)
        {
            var autoorder = Exigo.GetCustomerAutoOrder(Identity.Current.CustomerID, autoorderid);

            var configuration = Identity.Current.Market.Configuration.AutoOrders;
            var products = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = configuration,
                IncludeChildCategories = true
            }).ToList();

            var orderItems = autoorder.Details.ToList();
            var itemCodeList = orderItems.Select(c => c.ItemCode).ToList();

            products.Where(p => itemCodeList.Contains(p.ItemCode)).ToList().ForEach(p => products.Remove(p));


            // Populate our model with the products and the Auto Order
            var model = new AutoOrderAddEditCartViewModel();
            model.AutoOrder = autoorder;
            model.ProductsList = products;


            string html = RenderPartialViewToString("displaytemplates/autoordereditorder", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        public ActionResult FetchAutoOrderShippingModule(int autoorderid)
        {
            var autoorder = Exigo.GetCustomerAutoOrder(Identity.Current.CustomerID, autoorderid);

            string html = RenderPartialViewToString("displaytemplates/autoordershippingaddress", autoorder);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        public ActionResult FetchAutoOrderShipMethodModule(int autoorderid)
        {
            var autoorder = Exigo.OData().AutoOrders.Expand("Details")
                .Where(a => a.CustomerID == Identity.Current.CustomerID)
                .Where(a => a.AutoOrderID == autoorderid)
                .First();

            var model = new AutoOrderShipMethodViewModel();
            model.AutoorderID = autoorderid;

            var address = new Address()
            {
                Address1 = autoorder.Address1,
                Address2 = autoorder.Address2,
                City = autoorder.City,
                State = autoorder.State,
                Zip = autoorder.Zip,
                Country = autoorder.Country
            };

            var shipmethods = Exigo.CalculateOrder(new OrderCalculationRequest
            {
                CustomerID = Identity.Current.CustomerID, //20161129 #82854 DV. For BO there is not likely ever a reason to not have the customerID available to pass to the CalculateOrder method
                Address = address,
                Configuration = Identity.Current.Market.Configuration.AutoOrders,
                Items = autoorder.Details.Where(c => c.PriceTotal > 0).Select(c => new Item { ItemCode = c.ItemCode, Quantity = c.Quantity, Price = c.PriceEach }).ToList(),
                ReturnShipMethods = true, 
                ShipMethodID = autoorder.ShipMethodID
            }).ShipMethods;

            if (shipmethods.Count() > 0)
            {
                foreach (var shipmethod in shipmethods)
                {
                    if (autoorder.ShipMethodID == shipmethod.ShipMethodID)
                    {
                        shipmethod.Selected = true;
                    }
                }

                model.ShipMethods = shipmethods;
            }
            else
            {
                model.Error = "We are having trouble calculating shipping for this order. Please double-check your shipping address or contact support@bonvera.com for assistance";
            }

            string html = RenderPartialViewToString("displaytemplates/autoordershipmethod", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        public ActionResult FetchAutoOrderEditPaymentMethodModule(int autoorderid)
        {
            var model = new AutoOrderPaymentViewModel();

            var customerID = Identity.Current.CustomerID;
            var autoOrderTypeID = 0;

            // Get our payment type so we know which card to show as selected when the payment module loads up - Mike M.
            var task = Task.Factory.StartNew(() =>
            {
                autoOrderTypeID = Exigo.OData().AutoOrders.Where(c => c.CustomerID == customerID && c.AutoOrderID == autoorderid).FirstOrDefault().AutoOrderPaymentTypeID;
            });

            model.AutoorderID = autoorderid;
            model.PaymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = customerID,
                ExcludeIncompleteMethods = true,
                ExcludeInvalidMethods = true
            });

            Task.WaitAll(task);

            // Auto Order Payment Type: 1 Primary Card on File, 2 Secondary Card on File
            model.SelectedCardType = (autoOrderTypeID == 1) ? CreditCardType.Primary : CreditCardType.Secondary;

            string html = RenderPartialViewToString("displaytemplates/autoordereditpaymentmethod", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        public ActionResult FetchAutoOrderEditDateModule(int autoorderid)
        {
            var customerID = Identity.Current.CustomerID;
            var autoorder = Exigo.WebService().GetAutoOrders(new GetAutoOrdersRequest
            {
                AutoOrderID = autoorderid,
                CustomerID = customerID
            }).AutoOrders[0];

            DateTime createdDate = Exigo.OData().Customers.Where(c => c.CustomerID == customerID).FirstOrDefault().CreatedDate;

            var model = new AutoOrderDateViewModel();
            model.AutoorderID = autoorderid;
            model.Frequency = Exigo.GetFrequencyTypeID(autoorder.Frequency);
            model.NextDate = autoorder.NextRunDate;
            model.CreatedDate = createdDate;


            string html = RenderPartialViewToString("displaytemplates/autoordereditdate", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        public JsonNetResult GetAutoOrderModal(int orderid)
        {
            var model = new AutoOrderAddEditCartViewModel();

            var autoorder = (ExigoService.AutoOrder)Exigo.OData().AutoOrders.Expand("Details")
                .Where(a => a.CustomerID == Identity.Current.CustomerID)
                .Where(a => a.AutoOrderID == orderid)
                .First();

            model.AutoOrder = autoorder;

            var configuration = Identity.Current.Market.Configuration.BackOfficeAutoOrders;

            //Get the available products
            var products = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = configuration,
                IncludeChildCategories = true
            }).ToList();

            products = products
                .Where(c => c.AllowOnAutoOrder == true)
                .Where(c => c.Price != 0).ToList();

            foreach (var product in products)
            {
                var autoOrderDetail = autoorder.Details.Where(c => c.ItemCode == product.ItemCode).FirstOrDefault();

                product.Quantity = (autoOrderDetail != null) ? autoOrderDetail.Quantity : 0;
            }

            model.ProductsList = products;

            var html = this.RenderPartialViewToString("../AutoOrders/DisplayTemplates/AutoOrderEditOrder", model);

            return new JsonNetResult(new
            {
                html = html
            });
        }

        public JsonNetResult DeleteAutoOrder(int autoOrderID)
        {
            try
            {
                var customerID = Identity.Current.CustomerID;

                Exigo.DeleteCustomerAutoOrder(customerID, autoOrderID);

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

        public JsonNetResult ModifyAutoOrderStatus(int autoOrderID, string action)
        {
            try
            {
                var customerID = Identity.Current.CustomerID;
                AutoOrderStatusType status;
                string successMessage = "";

                switch (action)
                {
                    case "delete":
                        status = AutoOrderStatusType.Deleted;
                        successMessage = "Your Replenish has been deleted.";
                        break;
                    case "pause":
                        status = AutoOrderStatusType.Inactive;
                        successMessage = "Your Replenish has been paused.";
                        break;
                    case "activate":
                        status = AutoOrderStatusType.Active;
                        successMessage = "Your Replenish has been activated.";
                        break;
                    default:
                        return new JsonNetResult(new
                        {
                            success = false,
                            successMessage = successMessage
                        });
                }

                Exigo.ModifyCustomerAutoOrder(customerID, autoOrderID, status);

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;


            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

    }
}

