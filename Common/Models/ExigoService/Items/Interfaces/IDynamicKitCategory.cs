using System.Collections.Generic;

namespace ExigoService
{
    public interface IDynamicKitCategory
    {
        int DynamicKitCategoryID { get; set; }
        string DynamicKitCategoryDescription { get; set; }
        decimal Quantity { get; set; }

        IEnumerable<DynamicKitCategoryItem> Items { get; set; }
    }
}