
namespace Common.Api.ExigoOData
{
    public partial class ItemDynamicKitCategoryItemMember
    {
        public static explicit operator ExigoService.DynamicKitCategoryItem(ItemDynamicKitCategoryItemMember member)
        {
            var model = new ExigoService.DynamicKitCategoryItem();
            if (member == null) return model;

            model.ItemID = member.ItemID;

            if(member.Item != null)
            {
                model.ItemCode        = member.Item.ItemCode;
                model.ItemDescription = member.Item.ItemDescription;
                model.TinyImageUrl    = member.Item.TinyImageUrl;
                model.SmallImageUrl   = member.Item.SmallImageUrl;
                model.LargeImageUrl   = member.Item.LargeImageUrl;
            }

            if(member.DynamicKitCategory != null)
            {
                model.DynamicKitCategory = member.DynamicKitCategory.DynamicKitCategoryDescription;
            }

            return model;
        }
    }
}