using Backoffice.Filters;
using Common;
using Common.Services;
using ExigoService;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Backoffice.Controllers
{
        [BackofficeAuthorize(RequiresLogin = true, ValidateSubscription = false)]
        [PreLaunchHide(AllowPreLaunch = false)]

    public class OrdersController : Controller
    {
        public int RowCount = 10;

        //I.M. 11/29/2016 #80967 Created a route for orderlist...DatePicker Method 
        //[Route("~/orders/{page:int:min(1)=1}")]
        //public ActionResult DatePicker(int page)
        //{
        //    SetCommonViewBagData();

        //    var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
        //    {
        //        CustomerID = Identity.Current.CustomerID,
        //        RowCount = RowCount,
        //        Page = page,
        //        IncludeOrderDetails = true

        //    }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

        //    return View("OrderList", model);
        //}
        
        //20161229 80967 DV. Added per client request to display orders containing items with fees and services
        [Route("~/orders/feesandservices/{page:int:min(1)=1}")]
        public ActionResult FeesOrdersList(int page, DateTime? StartDate, DateTime? EndDate, int? OrderID)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderID = OrderID, //20161228 80967 DV. 
                StartDate = StartDate, //20161222 80967 DV. Client requested ability to work with date range.  
                EndDate = EndDate,     //20161222 80967 DV. Client requested ability to work with date range.
                IncludeOrderDetails = true,
                ShowOnlyFeesAndServicesOrders = true //20161222 80967 DV. Use this to only show orders that contain ItemID 58 or 627
            }); //.Where(c => !c.Other11.IsNullOrEmpty()).ToList(); //20121222 80967 DV. Per client remove this filter entirely.  If we bring it back move it inside the GetCustomerOrders method since it was not working correctly anyway

            return View("OrderList", model);
        }

        [Route("~/orders/{page:int:min(1)=1}")]
        public ActionResult OrderList(int page, DateTime? StartDate, DateTime? EndDate, int? OrderID)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderID = OrderID, //20161228 80967 DV. 
                StartDate = StartDate, //20161211 82887 DV. Client requested ability to work with date range.  
                EndDate = EndDate,     //20161211 82887 DV. Client requested ability to work with date range.
                IncludeOrderDetails = true
            }); //.Where(c => c.Other11.IsNullOrEmpty()).ToList(); //20161216 80967 DV. Per client remove this filter entirely.  If we bring it back move it inside the GetCustomerOrders method 

            return View("OrderList", model);
        }
        [Route("~/orders/partnerstore/{page:int:min(1)=1}")]
        public ActionResult PartnerStoreOrdersList(int page, DateTime? StartDate, DateTime? EndDate, int? OrderID)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderID = OrderID, //20161228 80967 DV. 
                StartDate = StartDate, //20161222 80967 DV. Client requested ability to work with date range.  
                EndDate = EndDate,     //20161222 80967 DV. Client requested ability to work with date range.
                IncludeOrderDetails = true,
                ShowOnlyPartnerAffiliateOrders = true //20161222 80967 DV. Use this to only show orders that contain ItemID 58 or 627
            }); //.Where(c => !c.Other11.IsNullOrEmpty()).ToList(); //20121222 80967 DV. Per client remove this filter entirely.  If we bring it back move it inside the GetCustomerOrders method since it was not working correctly anyway
            ViewBag.isPartnerStoreOrders = true; //20161226 80967 DV. Do not confuse this flag with the ShowOnlyPartnerAffiliateOrders flag.  This flag affects the HTML on the view while ShowOnlyPartnerAffiliateOrders is a filter
            return View("OrderList", model);
        }
        [Route("~/orders/cancelled/{page:int:min(1)=1}")]
        public ActionResult CancelledOrdersList(int page)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderStatuses = new int[] { OrderStatuses.Cancelled },
                IncludeOrderDetails = true
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("~/orders/open/{page:int:min(1)=1}")]
        public ActionResult OpenOrdersList(int page)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderStatuses = new int[] { 
                    OrderStatuses.Incomplete, 
                    OrderStatuses.Pending, 
                    OrderStatuses.CCDeclined, 
                    OrderStatuses.ACHDeclined, 
                    OrderStatuses.CCPending, 
                    OrderStatuses.ACHPending, 
                    OrderStatuses.PendingInventory,
                    OrderStatuses.Accepted
                },
                IncludeOrderDetails = true
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("~/orders/shipped/{page:int:min(1)=1}")]
        public ActionResult ShippedOrdersList(int page)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderStatuses = new int[] { OrderStatuses.Shipped },
                IncludeOrderDetails = true
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("~/orders/declined/{page:int:min(1)=1}")]
        public ActionResult DeclinedOrdersList(int page)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderStatuses = new int[] { 
                    OrderStatuses.Incomplete, 
                    OrderStatuses.CCDeclined, 
                    OrderStatuses.ACHDeclined 
                },
                IncludeOrderDetails = true
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("~/orders/returns/{page:int:min(1)=1}")]
        public ActionResult ReturnedOrdersList(int page)
        {
            SetCommonViewBagData();

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                Page = page,
                RowCount = RowCount,
                OrderTypes = new int[] { 
                    OrderTypes.ReturnOrder
                },

                IncludeOrderDetails = true
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("~/orders/search/{id:int}")]
        public ActionResult SearchOrdersList(int id)
        {
            SetCommonViewBagData();
            ViewBag.IsSearch = true;

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                OrderID = id,
                IncludeOrderDetails = true
            }).Where(c => c.Other11.IsNullOrEmpty()).ToList();

            return View("OrderList", model);
        }

        [Route("~/order/cancel")]
        public ActionResult CancelOrder(string token)
        {
            SetCommonViewBagData();
            var orderID = Convert.ToInt32(Security.Decrypt(token, Identity.Current.CustomerID));

            Exigo.CancelOrder(orderID);

            return Redirect(Request.UrlReferrer.ToString());
        }

        [Route("~/order")]
        public ActionResult OrderDetail(string token)
        {
            SetCommonViewBagData();

            var orderID = Convert.ToInt32(Security.Decrypt(token, Identity.Current.CustomerID));
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                OrderID = orderID,
                IncludeOrderDetails = true
            }).ToList();

            return View("OrderList", model);
        }

        [Route("~/invoice")]
        public ActionResult OrderInvoice(string token)
        {
            var orderID = Convert.ToInt32(Security.Decrypt(token, Identity.Current.CustomerID));

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Current.CustomerID,
                OrderID = orderID,
                IncludeOrderDetails = true,
                IncludePayments = true
            }).FirstOrDefault();

            return View("OrderInvoice", model);
        }


        // Helpers
        private void SetCommonViewBagData()
        {
            ViewBag.RowCount = RowCount;
        }
    }
}