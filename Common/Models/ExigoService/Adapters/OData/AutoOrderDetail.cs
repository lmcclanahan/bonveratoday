namespace Common.Api.ExigoOData
{
    public partial class AutoOrderDetail
    {
        public static explicit operator ExigoService.AutoOrderDetail(AutoOrderDetail detail)
        {
            var model = new ExigoService.AutoOrderDetail();
            if (detail == null) return model;

            model.AutoOrderDetailID         = detail.AutoOrderDetailID;
            model.AutoOrderID               = detail.AutoOrderID;

            model.ItemID                    = detail.ItemID;
            model.ItemCode                  = detail.ItemCode;
            model.ItemDescription           = detail.ItemDescription;
            model.Quantity                  = detail.Quantity;
            model.PriceEach                 = detail.PriceEach;
            model.PriceTotal                = detail.PriceTotal;
            model.IsVirtual                 = false;

            model.BVEach                    = detail.BusinessVolumeEach;
            model.BV                        = detail.BusinessVolume;
            model.CVEach                    = detail.CommissionableVolumeEach;
            model.CV                        = detail.CommissionableVolume;

            model.PriceEachOverride         = detail.PriceEachOverride;
            model.TaxableEachOverride       = detail.TaxableEachOverride;
            model.ShippingPriceEachOverride = detail.ShippingPriceEachOverride;
            model.BVEachOverride            = detail.BusinessVolumeEachOverride;
            model.CVEachOverride            = detail.CommissionableVolumeEachOverride;

            return model;
        }
    }
}