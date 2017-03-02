using System.Collections.Generic;

namespace ExigoService
{
    public class ItemCategory : IItemCategory
    {
        public ItemCategory()
        {
            Subcategories = new List<ItemCategory>();
        }

        public int ItemCategoryID { get; set; }
        public string ItemCategoryDescription { get; set; }
        public string ItemCategoryViewName { get; set; }
        public int SortOrder { get; set; }

        public int? ParentItemCategoryID { get; set; }
        public string ParentItemCategoryDescription { get; set; }
        public string ParentItemCategoryViewName { get; set; }
        public List<ItemCategory> Subcategories { get; set; }
    }
}
