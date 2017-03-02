using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static List<ResourceCategory> GetResourceCategories(GetResourceCategoriesRequest request)
        {
            var context = Exigo.ODataResources();
            var query = context.ResourceManagerCategories.AsQueryable();
            var model = new List<ResourceCategory>();

            // Apply the filters
            if (request.ResourceCategoryID != null)
            {
                query = query.Where(c => c.ResourceCategoryID == (int)request.ResourceCategoryID);
            }

            var categories = query.ToList();

            foreach (var category in categories)
            {
                var cat = new ResourceCategory();

                cat.ResourceCategoryDescription = category.ResourceCategoryDescription;
                cat.ResourceCategoryID = category.ResourceCategoryID;
                cat.ResourceCategoryOrder = category.ResourceCategoryOrder;
                model.Add(cat);
            }
            return model;
        }
        public static List<Resource> GetResources(GetResourcesRequest request)
        {
            var context = Exigo.ODataResources();
            var model = new List<Resource>();

            // Establish the base query
            var query = context.ResourceManagement.Expand("ResourceManagerCategory,ResourceType")
                .AsQueryable();

            // Apply any filters
            if (request.ResourceID != null)
            {
                query = query.Where(c => c.ResourceID == (int)request.ResourceID);
            }

            if (request.ResourceCategoryID != null)
            {
                query = query.Where(c => c.ResourceCategoryID == (int)request.ResourceCategoryID);
            }

            if (request.ResourceStatusID != null)
            {
                query = query.Where(c => c.ResourceStatusID == (int)request.ResourceStatusID);
            }

            if (request.ResourceTypeID != null)
            {
                query = query.Where(c => c.ResourceTypeID == (int)request.ResourceTypeID);
            }

            if (request.SearchFilter.IsNotNullOrEmpty())
            {
                query = query.Where(c => c.Title.Contains(request.SearchFilter) || c.Description.Contains(request.SearchFilter));
            }

            // Get the data
            var apiResources = query.ToList();

            foreach (var category in apiResources)
            {
                var cat = new Resource();

                cat.CreatedDate = category.CreatedDate;
                cat.Description = category.Description;
                cat.ResourceCategoryID = category.ResourceCategoryID;
                cat.ResourceID = category.ResourceID;
                cat.ResourceStatusID = category.ResourceStatusID;
                cat.ResourceTypeID = category.ResourceTypeID;
                cat.Title = category.Title;
                cat.UploadedFilePath = category.UploadedFilePath;
                cat.ResourceOrder = category.ResourceOrder;
                cat.Url = category.Url;

                model.Add(cat);
            }
            return model;
        }
    }
}