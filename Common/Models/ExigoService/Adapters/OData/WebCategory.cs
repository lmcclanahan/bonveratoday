namespace Common.Api.ExigoOData
{
    public partial class WebCategory
    {
        public static explicit operator ExigoService.ItemCategory(WebCategory category)
        {
            var model = new ExigoService.ItemCategory();
            if (category == null) return model;

            model.ItemCategoryID          = category.WebCategoryID;
            model.ItemCategoryDescription = category.WebCategoryDescription;
            model.SortOrder               = category.SortOrder;
            model.ParentItemCategoryID    = category.ParentID;

            return model;
        }
    }
}