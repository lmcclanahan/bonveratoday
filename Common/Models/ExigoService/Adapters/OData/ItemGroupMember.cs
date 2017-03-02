

namespace Common.Api.ExigoOData
{
    public partial class ItemGroupMember
    {
        public static explicit operator ExigoService.ItemGroupMember(ItemGroupMember member)
        {
            var model = new ExigoService.ItemGroupMember();
            if (member == null) return model;

            model.ItemCode          = member.ItemCode;
            model.MasterItemCode    = member.MasterItemCode;
            model.MemberDescription = member.MemberDescription;
            model.SortOrder         = member.SortOrder;

            if (member.Item != null)
            {
                model.Item = (ExigoService.Item)member.Item;
            }

            return model;
        }
    }
}