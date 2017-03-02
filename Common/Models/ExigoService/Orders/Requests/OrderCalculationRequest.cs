﻿using System.Collections.Generic;

namespace ExigoService
{
    public class OrderCalculationRequest
    {
        public int? CustomerID { get; set; }
        public IOrderConfiguration Configuration { get; set; }
        public IEnumerable<IShoppingCartItem> Items { get; set; }
        public IAddress Address { get; set; }
        public int ShipMethodID { get; set; }
        public bool ReturnShipMethods { get; set; }
        public decimal? TaxRateOverride { get; set; }
        public Dictionary<string, decimal> ItemPriceOverrides { get; set; }
        public int? PartyID { get; set; }
        //T.W 7/5/2016 77999 grabs shipping discount field for reduced BV
        public string Other15 { get; set; }

    }
}