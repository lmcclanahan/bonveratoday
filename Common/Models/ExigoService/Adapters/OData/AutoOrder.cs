using System.Collections.Generic;

namespace Common.Api.ExigoOData
{
    public partial class AutoOrder
    {
        public static explicit operator ExigoService.AutoOrder(AutoOrder autoorder)
        {
            var model = new ExigoService.AutoOrder();
            if (autoorder == null) return model;

            model.AutoOrderID            = autoorder.AutoOrderID;
            model.CustomerID             = autoorder.CustomerID;

            model.CurrencyCode           = autoorder.CurrencyCode;
            model.WarehouseID            = autoorder.WarehouseID;
            model.ShipMethodID           = autoorder.ShipMethodID;
            model.AutoOrderStatusID      = autoorder.AutoOrderStatusID;
            model.FrequencyTypeID        = autoorder.FrequencyTypeID;
            model.AutoOrderPaymentTypeID = autoorder.AutoOrderPaymentTypeID;
            model.AutoOrderProcessTypeID = autoorder.AutoOrderProcessTypeID;
            model.Notes                  = autoorder.Notes;

            model.StartDate              = autoorder.StartDate;
            model.StopDate               = autoorder.StopDate;
            model.LastRunDate            = autoorder.LastRunDate;
            model.NextRunDate            = autoorder.NextRunDate;
            model.CancelledDate          = autoorder.CancelledDate;

            model.Recipient              = new ExigoService.ShippingAddress
            {
                FirstName                = autoorder.FirstName,
                MiddleName               = autoorder.MiddleName,
                LastName                 = autoorder.LastName,
                Company                  = autoorder.Company,
                AddressType              = ExigoService.AddressType.Other,
                Address1                 = autoorder.Address1,
                Address2                 = autoorder.Address2,
                City                     = autoorder.City,
                State                    = autoorder.State,
                Zip                      = autoorder.Zip,
                Country                  = autoorder.Country,
                Email                    = autoorder.Email,
                Phone                    = autoorder.Phone
            };

            if (autoorder.Details.Count > 0)
            {
                var details = new List<ExigoService.IAutoOrderDetail>();
                foreach (var detail in autoorder.Details)
                {
                    details.Add((ExigoService.AutoOrderDetail)detail);
                }
                model.Details = details;
            }

            model.Total                  = autoorder.Total;
            model.Subtotal               = autoorder.SubTotal;
            model.TaxTotal               = autoorder.TaxTotal;
            model.ShippingTotal          = autoorder.ShippingTotal;
            model.DiscountTotal          = autoorder.DiscountTotal;
            model.BVTotal                = autoorder.BusinessVolumeTotal;
            model.CVTotal                = autoorder.CommissionableVolumeTotal;

            model.Other11                = autoorder.Other11;
            model.Other12                = autoorder.Other12;
            model.Other13                = autoorder.Other13;
            model.Other14                = autoorder.Other14;
            model.Other15                = autoorder.Other15;
            model.Other16                = autoorder.Other16;
            model.Other17                = autoorder.Other17;
            model.Other18                = autoorder.Other18;
            model.Other19                = autoorder.Other19;
            model.Other20                = autoorder.Other20;

            model.CreatedDate            = autoorder.CreatedDate;
            model.CreatedBy              = autoorder.CreatedBy;

            return model;
        }
    }
}