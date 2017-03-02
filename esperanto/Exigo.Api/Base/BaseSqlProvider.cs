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
    public abstract class BaseSqlProvider
    {
        public IApiSettings ApiSettings { get; set; }

        public BaseSqlProvider(IApiSettings ApiSettings = null)
        {
            this.ApiSettings = ApiSettings ?? new DefaultApiSettings();

            // Stop here if we are using this provider and don't have access.
            if(!this.ApiSettings.IsEnterprise) throw new Exception("In order to use a Sql provider, you must have Enterprise-level access. Please choose another provider for your Exigo services.");
        }

        public SqlHelper GetContext()
        {
            return new SqlHelper(this.ApiSettings.ConnectionString);
        }
    }
}
