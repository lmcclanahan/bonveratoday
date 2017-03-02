using Backoffice.ViewModels;
using Common.Api.ExigoOData.ResourceManager;
using ExigoService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ResourceContext = Common.Api.ExigoOData.ResourceManager;
using Backoffice.Filters;

namespace Backoffice.Controllers
{
    [RoutePrefix("resources"), BackofficeSubscriptionRequired]
    public class ResourcesController : Controller
    {
        private const int InitialOrderValue = 1;

        #region Actions
        // TEMP UNTIL AFTER SOFT LAUNCH - Mike M. 2/12/2016
        public ActionResult Index()
        {
            return View();
        }

        //[Route("{categoryID:int?}")]
        //public ActionResult ResourceList(int? categoryID)
        //{
        //    var model = new ResourceListViewModel();

        //    if (categoryID != null)
        //    {
        //        model.ResourceList = Exigo.ODataResources().ResourceManagement.Where(c => c.ResourceCategoryID == categoryID).OrderBy(r=>r.ResourceOrder).ToList();
        //    }
        //    else
        //    {
        //        model.ResourceList = Exigo.ODataResources().ResourceManagement.OrderBy(r => r.ResourceCategoryID).ThenBy(r => r.ResourceOrder).ToList();
        //    }

        //    model.ResourceCategories = Exigo.GetResourceCategories(new GetResourceCategoriesRequest()).OrderBy(c=>c.ResourceCategoryOrder);

        //    return View(model);
        //}

        /// <summary>
        /// fetch data for ManageResources.chtml
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageResources()
        {
            //set up the model/service and fetch data
            var model = new ResourceListViewModel();
            model.ResourceList = Exigo.ODataResources().ResourceManagement.ToList().OrderBy(r => r.ResourceCategoryID).ThenBy(r => r.ResourceOrder);
            model.ResourceCategories = Exigo.GetResourceCategories(new GetResourceCategoriesRequest()).OrderBy(c => c.ResourceCategoryOrder);
            model.ResourceTypes = Exigo.ODataResources().ResourceTypes;

            return View(model);
        }

