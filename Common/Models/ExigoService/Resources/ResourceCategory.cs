namespace ExigoService
{
    public class ResourceCategory : IResourceCategory
    {
        public int ResourceCategoryID { get; set; }
        public string ResourceCategoryDescription { get; set; }
        public int? ResourceCategoryOrder { get; set; }
    }
}
