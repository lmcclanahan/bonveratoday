using Exigo.Api.Base;
using Exigo.Api.OData;
using Exigo.Api.WebService;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class AuthenticationService : IAuthenticationService
    {
        public IAuthenticationService Provider { get; set; }
        public AuthenticationService(IAuthenticationService Provider = null)
        {
            if (Provider != null)
            {                
                this.Provider = Provider;
            }
            else
            {
                var defaultApiSettings = new DefaultApiSettings();
                if(defaultApiSettings.IsEnterprise) this.Provider = new SqlAuthenticationProvider(defaultApiSettings);
                else this.Provider = new ODataAuthenticationProvider(defaultApiSettings);
            }
        }

        public int AuthenticateCustomer(string LoginName, string Password)
        {
            return Provider.AuthenticateCustomer(LoginName, Password);
        }
        public bool AuthenticateExigoUser(string LoginName, string Password)
        {
            return Provider.AuthenticateExigoUser(LoginName, Password);
        }
    }

    public interface IAuthenticationService
    {
        int AuthenticateCustomer(string LoginName, string Password);
        bool AuthenticateExigoUser(string LoginName, string Password);
    }

    #region Web Service
    public class WebServiceAuthenticationProvider : BaseWebServiceProvider, IAuthenticationService
    {
        public WebServiceAuthenticationProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public int AuthenticateCustomer(string LoginName, string Password)
        {
            var result = 0;

            try
            {
                var response = GetContext().AuthenticateCustomer(new AuthenticateCustomerRequest
                {
                    LoginName = LoginName,
                    Password = Password
                });

                if (response.Result.Status == ResultStatus.Success)
                {
                    result = response.CustomerID;
                }
                
                return result;
            }
            catch
            {
                return 0;
            }
        }
        public bool AuthenticateExigoUser(string LoginName, string Password)
        {
            var result = false;

            var response = GetContext().AuthenticateUser(new AuthenticateUserRequest()
            {
                LoginName = LoginName,
                Password = Password
            });

            result = (response.Result.Status == ResultStatus.Success);

            return result;
        }
    }
    #endregion

    #region OData
    public class ODataAuthenticationProvider : BaseODataProvider, IAuthenticationService
    {
        public ODataAuthenticationProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public int AuthenticateCustomer(string LoginName, string Password)
        {
            var result = 0;

            try
            {
                var response = GetContext().CreateQuery<Customer>("AuthenticateLogin")
                    .AddQueryOption("loginName", "'" + LoginName + "'")
                    .AddQueryOption("password", "'" + Password + "'")
                    .Select(c => new { c.CustomerID }).FirstOrDefault();
                if(response == null) return 0;

                result = Convert.ToInt32(response);
                
                return result;
            }
            catch
            {
                return 0;
            }
        }
        public bool AuthenticateExigoUser(string LoginName, string Password)
        {
            return new WebServiceAuthenticationProvider(ApiSettings).AuthenticateExigoUser(LoginName, Password);
        }
    }
    #endregion

    #region Sql
    public class SqlAuthenticationProvider : BaseSqlProvider, IAuthenticationService
    {
        public SqlAuthenticationProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public int AuthenticateCustomer(string LoginName, string Password)
        {
            // If we are not an enterprise client, return the web service result.
            if(!ApiSettings.IsEnterprise) return new ODataAuthenticationProvider(ApiSettings).AuthenticateCustomer(LoginName, Password);

            var result = 0;

            try
            {
                var response = GetContext().GetField("AuthenticateCustomer {0}, {1}", LoginName, Password);
                if(response == null) return 0;

                result = Convert.ToInt32(response);
                
                return result;
            }
            catch
            {
                return 0;
            }
        }
        public bool AuthenticateExigoUser(string LoginName, string Password)
        {
            return new WebServiceAuthenticationProvider(ApiSettings).AuthenticateExigoUser(LoginName, Password);
        }
    }
    #endregion
}
