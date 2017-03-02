using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class DefaultApiSettings : EnterpriseApiSettings {  }

    public class EnterpriseApiSettings : IApiSettings
    {
        public EnterpriseApiSettings()
        {
            /*
                public const string LoginName = "API_Bonvera";
                public const string Password = "API_7&3$@!gHQ";
                //Errors out without exception and won't authenticate when CompanyKey is incorrect
                public const string CompanyKey = "allbrands";
             */
            this.LoginName  = "API_Bonvera";
            this.Password   = "API_7&3$@!gHQ";
            this.CompanyKey = "allbrands";

            this.WebServiceUrl = "http://api.exigo.com/3.0/ExigoApi.asmx";
            this.ODataUrl = string.Format("http://api.exigo.com/4.0/{0}/model", this.CompanyKey);
            this.CustomerImagesUrl = string.Format("http://api.exigo.com/4.0/{0}/images", this.CompanyKey);
            this.ProductImagesUrl = string.Format("http://api.exigo.com/4.0/{0}/productimages", this.CompanyKey);

            this.IsEnterprise = true;
            this.ConnectionString = "Data Source=bi.exigo.com;Initial Catalog=exigodemoreporting;Persist Security Info=True;User=exigodemoweb;Password=ExigoDem0;Pooling=False";
        }

        public bool IsEnterprise { get; set; }
        public string ConnectionString { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string CompanyKey { get; set; }
        public string WebServiceUrl { get; set; }
        public string ODataUrl { get; set; }
        public string CustomerImagesUrl { get; set; }
        public string ProductImagesUrl { get; set; }
    }
    public class NonEnterpriseApiSettings : EnterpriseApiSettings
    {
        public NonEnterpriseApiSettings() : base()
        {
            this.IsEnterprise = false;
        }
    }

    public class EnterpriseSandboxApiSettings : IApiSettings
    {
        public EnterpriseSandboxApiSettings()
        {
            this.LoginName = "API_Bonvera";
            this.Password = "API_7&3$@!gHQ";
            this.CompanyKey = "allbrands";

            this.WebServiceUrl = "http://sandboxapi2.exigo.com/3.0/ExigoApi.asmx";
            this.ODataUrl = string.Format("http://sandboxapi2.exigo.com/4.0/{0}/model", this.CompanyKey);
            this.CustomerImagesUrl = string.Format("http://sandboxapi2.exigo.com/4.0/{0}/images", this.CompanyKey);
            this.ProductImagesUrl = string.Format("http://sandboxapi2.exigo.com/4.0/{0}/productimages", this.CompanyKey);

            this.IsEnterprise = true;
            this.ConnectionString = "Data Source=bi.exigo.com;Initial Catalog=exigodemoreporting;Persist Security Info=True;User=exigodemoweb;Password=ExigoDem0;Pooling=False";
        }

        public bool IsEnterprise { get; set; }
        public string ConnectionString { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string CompanyKey { get; set; }
        public string WebServiceUrl { get; set; }
        public string ODataUrl { get; set; }
        public string CustomerImagesUrl { get; set; }
        public string ProductImagesUrl { get; set; }
    }
    public class NonEnterpriseSandboxApiSettings : EnterpriseSandboxApiSettings
    {
        public NonEnterpriseSandboxApiSettings() : base()
        {
            this.IsEnterprise = false;
        }
    }

    public interface IApiSettings
    {
        string LoginName { get; set; }
        string Password { get; set; }
        string CompanyKey { get; set; }

        string WebServiceUrl { get; set; }
        string ODataUrl { get; set; }
        string CustomerImagesUrl { get; set; }
        string ProductImagesUrl { get; set; }

        bool IsEnterprise { get; set; }
        string ConnectionString { get; set; }
    }
}
