using Backoffice.Models;
using Backoffice.Services;
using Backoffice.ViewModels;
using Common;
using Common.Api.ExigoWebService;
using Common.Services;
using ExigoService;
using ExigoWeb.Kendo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dapper;
using System.Dynamic;
using Backoffice.Models.Orginization;
using Common.Kendo;
using Common.Helpers;
using Backoffice.Filters;

namespace Backoffice.Controllers
{
    [BackofficeSubscriptionRequired]
    public class OrganizationController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        #region Leads
        [Route("~/leadslist")]
        public ActionResult LeadsList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();

            using (var context = new KendoGridDataContext(Exigo.Sql()))
            {
                return context.Query(request, @"
                    SELECT
                c.CustomerID,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Phone,
                        c.CreatedDate,
                c.MainCity,
                c.MainState
                    FROM
	                    Customers c
                    WHERE
	                    c.EnrollerID = @enroller
                        AND c.CustomerTypeID = @customertype
                        AND c.CustomerStatusID = @customerstatus
                ", new
                 {
                     enroller = Identity.Current.CustomerID,
                     customertype = CustomerTypes.RetailCustomer,
                     customerstatus = CustomerStatuses.Active
                 }).Tokenize("CustomerID");
            }
        }

        [Route("~/createlead")]
        public ActionResult CreateLead()
        {
            var model = new CreateLeadViewModel();

            return View(model);
        }

        [HttpPost]
        [Route("~/createlead")]
        public ActionResult CreateLead(CreateLeadViewModel model)
        {
            try
            {
                var request = new CreateCustomerRequest();

                request.Field1 = "Manual";

                request.FirstName = model.Lead.FirstName;
                request.LastName = model.Lead.LastName;
                request.Email = model.Lead.Email;
                request.Phone = model.Lead.PrimaryPhone;
                request.Phone2 = model.Lead.SecondaryPhone;
                request.MobilePhone = model.Lead.MobilePhone;

                request.MainAddress1 = model.Lead.MainAddress.Address1;
                request.MainAddress2 = model.Lead.MainAddress.Address2;
                request.MainCity = model.Lead.MainAddress.City;
                request.MainState = model.Lead.MainAddress.State;
                request.MainZip = model.Lead.MainAddress.Zip;
                request.MainCountry = model.Lead.MainAddress.Country;

                request.CanLogin = false;
                request.CustomerType = CustomerTypes.SmartShopper;
                request.CustomerStatus = CustomerStatuses.Active;
                request.InsertEnrollerTree = true;
                request.EnrollerID = Identity.Current.CustomerID;
                request.EntryDate = DateTime.Now;

                var response = Exigo.WebService().CreateCustomer(request);

                if (response.Result.Status == ResultStatus.Success)
                {
                    return new JsonNetResult(new
                    {
                        success = true
                    });
                }
                else
                {
                    return new JsonNetResult(new
                    {
                        success = false
                    });
                }
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

        [Route("~/managelead/{leadID:int}")]
        public ActionResult ManageLead(int leadID)
        {
            var model = new ManageLeadViewModel();
            model.Lead = Exigo.GetCustomerLead(leadID);

            return View(model);
        }

        [HttpPost]
        [Route("~/managelead/{leadID:int}")]
        public ActionResult ManageLead(int leadID, CreateLeadViewModel model)
        {
            try
            {
                Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest()
                {
                    CustomerID = leadID,
                    FirstName = model.Lead.FirstName,
                    LastName = model.Lead.LastName,
                    Email = model.Lead.Email,
                    Phone = model.Lead.PrimaryPhone,
                    Phone2 = model.Lead.SecondaryPhone,
                    MobilePhone = model.Lead.MobilePhone,
                    Fax = model.Lead.Fax,
                    MainAddress1 = model.Lead.MainAddress.Address1,
                    MainAddress2 = model.Lead.MainAddress.Address2,
                    MainCity = model.Lead.MainAddress.City,
                    MainState = model.Lead.MainAddress.State,
                    MainZip = model.Lead.MainAddress.Zip,
                    MainCountry = model.Lead.MainAddress.Country
                });

                return new JsonNetResult(new
                {
                    success = true
                });
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

        public JsonNetResult DeleteLead(int leadID)
        {
            try
            {
                Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest()
                {
                    CustomerID = leadID,
                    CustomerStatus = CustomerStatuses.Deleted
                });

                return new JsonNetResult(new
                {
                    success = true
                });
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
        #endregion

        #region Company News
        [Route("~/news")]
        public ActionResult CompanyNewsList()
        {
            var model = new CompanyNewsViewModel();

            model.CompanyNewsItems = Exigo.GetCompanyNews(new ExigoService.GetCompanyNewsRequest()
            {
                NewsDepartments = new[] { 7, 8 },
                RowCount = 0
            });

            return View(model);
        }

        [Route("~/news/{newsid:int}")]
        public ActionResult CompanyNewsDetail(int newsid)
        {
            var model = new CompanyNewsViewModel();

            model.CompanyNewsItems = Exigo.GetCompanyNews(new ExigoService.GetCompanyNewsRequest()
            {
                NewsItemIDs = new[] { newsid },
                IncludeBody = true
            });

            return View(model);
        }
        #endregion

        #region Trees
        public ActionResult BinaryTreeViewer()
        {
            return View();
        }

        public ActionResult UnilevelTreeViewer()
        {
            var model = new TreeViewerViewModel();

            var ranks = Exigo.GetRanks();

            foreach (var rank in ranks)
            {
                var listItem = new ExigoService.Rank();

                listItem.RankID = rank.RankID;
                listItem.RankDescription = rank.RankDescription;

                model.Ranks.Add(listItem);
            }

            return View(model);
        }

        [Route("popoversummary/{id:int=0}")]
        public ActionResult PopoverSummary(int id)
        {
            var model = new TreeNodePopoverViewModel();

            if (id == 0) id = Identity.Current.CustomerID;

            model.Customer = Exigo.GetCustomer(id);

            if (Request.IsAjaxRequest()) return PartialView("_TreeNodePopover", model);
            else return View(model);
        }

        public JsonNetResult FetchBinaryTree(int id)
        {
            var currentPeriod = Exigo.GetCurrentPeriod(PeriodTypes.Default);
            var backofficeOwnerID = Identity.Current.CustomerID;
            var customerID = (id != 0) ? id : backofficeOwnerID;
            var controller = this;
            var tasks = new List<Task>();



            // Assemble Tree
            var treehtml = string.Empty;
            tasks.Add(Task.Factory.StartNew(() =>
            {
                var request = new GetTreeRequest
                {
                    TopCustomerID = customerID,
                    Levels = 3,
                    Legs = 2,
                    IncludeNullPositions = true,
                    IncludeOpenPositions = true
                };

                var nodes = Exigo.GetBinaryTree<BinaryNestedTreeNode>(request);
                var customerIDs = nodes.Select(c => c.CustomerID).ToList();

                using (var context = Exigo.Sql())
                {
                    var nodeDataRecords = context.Query(@"
                        select
	                        c.CustomerID,
	                        c.FirstName,
	                        c.LastName,
	                        c.Company,
	                        c.EnrollerID,
	                        c.CreatedDate,
	                        pv.PaidRankID,
	                        PaidRankDescription = COALESCE(prr.RankDescription, 'Unknown'),
	                        HasAutoOrder = cast(case when COUNT(ao.AutoOrderID) > 0 then 1 else 0 end as bit),
	                        IsPersonallyEnrolled = cast(case when c.EnrollerID = @topcustomerid then 1 else 0 end as bit)
                        from Customers c
                        left join PeriodVolumes pv
	                        on pv.CustomerID = c.CustomerID
	                        and pv.PeriodTypeID = @periodtypeid
	                        and pv.PeriodID = @periodid
                        left join Ranks prr
	                        on prr.RankID = pv.RankID
                        left join AutoOrders ao
	                        on ao.CustomerID = c.CustomerID
	                        and ao.AutoOrderStatusID = 0
                        where 
	                        c.CustomerID in @customerids
                        group by
	                        c.CustomerID,
	                        c.FirstName,
	                        c.LastName,
	                        c.Company,
	                        c.EnrollerID,
	                        c.CreatedDate,
	                        pv.PaidRankID,
	                        prr.RankDescription
                    ", new
                     {
                         customerids = customerIDs,
                         periodtypeid = currentPeriod.PeriodTypeID,
                         periodid = currentPeriod.PeriodID,
                         topcustomerid = backofficeOwnerID
                     }).ToList();

                    foreach (var nodeDataRecord in nodeDataRecords)
                    {
                        var node = nodes.Where(c => c.CustomerID == nodeDataRecord.CustomerID).Single();

                        node.HasAutoOrder = nodeDataRecord.HasAutoOrder;
                        node.IsPersonallyEnrolled = nodeDataRecord.IsPersonallyEnrolled;

                        node.Customer.CustomerID = nodeDataRecord.CustomerID;
                        node.Customer.FirstName = nodeDataRecord.FirstName;
                        node.Customer.LastName = nodeDataRecord.LastName;
                        node.Customer.Company = nodeDataRecord.Company;
                        node.Customer.EnrollerID = nodeDataRecord.EnrollerID;
                        node.Customer.CreatedDate = nodeDataRecord.CreatedDate;

                        node.CurrentRank.RankID = nodeDataRecord.PaidRankID;
                        node.CurrentRank.RankDescription = nodeDataRecord.PaidRankDescription;
                    }
                }

                var nodeTree = Exigo.OrganizeNestedTreeNodes(nodes, request).FirstOrDefault();

                var container = new TagBuilder("div");
                container.AddCssClass("jOrgChart");

                var service = new TreeService();
                treehtml = service.BuildTree(container, controller, nodeTree, TreeTypes.Binary);
            }));






            // Assemble Customer Details
            var detailsModel = new CustomerDetailsViewModel();
            tasks.Add(Task.Factory.StartNew(() =>
            {
                detailsModel.Customer = Exigo.GetCustomer(customerID);

                var volumes = Exigo.GetCustomerVolumes(new GetCustomerVolumesRequest()
                {
                    CustomerID = customerID,
                    PeriodID = currentPeriod.PeriodID,
                    PeriodTypeID = currentPeriod.PeriodTypeID
                });

                detailsModel.PaidRank = volumes.PayableAsRank;
                detailsModel.HighestRank = volumes.HighestAchievedRankThisPeriod;
            }));








            // Get the upline
            var uplineModel = new UplineViewModel();
            tasks.Add(Task.Factory.StartNew(() =>
            {
                var uplineNodes = new List<UplineNodeViewModel>();

                using (var context = Exigo.Sql())
                {
                    uplineNodes = context.Query<UplineNodeViewModel>(@"
                     SELECT 
                        b.CustomerID,
                        b.Level,
                        c.FirstName,
                        c.LastName                        
                    FROM BinaryUpline b
                        LEFT JOIN Customers c 
                            ON c.CustomerID = b.CustomerID
                    WHERE 
                        b.UplineCustomerID = @bottomcustomerid
                    ORDER BY Level",
                        new
                        {
                            bottomcustomerid = customerID
                        }).ToList();
                }

                // Filter out the nodes that don't belong
                var isFound = false;
                var filteredNodes = new List<UplineNodeViewModel>();
                foreach (var node in uplineNodes)
                {
                    if (node.CustomerID == backofficeOwnerID)
                    {
                        isFound = true;
                    }

                    if (isFound) filteredNodes.Add(node);
                }

                // Set the levels
                for (int i = 0; i < filteredNodes.Count; i++)
                {
                    filteredNodes[i].Level = i;
                }

                // Assemble Upline HTML
                uplineModel.UplineNodes = filteredNodes;
            }));


            Task.WaitAll(tasks.ToArray());
            tasks.Clear();


            // Render the partials
            var customerdetailshtml = this.RenderPartialViewToString("_CustomerDetails", detailsModel);
            var uplinehtml = this.RenderPartialViewToString("_CustomerUpline", uplineModel);


            // Return our data
            return new JsonNetResult(new
            {
                treehtml = treehtml,
                customerdetailshtml = customerdetailshtml,
                uplinehtml = uplinehtml,
                id = customerID
            });
        }

        public JsonNetResult FetchUnilevelTree(int id)
        {
            try
            {
                var currentPeriod = Exigo.GetCurrentPeriod(PeriodTypes.Default);
                var backofficeOwnerID = Identity.Current.CustomerID;
                var customerID = (id != 0) ? id : backofficeOwnerID;
                var controller = this;
                var tasks = new List<Task>();



                // Assemble Tree
                var treehtml = string.Empty;
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var request = new GetTreeRequest
                    {
                        TopCustomerID = customerID,
                        Levels = 3,
                        Legs = 0,
                        IncludeNullPositions = true,
                        IncludeOpenPositions = true
                    };


                    var nodeDataRecords = new List<dynamic>();
                    using (var context = Exigo.Sql())
                    {
                        nodeDataRecords = context.Query(@"
                        select
	                        c.CustomerID,
	                        c.FirstName,
	                        c.LastName,
	                        c.Company,
	                        c.EnrollerID,
	                        c.CreatedDate,
                            RankID = COALESCE(pv.RankID, 0),
	                        RankDescription = COALESCE(prr.RankDescription, 'Unknown'),
	                        PaidRankID = COALESCE(pv.PaidRankID, 0),
	                        PaidRankDescription = COALESCE(prr.RankDescription, 'Unknown'),
	                        HasAutoOrder = cast(case when COUNT(ao.AutoOrderID) > 0 then 1 else 0 end as bit),
	                        IsPersonallyEnrolled = cast(case when c.EnrollerID = @topcustomerid then 1 else 0 end as bit),

                            ud.SponsorID,
                            ud.Level,
                            ud.Placement,
                            ud.IndentedSort

                        from UnilevelDownline ud
                        left join Customers c
                            on c.CustomerID = ud.CustomerID
                        left join PeriodVolumes pv
	                        on pv.CustomerID = c.CustomerID
	                        and pv.PeriodTypeID = @periodtypeid
	                        and pv.PeriodID = @periodid
                        left join Ranks prr
	                        on prr.RankID = pv.RankID
                        left join AutoOrders ao
	                        on ao.CustomerID = c.CustomerID
	                        and ao.AutoOrderStatusID = 0
                        where ud.DownlineCustomerID = @topcustomerid
	                        and Level <= @level 
                        group by
	                        c.CustomerID,
	                        c.FirstName,
	                        c.LastName,
	                        c.Company,
	                        c.EnrollerID,
	                        c.CreatedDate,
	                        pv.PaidRankID,
                            pv.RankID,
	                        prr.RankDescription,

                            ud.SponsorID,
                            ud.Level,
                            ud.Placement,
                            ud.IndentedSort
                    ", new
                     {
                         periodtypeid = currentPeriod.PeriodTypeID,
                         periodid = currentPeriod.PeriodID,
                         topcustomerid = customerID,
                         level = request.Levels
                     }).ToList();
                    }


                    var nodes = new List<UnilevelNestedTreeNode>();
                    foreach (var nodeDataRecord in nodeDataRecords)
                    {
                        //var node = nodes.Single(c => c.CustomerID == nodeDataRecord.CustomerID);
                        var node = new UnilevelNestedTreeNode();

                        node.CustomerID = nodeDataRecord.CustomerID;
                        node.ParentCustomerID = nodeDataRecord.SponsorID;
                        node.Level = nodeDataRecord.Level;
                        node.PlacementID = nodeDataRecord.Placement;
                        node.IndentedSort = nodeDataRecord.IndentedSort;

                        node.HasAutoOrder = nodeDataRecord.HasAutoOrder;
                        node.IsPersonallyEnrolled = nodeDataRecord.IsPersonallyEnrolled;

                        node.Customer.CustomerID = nodeDataRecord.CustomerID;
                        node.Customer.FirstName = nodeDataRecord.FirstName;
                        node.Customer.LastName = nodeDataRecord.LastName;
                        node.Customer.Company = nodeDataRecord.Company;
                        node.Customer.EnrollerID = nodeDataRecord.EnrollerID;
                        node.Customer.CreatedDate = nodeDataRecord.CreatedDate;

                        node.CurrentRank.RankID = nodeDataRecord.RankID;
                        node.CurrentRank.RankDescription = nodeDataRecord.RankDescription;

                        nodes.Add(node);
                    }

                    var nodeTree = Exigo.OrganizeNestedTreeNodes(nodes, request).FirstOrDefault();

                    var container = new TagBuilder("div");
                    container.AddCssClass("jOrgChart");

                    var service = new TreeService();
                    treehtml = service.BuildTree(container, controller, nodeTree, TreeTypes.Unilevel);
                }));






                // Assemble Customer Details
                var detailsModel = new CustomerDetailsViewModel();
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    detailsModel.Customer = Exigo.GetCustomer(customerID);

                    var volumes = Exigo.GetCustomerVolumes(new GetCustomerVolumesRequest()
                    {
                        CustomerID = customerID,
                        PeriodID = currentPeriod.PeriodID,
                        PeriodTypeID = currentPeriod.PeriodTypeID
                    });



                    detailsModel.PaidRank = volumes.PayableAsRank;
                    detailsModel.HighestRank = volumes.HighestAchievedRankThisPeriod;
                }));



                // Assemble Waiting Room
                var waitingRoomModel = new WaitingRoomListViewModel();
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var oDataContext = Exigo.OData();
                    var oDataCustomers = new List<Common.Api.ExigoOData.Customer>();

                    int lastResultCount = 50;
                    int callsMade = 0;

                    while (lastResultCount == 50)
                    {

                        var results = oDataContext.Customers
                                    .Where(c => c.EnrollerID == backofficeOwnerID)
                                    .Where(c => c.SponsorID == backofficeOwnerID)
                                    .Where(c => c.CustomerStatusID == CustomerStatusTypes.Active)
                                    .Where(c => c.CustomerTypeID == CustomerTypes.Associate)
                                    .OrderBy(c => c.CreatedDate)
                                    .Skip(callsMade * 50)
                                    .Take(50).ToList();
                        results.ForEach(c => oDataCustomers.Add(c));

                        callsMade++;
                        lastResultCount = results.Count;
                    }

                    var filteredODataCustomers = oDataCustomers.Where(c => DateTime.Now < c.CreatedDate.AddMonths(1).EndOfMonth()).ToList();

                    var customers = new List<WaitingRoomNode>();
                    foreach (var odataCustomer in filteredODataCustomers)
                    {
                        var customer = new WaitingRoomNode();
                        customer.CustomerID = odataCustomer.CustomerID;
                        customer.Email = odataCustomer.Email;
                        customer.EnrollerID = odataCustomer.EnrollerID;
                        customer.EnrollmentDate = odataCustomer.CreatedDate;
                        customer.FirstName = odataCustomer.FirstName;
                        customer.LastName = odataCustomer.LastName;
                        customer.Phone = odataCustomer.Phone;

                        customers.Add(customer);
                    }

                    waitingRoomModel.WaitingRoomCustomers = customers;
                }));




                // Get the upline
                var uplineModel = new UplineViewModel();
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var uplineNodes = new List<UplineNodeViewModel>();

                    using (var context = Exigo.Sql())
                    {
                        uplineNodes = context.Query<UplineNodeViewModel>(@"
                     SELECT 
                        ul.CustomerID,
                        ul.Level,
                        c.FirstName,
                        c.LastName                        
                    FROM UnilevelUpline ul
                        LEFT JOIN Customers c 
                            ON c.CustomerID = ul.CustomerID
                    WHERE 
                        ul.UplineCustomerID = @bottomcustomerid
                    ORDER BY Level",
                            new
                            {
                                bottomcustomerid = customerID
                            }).ToList();
                    }

                    // Filter out the nodes that don't belong
                    var isFound = false;
                    var filteredNodes = new List<UplineNodeViewModel>();
                    foreach (var node in uplineNodes)
                    {
                        if (node.CustomerID == backofficeOwnerID)
                        {
                            isFound = true;
                        }

                        if (isFound) filteredNodes.Add(node);
                    }

                    // Set the levels
                    for (int i = 0; i < filteredNodes.Count; i++)
                    {
                        filteredNodes[i].Level = i;
                    }

                    // Assemble Upline HTML
                    uplineModel.UplineNodes = filteredNodes;
                }));


                Task.WaitAll(tasks.ToArray());
                tasks.Clear();


                // Render the partials
                var customerdetailshtml = this.RenderPartialViewToString("_CustomerDetails", detailsModel);
                var uplinehtml = this.RenderPartialViewToString("_CustomerUpline", uplineModel);
                var waitingroomlisthtml = this.RenderPartialViewToString("_WaitingRoomPlacement", waitingRoomModel);


                // Return our data
                return new JsonNetResult(new
                {
                    treehtml = treehtml,
                    customerdetailshtml = customerdetailshtml,
                    uplinehtml = uplinehtml,
                    waitingroomlisthtml = waitingroomlisthtml,
                    id = customerID
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public JsonNetResult UpOneLevel(int id)
        {

            var inOwnersTree = Exigo.IsCustomerInUniLevelTree(id);

            if (inOwnersTree)
            {
                using (var context = Exigo.Sql())
                {
                    var parentid = context.Query<int>(@"
                     SELECT 
                        ut.ParentID                       
                    FROM UniLevelTree ut

                    WHERE 
                        ut.CustomerID = @customerid
                    ORDER BY Level",
                         new
                         {
                             customerid = id
                         }).ToList();
                    return new JsonNetResult(new
                    {
                        success = true,
                        parentId = parentid
                    });
                }
            }
            return new JsonNetResult(new
            {
                success = false
            });
        }
        public JsonNetResult Search(string query)
        {
            try
            {
                // assemble a list of customers who match the search criteria
                var results = new List<SearchResult>();


                using (var context = Exigo.Sql())
                {
                    results = context.Query<SearchResult>(@"
                        SELECT 
                            c.CustomerID,
                            c.FirstName,
                            c.LastName
                                   
                        FROM BinaryDownline bd                         

                        LEFT JOIN  Customers c
                           ON bd.CustomerID = c.CustomerID
                            
                        WHERE bd.downlinecustomerid = @topCustomerID

                        AND (c.FirstName LIKE '%' + @query + '%' 
                                OR c.LastName LIKE '%' + @query + '%' 
                                OR c.CustomerID LIKE @query)                                            
                        ", new
                         {
                             query = query,
                             topCustomerID = Identity.Current.CustomerID
                         }).ToList();
                }

                return new JsonNetResult(new
                {
                    success = true,
                    results = results
                });
            }
            catch (Exception)
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }
        public JsonNetResult GetBinaryTreeNodes(int? CustomerID)
        {
            var nodes = Exigo.GetBinaryTree<TreeNode>(new GetTreeRequest
            {
                TopCustomerID = CustomerID ?? Identity.Current.CustomerID,
                Levels = 1
            });

            if (CustomerID == null) return new JsonNetResult(nodes.Where(c => c.Level == 0));
            else return new JsonNetResult(nodes.Where(c => c.Level == 1));
        }

        public JsonNetResult GetUnilevelTreeNodes(int? CustomerID)
        {
            var nodes = Exigo.GetUnilevelTree<TreeNode>(new GetTreeRequest
            {
                TopCustomerID = CustomerID ?? Identity.Current.CustomerID,
                Levels = 1
            });

            if (CustomerID == null) return new JsonNetResult(nodes.Where(c => c.Level == 0));
            else return new JsonNetResult(nodes.Where(c => c.Level == 1));
        }
        #endregion

        #region Reports
        [Route("~/personally-enrolled-team")]
        public ActionResult PersonallyEnrolledList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();


            // Establish the query

            var distributorCustomerTypes = new List<int> { (int)CustomerTypes.SmartShopper, (int)CustomerTypes.RetailCustomer, (int)CustomerTypes.Associate };

            var sortByClause = KendoUtilitiesCustom.GetSqlOrderByClause(request.SortObjects, new SortObject("CustomerID", "asc"));
            var whereClause = KendoUtilitiesCustom.GetSqlWhereClause(request.FilterObjectWrapper.FilterObjects);
            dynamic personallyEnrolledList = null;
            try
            {
                using (var context = Exigo.Sql())
                {
                    personallyEnrolledList = context.Query(@"
               Declare @PeriodTy int = @periodtype, @CustomerID int = @id, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)

;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   c.EnrollerID,
      c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   0 as 'team'
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
		Inner Join UniLevelDownline ud
		on ud.DownlineCustomerID = @CustomerID
 Where
  c.CustomerID = @CustomerID 
), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   1 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume50 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   2 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume51 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   3 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume52 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   4 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume53 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   5 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume54 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   6 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume90 =  ud.DownlineCustomerID
Where ud.DownlineCustomerID <> 0
  
), cte_combine as (
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	Level

From
 cte_Primary
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team1
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team2
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team3
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team4
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team5

UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team6

)
select
	CustomerID = CustomerID,
    FirstName    = FirstName,
    LastName     = LastName,
    CreatedDate  = CreatedDate,
 Email,
   EnrollerID,
	Team = Team,
	  Level
From
 cte_combine
	where EnrollerID = @CustomerID
                        " + whereClause + @"
Order By
                        " + sortByClause + @"
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY
                   
            option (maxrecursion 0)", new
             {

                 periodtype = PeriodTypes.Monthly,
                 skip = request.Skip,
                 take = request.Take,
                 customerTypes = distributorCustomerTypes,
                 id = Identity.Current.CustomerID
             });
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }

            if (request.Total == 0 || request.FilterObjectWrapper.FilterObjects.Count() > 0)
            {
                using (var context = Exigo.Sql())
                {
                    request.Total = context.Query<int>(@"
               Declare @PeriodTy int = @periodtype, @CustomerID int = @id, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)

;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   c.EnrollerID,
      c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   0 as 'team'
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
		Inner Join UniLevelDownline ud
		on ud.DownlineCustomerID = @CustomerID
 Where
  c.CustomerID = @CustomerID 
), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   1 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume50 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   2 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume51 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   3 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume52 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   4 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume53 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   5 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume54 =  ud.DownlineCustomerID
  
Where ud.DownlineCustomerID <> 0
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
	  c.EnrollerID,
   c.Email,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
	  ud.Level,
   6 as 'team'
 From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume90 =  ud.DownlineCustomerID
Where ud.DownlineCustomerID <> 0
 
), cte_combine as (
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	Level

From
 cte_Primary
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team1
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team2
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team3
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team4
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team5

UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
   Email,
   EnrollerID,
	  Level
From
 cte_Team6

)
select
	Count(*)