        /// <summary>
        /// create resource - ManageResources.chtml
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateResource(ResourceListViewModel res, HttpPostedFileBase UploadFile)
        {
            var context = Exigo.ODataResources();

            int CategoryID = 0;
            if (!string.IsNullOrEmpty(res.SelectedCategoryID))
            {
                CategoryID = Convert.ToInt32(res.SelectedCategoryID);
            }

            var resource = new ResourceContext.ResourceManager();

            resource.Description = res.Description;
            resource.CreatedDate = DateTime.Now;
            resource.ResourceCategoryID = CategoryID;

            //2015-09-10
            //Ivan S.
            //66
            //Ben wanted me to save the ResourceType, originally the Title was saving the type from a fixed list of File types
            //I now get the description from the resource types and save the right resource typeID
            resource.Title = context.ResourceTypes.Where(t => t.ResourceTypeID == res.ResourceTypeID).FirstOrDefault().ResourceTypeDescription;
            resource.ResourceTypeID = (Int32)res.ResourceTypeID; //It was missing 


            //2015-09-08
            //Ivan S.
            //66
            //Sets the initial order for the new resource to the maximum number for that category
            var lastResource = context.ResourceManagement.Where(r => r.ResourceCategoryID == resource.ResourceCategoryID).OrderByDescending(r => r.ResourceOrder).FirstOrDefault();
            int? lastResourceOrder = InitialOrderValue - 1;
            if (lastResource != null)
                lastResourceOrder = lastResource.ResourceOrder;
            resource.ResourceOrder = ++lastResourceOrder;


            resource.Url = res.Url;
            resource.UploadedThumbnailPath = res.UploadedThumbnailPath;

            context.AddToResourceManagement(resource);
            context.SaveChanges();

            return RedirectToAction("ManageResources");
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateCategory(ResourceListViewModel res)
        {
            var context = Exigo.ODataResources();

            var resource = new ResourceContext.ResourceManagerCategory();
            resource.ResourceCategoryDescription = res.CategoryDescription;

            //2015-09-08
            //Ivan S.
            //66
            //Sets the initial order for the new category to the maximum number for all the categories
            var lastCategory = context.ResourceManagerCategories.OrderByDescending(r => r.ResourceCategoryOrder).FirstOrDefault();
            int? lastCategoryOrder = InitialOrderValue - 1;
            if (lastCategory != null)
                lastCategoryOrder = lastCategory.ResourceCategoryOrder;
            resource.ResourceCategoryOrder = ++lastCategoryOrder;

            context.AddToResourceManagerCategories(resource);
            context.SaveChanges();

            return RedirectToAction("ManageResources");
        }

        /// <summary>
        /// Edit Resource - ManageResources.chtml
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditResource(ResourceListViewModel res, HttpPostedFileBase UploadFile)
        {
            var context = Exigo.ODataResources();

            var ResourceID = Convert.ToInt32(res.EditCategoryID);
            var CategoryID = Convert.ToInt32(res.SelectedCategoryID);

            var resource = context.ResourceManagement.Where(r => r.ResourceID == ResourceID).FirstOrDefault();

            resource.Description = res.Description;
            resource.CreatedDate = DateTime.Now;

            //2015-09-10
            //Ivan S.
            //66
            //Ben wanted me to save the ResourceType, originally the Title was saving the type from a fixed list of File types
            //I now get the description from the resource types and save the right resource typeID
            resource.Title = context.ResourceTypes.Where(t => t.ResourceTypeID == res.ResourceTypeID).FirstOrDefault().ResourceTypeDescription;
            resource.ResourceTypeID = (Int32)res.ResourceTypeID; //It was missing 

            if (resource.ResourceCategoryID != CategoryID)
            {
                //If the categoryID was changed

                //2015-09-08
                //Ivan S.
                //66
                //If the category of the resource was changed,it has to be added at the end when it comes to the resource order 
                //It calculates the last order values for the new category and assigns it to the resource

                var previousResourceOrder = resource.ResourceOrder;

                var lastResource = context.ResourceManagement.Where(r => r.ResourceCategoryID == CategoryID).OrderByDescending(r => r.ResourceOrder).FirstOrDefault();
                int? lastResourceOrder = InitialOrderValue - 1;
                if (lastResource != null)
                    lastResourceOrder = lastResource.ResourceOrder;

                resource.ResourceOrder = ++lastResourceOrder;

                if (resource.ResourceCategoryID != 0)
                {
                    //Category 0 is for the items that were in a category that was deleted and were put temporarily there (ResourceCategoryID=0)
                    //Therefore it is not necessary to reorder the other items

                    //It also changes the order of the resources in the previous category that had an order value greater 
                    //than the one the resource that is being moved had
                    foreach (var resourceItem in context.ResourceManagement.Where(c => c.ResourceCategoryID == resource.ResourceCategoryID && c.ResourceOrder > previousResourceOrder))
                    {
                        resourceItem.ResourceOrder--;
                        context.UpdateObject(resourceItem);
                    }
                }
                resource.ResourceCategoryID = CategoryID;
            }

            resource.Url = res.Url;
            resource.UploadedThumbnailPath = res.UploadedThumbnailPath;

            context.UpdateObject(resource);
            context.SaveChanges();

            return RedirectToAction("ManageResources");
        }

        /// <summary>
        /// Delete Resources - ManageResources.chtml
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteResource(ResourceListViewModel res)
        {
            var ResourceID = Convert.ToInt32(res.EditCategoryID);

            var context = Exigo.ODataResources();
            var resource = context.ResourceManagement.Where(r => r.ResourceID == ResourceID).FirstOrDefault();
            var resourceCategoryID = resource.ResourceCategoryID;
            var resourceOrder = resource.ResourceOrder;

            context.DeleteObject(resource);

            //2015-09-08
            //Ivan S.
            //66
            //Reorders the following resources (setting their order to a minus 1 value)
            foreach (var resourceItem in context.ResourceManagement.Where(c => c.ResourceCategoryID == resourceCategoryID && c.ResourceOrder > resourceOrder))
            {
                resourceItem.ResourceOrder--;
                context.UpdateObject(resourceItem);
            }

            context.SaveChanges();

            return RedirectToAction("ManageResources");
        }

        /// <summary>
        /// delete a category
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public ActionResult DeleteCategory(string categoryID)
        {
            var context = Exigo.ODataResources();
            var ResourceID = Convert.ToInt32(categoryID);

            //reset the resource items with a value of zero for the ResourceCategoryID
            var resources = context.ResourceManagement.Where(r => r.ResourceCategoryID == ResourceID).ToList();
            foreach (var res in resources)
            {
                res.ResourceCategoryID = 0;
                context.UpdateObject(res);
                context.SaveChanges();
            }

            //delete the resource
            var category = context.ResourceManagerCategories.Where(r => r.ResourceCategoryID == ResourceID).FirstOrDefault();
            var categoryOrder = category.ResourceCategoryOrder;
            context.DeleteObject(category);

            //2015-09-08
            //Ivan S.
            //66
            //Decreases the order for the following categories
            foreach (var resourceCat in context.ResourceManagerCategories.Where(c => c.ResourceCategoryOrder > categoryOrder))
            {
                resourceCat.ResourceCategoryOrder--;
                context.UpdateObject(resourceCat);
            }

            context.SaveChanges();

            return RedirectToAction("ManageResources");
        }
        #endregion

        #region JSON
        /// <summary>
        /// Temporary internal function we can call in debug mode to initialize the data in the new fields for any customer who alredy has information in the tables
        /// </summary>
        private void Temporary_InitializeDataForOrderColums()
        {
            //2015-09-08
            //Ivan S.
            //66
            //I created this method to initialize the order for the resources and their categories
            //In order for the new code to work on the customer production database 2 fields need to be created:
            //ResourceManagement.ResourceOrder int nullable
            //ResourceManagerCategories.ResourceCategoryOrder int nullable

            var context = Exigo.ODataResources();
            int j = InitialOrderValue;
            int i;
            foreach (var resourceCat in context.ResourceManagerCategories)
            {
                resourceCat.ResourceCategoryOrder = j;
                context.UpdateObject(resourceCat);
                i = InitialOrderValue;
                foreach (var resource in context.ResourceManagement.Where(r => r.ResourceCategoryID == resourceCat.ResourceCategoryID))
                {
                    resource.ResourceOrder = i;
                    i++;
                    context.UpdateObject(resource);
                }
                j++;
            }
            context.SaveChanges();
        }

        /// <summary>
        /// The method swaps the order of the categories in the extended table
        /// </summary>
        /// <param name="targetCategoryID">Category selected</param>
        /// <param name="draggedCategoryID">Category element being drop</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult SwapCategories(int? targetCategoryID, int? draggedCategoryID)
        {
            //2015-09-08
            //Ivan S.
            //66
            //Modifies the order of the Categories
            try
            {
                bool movedBefore;
                if (targetCategoryID == null || draggedCategoryID == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing data for swaping categories"
                    });


                var context = Exigo.ODataResources();

                var targetCategory = context.ResourceManagerCategories.Where(c => c.ResourceCategoryID == targetCategoryID).FirstOrDefault();
                if (targetCategory == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing category " + targetCategoryID
                    });

                var draggedCategory = context.ResourceManagerCategories.Where(c => c.ResourceCategoryID == draggedCategoryID).FirstOrDefault();
                if (draggedCategory == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing category " + draggedCategoryID
                    });


