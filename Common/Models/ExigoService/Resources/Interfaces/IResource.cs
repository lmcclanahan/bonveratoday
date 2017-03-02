using System;

namespace ExigoService
{
    public interface IResource
    {
        int ResourceID { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        string Url { get; set; }
        int ResourceTypeID { get; set; }
        int ResourceCategoryID { get; set; }
        int ResourceStatusID { get; set; }
        DateTime CreatedDate { get; set; }
        int ResourceActionTypeID { get; set; }
        string UploadedFilePath { get; set; }
    }
}
