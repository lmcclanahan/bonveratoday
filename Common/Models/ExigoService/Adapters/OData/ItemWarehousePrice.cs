
namespace Common.Api.ExigoOData
{
    public partial class ItemWarehousePrice
    {
        public static explicit operator ExigoService.Item(ItemWarehousePrice item)
        {
            var model = (item.Item != null) ? (ExigoService.Item)item.Item : new ExigoService.Item();
            if (item == null) return model;

            model.ItemID           = item.ItemID;
            model.Price            = item.Price;
            model.CurrencyCode     = item.CurrencyCode;
            model.Price            = item.Price;
            model.BV               = item.BusinessVolume;
            model.CV               = item.CommissionableVolume;
            model.OtherPrice1      = item.Other1Price;
            model.OtherPrice2      = item.Other2Price;
            model.OtherPrice3      = item.Other3Price;
            model.OtherPrice4      = item.Other4Price;
            model.OtherPrice5      = item.Other5Price;
            model.OtherPrice6      = item.Other6Price;
            model.OtherPrice7      = item.Other7Price;
            model.OtherPrice8      = item.Other8Price;
            model.OtherPrice9      = item.Other9Price;
            model.OtherPrice10     = item.Other10Price;

            return model;
        }
    }
}