                var targetCategoryOrder = targetCategory.ResourceCategoryOrder;

                if (targetCategoryOrder < draggedCategory.ResourceCategoryOrder)
                {
                    //If the category dragged was after the target category, it is put before the target category
                    foreach (var resourceCat in context.ResourceManagerCategories.Where(c => c.ResourceCategoryOrder >= targetCategoryOrder && c.ResourceCategoryOrder < draggedCategory.ResourceCategoryOrder))
                    {
                        resourceCat.ResourceCategoryOrder++;
                        context.UpdateObject(resourceCat);
                    }
                    draggedCategory.ResourceCategoryOrder = targetCategoryOrder;
                    movedBefore = true;
                }
                else
                {
                    //The category dragged was before the target category, it is put after the target category
                    foreach (var resourceCat in context.ResourceManagerCategories.Where(c => c.ResourceCategoryOrder > draggedCategory.ResourceCategoryOrder && c.ResourceCategoryOrder <= targetCategoryOrder))
                    {
                        resourceCat.ResourceCategoryOrder--;
                        context.UpdateObject(resourceCat);
                    }
                    draggedCategory.ResourceCategoryOrder = targetCategoryOrder;
                    movedBefore = false;
                }

                context.UpdateObject(draggedCategory);
                context.SaveChanges();

