using Common.Api.ExigoWebService;
using System.Collections.Generic;

namespace ExigoService
{
    public class OrderCalculationResponse
    {
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public OrderDetailResponse[] Details { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        //T.W 7/5/2016 77999 grabs shipping discount field for reduced BV
        public string Other16 { get; set; }
    }
}