From
 cte_combine
	where EnrollerID = @CustomerID
                        " + whereClause + @"
                        option (maxrecursion 0)", new
                     {

                         periodtype = PeriodTypes.Monthly,
                         customerTypes = distributorCustomerTypes,
                         id = Identity.Current.CustomerID
                     }).FirstOrDefault();
                }
            }



            var results = new List<dynamic>();
            foreach (var item in personallyEnrolledList)
            {
                var newItem = item as IDictionary<String, object>;

                var value = newItem["CustomerID"];
                var token = Security.Encrypt(value, Identity.Current.CustomerID);

                newItem = DynamicHelper.AddProperty(item, "CustomerID" + "Token", token);


                results.Add(newItem);
            }

            personallyEnrolledList = results;
            return new JsonNetResult(new
            {
                data = personallyEnrolledList,
                total = request.Total
            });

        }

        [Route("~/retail-customers")]
        public ActionResult RetailCustomerList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();


            if (Request.HttpMethod.ToUpper() == "GET") return View();


            using (var context = new KendoGridDataContext(Exigo.Sql()))
            {
                return context.Query(request, @"
                    SELECT
                        c.CustomerID,
                        c.FirstName,
                        c.LastName,
                        c.Email,
                        c.Phone,
                        c.CreatedDate
                    FROM
	                    Customers c
                    WHERE
	                    c.EnrollerID = @enroller
                        AND c.CustomerTypeID = @customertype
                ", new
            {
                enroller = Identity.Current.CustomerID,
                customertype = CustomerTypes.RetailCustomer
            }).Tokenize("CustomerID");
            }
        }

        [Route("~/preferred-customers")]
        public ActionResult PreferredCustomerList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();


            var preferredCustomerTypes = new List<int> { (int)CustomerTypes.SmartShopper, (int)CustomerTypes.RetailCustomer };
            // Establish the query
            using (var context = new KendoGridDataContext(Exigo.Sql()))
            {
                return context.Query(request, @"
                    SELECT
                        c.CustomerID,
                        c.FirstName,
                        c.LastName,
                        c.Email,
                        c.Phone,
                        c.CreatedDate,
                        c.CustomerTypeID,
CASE WHEN c.CustomerTypeID = @smartShopperID THEN 'True' ELSE 'False' END as IsSmartShopper
                    FROM
	                    Customers c
                    WHERE
	                    c.EnrollerID = @enroller
                        AND c.CustomerTypeID in @customertypes
                ", new
            {
                enroller = Identity.Current.CustomerID,
                customertypes = preferredCustomerTypes,
                smartShopperID = CustomerTypes.SmartShopper
            }).Tokenize("CustomerID");
            }
        }

        [Route("~/downline-orders")]
        public ActionResult DownlineOrdersList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();

            var sortByClause = KendoUtilitiesCustom.GetSqlOrderByClause(request.SortObjects, new SortObject("cte.CustomerID", "asc"));
            var whereClause = KendoUtilitiesCustom.GetSqlWhereClause(request.FilterObjectWrapper.FilterObjects);
            dynamic downlineOrdersList = null;
            // Fetch the data
            using (var context = Exigo.Sql())
            {
                try
                {
                    downlineOrdersList = context.Query(@"
                Declare @PeriodTy int = @periodtype, @CustomerID int = @downlinecustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   0 as 'team'
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
  Inner Join UniLevelDownline ud
		on ud.DownlineCustomerID = @CustomerID
 Where
  c.CustomerID = @CustomerID 
  
), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   1 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume50 =  ud.DownlineCustomerID
		
  
Where ud.DownlineCustomerID <> 0             
), cte_Team2 as
(
  Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   2 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume51 =  ud.DownlineCustomerID


Where ud.DownlineCustomerID <> 0  
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   3 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume52 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0  
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   4 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume53 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0  
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   5 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume54 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0  
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   6 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume90 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0                
               
), cte_combine as (
Select
 Team, CustomerID, 
 
   FirstName,
   LastName
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team1
UNION
Select
 Team, CustomerID,
   
   FirstName,
   LastName
From
 cte_Team2
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team6
)
select
Team, 
cte_CustomerID = cte.CustomerID, 
   Total,
   BusinessVolumeTotal,
  cte_FirstName = cte.FirstName,
  cte_LastName = cte.LastName,
OrderID,
   OrderStatusID,
   OrderDate = CAST(o.OrderDate AS date)
From
 cte_combine cte

  Inner join Orders o
  on o.CustomerID = cte.CustomerID
Where cte.CustomerID != @downlinecustomerid
and 
                         OrderDate > GETDATE() - 7
                        AND OrderStatusID >= 7

                        " + whereClause + @"
Order By

                        " + sortByClause + @"
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY
                option (maxrecursion 0)", new
                                        {
                                            periodtype = PeriodTypes.Monthly,
                                            skip = request.Skip,
                                            take = request.Take,
                                            downlinecustomerid = Identity.Current.CustomerID
                                        });


                    if (request.Total == 0 || request.FilterObjectWrapper.FilterObjects.Count() > 0)
                    {


                        request.Total = context.Query<int>(@"
                Declare @PeriodTy int = @periodtype, @CustomerID int = @downlinecustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   0 as 'team'
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
  Inner Join UniLevelDownline ud
		on ud.DownlineCustomerID = @CustomerID
 Where
  c.CustomerID = @CustomerID 
  
), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   1 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume50 =  ud.DownlineCustomerID
		
  
Where ud.DownlineCustomerID <> 0             
), cte_Team2 as
(
  Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   2 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume51 =  ud.DownlineCustomerID


Where ud.DownlineCustomerID <> 0  
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   3 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume52 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0  
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   4 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume53 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0  
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   5 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume54 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0  
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   ud.Level,
   6 as 'team'
 From
 UniLevelDownline ud
 INNER JOIN Customers c
 on c.CustomerID = ud.CustomerID
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
		on team.Volume90 =  ud.DownlineCustomerID



Where ud.DownlineCustomerID <> 0                
               
), cte_combine as (
Select
 Team, CustomerID, 
 
   FirstName,
   LastName
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team1
UNION
Select
 Team, CustomerID,
   
   FirstName,
   LastName
From
 cte_Team2
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName
From
 cte_Team6
)
select
  Count = COUNT(*)
