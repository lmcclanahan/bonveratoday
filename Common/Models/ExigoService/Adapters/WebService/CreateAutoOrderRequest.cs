using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Api.ExigoWebService
{
    public partial class CreateAutoOrderRequest
    {
        public CreateAutoOrderRequest() {}
        public CreateAutoOrderRequest(Common.Api.ExigoOData.AutoOrder autoOrder)
        {
            var model = new CreateAutoOrderRequest();
            if (autoOrder == null) return;

            CustomerID                           = autoOrder.CustomerID;
            ExistingAutoOrderID                  = autoOrder.AutoOrderID;
            OverwriteExistingAutoOrder           = true;

            Frequency                            = Exigo.GetFrequencyType(autoOrder.FrequencyTypeID);
            StartDate                            = autoOrder.StartDate;
            StopDate                             = autoOrder.StopDate;
            CurrencyCode                         = autoOrder.CurrencyCode;
            WarehouseID                          = autoOrder.WarehouseID;
            ShipMethodID                         = autoOrder.ShipMethodID;
            PriceType                            = PriceTypes.Wholesale;
            PaymentType                          = Exigo.GetAutoOrderPaymentType(autoOrder.AutoOrderPaymentTypeID);
            ProcessType                          = Exigo.GetAutoOrderProcessType(autoOrder.AutoOrderProcessTypeID);
            Details                              = autoOrder.Details.Select(c => new OrderDetailRequest()
            {
                ItemCode                         = c.ItemCode,
                Quantity                         = c.Quantity,
                ParentItemCode                   = c.ParentItemCode,
                BusinessVolumeEachOverride       = c.BusinessVolumeEachOverride,
                CommissionableVolumeEachOverride = c.CommissionableVolumeEachOverride,
                DescriptionOverride              = c.ItemDescription,
                PriceEachOverride                = c.PriceEachOverride,
                ShippingPriceEachOverride        = c.ShippingPriceEachOverride,
                TaxableEachOverride              = c.TaxableEachOverride
            }).ToArray();

            FirstName   = autoOrder.FirstName;
            MiddleName  = autoOrder.MiddleName;
            LastName    = autoOrder.LastName;
            Company     = autoOrder.Company;
            Address1    = autoOrder.Address1;
            Address2    = autoOrder.Address2;
            City        = autoOrder.City;
            State       = autoOrder.State;
            Zip         = autoOrder.Zip;
            Country     = autoOrder.Country;
            Email       = autoOrder.Email;
            Phone       = autoOrder.Phone;

            Notes       = autoOrder.Notes;
            Other11     = autoOrder.Other11;
            Other12     = autoOrder.Other12;
            Other13     = autoOrder.Other13;
            Other14     = autoOrder.Other14;
            Other15     = autoOrder.Other15;
            Other16     = autoOrder.Other16;
            Other17     = autoOrder.Other17;
            Other18     = autoOrder.Other18;
            Other19     = autoOrder.Other19;
            Other20     = autoOrder.Other20;
            Description = autoOrder.AutoOrderDescription;
        }
        public CreateAutoOrderRequest(IOrderConfiguration configuration, AutoOrderPaymentType paymentType, DateTime startDate, int shipMethodID, IEnumerable<IShoppingCartItem> items, ShippingAddress address)
        {
            WarehouseID  = configuration.WarehouseID;
            PriceType    = configuration.PriceTypeID;
            CurrencyCode = configuration.CurrencyCode;
            StartDate    = startDate;
            PaymentType  = paymentType;
            ProcessType  = AutoOrderProcessType.AlwaysProcess;
            ShipMethodID = shipMethodID;

            Details      = items.Select(c => (OrderDetailRequest)(c as ShoppingCartItem)).ToArray();

            FirstName    = address.FirstName;
            MiddleName   = address.MiddleName;
            LastName     = address.LastName;
            Email        = address.Email;
            Phone        = address.Phone;
            Address1     = address.Address1;
            Address2     = address.Address2;
            City         = address.City;
            State        = address.State;
            Zip          = address.Zip;
            Country      = address.Country;
        }
    }
}