namespace ExigoService
{
    public class GetResourcesRequest
    {
        public int? ResourceID { get; set; }
        public int? ResourceTypeID { get; set; }
        public int? ResourceStatusID { get; set; }
        public int? ResourceCategoryID { get; set; }
        public string SearchFilter { get; set; }
    }
}
