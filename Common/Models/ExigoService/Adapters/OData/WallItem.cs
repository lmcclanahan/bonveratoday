namespace Common.Api.ExigoOData
{
    public partial class WallItem
    {
        public static explicit operator ExigoService.CustomerWallItem(WallItem item)
        {
            var model = new ExigoService.CustomerWallItem();
            if (item == null) return model;

            model.CustomerWallItemID = item.WallItemID;
            model.CustomerID         = item.CustomerID;
            model.EntryDate          = item.EntryDate;
            model.Text               = item.Text;

            model.Field1             = item.Field1;
            model.Field2             = item.Field2;
            model.Field3             = item.Field3;

            return model;
        }
    }
}