using Common;
using Common.Api.ExigoWebService;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static Customer GetCustomer(int customerID)
        {
            var customer = Exigo.OData().Customers.Expand("CustomerStatus,CustomerType,Rank,Enroller,Sponsor")
                .Where(c => c.CustomerID == customerID)
                .FirstOrDefault();
            if (customer == null) return null;

            return (Customer)customer;
        }
        public static IEnumerable<CustomerWallItem> GetCustomerRecentActivity(GetCustomerRecentActivityRequest request)
        {
            var query = Exigo.OData().CustomerWall
                .Where(c => c.CustomerID == request.CustomerID);

            if (request.StartDate != null)
            {
                query = query.Where(c => c.EntryDate >= request.StartDate);
            }

            var items = query
                .OrderByDescending(c => c.EntryDate)
                .Select(c => c)
                .Skip(request.Skip)
                .Take(request.Take);


            foreach (var item in items)
            {
                var wallItem = (CustomerWallItem)item;
                yield return wallItem;
            }
        }

        public static CustomerStatus GetCustomerStatus(int customerStatusID)
        {
            var customerStatus = Exigo.OData().CustomerStatuses
                .Where(c => c.CustomerStatusID == customerStatusID)
                .FirstOrDefault();
            if (customerStatus == null) return null;

            return (CustomerStatus)customerStatus;
        }
        public static CustomerType GetCustomerType(int customerTypeID)
        {
            var customerType = Exigo.OData().CustomerTypes
                .Where(c => c.CustomerTypeID == customerTypeID)
                .FirstOrDefault();
            if (customerType == null) return null;

            return (CustomerType)customerType;
        }

        public static void SetCustomerPreferredLanguage(int customerID, int languageID)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = customerID,
                LanguageID = languageID
            });

            var language = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.LanguageID == languageID).FirstOrDefault();
            if (language != null)
            {
                GlobalUtilities.SetCurrentUICulture(language.CultureCode);
            }
        }
        
        public static bool IsEmailAvailable(string email)
        {
            // Validate the email address
            return Exigo.OData().Customers
                .Where(c => c.Email == email)
                .Count() == 0;
        }
        public static bool IsEmailAvailable(int customerID, string email)
        {
            // Validate the email address
            return Exigo.OData().Customers
                .Where(c => c.CustomerID != customerID)
                .Where(c => c.Email == email)
                .Count() == 0;
        }
        public static bool IsLoginNameAvailable(string loginname, int customerID = 0)
        {
            // Ensure that the login name entered actually has a value
            if (loginname.IsNullOrEmpty())
            {
                return false;
            }

            if (customerID > 0)
            {
                // Get the current login name to see if it matches what we passed. If so, it's still valid.
                var currentLoginName = Exigo.GetCustomer(customerID).LoginName;
                if (loginname.Equals(currentLoginName, StringComparison.InvariantCultureIgnoreCase)) return true;
            }

            var apiCustomer = Exigo.WebService().GetCustomers(new GetCustomersRequest()
            {
                LoginName = loginname
            }).Customers.FirstOrDefault();

            return (apiCustomer == null);
        }
        public static bool IsTaxIDAvailable(string taxID, int taxIDType = 0, int customerID = 0)
        {
            if (taxIDType == 0)
            {
                taxIDType = (int)TaxIDType.SSN;
            }

            var request = new IsTaxIDAvailableValidateRequest { TaxID = taxID, TaxTypeID = taxIDType };
            if (customerID > 0)
            {
                request.ExcludeCustomerID = customerID;
            }
            var validResponse = Exigo.WebService().Validate(request);

            return validResponse.IsValid;
        }

        public static void SendEmailVerification(int customerID, string email, string body = "")
        {
            // Create the publicly-accessible verification link
            string sep = "&";
            if (!GlobalSettings.Emails.VerifyEmailUrl.Contains("?")) sep = "?";

            string encryptedValues = Security.Encrypt(new
            {
                CustomerID = customerID,
                Email = email,
                Date = DateTime.Now
            });

            var verifyEmailUrl = GlobalSettings.Emails.VerifyEmailUrl + sep + "token=" + encryptedValues;


            // Send the email
            Exigo.SendEmail(new SendEmailRequest
            {
                To = new[] { email },
                From = GlobalSettings.Emails.NoReplyEmail,
                ReplyTo = new[] { GlobalSettings.Emails.NoReplyEmail },
                SMTPConfiguration = GlobalSettings.Emails.SMTPConfigurations.Default,
                Subject = "{0} - Verify your email".FormatWith(GlobalSettings.Company.Name),
                Body = (body.IsNullOrEmpty()) ? @"
                    <p>
                        {1} has received a request to enable this email account to receive email notifications from {1} and your upline.
                    </p>

                    <p> 
                        To confirm this email account, please click the following link:<br />
                        <a href='{0}'>{0}</a>
                    </p>

                    <p>
                        If you did not request email notifications from {1}, or believe you have received this email in error, please contact {1} customer service.
                    </p>

                    <p>
                        Sincerely, <br />
                        {1} Customer Service
                    </p>"
                    .FormatWith(verifyEmailUrl, GlobalSettings.Company.Name) : body.FormatWith(verifyEmailUrl, GlobalSettings.Company.Name)
            });
        }
        public static void OptInCustomer(string token)
        {
            var decryptedToken = Security.Decrypt(token);

            var customerID = Convert.ToInt32(decryptedToken.CustomerID);
            var email = decryptedToken.Email.ToString();

            OptInCustomer(customerID, email);
        }
        public static void OptInCustomer(int customerID, string email)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = customerID,
                Email = email,
                SubscribeToBroadcasts = true,
                SubscribeFromIPAddress = GlobalUtilities.GetClientIP()
            });
        }
        public static void OptOutCustomer(int customerID)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = customerID,
                SubscribeToBroadcasts = false
            });
        }

        public static List<Customer> GetNewestDistributors(GetNewestDistributorsRequest request)
        {
            var query = Exigo.OData().UniLevelTree.Expand("Customer")
                .Where(c => c.TopCustomerID == request.CustomerID && c.Customer.CustomerTypeID == (int)CustomerTypes.Associate)
                .Where(c => c.Level <= request.MaxLevel)
                .OrderByDescending(c => c.Customer.CreatedDate).Take(request.RowCount).ToList();

            var customers = query.Select(c => (Customer)c.Customer).ToList();

            return customers;
        }

        /// <summary>
        /// Based on requested Enroller, we need to find the next sponsor that falls within their placement preference tree
        /// </summary>
        /// <param name="enrollerID">Enroller to look under to find our Sponsor ID</param>
        /// <returns>Sponsor ID</returns>
        public static int GetCustomersSponsorPreference(int enrollerID)
        {
            var sponsorID = 0;

            try
            {

                // Try to use the team query to pull the customer's preferred placement Team
                var preferredTeamID = 0;

                using (var context = Exigo.Sql())
                {
                    preferredTeamID = context.Query<int>(@"
                     select top 1 COALESCE(NULLIF(Field1,''), '0') 
                    from Customers 
                    where CustomerID = @enrollerID
                    ", new
                     { 
                         enrollerID = enrollerID 
                     }).FirstOrDefault();
                }
                // In case this has not been set, we default to Team 1
                if (preferredTeamID == 0)
                {
                    preferredTeamID = 1;
                }

                var periodID = Exigo.GetCurrentPeriod((int)PeriodTypes.Monthly).PeriodID;


                // Now we run our query, if it fails then we return enrollerID vs. 0, which would fail placement
                using (var context = Exigo.Sql())
                {
                    var query = context.Query<CustomerNode>(@"
                   Declare @PeriodTy int = 1, @CustomerID int = @enrollerID
                    ;with cte_Primary as
                    (
                        Select 
                        c.CustomerID,
                        pv.Volume50,
                        pv.Volume51,
                        pv.Volume52,
                        pv.Volume53,
                        pv.Volume54,
                        0 as 'team'
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
                        1 as 'team'
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
                        1 as 'team'
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
                        2 as 'team'
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
                        2 as 'team'
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
                        3 as 'team'
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
                        3 as 'team'
                        From
                        Customers c
                        Inner Join PeriodVolumes pv
                        on pv.CustomerID = c.CustomerID
                        and pv.PeriodID = @PeriodID
                        and pv.PeriodTypeID = @PeriodTy
                        Inner Join cte_Team3 team
                        on Team.Volume50 = c.CustomerID
                    ), cte_Team4 as
                    (
                        Select 
                        c.CustomerID,
                        pv.Volume50,
                        pv.Volume51,
                        pv.Volume52,
                        pv.Volume53,
                        pv.Volume54,
                        4 as 'team'
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
                        4 as 'team'
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
                        5 as 'team'
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
                        5 as 'team'
                        From
                        Customers c
                        Inner Join PeriodVolumes pv
                        on pv.CustomerID = c.CustomerID
                        and pv.PeriodID = @PeriodID
                        and pv.PeriodTypeID = @PeriodTy
                        Inner Join cte_Team5 team
                        on team.Volume50 =  c.CustomerID
 
                    ), cte_combine as (
                    Select
                        Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54
                    From
                        cte_Primary
                    UNION
                    Select
                        Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54
                    From
                        cte_Team1
                    UNION
                    Select
                        Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54
                    From
                        cte_Team2
                    UNION
                    Select
                        Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54
                    From
                        cte_Team3
                    UNION
                    Select
                        Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54
                    From
                        cte_Team4
                    UNION
                    Select
                        Team, CustomerID, Volume50, Volume51, Volume52, Volume53, Volume54
                    From
                        cte_Team5
                    )
                    select
                    Team, CustomerID
                    From
                        cte_combine
                        where (Team = @preferredTeamID and Volume50 = 0)
                    Order By
                        team, CustomerID
                    option (maxrecursion 0)", new
                     {
                         enrollerID = enrollerID,
                         PeriodID = periodID,
                         preferredTeamID = preferredTeamID
                     }).FirstOrDefault();

                    if (query != null && query.CustomerID != 0)
                    {
                        sponsorID = query.CustomerID;
                    }
                    else
                    {
                        sponsorID = enrollerID;
                    }
                }
            }
            catch (Exception ex)
            {
                // All else fails, return the enroller id instead of the sponsor id - Mike M.
                sponsorID = enrollerID;
                Console.Write(ex);
            }

            return sponsorID;
        }

        /// <summary>
        /// Use this utility to get a Customer's username and password via a Custom Report
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns>CustomerCredential containing UserName and Password</returns>
        public static CustomerCredential GetCustomerCredentials(int customerID)
        {
            var model = new CustomerCredential();

            try
            {
                var request = new GetCustomReportRequest { ReportID = 10 };
                var parameters = new List<ParameterRequest>() { new ParameterRequest { ParameterName = "CustomerID", Value = customerID } };

                request.Parameters = parameters.ToArray();

                var response = Exigo.WebService().GetCustomReport(request);
                var data = response.ReportData.Tables[0].Rows[0];

                model.CustomerID = customerID;
                model.UserName = data["UserName"].ToString();
                model.Password = data["Password"].ToString();
            }
            catch { }

            return model;
        }
    }    
}