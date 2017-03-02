using ExigoService;
using Common.Api.ExigoOData.ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class ResourceListViewModel
    {        
        public IEnumerable<ResourceManager> ResourceList { get; set; }
        public IEnumerable<ResourceCategory> ResourceCategories { get; set; }
        public IEnumerable<Common.Api.ExigoOData.ResourceManager.ResourceType> ResourceTypes { get; set; }        

        public string CategoryDescription { get; set; }
        public string SelectedCategoryID { get; set; }

        //hidden properties
        public string EditCategoryID { get; set; }
        public string DeleteCategoryID { get; set; }

        public int ResourceID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string UploadedThumbnailPath { get; set; }
        public int? ResourceCategoryID { get; set; }
        public int? ResourceStatusID { get; set; }
        public int? ResourceTypeID { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}