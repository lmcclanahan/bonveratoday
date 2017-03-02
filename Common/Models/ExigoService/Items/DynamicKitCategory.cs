using System.Collections.Generic;

namespace ExigoService
{
    public class DynamicKitCategory : IDynamicKitCategory
    {
        public int DynamicKitCategoryID { get; set; }
        public string DynamicKitCategoryDescription { get; set; }
        public decimal Quantity { get; set; }

        public IEnumerable<DynamicKitCategoryItem> Items { get; set; }
    }
}