From
 cte_combine cte
Inner join 
Orders o 
on o.CustomerID = cte.CustomerID
Where cte.CustomerID != @downlinecustomerid
and 
                         OrderDate > GETDATE() - 7
                        AND OrderStatusID >= 7

                        " + whereClause + @"
                        option (maxrecursion 0)", new
                                                {


                                                    periodtype = PeriodTypes.Monthly,
                                                    downlinecustomerid = Identity.Current.CustomerID
                                                }).FirstOrDefault();
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }

                var results = new List<dynamic>();
                foreach (var item in downlineOrdersList)
                {
                    var newItem = item as IDictionary<String, object>;

                    var value = newItem["cte_CustomerID"];
                    var token = Security.Encrypt(value, Identity.Current.CustomerID);

                    newItem = DynamicHelper.AddProperty(item, "CustomerID" + "Token", token);


                    results.Add(newItem);
                }


                downlineOrdersList = results;
                return new JsonNetResult(new
                {
                    data = downlineOrdersList,
                    total = request.Total
                });
            }
        }

        [Route("~/downline-autoorders")]
        public ActionResult DownlineAutoOrdersList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();


            var sortByClause = KendoUtilitiesCustom.GetSqlOrderByClause(request.SortObjects, new SortObject("cte.CustomerID", "asc"));
            var whereClause = KendoUtilitiesCustom.GetSqlWhereClause(request.FilterObjectWrapper.FilterObjects);
            dynamic downlineAutoOrdersList = null;
            // Fetch the data
            using (var context = Exigo.Sql())
            {
                try
                {
                    downlineAutoOrdersList = context.Query(@"
                Declare @PeriodTy int = @periodtype, @CustomerID int = @downlinecustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
                                            ;with cte_Primary as
                                            (
                                             Select 
                                               c.CustomerID,
                                               c.FirstName,
                                               c.LastName,
                                               pv.Volume50,
                                               pv.Volume51,
                                               pv.Volume52,
                                               pv.Volume53,
                                               pv.Volume54,
                                               pv.Volume90,
                                               ud.Level,
                                               0 as 'team'
                                             From
                                              Customers c
                                             Inner Join PeriodVolumes pv
                                              on pv.CustomerID = c.CustomerID
                                              and pv.PeriodID = @PeriodID
                                              and pv.PeriodTypeID = @PeriodTy
                                              Inner Join UniLevelDownline ud
		                                            on ud.DownlineCustomerID = @CustomerID
                                             Where
                                              c.CustomerID = @CustomerID 
  
                                            ), cte_Team1 as
                                            (
                                             Select 
                                               c.CustomerID,
                                               c.FirstName,
                                               c.LastName,
                                               pv.Volume50,
                                               pv.Volume51,
                                               pv.Volume52,
                                               pv.Volume53,
                                               pv.Volume54,
                                               pv.Volume90,
                                               ud.Level,
                                               1 as 'team'
                                             From
                                             UniLevelDownline ud
                                             INNER JOIN Customers c
                                             on c.CustomerID = ud.CustomerID
                                             Inner Join PeriodVolumes pv
                                              on pv.CustomerID = c.CustomerID
                                              and pv.PeriodID = @PeriodID
                                              and pv.PeriodTypeID = @PeriodTy
                                             Inner Join cte_Primary team
		                                            on team.Volume50 =  ud.DownlineCustomerID
		
  
                                            Where ud.DownlineCustomerID <> 0             
                                            ), cte_Team2 as
                                            (
                                              Select 
                                               c.CustomerID,
                                               c.FirstName,
                                               c.LastName,
                                               pv.Volume50,
                                               pv.Volume51,
                                               pv.Volume52,
                                               pv.Volume53,
                                               pv.Volume54,
                                               pv.Volume90,
                                               ud.Level,
                                               2 as 'team'
                                             From
                                             UniLevelDownline ud
                                             INNER JOIN Customers c
                                             on c.CustomerID = ud.CustomerID
                                             Inner Join PeriodVolumes pv
                                              on pv.CustomerID = c.CustomerID
                                              and pv.PeriodID = @PeriodID
                                              and pv.PeriodTypeID = @PeriodTy
                                             Inner Join cte_Primary team
		                                            on team.Volume51 =  ud.DownlineCustomerID


                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team3 as
                                            (
                                             Select 
                                               c.CustomerID,
                                               c.FirstName,
                                               c.LastName,
                                               pv.Volume50,
                                               pv.Volume51,
                                               pv.Volume52,
                                               pv.Volume53,
                                               pv.Volume54,
                                               pv.Volume90,
                                               ud.Level,
                                               3 as 'team'
                                             From
                                             UniLevelDownline ud
                                             INNER JOIN Customers c
                                             on c.CustomerID = ud.CustomerID
                                             Inner Join PeriodVolumes pv
                                              on pv.CustomerID = c.CustomerID
                                              and pv.PeriodID = @PeriodID
                                              and pv.PeriodTypeID = @PeriodTy
                                             Inner Join cte_Primary team
		                                            on team.Volume52 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team4 as
                                            (
                                             Select 
                                               c.CustomerID,
                                               c.FirstName,
                                               c.LastName,
                                               pv.Volume50,
                                               pv.Volume51,
                                               pv.Volume52,
                                               pv.Volume53,
                                               pv.Volume54,
                                               pv.Volume90,
                                               ud.Level,
                                               4 as 'team'
                                             From
                                             UniLevelDownline ud
                                             INNER JOIN Customers c
                                             on c.CustomerID = ud.CustomerID
                                             Inner Join PeriodVolumes pv
                                              on pv.CustomerID = c.CustomerID
                                              and pv.PeriodID = @PeriodID
                                              and pv.PeriodTypeID = @PeriodTy
                                             Inner Join cte_Primary team
		                                            on team.Volume53 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team5 as
                                            (
                                             Select 
                                               c.CustomerID,
                                               c.FirstName,
                                               c.LastName,
                                               pv.Volume50,
                                               pv.Volume51,
                                               pv.Volume52,
                                               pv.Volume53,
                                               pv.Volume54,
                                               pv.Volume90,
                                               ud.Level,
                                               5 as 'team'
                                             From
                                             UniLevelDownline ud
                                             INNER JOIN Customers c
                                             on c.CustomerID = ud.CustomerID
                                             Inner Join PeriodVolumes pv
                                              on pv.CustomerID = c.CustomerID
                                              and pv.PeriodID = @PeriodID
                                              and pv.PeriodTypeID = @PeriodTy
                                             Inner Join cte_Primary team
		                                            on team.Volume54 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team6 as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                6 as 'team'
                                                From
                                                UniLevelDownline ud
                                                INNER JOIN Customers c
                                                on c.CustomerID = ud.CustomerID
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join cte_Primary team
		                                            on team.Volume90 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0                
               
                                            ), cte_combine as (
                                            Select
                                                Team, CustomerID, 
 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Primary
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team1
                                            UNION
                                            Select
                                                Team, CustomerID,
   
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team2
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team3
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team4
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team5

                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team6
                                            )
                                            select
                                            Team, 
                                            cte_CustomerID = cte.CustomerID, 
  
                                                cte_FirstName = cte.FirstName,
                                                cte_LastName = cte.LastName,
                                            Total = ao.Total,
		                                        BusinessVolumeTotal = ao.BusinessVolumeTotal,
		                                            LastRunDate = CAST(ao.LastRunDate AS date),
		                                            NextRunDate = CAST(ao.NextRunDate AS date),
                                            AutoOrderStatusID
                                            From
                                                cte_combine cte

                                                Inner join AutoOrders ao
                                                on ao.CustomerID = cte.CustomerID
                                            Where cte.CustomerID != @downlinecustomerid 
                                                and AutoOrderStatusID = 0

                                            " + whereClause + @"

                                            order by
                                            " + sortByClause + @"


                                            OFFSET @skip ROWS
                                            FETCH NEXT @take ROWS ONLY
                                            option (maxrecursion 0)", new
                                        {
                                            periodtype = PeriodTypes.Monthly,

                                            skip = request.Skip,
                                            take = request.Take,
                                            downlinecustomerid = Identity.Current.CustomerID
                                        });


                    if (request.Total == 0 || request.FilterObjectWrapper.FilterObjects.Count() > 0)
                    {


                        request.Total = context.Query<int>(@"
                    Declare @PeriodTy int = @periodtype, @CustomerID int = @downlinecustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
                                            ;with cte_Primary as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                0 as 'team'
                                                From
                                                Customers c
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join UniLevelDownline ud
		                                            on ud.DownlineCustomerID = @CustomerID
                                                Where
                                                c.CustomerID = @CustomerID 
  
                                            ), cte_Team1 as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                1 as 'team'
                                                From
                                                UniLevelDownline ud
                                                INNER JOIN Customers c
                                                on c.CustomerID = ud.CustomerID
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join cte_Primary team
		                                            on team.Volume50 =  ud.DownlineCustomerID
		
  
                                            Where ud.DownlineCustomerID <> 0             
                                            ), cte_Team2 as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                2 as 'team'
                                                From
                                                UniLevelDownline ud
                                                INNER JOIN Customers c
                                                on c.CustomerID = ud.CustomerID
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join cte_Primary team
		                                            on team.Volume51 =  ud.DownlineCustomerID


                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team3 as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                3 as 'team'
                                                From
                                                UniLevelDownline ud
                                                INNER JOIN Customers c
                                                on c.CustomerID = ud.CustomerID
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join cte_Primary team
		                                            on team.Volume52 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team4 as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                4 as 'team'
                                                From
                                                UniLevelDownline ud
                                                INNER JOIN Customers c
                                                on c.CustomerID = ud.CustomerID
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join cte_Primary team
		                                            on team.Volume53 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team5 as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                5 as 'team'
                                                From
                                                UniLevelDownline ud
                                                INNER JOIN Customers c
                                                on c.CustomerID = ud.CustomerID
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join cte_Primary team
		                                            on team.Volume54 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0  
                                            ), cte_Team6 as
                                            (
                                                Select 
                                                c.CustomerID,
                                                c.FirstName,
                                                c.LastName,
                                                pv.Volume50,
                                                pv.Volume51,
                                                pv.Volume52,
                                                pv.Volume53,
                                                pv.Volume54,
                                                pv.Volume90,
                                                ud.Level,
                                                6 as 'team'
                                                From
                                                UniLevelDownline ud
                                                INNER JOIN Customers c
                                                on c.CustomerID = ud.CustomerID
                                                Inner Join PeriodVolumes pv
                                                on pv.CustomerID = c.CustomerID
                                                and pv.PeriodID = @PeriodID
                                                and pv.PeriodTypeID = @PeriodTy
                                                Inner Join cte_Primary team
		                                            on team.Volume90 =  ud.DownlineCustomerID



                                            Where ud.DownlineCustomerID <> 0                
               
                                            ), cte_combine as (
                                            Select
                                                Team, CustomerID, 
 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Primary
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team1
                                            UNION
                                            Select
                                                Team, CustomerID,
   
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team2
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team3
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team4
                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team5

                                            UNION
                                            Select
                                                Team, CustomerID, 
                                                FirstName,
                                                LastName
                                            From
                                                cte_Team6
                                            )

                                            select
                                                Count = COUNT(*)
                                            From
                                                cte_combine cte

                                            Inner Join AutoOrders ao
                                            on ao.CustomerID = cte.CustomerID
                                            Where cte.CustomerID != @downlinecustomerid
                                                and AutoOrderStatusID = 0
                                                                    " + whereClause + @"
                                                                    option (maxrecursion 0) "
                                            , new
                                            {
                                                periodtype = PeriodTypes.Monthly,
                                                downlinecustomerid = Identity.Current.CustomerID
                                            }).FirstOrDefault();


                    }

                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
                var results = new List<dynamic>();
                foreach (var item in downlineAutoOrdersList)
                {
                    var newItem = item as IDictionary<String, object>;

                    var value = newItem["cte_CustomerID"];
                    var token = Security.Encrypt(value, Identity.Current.CustomerID);

                    newItem = DynamicHelper.AddProperty(item, "CustomerID" + "Token", token);


                    results.Add(newItem);

                }

                downlineAutoOrdersList = results;

                return new JsonNetResult(new
                {
                    data = downlineAutoOrdersList,
                    total = request.Total
                });
            }
        }


        [Route("~/upcoming-promotions")]
        public ActionResult UpcomingPromotionsList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();

            var sortByClause = KendoUtilitiesCustom.GetSqlOrderByClause(request.SortObjects, new SortObject("ct.CustomerID", "asc"));
            var whereClause = KendoUtilitiesCustom.GetSqlWhereClause(request.FilterObjectWrapper.FilterObjects);
            dynamic upcomingPromotionsList = null;
            // Fetch the data

            using (var context = Exigo.Sql())
            {
                upcomingPromotionsList = context.Query(@"


 Declare @PeriodTy int = @periodtype, @CustomerID int = @id, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   0 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Where
  c.CustomerID = @id 
  ), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
 where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team1 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
   where ed.DownlineCustomerID = @CustomerID
 
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume51 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team2 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume52 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
 where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team3 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume53 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team4 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume54 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume90 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_combine as (
Select
 Team, CustomerID,
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team1
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID  
From
 cte_Team2
UNION
Select
Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team6
)
select
 ct_Team = ct.Team, ct_CustomerID = ct.CustomerID, 
  ct_FirstName = ct.FirstName,
 ct_LastName =  ct.LastName,
 ct_PeriodTypeID =  ct.PeriodTypeID,
  ct_PaidRankID = ct.PaidRankID,
  ct_PeriodID = ct.PeriodID,
 prso_Score = Score, 
                        TotalScore = Score * prso.PaidRankID,
                        prso_RankID = prso.PaidRankID,
                        r_RankDescription = r.RankDescription
From
 cte_combine ct
 Left outer join PeriodRankScores prso 
                            ON prso.PeriodTypeID = ct.PeriodTypeID
                            AND prso.PeriodID = ct.PeriodID 
                            AND prso.CustomerID =ct.CustomerID
                            AND prso.PaidRankID = (select top 1 RankID from Ranks where RankID > ct.PaidRankID)
                         Left outer JOIN Ranks r
                            ON r.RankID = prso.PaidRankID
                        Where ct.CustomerID != @id

	                    AND prso.PaidRankID is not null
	                    AND Score > 50
                        " + whereClause + @"
                        order by 
                        " + sortByClause + @"
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY
                    option (maxrecursion 0)",
                    new
                    {
                        periodtype = PeriodTypes.Monthly,
                        skip = request.Skip,
                        take = request.Take,
                        id = Identity.Current.CustomerID,
                        periodtypeid = PeriodTypes.Default
                    });
            }

            if (request.Total == 0 || request.FilterObjectWrapper.FilterObjects.Count() > 0)
            {
                using (var context = Exigo.Sql())
                {
                    request.Total = context.Query<int>(@"
                
 Declare @PeriodTy int = @periodtype, @CustomerID int = @id, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   0 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Where
  c.CustomerID = @id 
  ), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
 where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team1 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
   where ed.DownlineCustomerID = @CustomerID
 
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume51 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team2 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume52 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
 where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team3 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume53 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team4 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume54 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume90 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   pv.PeriodTypeID,
   pv.PaidRankID,
   pv.PeriodID 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join EnrollerDownline ed
  on ed.CustomerID = c.CustomerID
  where ed.DownlineCustomerID = @CustomerID
 
), cte_combine as (
Select
 Team, CustomerID,
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team1
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID  
From
 cte_Team2
UNION
Select
Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   PeriodTypeID,
   PaidRankID,
   PeriodID 
From
 cte_Team6
)
select
Count(*) as Count
From
 cte_combine ct
 Left outer join PeriodRankScores prso 
                            ON prso.PeriodTypeID = ct.PeriodTypeID
                            AND prso.PeriodID = ct.PeriodID 
                            AND prso.CustomerID =ct.CustomerID
                            AND prso.PaidRankID = (select top 1 RankID from Ranks where RankID > ct.PaidRankID)
                         Left outer JOIN Ranks r
                            ON r.RankID = prso.PaidRankID
                        Where ct.CustomerID != @id

	                    AND prso.PaidRankID is not null
	                    AND Score > 50
                        " + whereClause + @"
                        option (maxrecursion 0)", new
                         {

                             periodtype = PeriodTypes.Monthly,
                             id = Identity.Current.CustomerID,
                             periodtypeid = PeriodTypes.Default
                         }).FirstOrDefault();
                }
            }
            var results = new List<dynamic>();
            foreach (var item in upcomingPromotionsList)
            {
                var newItem = item as IDictionary<String, object>;

                var value = newItem["ct_CustomerID"];
                var token = Security.Encrypt(value, Identity.Current.CustomerID);

                newItem = DynamicHelper.AddProperty(item, "CustomerID" + "Token", token);


                results.Add(newItem);
            }

            upcomingPromotionsList = results;
            return new JsonNetResult(new
            {
                data = upcomingPromotionsList,
                total = request.Total
            });


        }

        [Route("~/recent-activity")]
        public ActionResult RecentActivityList()
        {
            var model = new RecentActivityListViewModel();

            try
            {
                // Get the customer's recent organization activity
                var RecentActivities = Exigo.GetCustomerRecentActivity(new GetCustomerRecentActivityRequest
                {
                    CustomerID = Identity.Current.CustomerID,
                    Page = 1,
                    RowCount = 50
                }).ToList();

                if (RecentActivities == null)
                {
                    ViewBag.ActivityDefault = "No Recent Activity";
                }

                model.RecentActivities.AddRange(RecentActivities);

            }
            catch (Exception exception)
            {
                ViewBag.Error = exception.Message;
            }

            return View(model);
        }

        [Route("~/downline-ranks")]
        public ActionResult DownlineRanksList(KendoGridRequest request = null, int team = 0)
        {

            var teamWhere = (team > 0) ? "AND ct.Team = " + team : "";


            var sortByClause = KendoUtilitiesCustom.GetSqlOrderByClause(request.SortObjects, new SortObject("ct.CustomerID", "asc"));
            var whereClause = KendoUtilitiesCustom.GetSqlWhereClause(request.FilterObjectWrapper.FilterObjects);
            if (Request.HttpMethod.ToUpper() == "GET")
            {
                var viewModel = new DownlineRanksListViewModel();
                using (var context = Exigo.Sql())
                {
                    try
                    {
                        context.Open();
                        viewModel.DownlineRanks = context.Query<Rank, DownlineRankCountViewModel, DownlineRankCountViewModel>(@"

 Declare @PeriodTy int = @periodtype, @CustomerID int = @topcustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   0 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
  
 Where
  c.CustomerID = @topcustomerid  
  ), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team1 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume51 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team2 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume52 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team3 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume53 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team4 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume54 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume90 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team6 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_combine as (
Select
 Team, CustomerID,
   FirstName,
   LastName,
   RankID
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team1
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team2
UNION
Select
Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team6
)
select
  ct.RankID,
  r.RankDescription,
 Total = COUNT(ct.RankID)
From
 cte_combine ct
 LEFT OUTER JOIN Ranks r
 on r.RankID = ct.RankID
 
Where ct.CustomerID != @CustomerID

                            " + teamWhere + @"
                            " + whereClause + @"
 group by
	                         ct.RankID,
  r.RankDescription
                        order by 
                            ct.RankID
                        option (maxrecursion 0)", (rank, model) =>
                             {
                                 model.Rank = rank;
                                 return model;
                             }, new
                             {
                                 periodtype = PeriodTypes.Monthly,
                                 topcustomerid = Identity.Current.CustomerID
                             }, splitOn: "Total").ToList();



                        var teamCount = context.Query(@"
                                                       Select 
	    (
		  CASE WHEN Volume50 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume51 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume52 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume53 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume54 > 0 THEN 1 ELSE 0 END + 
          CASE WHEN Volume90 > 0 THEN 1 ELSE 0 END ) as Count,
		  PBV = pv.Volume2,
		  PCBV = pv.Volume16,
		  TGBV = pv.Volume4
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = 1
		and pv.PeriodTypeID = @periodtype
	Where
		c.CustomerID = @id
                                                         ", new
                                                         {

                                                             periodtype = PeriodTypes.Monthly,
                                                             id = Identity.Current.CustomerID
                                                         }).First();


                        viewModel.TeamCount = teamCount.Count;

                        context.Close();
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                }

                return View(viewModel);
            }

            dynamic downlineRanks = null;

            using (var context = Exigo.Sql())
            {

                downlineRanks = context.Query(@"


                   Declare @PeriodTy int = @periodtype, @CustomerID int = @topcustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   0 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
  
 Where
  c.CustomerID = @topcustomerid 
  ), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team1 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume51 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team2 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume52 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team3 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume53 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team4 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume54 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume90 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team6 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_combine as (
Select
 Team, CustomerID,
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team1
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team2
UNION
Select
Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team6
)
select
 ct_Team = ct.Team, 
ct_CustomerID = ct.CustomerID, 
 ct_FirstName =  ct.FirstName,
 ct_LastName =  ct.LastName,
 ct_RankID =  ct.RankID,
 ct_CreatedDate =   ct.CreatedDate,
 r_RankDescription =   r.RankDescription
From
 cte_combine ct
 LEFT OUTER JOIN Ranks r
 on r.RankID = ct.RankID
 
Where ct.CustomerID != @CustomerID

                            " + teamWhere + @"
                            " + whereClause + @"
 group by
	                        ct.RankID,
	                        r.RankDescription,
							ct.Team,
							ct.CustomerID,
							 ct.FirstName,
                            ct.CreatedDate,
   ct.LastName
                        order by 
                        " + sortByClause + @"
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY                  
                option (maxrecursion 0)", new
             {
                 periodtype = PeriodTypes.Monthly,
                 skip = request.Skip,
                 take = request.Take,
                 topcustomerid = Identity.Current.CustomerID
             }).ToList();

                request.Total = context.Query<int>(@"
                
Declare @PeriodTy int = @periodtype, @CustomerID int = @topcustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   0 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
  
 Where
  c.CustomerID = @topcustomerid  
  ), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team1 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume51 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team2 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume52 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team3 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume53 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team4 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume54 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume90 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   c.CreatedDate,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team6 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_combine as (
Select
 Team, CustomerID,
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team1
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team2
UNION
Select
Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID,
   CreatedDate
From
 cte_Team6
)
select
 Count = COUNT(*)
From
 cte_combine ct
 LEFT OUTER JOIN Ranks r
 on r.RankID = ct.RankID
 
Where ct.CustomerID != @CustomerID                     
                        " + teamWhere + @"   
                        " + whereClause + @"
                        option (maxrecursion 0)", new
                         {


                             periodtype = PeriodTypes.Monthly,
                             topcustomerid = Identity.Current.CustomerID
                         }).FirstOrDefault();

            }

            var results = new List<dynamic>();
            foreach (var item in downlineRanks)
            {
                var newItem = item as IDictionary<String, object>;

                var value = newItem["ct_CustomerID"];
                var token = Security.Encrypt(value, Identity.Current.CustomerID);

                newItem = DynamicHelper.AddProperty(item, "CustomerID" + "Token", token);


                results.Add(newItem);
            }

            downlineRanks = results;

            return new JsonNetResult(new
            {
                data = downlineRanks,
                total = request.Total
            });



        }


        public JsonNetResult GetDownlineRanksPartial(int team = 0)
        {
            var viewModel = new DownlineRanksListViewModel();


            var teamWhere = (team > 0) ? "AND ct.Team = " + team : "";
            using (var context = Exigo.Sql())
            {
                try
                {
                    context.Open();
                    viewModel.DownlineRanks = context.Query<Rank, DownlineRankCountViewModel, DownlineRankCountViewModel>(@"

Declare @PeriodTy int = @periodtype, @CustomerID int = @topcustomerid, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                       
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   0 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
  
 Where
  c.CustomerID = @topcustomerid  
  ), cte_Team1 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   1 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team1 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team2 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume51 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   2 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
 
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team2 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team3 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume52 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   3 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team3 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team4 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume53 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   4 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team4 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team5 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume54 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   5 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team5 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_Team6 as
(
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Primary team
  on team.Volume90 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 UNION ALL
 Select 
   c.CustomerID,
   c.FirstName,
   c.LastName,
   pv.Volume50,
   pv.Volume51,
   pv.Volume52,
   pv.Volume53,
   pv.Volume54,
   pv.Volume90,
   6 as 'team',
   c.RankID
 
 From
  Customers c
 Inner Join PeriodVolumes pv
  
  on pv.CustomerID = c.CustomerID
  and pv.PeriodID = @PeriodID
  and pv.PeriodTypeID = @PeriodTy
 Inner Join cte_Team6 team
  on team.Volume50 =  c.CustomerID
  Inner Join UniLevelDownline ul
  on ul.CustomerID = c.CustomerID
  where ul.DownlineCustomerID = @CustomerID
 
), cte_combine as (
Select
 Team, CustomerID,
   FirstName,
   LastName,
   RankID
From
 cte_Primary
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team1
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team2
UNION
Select
Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team3
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team4
UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team5

UNION
Select
 Team, CustomerID, 
   FirstName,
   LastName,
   RankID
From
 cte_Team6
)
select
  ct.RankID,
  r.RankDescription,
 Total = COUNT(ct.RankID)
From
 cte_combine ct
 LEFT OUTER JOIN Ranks r
 on r.RankID = ct.RankID
 
Where ct.CustomerID != @CustomerID

                            " + teamWhere + @"
 group by
	                         ct.RankID,
  r.RankDescription
                        order by 
                            ct.RankID
                        option (maxrecursion 0)", (rank, model) =>
                         {
                             model.Rank = rank;
                             return model;
                         }, new
                         {
                             periodtype = PeriodTypes.Monthly,
                             topcustomerid = Identity.Current.CustomerID
                         }, splitOn: "Total").ToList();

                    context.Close();
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }
            var html = this.RenderPartialViewToString("Partials/DownlineRanksRanksPartial", viewModel);

            return new JsonNetResult(new
            {
                success = true,
                html = html
            });
        }
        [Route("~/waiting-room")]
        public ActionResult WaitingRoomList()
        {
            var model = new WaitingRoomViewModel();

            // Get the waiting room customers
            model.Customers = new List<WaitingRoomCustomerViewModel>();
            var getWaitingRoomNodesRequest = new GetCustomerWaitingRoomRequest
            {
                EnrollerID = Identity.Current.CustomerID,
                GracePeriod = GlobalSettings.Backoffices.WaitingRooms.GracePeriod
            };
            var waitingRoomNodes = Exigo.GetCustomerWaitingRoom(getWaitingRoomNodesRequest).ToList();
            foreach (var node in waitingRoomNodes)
            {
                model.Customers.Add(new WaitingRoomCustomerViewModel()
                {
                    Customer = node,
                    PlacementExpirationDate = node.EnrollmentDate.AddDays(getWaitingRoomNodesRequest.GracePeriod)
                });
            }


            // Fetch the data
            using (var context = Exigo.Sql())
            {
                model.Sponsors = context.Query<WaitingRoomSponsorViewModel>(@"
                    -- Available Sponsors
                    SELECT 
                          c.CustomerID
	                    , c.FirstName
	                    , c.LastName
                        , c.Company
                    FROM
	                    UniLevelDownline d
	                    INNER JOIN Customers c
		                    ON c.CustomerID = d.CustomerID   
                    WHERE
	                    d.DownlineCustomerID = @downlinecustomerid
                    ORDER BY
                        c.LastName
                ", new
                 {
                     downlinecustomerid = Identity.Current.CustomerID,
                     graceperioddays = GlobalSettings.Backoffices.WaitingRooms.GracePeriod,
                     customertypeids = new List<int> { CustomerTypes.Associate }
                 }).ToList();
            }


            // Return the data
            return View(model);
        }
        [HttpPost]
        public JsonNetResult PlaceWaitingRoomCustomer(int customerid, int sponsorid)
        {
            try
            {
                Exigo.PlaceUniLevelCustomer(new PlaceUniLevelCustomerRequest
                {
                    CustomerID = customerid,
                    ToSponsorID = sponsorid,
                    Reason = "Waiting room placement"
                });

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [Route("~/new-distributors")]
        public ActionResult NewDistributorsList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();


            var sortByClause = KendoUtilitiesCustom.GetSqlOrderByClause(request.SortObjects, new SortObject("Level", "asc"));
            var whereClause = KendoUtilitiesCustom.GetSqlWhereClause(request.FilterObjectWrapper.FilterObjects);
            dynamic newDistributorsList = null;
            // Fetch the data

            try
            {
                using (var context = Exigo.Sql())
                {
                    newDistributorsList = context.Query(@"
                Declare @PeriodTy int = @periodtype, @CustomerID int = @id, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)

;with cte_Primary as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  0 as 'team'
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
		Inner Join UniLevelDownline ud
		on ud.DownlineCustomerID = @CustomerID
	Where
		c.CustomerID = @CustomerID 
), cte_Team1 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  1 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume50 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team2 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  2 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume51 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team3 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  3 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume52 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team4 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  4 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume53 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team5 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  5 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume54 =  ud.DownlineCustomerID

Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team6 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  6 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume90 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_combine as (
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	Level
	
From
 cte_Primary
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team1
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team2
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team3
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team4
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team5

UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team6

)
select
	CustomerID = CustomerID,
    FirstName    = FirstName,
    LastName     = LastName,
    CreatedDate  = CreatedDate,
	Team = Team,
	  Level
From
	cte_combine
	
 Where CreatedDate >= GetDate() - 120
and CustomerID != @CustomerID


"

                          + whereClause + @"
                        order by 
                        Team,
                        " + sortByClause + @"
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY
option (maxrecursion 0)"


                    , new
                    {
                        periodtype = PeriodTypes.Monthly,
                        skip = request.Skip,
                        take = request.Take,
                        id = Identity.Current.CustomerID
                    });

                    if (request.Total == 0 || request.FilterObjectWrapper.FilterObjects.Count() > 0)
                    {
                        request.Total = context.Query<int>(@"
                           Declare @PeriodTy int = @periodtype, @CustomerID int = @id, @PeriodID int = (SELECT periodid                                                                     
						                    FROM   periods                                                                     
						                    WHERE  PeriodTypeID = @periodtype                                                                        
						                    AND Getdate() >= StartDate                                                                         
						                    AND Getdate() < EndDate + 1)
;with cte_Primary as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  0 as 'team'
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
		Inner Join UniLevelDownline ud
		on ud.DownlineCustomerID = @CustomerID
	Where
		c.CustomerID = @CustomerID 
), cte_Team1 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  1 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume50 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team2 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  2 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume51 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team3 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  3 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume52 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team4 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  4 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume53 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team5 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  5 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume54 =  ud.DownlineCustomerID

Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_Team6 as
(
	Select 
	  c.CustomerID,
	  c.FirstName,
	  c.LastName,
	  c.CreatedDate,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  ud.Level,
	  6 as 'team'
	From
		UniLevelDownline ud
	Inner Join Customers c
	on c.CustomerID = ud.CustomerID
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume90 =  ud.DownlineCustomerID
	
Where ud.DownlineCustomerID <> 0
 and c.CreatedDate >= GetDate() - 120
), cte_combine as (
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	Level
	
From
 cte_Primary
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team1
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team2
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team3
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team4
UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team5

UNION
Select
	Team, CustomerID, CreatedDate,FirstName,LastName,
	  Level
From
	cte_Team6

)
select
	Count(*)
From
	cte_combine
	
 Where CreatedDate >= GetDate() - 120
and CustomerID != @CustomerID


"

                              + whereClause + @"


                option (maxrecursion 0)", new
                         {

                             periodtype = PeriodTypes.Monthly,
                             id = Identity.Current.CustomerID
                         }).FirstOrDefault();
                    }
                }

            }
            catch (Exception e)
            {
                Console.Write(e);
            }
            var results = new List<dynamic>();
            foreach (var item in newDistributorsList)
            {
                var newItem = item as IDictionary<String, object>;

                var value = newItem["CustomerID"];
                var token = Security.Encrypt(value, Identity.Current.CustomerID);

                newItem = DynamicHelper.AddProperty(item, "CustomerID" + "Token", token);


                results.Add(newItem);
            }

            newDistributorsList = results;
            return new JsonNetResult(new
             {
                 data = newDistributorsList,
                 total = request.Total
             });

        }

        [Route("~/organization/viewer")]
        public ActionResult OrganizationViewer()
        {
            return View();
        }

        [Route("~/organization/browser")]
        public ActionResult OrganizationBrowser(string token = "")
        {
            // Determine whose tree to look at
            var topCustomerID = (token.IsNotNullOrEmpty()) ? Convert.ToInt32(Security.Decrypt(token, Identity.Current.CustomerID)) : Identity.Current.CustomerID;


            var model = new OrganizationBrowserViewModel();

            // Get the top customer
            model.TopCustomer = Exigo.GetCustomer(topCustomerID);

            // Get the first set of data for their first level
            model.Customers = new List<Customer>();

            var context = Exigo.OData();
            var nodes = context.UniLevelTree.Expand("Customer")
                .Where(c => c.TopCustomerID == Identity.Current.CustomerID)
                .Where(c => c.Level == 1)
                .Skip(0)
                .Take(6)
                .ToList();

            model.Customers = nodes.Select(c => (Customer)c.Customer).ToList();


            return View(model);
        }

        #endregion

        #region My Team
        [Route("~/myteam")]
        public ActionResult MyTeam(int customerid = 0)
        {
            var model = new MyTeamViewModel();
            if (customerid == 0)
            {
                model.CurrentTeamMember = Exigo.GetCustomer(Identity.Current.CustomerID);
                customerid = Identity.Current.CustomerID;
            }

            model.Periods = FetchPeriods(customerid);
            return View(model);
        }

        public JsonNetResult FetchMyTeamDashboard(int id, int team = 1, int period = 0)
        {
            var model = new MyTeamViewModel();

            var teams = 0;
            var rand = new Random();
            var teamcount = 0;
            dynamic teamTGBV = null;

            var periodstring = (period > 0) ? " " + period + " " : "(SELECT periodid FROM periods WHERE  PeriodTypeID = @periodtype AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";
            var periodstartstring = (period > 0) ? "(SELECT StartDate FROM periods  WHERE  PeriodTypeID = @periodtype  AND PeriodID = " + period + " )" : "(SELECT StartDate FROM periods  WHERE  PeriodTypeID = @periodtype  AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";
            var periodendstring = (period > 0) ? "(SELECT EndDate FROM periods  WHERE  PeriodTypeID = @periodtype  AND PeriodID = " + period + " )" : "(SELECT EndDate FROM periods  WHERE  PeriodTypeID = @periodtype  AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";

            try
            {
                using (var context = Exigo.Sql())
                {

                    teamTGBV = context.Query(@"
    
    Declare @periodty int = @periodtype, @CustomerID int = @id, @PeriodStart date = " + periodstartstring + @",
											@PeriodEnd date = " + periodendstring + @",
                                            @PeriodID int = " + periodstring + @"
                                            
;with cte_Primary as
(
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID, 
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  pv.Volume54,
	  0 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Where
		c.CustomerID = @CustomerID 
), cte_Team1 as
(
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  1 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
	
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume50 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  1 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team1 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team2 as
(
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  2 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume51 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  2 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
	
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team2 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team3 as
(
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  3 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume52 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  3 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team3 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team4 as
(
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  4 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume53 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  4 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team4 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team5 as
(
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  5 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume54 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  5 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team5 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team6 as
(
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  6 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume90 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  c.CustomerTypeID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
      pv.Volume92,
      pv.Volume93,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
      c.CreatedDate,
	  6 as 'team',
      pv.Volume2,
  pv.Volume16,
 pv.Volume67, 
 pv.Volume68, 
 pv.Volume69, 
 pv.Volume70, 
 pv.Volume71,
 pv.Volume74, 
 pv.Volume75, 
 pv.Volume76, 
 pv.Volume77, 
 pv.Volume78 
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team6 team
		on team.Volume50 =  c.CustomerID
	
), cte_combine as (
Select
	Team, CustomerID,CustomerTypeID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      CreatedDate,
	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91,
      Volume92,
      Volume93,
Volume67, 
  Volume68, 
  Volume69, 
  Volume70, 
  Volume71,
  Volume74, 
  Volume75, 
  Volume76, 
  Volume77, 
  Volume78 
From
 cte_Primary
UNION
Select
	Team, CustomerID,CustomerTypeID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      CreatedDate,
	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91,
      Volume92,
      Volume93,
Volume67, 
  Volume68, 
  Volume69, 
  Volume70, 
  Volume71,
  Volume74, 
  Volume75, 
  Volume76, 
  Volume77, 
  Volume78 
From
	cte_Team1
UNION
Select
	Team, CustomerID,CustomerTypeID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      CreatedDate,
	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91,
      Volume92,
      Volume93,
Volume67, 
  Volume68, 
  Volume69, 
  Volume70, 
  Volume71,
  Volume74, 
  Volume75, 
  Volume76, 
  Volume77, 
  Volume78 
From
	cte_Team2
UNION
Select
	Team, CustomerID,CustomerTypeID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      CreatedDate,
	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91,
      Volume92,
      Volume93,
Volume67, 
  Volume68, 
  Volume69, 
  Volume70, 
  Volume71,
  Volume74, 
  Volume75, 
  Volume76, 
  Volume77, 
  Volume78 
From
	cte_Team3
UNION
Select
	Team, CustomerID,CustomerTypeID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      CreatedDate,
	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91,
      Volume92,
      Volume93,
Volume67, 
  Volume68, 
  Volume69, 
  Volume70, 
  Volume71,
  Volume74, 
  Volume75, 
  Volume76, 
  Volume77, 
  Volume78 
From
	cte_Team4
UNION
Select
	Team, CustomerID,CustomerTypeID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      CreatedDate,
	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91,
      Volume92,
      Volume93,
Volume67, 
  Volume68, 
  Volume69, 
  Volume70, 
  Volume71,
  Volume74, 
  Volume75, 
  Volume76, 
  Volume77, 
  Volume78 
From
	cte_Team5

UNION
Select
	Team, CustomerID,CustomerTypeID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      CreatedDate,
	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91,
      Volume92,
      Volume93,
Volume67, 
  Volume68, 
  Volume69, 
  Volume70, 
  Volume71,
  Volume74, 
  Volume75, 
  Volume76, 
  Volume77, 
  Volume78 
From
	cte_Team6
)
select *, Team1Depth =(Select Count(*) from cte_combine where CreatedDate > @PeriodStart and CreatedDate < @PeriodEnd and Team = '1' and CustomerTypeID = @customertype),
Team2Depth =(Select Count(*) from cte_combine where CreatedDate > @PeriodStart and CreatedDate < @PeriodEnd and Team = '2' and CustomerTypeID = @customertype),
Team3Depth =(Select Count(*) from cte_combine where CreatedDate > @PeriodStart and CreatedDate < @PeriodEnd and Team = '3' and CustomerTypeID = @customertype ),
Team4Depth =(Select Count(*) from cte_combine where CreatedDate > @PeriodStart and CreatedDate < @PeriodEnd and Team = '4' and CustomerTypeID = @customertype),
Team5Depth =(Select Count(*) from cte_combine where CreatedDate > @PeriodStart and CreatedDate < @PeriodEnd and Team = '5' and CustomerTypeID = @customertype),
Team6Depth =(Select Count(*) from cte_combine where CreatedDate > @PeriodStart and CreatedDate < @PeriodEnd and Team = '6' and CustomerTypeID = @customertype),
		  (
		  CASE WHEN Volume50 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume51 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume52 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume53 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume54 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume90 > 0 THEN 1 ELSE 0 END ) as t_TeamCount


From
	cte_combine
	where CustomerID = @CustomerID
Order By
 team, CustomerID
            option (maxrecursion 0)", new
             {
                 customertype = CustomerTypes.Associate,
                 periodtype = PeriodTypes.Monthly,
                 id = id
             }).FirstOrDefault();
                    teamcount = teamTGBV.t_TeamCount;
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
            while (teams != teamcount)
            {
                decimal depth = 0;
                decimal tgbv = 0;
                decimal newassociates = 0;
                decimal smartshoppers = 0;

                if (teams == 0)
                {
                    depth = Convert.ToDecimal(teamTGBV.Team1Depth);
                    tgbv = Convert.ToDecimal(teamTGBV.Volume55);
                    newassociates = Convert.ToDecimal(teamTGBV.Volume67);
                    smartshoppers = Convert.ToDecimal(teamTGBV.Volume74);
                }
                else if (teams == 1)
                {

                    depth = Convert.ToDecimal(teamTGBV.Team2Depth);
                    tgbv = Convert.ToDecimal(teamTGBV.Volume56);
                    newassociates = Convert.ToDecimal(teamTGBV.Volume68);
                    smartshoppers = Convert.ToDecimal(teamTGBV.Volume75);
                }
                else if (teams == 2)
                {

                    depth = Convert.ToDecimal(teamTGBV.Team3Depth);
                    tgbv = Convert.ToDecimal(teamTGBV.Volume57);
                    newassociates = Convert.ToDecimal(teamTGBV.Volume69);
                    smartshoppers = Convert.ToDecimal(teamTGBV.Volume76);
                }
                else if (teams == 3)
                {

                    depth = Convert.ToDecimal(teamTGBV.Team4Depth);
                    tgbv = Convert.ToDecimal(teamTGBV.Volume58);
                    newassociates = Convert.ToDecimal(teamTGBV.Volume70);
                    smartshoppers = Convert.ToDecimal(teamTGBV.Volume77);

                }
                else if (teams == 4)
                {

                    depth = Convert.ToDecimal(teamTGBV.Team5Depth);
                    tgbv = Convert.ToDecimal(teamTGBV.Volume59);
                    newassociates = Convert.ToDecimal(teamTGBV.Volume71);
                    smartshoppers = Convert.ToDecimal(teamTGBV.Volume78);

                }
                else if (teams == 5)
                {

                    depth = Convert.ToDecimal(teamTGBV.Team6Depth);
                    tgbv = Convert.ToDecimal(teamTGBV.Volume91);
                    newassociates = Convert.ToDecimal(teamTGBV.Volume92);
                    smartshoppers = Convert.ToDecimal(teamTGBV.Volume93);

                }
                var Team = new Team
                {
                    Depth = depth,

                    NewAssociates = newassociates,
                    SmartShoppers = smartshoppers,
                    TGBV = tgbv,
                    TeamNumber = teams + 1
                };

                model.Teams.Add(Team);
                teams++;
            }
            try
            {
                model.CurrentTeamMember = Exigo.GetCustomer(id);
                var html = this.RenderPartialViewToString("Partials/MyTeamDashboard", model);
                return new JsonNetResult(new
                {
                    success = true,
                    html = html
                });
            }
            catch (Exception e)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    error = e.Message
                });
            }

        }

        public ActionResult FetchMyTeamPeriodsView(int id = 0, int period = 0)
        {
            var model = new MyTeamViewModel();
            model.Periods = FetchPeriods(id);
            model.CurrentTeamMember = Exigo.GetCustomer(id);

            model.CurrentPeriod = (period > 0) ? period : model.Periods.OrderByDescending(c => c.StartDate).First().PeriodID;

            var html = this.RenderPartialViewToString("Partials/MyPeriodsPartial", model);
            return new JsonNetResult(new
            {
                success = true,
                html = html
            });

        }

        public ActionResult FetchMyTeamCount(int id = 0, int period = 0)
        {
            var model = new MyTeamViewModel();


            var periodstring = (period > 0) ? " " + period + " " : "(SELECT periodid FROM periods WHERE  PeriodTypeID = @periodtype AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";


            model.CurrentPeriod = period;

            using (var context = Exigo.Sql())
            {

                var teamCount = context.Query(@"Select 
	    (
		  CASE WHEN Volume50 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume51 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume52 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume53 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume54 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume90 > 0 THEN 1 ELSE 0 END ) as Count,
		  PBV = pv.Volume2,
		  PCBV = pv.Volume16,
		  TGBV = pv.Volume4
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = " + periodstring + @"
		and pv.PeriodTypeID = @periodtype
	Where
		c.CustomerID = @id
		

            ", new
             {
                 periodtype = PeriodTypes.Monthly,
                 id = id
             }).First();

                return new JsonNetResult(new
                {
                    success = true,
                    count = teamCount.Count
                });

            }
        }

        public JsonNetResult IsInMyTeam(int periodid, int currentcustomerid, string input = "")
        {


            var whereclause = "";
            int customerid = 0;
            if (input.IsNullOrEmpty()) return new JsonNetResult(new
            {
                success = false,
                message = "No value entered"
            });

            if (int.TryParse(input, out customerid))
            {
                whereclause = "AND c.CustomerID = " + customerid;
            }
            else
            {
                whereclause = "AND c.LastName = '" + input + "'";
            }
            try
            {
                using (var context = Exigo.Sql())
                {

                    var isInTeam = context.Query(@"

select
    c_CustomerID = c.CustomerID,
    c_SponsorID = c.SponsorID,
    c_LastName = c.LastName,
    c_FirstName = c.FirstName,
    CustomerStatusDescription,
    CustomerTypeDescription
From
	UniLevelDownline ud
Inner join Customers c on
ud.CustomerID = c.CustomerID
inner join CustomerStatuses cs
on cs.CustomerStatusID = c.CustomerStatusID
inner join CustomerTypes ct
on ct.CustomerTypeID = c.CustomerTypeID
Where c.CustomerID != @currentID
and ud.DownlineCustomerID = @currentID
" + whereclause + @"

 ", new
  {
      periodtype = PeriodTypes.Monthly,
      id = customerid,
      currentID = Identity.Current.CustomerID,
      period = periodid
  }).ToList();

                    var model = new List<Customer>();
                    if (isInTeam.Count > 1)
                    {
                        foreach (var cust in isInTeam)
                        {
                            var customer = new Customer();
                            customer.FirstName = cust.c_FirstName;
                            customer.LastName = cust.c_LastName;
                            customer.CustomerID = cust.c_CustomerID;
                            customer.CustomerStatus = new CustomerStatus();
                            customer.CustomerType = new CustomerType();
                            customer.CustomerStatus.CustomerStatusDescription = cust.CustomerStatusDescription;

                            customer.CustomerType.CustomerTypeDescription = cust.CustomerTypeDescription;
                            model.Add(customer);
                        }
                    }

                    if (isInTeam != null)
                    {
                        return new JsonNetResult(new
                        {
                            success = true,
                            multiple = (isInTeam.Count > 1),
                            customerid = isInTeam.FirstOrDefault().c_CustomerID,
                            resultsview = this.RenderPartialViewToString("Partials/MyTeamMultiplePeoplePartial", model)
                        });
                    }
                    else
                    {
                        return new JsonNetResult(new
                        {
                            success = false,
                            message = "No user found in any of your teams"
                        });
                    }


                }
            }
            catch (Exception e)
            {
                Console.Write(e);
                return new JsonNetResult(new
                {
                    success = false,
                    message = "No user found in any of your teams"
                });

            }
        }

        public ActionResult FetchMyTeamTeamSelector(int id = 0, int period = 0)
        {
            var model = new MyTeamViewModel();


            var periodstring = (period > 0) ? " " + period + " " : "(SELECT periodid FROM periods WHERE  PeriodTypeID = @periodtype AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";


            model.CurrentPeriod = period;

            using (var context = Exigo.Sql())
            {

                var teamCount = context.Query(@"Select 
	    (
		  CASE WHEN Volume50 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume51 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume52 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume53 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume54 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume90 > 0 THEN 1 ELSE 0 END ) as Count,
		  PBV = pv.Volume2,
		  PCBV = pv.Volume16,
		  TGBV = pv.Volume4
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = " + periodstring + @"
		and pv.PeriodTypeID = @periodtype
	Where
		c.CustomerID = @id
		

            ", new
             {
                 periodtype = PeriodTypes.Monthly,
                 id = id
             }).First();
                try
                {
                    model.CurrentTeamMember = Exigo.GetCustomer(id);
                    model.CurrentTeamMemberTeamCount = teamCount.Count;

                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
                var html = this.RenderPartialViewToString("Partials/MyTeamTeamSelector", model);
                return new JsonNetResult(new
                {
                    success = true,
                    html = html
                });

            }
        }

        public ActionResult FetchMyTeamTable(KendoGridRequest request = null, int id = 0, int team = 1, int period = 0)
        {
            var model = new MyTeamViewModel();


            var periodstring = (period > 0) ? " " + period + " " : "(SELECT periodid FROM periods WHERE  PeriodTypeID = @periodtype AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";
            if (Request.HttpMethod.ToUpper() == "GET")
            {

                model.CurrentPeriod = period;

                using (var context = Exigo.Sql())
                {

                    var teamCount = context.Query(@"Select 
	    (
		  CASE WHEN Volume50 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume51 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume52 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume53 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume54 > 0 THEN 1 ELSE 0 END +
		    CASE WHEN Volume90 > 0 THEN 1 ELSE 0 END ) as Count,
		  PBV = pv.Volume2,
		  PCBV = pv.Volume16,
		  TGBV = pv.Volume4
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = " + periodstring + @"
		and pv.PeriodTypeID = @periodtype
	Where
		c.CustomerID = @id
		

            ", new
             {
                 periodtype = PeriodTypes.Monthly,
                 id = id,
                 periodstring
             }).First();
                    try
                    {
                        model.CurrentTeamMember = Exigo.GetCustomer(id);
                        model.CurrentTeamMemberTeamCount = teamCount.Count;

                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                    return PartialView("Partials/MyTeamTable", model);

                }
            };
            if (id == 0) id = Identity.Current.CustomerID;
            dynamic data;
            var sortByClause = KendoUtilitiesCustom.GetSqlOrderByClause(request.SortObjects, new SortObject("c.CustomerID", "asc"));
            var whereClause = KendoUtilitiesCustom.GetSqlWhereClause(request.FilterObjectWrapper.FilterObjects);

            periodstring = (period > 0) ? " " + period + " " : "(SELECT periodid FROM periods WHERE  PeriodTypeID = @periodtype AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";
            var distributorCustomerTypes = new List<int> { (int)CustomerTypes.SmartShopper, (int)CustomerTypes.RetailCustomer, (int)CustomerTypes.Associate };
            try
            {
                if (request.Take == 0) request.Take = 50;

                using (var context = Exigo.Sql())
                {
                    data = context.Query(@"
      Declare @periodty int = @periodtype, @CustomerID int = @id, @PeriodID int = " + periodstring + @"
;with cte_Primary as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  0 as 'team',
      pv.Volume2,
  pv.Volume16

	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Where
		c.CustomerID = @CustomerID 
), cte_Team1 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  1 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
	
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume50 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  1 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team1 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team2 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  2 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume51 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  2 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
	
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team2 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team3 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  3 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume52 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  3 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team3 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team4 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  4 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume53 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  4 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team4 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team5 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  5 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume54 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  5 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team5 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team6 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  6 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume90 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
	  pv.Volume55,
	  pv.Volume56,
	  pv.Volume57,
	  pv.Volume58,
	  pv.Volume59,
      pv.Volume90,
      pv.Volume91,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  6 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team6 team
		on team.Volume50 =  c.CustomerID
	
), cte_combine as (
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16,

	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91
From
 cte_Primary
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16,

	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91
From
	cte_Team1
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16,

	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91
From
	cte_Team2
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16,

	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91
From
	cte_Team3
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16,

	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91
From
	cte_Team4
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16,

	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91
From
	cte_Team5

UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16,

	  Volume55,
	  Volume56,
	  Volume57,
	  Volume58,
	  Volume59,
      Volume90,
      Volume91
From
	cte_Team6
)
Select
 c_CustomerID = cte.CustomerID,
    c_SponsorID = cte.SponsorID,
	  c_EnrollerID = cte.EnrollerID,
	  c_FirstName = cte.FirstName,
	  c_LastName = cte.LastName,
      v_PBV = cte.Volume2,
      v_PCBV =  cte.Volume16,

	  v_Team1TGBV = cte.Volume55,
	  v_Team2TGBV = cte.Volume56,
	  v_Team3TGBV = cte.Volume57,
	  v_Team4TGBV = cte.Volume58,
	  v_Team5TGBV = cte.Volume59,
      v_Team6TGBV = cte.Volume91,
    ud.Level,
		  (
		  CASE WHEN Volume50 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume51 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume52 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume53 > 0 THEN 1 ELSE 0 END +
		  CASE WHEN Volume54 > 0 THEN 1 ELSE 0 END +  
          CASE WHEN Volume90 > 0 THEN 1 ELSE 0 END) as t_TeamCount
From
	cte_combine cte
	Inner join UniLevelDownline ud
	on cte.CustomerID = ud.CustomerID
	Where ud.DownlineCustomerID = @id
and cte.CustomerID != @id 
AND Team = @teamnumber
                       
                     Order by Level
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY
            option (maxrecursion 0)", new
             {
                 periodtype = PeriodTypes.Monthly,
                 skip = request.Skip,
                 take = request.Take,
                 teamnumber = team,
                 id = id
             }).ToList();


                }

                if (request.Total == 0 || request.FilterObjectWrapper.FilterObjects.Count() > 0)
                {
                    using (var context = Exigo.Sql())
                    {
                        request.Total = context.Query<int>(@"
                                 Declare @periodty int = @periodtype, @CustomerID int = @id, @PeriodID int =" + periodstring + @"
;with cte_Primary as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  pv.Volume54,
	  0 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Where
		c.CustomerID = @CustomerID 
), cte_Team1 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  1 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
	
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume50 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  1 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team1 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team2 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  2 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume51 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  2 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
	
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team2 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team3 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  3 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume52 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  3 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team3 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team4 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  4 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume53 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  4 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team4 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team5 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  5 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume54 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  5 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team5 team
		on team.Volume50 =  c.CustomerID
	
), cte_Team6 as
(
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  6 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Primary team
		on team.Volume90 =  c.CustomerID
	UNION ALL
	Select 
	  c.CustomerID,
	  pv.Volume50,
	  pv.Volume51,
	  pv.Volume52,
	  pv.Volume53,
	  pv.Volume54,
      pv.Volume90,
	  c.SponsorID,
	  c.EnrollerID,
	  c.FirstName,
	  c.LastName,
	  6 as 'team',
      pv.Volume2,
  pv.Volume16
	From
		Customers c
	Inner Join PeriodVolumes pv
		
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = @PeriodID
		and pv.PeriodTypeID = @PeriodTy
	Inner Join cte_Team6 team
		on team.Volume50 =  c.CustomerID
	
), cte_combine as (
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16
From
 cte_Primary
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16
From
	cte_Team1
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16
From
	cte_Team2
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16
From
	cte_Team3
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16
From
	cte_Team4
UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16
From
	cte_Team5

UNION
Select
	Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54, SponsorID,
	  EnrollerID,
	  FirstName,
	  LastName,
      Volume2,
  Volume16
From
	cte_Team6
)
select
  Count = COUNT(*)
From
	cte_combine
Where CustomerID != @id 
AND Team = @teamnumber
" +
                        whereClause + @"
                        option (maxrecursion 0)", new
                         {
                             periodtype = PeriodTypes.Monthly,
                             teamnumber = team,
                             id = id
                         }).FirstOrDefault();
                    }
                }
                var results = new List<dynamic>();
                foreach (var item in data)
                {
                    var newItem = item as IDictionary<String, object>;

                    var value = newItem["c_CustomerID"];
                    var token = Security.Encrypt(value, Identity.Current.CustomerID);

                    newItem = DynamicHelper.AddProperty(item, "CustomerID" + "Token", token);


                    results.Add(newItem);
                }

                data = results;
                return new JsonNetResult(new
                {
                    data = data,
                    total = request.Total
                });
            }
            catch (Exception e)
            {
                Console.Write(e);
                data = null;
                return new JsonNetResult(new
                {
                    data = data,
                    total = request.Total
                });
            }

        }

        public JsonNetResult FetchMyTeamCurrentTeamMember(int id, int period = 0)
        {
            var model = new MyTeamViewModel();
            var periodstring = (period > 0) ? " " + period + " " : "(SELECT periodid FROM periods WHERE  PeriodTypeID = @periodtype AND Getdate() >= StartDate AND Getdate() < EndDate + 1)";

            using (var context = Exigo.Sql())
            {

                var volumes = context.Query(@"Select 
	    
		  PBV = pv.Volume2,
		  PCBV = pv.Volume16,
		  TGBV = pv.Volume4
	From
		Customers c
	Inner Join PeriodVolumes pv
		on pv.CustomerID = c.CustomerID
		and pv.PeriodID = " + periodstring + @"
		and pv.PeriodTypeID = @periodtype
	Where
		c.CustomerID = @id
		

            ", new
             {
                 periodtype = PeriodTypes.Monthly,
                 id = id
             }).First();

                model.CurrentTeamMemberPBV = volumes.PBV;
                model.CurrentTeamMemberPCBV = volumes.PCBV;

                model.CurrentTeamMemberTGBV = volumes.TGBV;
            }
            model.CurrentTeamMember = Exigo.GetCustomer(id);


            var html = this.RenderPartialViewToString("Partials/MyTeamCurrentTeamMember", model);
            return new JsonNetResult(new
            {
                success = true,
                html = html
            });
        }

        public List<Period> FetchPeriods(int id)
        {
            var model = new MyTeamViewModel();



            using (var context = Exigo.Sql())
            {

                var periods = context.Query<Period>(@" 
	            Select 
	    
		            p.PeriodID,
		            p.PeriodTypeID,
		            p.PeriodDescription,
		            p.StartDate,
		            p.EndDate
	            From
		            Periods p
	            Inner Join Customers c
		            on p.EndDate >= c.CreatedDate
	            Where
		            c.CustomerID = @id
		            and p.PeriodTypeID = @periodtype
		            and p.StartDate < GetDate()
            ", new
             {
                 periodtype = PeriodTypes.Monthly,
                 id = id
             }).ToList();

                return periods;
            }

        }
        #endregion



        #region Models and Enums

        public class SearchResult
        {
            public int CustomerID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        #endregion
    }
}
