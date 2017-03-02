using System;

namespace ExigoService
{
    public class Resource : IResource
    {
        public int ResourceID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int ResourceTypeID { get; set; }
        public int ResourceCategoryID { get; set; }
        public int ResourceStatusID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ResourceActionTypeID { get; set; }
        public string UploadedFilePath { get; set; }
        public int? ResourceOrder { get; set; }

        public ResourceType ResourceType { get; set; }
        public ResourceCategory ResourceCategory { get; set; }
    }
}
