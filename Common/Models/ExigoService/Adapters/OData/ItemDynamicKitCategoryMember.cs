
using System.Linq;

namespace Common.Api.ExigoOData
{
    public partial class ItemDynamicKitCategoryMember
    {
        public static explicit operator ExigoService.DynamicKitCategory(ItemDynamicKitCategoryMember member)
        {
            var model = new ExigoService.DynamicKitCategory();
            if (member == null) return model;

            model.DynamicKitCategoryID = member.DynamicKitCategoryID;
            model.Quantity             = member.Quantity;

            if (member.DynamicKitCategory != null)
            {
                model.DynamicKitCategoryDescription = member.DynamicKitCategory.DynamicKitCategoryDescription;

                if (member.DynamicKitCategory.DynamicKitCategoryItemMembers != null)
                {
                    model.Items = member.DynamicKitCategory.DynamicKitCategoryItemMembers.Select(c => (ExigoService.DynamicKitCategoryItem)c).ToList();
                }
            }

            return model;
        }
    }
}