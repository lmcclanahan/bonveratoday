using System;

namespace ExigoService
{
    public class GetCustomerOrdersRequest : DataRequest
    {
        public GetCustomerOrdersRequest()
            : base()
        {
            OrderStatuses = new int[0];
            OrderTypes = new int[0];
            ShowOnlyPartnerAffiliateOrders = false; //20161222 80967 DV. Use this to only include orders with ItemID 58 or 627
            ShowOnlyFeesAndServicesOrders = false;  //20161229 80967 DV. Use this to only include orders with  ItemID 45, 290, 289
        }

        public int CustomerID { get; set; }
        public int? OrderID { get; set; }
        public int[] OrderStatuses { get; set; }
        public int[] OrderTypes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }  //20161211 82887 DV. Client requested ability to work with date range.
        public bool IncludeOrderDetails { get; set; }
        public bool IncludePayments { get; set; }
        public bool ShowOnlyPartnerAffiliateOrders { get; set; } //20161222 80967 DV. Use this to only include orders with ItemID 58 or 627
        public bool ShowOnlyFeesAndServicesOrders { get; set; }  //20161229 80967 DV. Use this to only include orders with  ItemID 45, 290, 289
    }
}