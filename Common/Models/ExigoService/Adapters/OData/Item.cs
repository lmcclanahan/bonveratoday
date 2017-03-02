
using System.Linq;

namespace Common.Api.ExigoOData
{
    public partial class Item
    {
        public static explicit operator ExigoService.Item(Item item)
        {
            var model = new ExigoService.Item();
            if (item == null) return model;

            model.ItemID                     = item.ItemID;
            model.ItemCode                   = item.ItemCode;
            model.ItemDescription            = (item.IsGroupMaster) ? GlobalUtilities.Coalesce(item.GroupDescription, item.ItemDescription) : item.ItemDescription;
            model.Weight                     = item.Weight;
            model.ItemTypeID                 = item.ItemTypeID;

            model.TinyImageUrl               = GlobalUtilities.GetProductImagePath(item.TinyImageUrl);
            model.SmallImageUrl              = GlobalUtilities.GetProductImagePath(item.SmallImageUrl);
            model.LargeImageUrl              = GlobalUtilities.GetProductImagePath(item.LargeImageUrl);

            model.ShortDetail1               = item.ShortDetail;
            model.ShortDetail2               = item.ShortDetail2;
            model.ShortDetail3               = item.ShortDetail3;
            model.ShortDetail4               = item.ShortDetail4;
            model.LongDetail1                = item.LongDetail;
            model.LongDetail2                = item.LongDetail2;
            model.LongDetail3                = item.LongDetail3;
            model.LongDetail4                = item.LongDetail4;

            model.IsVirtual                  = item.IsVirtual;
            model.AllowOnAutoOrder           = item.AllowOnAutoOrder;

            model.IsGroupMaster              = item.IsGroupMaster;
            model.IsDynamicKitMaster         = item.IsDynamicKitMaster;
            model.GroupMasterItemDescription = item.GroupDescription;
            model.GroupMembersDescription    = item.GroupMembersDescription;
            if (item.GroupMembers != null)
            {
                model.GroupMembers           = item.GroupMembers.Select(c => (ExigoService.ItemGroupMember)c).OrderBy(c => c.SortOrder).ToList();
            }

            model.Field1                     = item.Field1;
            model.Field2                     = item.Field2;
            model.Field3                     = item.Field3;
            model.Field4                     = item.Field4;
            model.Field5                     = item.Field5;
            model.Field6                     = item.Field6;
            model.Field7                     = item.Field7;
            model.Field8                     = item.Field8;
            model.Field9                     = item.Field9;
            model.Field10                    = item.Field10;

            model.OtherCheck1                = item.OtherCheck1;
            model.OtherCheck2                = item.OtherCheck2;
            model.OtherCheck3                = item.OtherCheck3;
            model.OtherCheck4                = item.OtherCheck4;
            model.OtherCheck5                = item.OtherCheck5;

            return model;
        }
    }
}