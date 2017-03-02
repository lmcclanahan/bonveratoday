using Exigo.Api.Base;
using Exigo.Api.Extensions;
using Exigo.Api.Helpers;
using Exigo.Api.OData;
using Exigo.Api.WebService;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CustomerService : ICustomerService
    {
        public ICustomerService Provider { get; set; }
        public CustomerService(ICustomerService Provider = null)
        {
            if (Provider != null)
            {
                this.Provider = Provider;
            }
            else
            {
                var defaultApiSettings = new DefaultApiSettings();
                if(defaultApiSettings.IsEnterprise) this.Provider = new SqlCustomerProvider(defaultApiSettings);
                else this.Provider = new ODataCustomerProvider(defaultApiSettings);
            }
        }

        public Customer GetCustomer(int CustomerID)
        {
            return Provider.GetCustomer(CustomerID);
        }
        public void UpdateCustomer(int CustomerID, string FirstName, string LastName)
        {
            Provider.UpdateCustomer(CustomerID, FirstName, LastName);
        }

        public CustomerSite GetCustomerSite(int CustomerID)
        {
            return Provider.GetCustomerSite(CustomerID);
        }
        public void CreateCustomerSite(CustomerSite CustomerSite)
        {
            Provider.CreateCustomerSite(CustomerSite);
        }
        public void UpdateCustomerSite(CustomerSite CustomerSite)
        {
            Provider.UpdateCustomerSite(CustomerSite);
        }
        public void DeleteCustomerSite(int CustomerID)
        {
            Provider.DeleteCustomerSite(CustomerID);
        }

        public CustomerAvatar GetCustomerAvatar(int CustomerID)
        {
            return Provider.GetCustomerAvatar(CustomerID);
        }
        public void SetCustomerAvatar(int CustomerID, byte[] ImageBytes)
        {
            Provider.SetCustomerAvatar(CustomerID, ImageBytes);
        }

        public CustomerSubscription GetCustomerSubscription(int CustomerID, int SubscriptionID)
        {
            return Provider.GetCustomerSubscription(CustomerID, SubscriptionID);
        }
        public List<CustomerSubscription> GetCustomerSubscriptions(int CustomerID)
        {
            return Provider.GetCustomerSubscriptions(CustomerID);
        }

        public CustomerPointAccount GetCustomerPointAccount(int CustomerID, int PointAccountID)
        {
            return Provider.GetCustomerPointAccount(CustomerID, PointAccountID);
        }

        public CustomerType GetCustomerType(int CustomerTypeID)
        {
            return Provider.GetCustomerType(CustomerTypeID);
        }
        public CustomerStatus GetCustomerStatus(int CustomerStatusID)
        {
            return Provider.GetCustomerStatus(CustomerStatusID);
        }
    }

    public interface ICustomerService
    {
        Customer GetCustomer(int CustomerID);
        void UpdateCustomer(int CustomerID, string FirstName, string LastName);

        CustomerSite GetCustomerSite(int CustomerID);
        void CreateCustomerSite(CustomerSite CustomerSite);
        void UpdateCustomerSite(CustomerSite CustomerSite);
        void DeleteCustomerSite(int CustomerID);

        CustomerAvatar GetCustomerAvatar(int CustomerID);
        void SetCustomerAvatar(int CustomerID, byte[] ImageBytes);

        CustomerSubscription GetCustomerSubscription(int CustomerID, int SubscriptionID);
        List<CustomerSubscription> GetCustomerSubscriptions(int CustomerID);

        CustomerPointAccount GetCustomerPointAccount(int CustomerID, int PointAccountID);

        CustomerType GetCustomerType(int CustomerTypeID);
        CustomerStatus GetCustomerStatus(int CustomerStatusID);
    }

    #region Web Service
    public class WebServiceCustomerProvider : BaseWebServiceProvider, ICustomerService
    {
        public WebServiceCustomerProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public Customer GetCustomer(int CustomerID)
        {
            var result = new Customer();

            var customers = GetContext().GetCustomers(new GetCustomersRequest()
            {
                CustomerID = CustomerID
            });
            if(customers.RecordCount == 0) return null;

            var response = customers.Customers[0];
            result.CustomerID = response.CustomerID;
            result.FirstName = response.FirstName;
            result.LastName = response.LastName;
            result.Company = response.Company;
            result.Email = response.Email;
            result.Phone = response.Phone;

            result.MainAddress = new Address()
            {
                Address1 = response.MainAddress1,
                Address2 = response.MainAddress2,
                City = response.MainCity,
                State = response.MainState,
                Zip = response.MainZip,
                Country = response.MainCountry
            };

            return result;
        }
        public void UpdateCustomer(int CustomerID, string FirstName, string LastName)
        {
            GetContext().UpdateCustomer(new UpdateCustomerRequest()
            {
                CustomerID = CustomerID,
                FirstName = FirstName,
                LastName = LastName
            });
        }

        public CustomerSite GetCustomerSite(int CustomerID)
        {
            var result = new CustomerSite();
            
            var response = GetContext().GetCustomerSite(new GetCustomerSiteRequest()
            {
                CustomerID = CustomerID
            });
            if(response == null) return null;

            result.CustomerID = response.CustomerID;
            result.WebAlias = response.WebAlias;
            result.FirstName = response.FirstName;
            result.LastName = response.LastName;
            result.Company = response.Company;
            result.Email = response.Email;
            result.Phone = response.Phone;
            result.Phone2 = response.Phone2;
            result.Fax = response.Fax;
            result.Notes1 = response.Notes1;
            result.Notes2 = response.Notes2;
            result.Notes3 = response.Notes3;
            result.Notes4 = response.Notes4;

            result.Address = new Address()
            {
                Address1 = response.Address1,
                Address2 = response.Address2,
                City = response.City,
                State = response.State,
                Zip = response.Zip,
                Country = response.Country
            };

            return result;
        }
        public void CreateCustomerSite(CustomerSite CustomerSite)
        {
            var request = new SetCustomerSiteRequest();

            request.CustomerID = CustomerSite.CustomerID;
            request.WebAlias = CustomerSite.WebAlias;
            request.FirstName = CustomerSite.FirstName;
            request.LastName = CustomerSite.LastName;
            request.Company = CustomerSite.Company;
            request.Email = CustomerSite.Email;
            request.Phone = CustomerSite.Phone;
            request.Phone2 = CustomerSite.Phone2;
            request.Fax = CustomerSite.Fax;
            request.Notes1 = CustomerSite.Notes1;
            request.Notes2 = CustomerSite.Notes2;
            request.Notes3 = CustomerSite.Notes3;
            request.Notes4 = CustomerSite.Notes4;
            if(CustomerSite.Address != null)
            {
                request.Address1 = CustomerSite.Address.Address1;
                request.Address2 = CustomerSite.Address.Address2;
                request.City = CustomerSite.Address.City;
                request.State = CustomerSite.Address.State;
                request.Zip = CustomerSite.Address.Zip;
                request.Country = CustomerSite.Address.Country;
            }

            var response = GetContext().SetCustomerSite(request);
        }
        public void UpdateCustomerSite(CustomerSite CustomerSite)
        {
            // First, get the existing customer site.
            var existingCustomerSite = GetContext().GetCustomerSite(new GetCustomerSiteRequest
            {
                CustomerID = CustomerSite.CustomerID
            });
            if(existingCustomerSite == null) existingCustomerSite = new GetCustomerSiteResponse();


            // Now, save the information.
            var request = new SetCustomerSiteRequest();

            request.CustomerID = CustomerSite.CustomerID;
            request.WebAlias = CustomerSite.WebAlias ?? existingCustomerSite.WebAlias;
            request.FirstName = CustomerSite.FirstName ?? existingCustomerSite.FirstName;
            request.LastName = CustomerSite.LastName ?? existingCustomerSite.LastName;
            request.Company = CustomerSite.Company ?? existingCustomerSite.Company;
            request.Email = CustomerSite.Email ?? existingCustomerSite.Email;
            request.Phone = CustomerSite.Phone ?? existingCustomerSite.Phone;
            request.Phone2 = CustomerSite.Phone2 ?? existingCustomerSite.Phone;
            request.Fax = CustomerSite.Fax ?? existingCustomerSite.Fax;
            request.Notes1 = CustomerSite.Notes1 ?? existingCustomerSite.Notes1;
            request.Notes2 = CustomerSite.Notes2 ?? existingCustomerSite.Notes2;
            request.Notes3 = CustomerSite.Notes3 ?? existingCustomerSite.Notes3;
            request.Notes4 = CustomerSite.Notes4 ?? existingCustomerSite.Notes4;
            request.Address1 = CustomerSite.Address.Address1 ?? existingCustomerSite.Address1;
            request.Address2 = CustomerSite.Address.Address2 ?? existingCustomerSite.Address2;
            request.City = CustomerSite.Address.City ?? existingCustomerSite.City;
            request.State = CustomerSite.Address.State ?? existingCustomerSite.State;
            request.Zip = CustomerSite.Address.Zip ?? existingCustomerSite.Zip;
            request.Country = CustomerSite.Address.Country ?? existingCustomerSite.Country;

            var response = GetContext().SetCustomerSite(request);
        }
        public void DeleteCustomerSite(int CustomerID)
        {
            var request = new SetCustomerSiteRequest();

            request.CustomerID = CustomerID;
            request.WebAlias = "Deleted-" + DateTime.Now.ToString("yyyyMMddhhmmssfff");

            var response = GetContext().SetCustomerSite(request);
        }

        public CustomerAvatar GetCustomerAvatar(int CustomerID)
        {
            var imageBytes = Convert.FromBase64String(GlobalSettings.CustomerImages.DefaultCustomerAvatarAsBase64);

            try
            {
                var imageUrl = string.Format("{0}/customers/{1}/avatar.jpg?s={2}",
                    ApiSettings.CustomerImagesUrl,
                    CustomerID,
                    Guid.NewGuid().ToString().ToLower());

                var request = (HttpWebRequest)WebRequest.Create(imageUrl);
                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK) throw new Exception("GOTO_CATCH");
                using (var stream = response.GetResponseStream())
                {
                    using (var tempStream = new MemoryStream())
                    {
                        stream.CopyTo(tempStream);
                        imageBytes = tempStream.ToArray();
                    }
                }
            }
            catch { }

            var result = new CustomerAvatar()
            {
                CustomerID = CustomerID,
                ImageBytes = imageBytes
            };

            return result;
        }
        public void SetCustomerAvatar(int CustomerID, byte[] ImageBytes)
        {
            var url = string.Format("{0}/customers/{1}/avatar.jpg", ApiSettings.CustomerImagesUrl, CustomerID);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", ApiSettings.LoginName, ApiSettings.Password))));
            request.Method = "POST";
            request.ContentLength = ImageBytes.Length;
            var writer = request.GetRequestStream();
            writer.Write(ImageBytes, 0, ImageBytes.Length);
            writer.Close();
            var response = (HttpWebResponse)request.GetResponse();
        }

        public CustomerSubscription GetCustomerSubscription(int CustomerID, int SubscriptionID)
        {
            var result = new CustomerSubscription();

            var response = GetContext().GetSubscription(new GetSubscriptionRequest()
            {
                CustomerID = CustomerID,
                SubscriptionID = SubscriptionID
            });
            if(response == null) return null;

            result.CustomerID = CustomerID;
            result.SubscriptionID = SubscriptionID;
            result.IsActive = (response.Status == Exigo.Api.WebService.SubscriptionStatus.Active);
            result.StartDate = response.StartDate;
            result.ExpirationDate = response.ExpireDate;

            return result;
        }
        public List<CustomerSubscription> GetCustomerSubscriptions(int CustomerID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCustomerProvider(ApiSettings).GetCustomerSubscriptions(CustomerID);
            else return new ODataCustomerProvider(ApiSettings).GetCustomerSubscriptions(CustomerID);
        }

        public CustomerPointAccount GetCustomerPointAccount(int CustomerID, int PointAccountID)
        {
            var result = new CustomerPointAccount();

            var response = GetContext().GetPointAccount(new GetPointAccountRequest()
            {
                CustomerID = CustomerID,
                PointAccountID = PointAccountID
            });
            if(response == null) return null;

            result.CustomerID = CustomerID;
            result.PointAccountID = PointAccountID;
            result.Balance = response.Balance;

            return result;
        }

        public CustomerType GetCustomerType(int CustomerTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCustomerProvider(ApiSettings).GetCustomerType(CustomerTypeID);
            else return new ODataCustomerProvider(ApiSettings).GetCustomerType(CustomerTypeID);
        }
        public CustomerStatus GetCustomerStatus(int CustomerStatusID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCustomerProvider(ApiSettings).GetCustomerStatus(CustomerStatusID);
            else return new ODataCustomerProvider(ApiSettings).GetCustomerStatus(CustomerStatusID);
        }
    }
    #endregion

    #region OData
    public class ODataCustomerProvider : BaseODataProvider, ICustomerService
    {
        public ODataCustomerProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public Customer GetCustomer(int CustomerID)
        {
            var result = new Customer();

            var response = GetContext().Customers
                .Where(c => c.CustomerID == CustomerID)
                .FirstOrDefault();
            if(response == null) return null;

            result.CustomerID = response.CustomerID;
            result.FirstName = response.FirstName;
            result.LastName = response.LastName;
            result.Company = response.Company;
            result.Email = response.Email;
            result.Phone = response.Phone;

            result.MainAddress = new Address()
            {
                Address1 = response.MainAddress1,
                Address2 = response.MainAddress2,
                City = response.MainCity,
                State = response.MainState,
                Zip = response.MainZip,
                Country = response.MainCountry
            };

            return result;
        }
        public void UpdateCustomer(int CustomerID, string FirstName, string LastName)
        {
            new WebServiceCustomerProvider(ApiSettings).UpdateCustomer(CustomerID, FirstName, LastName);
        }

        public CustomerSite GetCustomerSite(int CustomerID)
        {
            if (ApiSettings.IsEnterprise) return new SqlCustomerProvider(ApiSettings).GetCustomerSite(CustomerID);
            else return new WebServiceCustomerProvider(ApiSettings).GetCustomerSite(CustomerID);
        }
        public void CreateCustomerSite(CustomerSite CustomerSite)
        {
            new WebServiceCustomerProvider(ApiSettings).CreateCustomerSite(CustomerSite);
        }
        public void UpdateCustomerSite(CustomerSite CustomerSite)
        {
            new WebServiceCustomerProvider(ApiSettings).UpdateCustomerSite(CustomerSite);
        }
        public void DeleteCustomerSite(int CustomerID)
        {
            new WebServiceCustomerProvider(ApiSettings).DeleteCustomerSite(CustomerID);
        }

        public CustomerAvatar GetCustomerAvatar(int CustomerID)
        {
            return new WebServiceCustomerProvider(ApiSettings).GetCustomerAvatar(CustomerID);
        }
        public void SetCustomerAvatar(int CustomerID, byte[] ImageBytes)
        {
            new WebServiceCustomerProvider(ApiSettings).SetCustomerAvatar(CustomerID, ImageBytes);
        }

        public CustomerSubscription GetCustomerSubscription(int CustomerID, int SubscriptionID)
        {
            var result = new CustomerSubscription();

            var response = GetContext().CustomerSubscriptions
                .Where(c => c.CustomerID == CustomerID)
                .Where(c => c.SubscriptionID == SubscriptionID)
                .FirstOrDefault();
            if(response == null) return null;

            result.CustomerID = CustomerID;
            result.SubscriptionID = SubscriptionID;
            result.IsActive = (response.SubscriptionStatus.SubscriptionStatusID == 1);
            result.StartDate = response.StartDate;
            result.ExpirationDate = response.ExpireDate;

            return result;
        }
        public List<CustomerSubscription> GetCustomerSubscriptions(int CustomerID)
        {
            var result = new List<CustomerSubscription>();
            

            // Get all records recursively
            var maxRecordsPerCall = GlobalSettings.OData.MaxRecordsPerCall;
            int lastResultCount = maxRecordsPerCall;
            int completedCallCount = 0;

            while (lastResultCount == maxRecordsPerCall)
            {
                var response = GetContext().CustomerSubscriptions.Expand("Subscription")
                    .Where(c => c.CustomerID == CustomerID)
                    .OrderByDescending(c => c.SubscriptionID)
                    .Skip(completedCallCount * maxRecordsPerCall)
                    .Take(maxRecordsPerCall)
                    .Select(c => new CustomerSubscription()
                    {
                        CustomerID = c.CustomerID,
                        SubscriptionID = c.SubscriptionID,
                        SubscriptionDescription = c.Subscription.SubscriptionDescription,
                        IsActive = (c.SubscriptionStatusID == 1),
                        StartDate = c.StartDate,
                        ExpirationDate = c.ExpireDate
                    }).ToList();

                response.ForEach(c => result.Add(c));

                completedCallCount++;
                lastResultCount = response.Count;
            }


            return result;
        }

        public CustomerPointAccount GetCustomerPointAccount(int CustomerID, int PointAccountID)
        {
            var result = new CustomerPointAccount();

            var response = GetContext().CustomerPointAccounts
                .Where(c => c.CustomerID == CustomerID)
                .Where(c => c.PointAccountID == PointAccountID)
                .FirstOrDefault();
            if(response == null) return null;

            result.CustomerID = CustomerID;
            result.PointAccountID = PointAccountID;
            result.Balance = response.PointBalance;

            return result;
        }

        public CustomerType GetCustomerType(int CustomerTypeID)
        {
            var result = new CustomerType();

            var response = GetContext().CustomerTypes.Expand("PriceType")
                .Where(c => c.CustomerTypeID == CustomerTypeID)
                .FirstOrDefault();
            if(response == null) return null;

            result.CustomerTypeID = response.CustomerTypeID;
            result.CustomerTypeDescription = response.CustomerTypeDescription;
            result.PriceType = new PriceType()
            {
                PriceTypeID = response.PriceTypeID,
                PriceTypeDescription = response.PriceType.PriceTypeDescription
            };

            return result;
        }
        public CustomerStatus GetCustomerStatus(int CustomerStatusID)
        {
            var result = new CustomerStatus();

            var response = GetContext().CustomerStatuses
                .Where(c => c.CustomerStatusID == CustomerStatusID)
                .FirstOrDefault();
            if(response == null) return null;

            result.CustomerStatusID = response.CustomerStatusID;
            result.CustomerStatusDescription = response.CustomerStatusDescription;

            return result;
        }
    }
    #endregion

    #region Sql
    public class SqlCustomerProvider : BaseSqlProvider, ICustomerService
    {
        public SqlCustomerProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public Customer GetCustomer(int CustomerID)
        {
            var result = new Customer();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        CustomerID,
                        FirstName,
                        LastName,
                        Company,
                        MainAddress1,
                        MainAddress2,
                        MainCity,
                        MainState,
                        MainZip,
                        MainCountry,
                        Email,
                        Phone
                    FROM Customers 
                    WHERE CustomerID = {0}
                ", CustomerID))
            {
                if (!reader.Read()) return null;

                result.CustomerID = reader.GetInt32("CustomerID");
                result.FirstName = reader.GetString("FirstName");
                result.LastName = reader.GetString("LastName");
                result.Company = reader.GetString("Company");
                result.Email = reader.GetString("Email");
                result.Phone = reader.GetString("Phone");

                result.MainAddress = new Address()
                {
                    Address1 = reader.GetString("MainAddress1"),
                    Address2 = reader.GetString("MainAddress2"),
                    City = reader.GetString("MainCity"),
                    State = reader.GetString("MainState"),
                    Zip = reader.GetString("MainZip"),
                    Country = reader.GetString("MainCountry")
                };
            }

            return result;
        }
        public void UpdateCustomer(int CustomerID, string FirstName, string LastName)
        {
            new WebServiceCustomerProvider(ApiSettings).UpdateCustomer(CustomerID, FirstName, LastName);
        }

        public CustomerSite GetCustomerSite(int CustomerID)
        {
            var result = new CustomerSite();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        CustomerID,
                        WebAlias,
                        FirstName,
                        LastName,
                        Company,
                        Address1,
                        Address2,
                        City,
                        State,
                        Zip,
                        Country,
                        Email,
                        Phone,
                        Phone2,
                        Fax,
                        Notes1,
                        Notes2,
                        Notes3,
                        Notes4
                    FROM CustomerSites
                    WHERE CustomerID = {0}
                ", CustomerID))
            {
                if (!reader.Read()) return null;

                result.CustomerID = reader.GetInt32("CustomerID");
                result.WebAlias = reader.GetString("WebAlias");
                result.FirstName = reader.GetString("FirstName");
                result.LastName = reader.GetString("LastName");
                result.Company = reader.GetString("Company");
                result.Email = reader.GetString("Email");
                result.Phone = reader.GetString("Phone");
                result.Phone2 = reader.GetString("Phone2");
                result.Fax = reader.GetString("Fax");
                result.Notes1 = reader.GetString("Notes1");
                result.Notes2 = reader.GetString("Notes2");
                result.Notes3 = reader.GetString("Notes3");
                result.Notes4 = reader.GetString("Notes4");

                result.Address = new Address()
                {
                    Address1 = reader.GetString("Address1"),
                    Address2 = reader.GetString("Address2"),
                    City = reader.GetString("City"),
                    State = reader.GetString("State"),
                    Zip = reader.GetString("Zip"),
                    Country = reader.GetString("Country")
                };
            }

            return result;
        }
        public void CreateCustomerSite(CustomerSite CustomerSite)
        {
            new WebServiceCustomerProvider(ApiSettings).CreateCustomerSite(CustomerSite);
        }
        public void UpdateCustomerSite(CustomerSite CustomerSite)
        {
            new WebServiceCustomerProvider(ApiSettings).UpdateCustomerSite(CustomerSite);
        }
        public void DeleteCustomerSite(int CustomerID)
        {
            new WebServiceCustomerProvider(ApiSettings).DeleteCustomerSite(CustomerID);
        }

        public CustomerAvatar GetCustomerAvatar(int CustomerID)
        {
            return new WebServiceCustomerProvider(ApiSettings).GetCustomerAvatar(CustomerID);
        }
        public void SetCustomerAvatar(int CustomerID, byte[] ImageBytes)
        {
            new WebServiceCustomerProvider(ApiSettings).SetCustomerAvatar(CustomerID, ImageBytes);
        }

        public CustomerSubscription GetCustomerSubscription(int CustomerID, int SubscriptionID)
        {
            var result = new CustomerSubscription();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        SubscriptionID,
                        CustomerID,
                        IsActive,
                        StartDate,
                        ExpireDate
                    FROM CustomerSubscriptions
                    WHERE 
                        CustomerID = {0}
                        AND SubscriptionID = {1}
                ", CustomerID, SubscriptionID))
            {
                if (!reader.Read()) return null;

                result.CustomerID = reader.GetInt32("CustomerID");
                result.SubscriptionID = reader.GetInt32("SubscriptionID");
                result.IsActive = reader.GetBoolean("IsActive");
                result.StartDate = reader.GetDateTime("StartDate");
                result.ExpirationDate = reader.GetDateTime("ExpireDate");
            }

            return result;
        }
        public List<CustomerSubscription> GetCustomerSubscriptions(int CustomerID)
        {
            var result = new List<CustomerSubscription>();

            using (var reader = GetContext().GetReader(@"
                SELECT
                    cs.*,
                    s.SubscriptionDescription
                FROM CustomerSubscriptions cs
                    INNER JOIN Subscriptions s
                    ON s.SubscriptionID = cs.SubscriptionID
                WHERE CustomerID = {0}
                ", CustomerID))
            {
               while (reader.Read())
                {
                    result.Add(new CustomerSubscription()
                    {
                        CustomerID = reader.GetInt32("CustomerID"),
                        SubscriptionID = reader.GetInt32("SubscriptionID"),
                        SubscriptionDescription = reader.GetString("SubscriptionDescription"),
                        IsActive = reader.GetBoolean("IsActive"),
                        StartDate = reader.GetDateTime("StartDate"),
                        ExpirationDate = reader.GetDateTime("ExpireDate")
                    });
                }
            }

            return result;
        }

        public CustomerPointAccount GetCustomerPointAccount(int CustomerID, int PointAccountID)
        {
            var result = new CustomerPointAccount();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        PointAccountID,
                        CustomerID,
                        PointBalance
                    FROM CustomerPointAccounts
                    WHERE 
                        CustomerID = {0}
                        AND PointAccountID = {1}
                ", CustomerID, PointAccountID))
            {
                if (!reader.Read()) return null;

                result.CustomerID = reader.GetInt32("CustomerID");
                result.PointAccountID = reader.GetInt32("PointAccountID");
                result.Balance = reader.GetDecimal("PointBalance");
            }

            return result;
        }

        public CustomerType GetCustomerType(int CustomerTypeID)
        {
            return new ODataCustomerProvider(ApiSettings).GetCustomerType(CustomerTypeID);
        }
        public CustomerStatus GetCustomerStatus(int CustomerStatusID)
        {
            var result = new CustomerStatus();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM CustomerStatuses
                    WHERE CustomerStatusID = {0}
                ", CustomerStatusID))
            {
                var closed = reader.IsClosed;
                if(!reader.Read()) return null;

                result.CustomerStatusID = reader.GetInt32("CustomerStatusID");
                result.CustomerStatusDescription = reader.GetString("CustomerStatusDescription");
            }

            return result;
        }
    }
    #endregion
}