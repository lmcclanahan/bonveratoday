using Exigo.Api.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api.Base
{
    public abstract class BaseWebServiceProvider
    {
        public IApiSettings ApiSettings { get; set; }

        public BaseWebServiceProvider(IApiSettings ApiSettings = null)
        {
            this.ApiSettings = ApiSettings ?? new DefaultApiSettings();
        }

        public ExigoApi GetContext()
        {
            var context = new ExigoApi()
            {
                ApiAuthenticationValue = new ApiAuthentication()
                {
                    LoginName = ApiSettings.LoginName,
                    Password = ApiSettings.Password,
                    Company = ApiSettings.CompanyKey
                },
                Url = ApiSettings.WebServiceUrl
            };

            return context;
        }
    }
}
