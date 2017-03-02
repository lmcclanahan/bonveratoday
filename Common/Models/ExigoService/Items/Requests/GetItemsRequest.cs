namespace ExigoService
{
    public class GetItemsRequest
    {
        public GetItemsRequest()
        {
            this.ItemCodes = new string[0];
        }

        public IOrderConfiguration Configuration { get; set; }
        public int? CategoryID { get; set; }
        public string[] ItemCodes { get; set; }
        public bool IncludeChildCategories { get; set; }
        public int LanguageID { get; set; }
        public int PriceTypeID { get; set; }
    }
}