                return new JsonNetResult(new { success = true, moved = true, movedBefore = movedBefore });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        /// <summary>
        /// The method swaps the order of the resources in the extended table
        /// </summary>
        /// <param name="targetResourceID">Category selected</param>
        /// <param name="draggedResourceID">Category element being drop</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult SwapResources(int? targetResourceID, int? draggedResourceID)
        {
            //2015-09-08
            //Ivan S.
            //66
            //Modifies the order of the resources
            try
            {
                bool movedBefore;
                if (targetResourceID == null || draggedResourceID == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing data for swaping resources"
                    });


                var context = Exigo.ODataResources();

                var targetResource = context.ResourceManagement.Where(c => c.ResourceID == targetResourceID).FirstOrDefault();
                if (targetResource == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing resource " + targetResourceID
                    });

                var draggedResource = context.ResourceManagement.Where(c => c.ResourceID == draggedResourceID).FirstOrDefault();
                if (draggedResource == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing resource " + draggedResourceID
                    });


                var targetResourceOrder = targetResource.ResourceOrder;

                if (targetResourceOrder < draggedResource.ResourceOrder)
                {
                    //If the resource dragged was after the target resource, it is put before the target resource
                    foreach (var resource in context.ResourceManagement.Where(c => c.ResourceCategoryID == targetResource.ResourceCategoryID && c.ResourceOrder >= targetResourceOrder && c.ResourceOrder < draggedResource.ResourceOrder))
                    {
                        resource.ResourceOrder++;
                        context.UpdateObject(resource);
                    }
                    draggedResource.ResourceOrder = targetResourceOrder;
                    movedBefore = true;
                }
                else
                {
                    //If the resource dragged was before the target resource, it is put after the target resource
                    foreach (var resource in context.ResourceManagement.Where(c => c.ResourceCategoryID == targetResource.ResourceCategoryID && c.ResourceOrder > draggedResource.ResourceOrder && c.ResourceOrder <= targetResourceOrder))
                    {
                        resource.ResourceOrder--;
                        context.UpdateObject(resource);
                    }
                    draggedResource.ResourceOrder = targetResourceOrder;
                    movedBefore = false;
                }

                context.UpdateObject(draggedResource);
                context.SaveChanges();

                return new JsonNetResult(new { success = true, moved = true, movedBefore = movedBefore });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        /// <summary>
        /// The method updates the category order in the extended table
        /// </summary>
        /// <param name="categoryID">Category selected</param>
        /// <param name="direction">-1=Up,1=down</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult changeCategoryOrder(int? categoryID, int direction = -1)
        {
            //2015-0-08
            //Ivan S.
            //66
            //Modifies the order of the Categories
            try
            {
                if (categoryID == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing data for updating the category order"
                    });


                var context = Exigo.ODataResources();

                var currentCategory = context.ResourceManagerCategories.Where(c => c.ResourceCategoryID == categoryID).FirstOrDefault();
                if (currentCategory == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing category"
                    });


                var currentCategoryOrder = currentCategory.ResourceCategoryOrder;
                ResourceManagerCategory categorytoBeSwitched;
                if (direction == -1)
                    categorytoBeSwitched = context.ResourceManagerCategories.Where(c => c.ResourceCategoryOrder < currentCategoryOrder).OrderByDescending(c => c.ResourceCategoryOrder).FirstOrDefault();
                else
                    categorytoBeSwitched = context.ResourceManagerCategories.Where(c => c.ResourceCategoryOrder > currentCategoryOrder).OrderBy(c => c.ResourceCategoryOrder).FirstOrDefault();

