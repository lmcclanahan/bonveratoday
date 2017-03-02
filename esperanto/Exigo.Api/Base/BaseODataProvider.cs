using Exigo.Api.OData;
using Exigo.Api.WebService;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api.Base
{
    public abstract class BaseODataProvider
    {
        public IApiSettings ApiSettings { get; set; }

        public BaseODataProvider(IApiSettings ApiSettings = null)
        {
            this.ApiSettings = ApiSettings ?? new DefaultApiSettings();
        }

        public ExigoContext GetContext()
        {
            var context = new ExigoContext(new Uri(ApiSettings.ODataUrl));
            context.IgnoreMissingProperties = true;
            context.IgnoreResourceNotFoundException = true;
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(ApiSettings.LoginName + ":" + ApiSettings.Password));
            context.SendingRequest +=
                    (object s, SendingRequestEventArgs e) =>
                            e.RequestHeaders.Add("Authorization", "Basic " + credentials);
            return context;
        }
    }
}
