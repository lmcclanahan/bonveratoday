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
    public class AddressService : IAddressService
    {
        public IAddressService Provider { get; set; }
        public AddressService(IAddressService Provider = null)
        {
            if (Provider != null)
            {
                this.Provider = Provider;
            }
            else
            {
                this.Provider = new WebServiceAddressProvider(new DefaultApiSettings());
            }
        }

        public Address VerifyAddress(Address Address)
        {
            return Provider.VerifyAddress(Address);
        }
    }

    public interface IAddressService
    {
        Address VerifyAddress(Address Address);
    }

    #region Web Service
    public class WebServiceAddressProvider : BaseWebServiceProvider, IAddressService
    {
        public WebServiceAddressProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public Address VerifyAddress(Address Address)
        {
            var result = Address;

            // Do not attempt to validate non-US addresses.
            if(Address.Country.ToUpper() != "US") return result;

            try
            {
                var response = GetContext().VerifyAddress(new VerifyAddressRequest
                {
                    Address = Address.FullAddress,
                    City = Address.City,
                    State = Address.State,
                    Zip = Address.Zip,
                    Country = Address.Country
                });

                if (response.Result.Status == ResultStatus.Success)
                {
                    Address.IsVerified = true;
                    result.Address1 = response.Address;
                    result.Address2 = string.Empty;
                    result.City = response.City;
                    result.State = response.State;
                    result.Zip = response.Zip;
                    result.Country = response.Country;
                }
                
                return result;
            }
            catch
            {
                result.IsVerified = false;
                return result;
            }
        }
    }
    #endregion
}