                if (categorytoBeSwitched == null)
                    return new JsonNetResult(new { success = true, moved = false }); //no changed needed because it is the last one

                //swaps the order of the categories

                currentCategory.ResourceCategoryOrder = categorytoBeSwitched.ResourceCategoryOrder;
                categorytoBeSwitched.ResourceCategoryOrder = currentCategoryOrder;

                context.UpdateObject(currentCategory);
                context.UpdateObject(categorytoBeSwitched);

                context.SaveChanges();

                return new JsonNetResult(new { success = true, moved = true });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// The method updates the resource order in the extended table
        /// </summary>
        /// <param name="resourceID">Category selected</param>
        /// <param name="direction">-1=Up,1=down</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult changeResourceOrder(int? resourceID, int direction = -1)
        {
            //2015-09-08
            //Ivan S.
            //66
            //Modifies the order of the resources 
            try
            {
                if (resourceID == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing data for updating the resource order"
                    });


                var context = Exigo.ODataResources();

                var currentResource = context.ResourceManagement.Where(c => c.ResourceID == resourceID).FirstOrDefault();
                if (currentResource == null)
                    return new JsonNetResult(new
                    {
                        success = false,
                        message = "Missing resource"
                    });


                var currentResourceOrder = currentResource.ResourceOrder;
                ResourceManager resourcetoBeSwitched;
                if (direction == -1)
                    resourcetoBeSwitched = context.ResourceManagement.Where(c => c.ResourceCategoryID == currentResource.ResourceCategoryID && c.ResourceOrder < currentResourceOrder).OrderByDescending(c => c.ResourceOrder).FirstOrDefault();
                else
                    resourcetoBeSwitched = context.ResourceManagement.Where(c => c.ResourceCategoryID == currentResource.ResourceCategoryID && c.ResourceOrder > currentResourceOrder).OrderBy(c => c.ResourceOrder).FirstOrDefault();

                if (resourcetoBeSwitched == null)
                    return new JsonNetResult(new { success = true, moved = false }); //no changed needed because it is the last one

                //swaps the order of the categories

                currentResource.ResourceOrder = resourcetoBeSwitched.ResourceOrder;
                resourcetoBeSwitched.ResourceOrder = currentResourceOrder;

                context.UpdateObject(currentResource);
                context.UpdateObject(resourcetoBeSwitched);

                context.SaveChanges();

                return new JsonNetResult(new { success = true, moved = true });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// fetch resource data for edit modal - ManageResources.chtml
        /// </summary>
        /// <param name="editResourceID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult FetchResourceData(string editResourceID)
        {
            try
            {
                var model = new ResourceListViewModel();

                //set up the model/service and fetch data
                var ResourceID = Convert.ToInt32(editResourceID);
                var res = Exigo.ODataResources().ResourceManagement.Where(r => r.ResourceID == ResourceID).FirstOrDefault();

                model.EditCategoryID = res.ResourceID.ToString();
                model.Title = res.Title;
                model.Description = res.Description;
                model.Url = res.Url;
                model.ResourceCategoryID = res.ResourceCategoryID;
                model.ResourceTypeID = res.ResourceTypeID;
                model.UploadedThumbnailPath = res.UploadedThumbnailPath;

                return new JsonNetResult(new
                {
                    success = true,
                    model = model
                });
            }
            catch
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// saves the file to the application
        /// </summary>
        /// <param name="UploadFile"></param>
        /// <returns></returns>
        public string SaveFilePath(HttpPostedFileBase UploadFile)
        {
            var fileName = Path.GetFileName(UploadFile.FileName);
            var filePath = "Content/Files/" + fileName;
            UploadFile.SaveAs(Server.MapPath("~/" + filePath)); //2014-10-30 DV. Redundant use of fileName...removed

            return filePath;
        }
        #endregion
    }
}