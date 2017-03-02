using Exigo.Api.Base;
using Exigo.Api.WebService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CountryRegionService : ICountryRegionService
    {
        public ICountryRegionService Provider { get; set; }
        public CountryRegionService(ICountryRegionService Provider = null)
        {
            if (Provider != null)
            {
                this.Provider = Provider;
            }
            else
            {
                var defaultApiSettings = new DefaultApiSettings();
                if(defaultApiSettings.IsEnterprise) this.Provider = new SqlCountryRegionProvider(defaultApiSettings);
                else this.Provider = new ODataCountryRegionProvider(defaultApiSettings);
            }
        }

        public List<Country> GetCountries()
        {
            return Provider.GetCountries();
        }
        public List<Region> GetRegions(string CountryCode)
        {
            return Provider.GetRegions(CountryCode);
        }
        public CountryRegionCollection GetCountryRegions(string CountryCode)
        {
            return Provider.GetCountryRegions(CountryCode);
        }
    }

    public interface ICountryRegionService
    {
        List<Country> GetCountries();
        List<Region> GetRegions(string CountryCode);
        CountryRegionCollection GetCountryRegions(string CountryCode);
    }

    #region Web Service
    public class WebServiceCountryRegionProvider : BaseWebServiceProvider, ICountryRegionService
    {
        public WebServiceCountryRegionProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public List<Country> GetCountries()
        {
            var result = new List<Country>();

            var response = GetContext().GetCountryRegions(new GetCountryRegionsRequest());
            if(response == null) return null;

            result = response.Countries.ToList()
                .Select(c => new Country()
                {
                    CountryCode = c.CountryCode,
                    CountryName = c.CountryName
                }).ToList();

            return result;
        }
        public List<Region> GetRegions(string CountryCode)
        {
            var result = new List<Region>();

            var response = GetContext().GetCountryRegions(new GetCountryRegionsRequest()
            {
                CountryCode = CountryCode
            });
            if(response == null) return null;

            result = response.Regions.ToList()
                .Select(c => new Region()
                {
                    RegionCode = c.RegionCode,
                    RegionName = c.RegionName
                }).ToList();

            return result;
        }
        public CountryRegionCollection GetCountryRegions(string CountryCode)
        {
            var result = new CountryRegionCollection();

            var response = GetContext().GetCountryRegions(new GetCountryRegionsRequest()
            {
                CountryCode = CountryCode
            });
            if(response == null) return null;

            result.Countries = response.Countries.ToList()
                .Select(c => new Country()
                {
                    CountryCode = c.CountryCode,
                    CountryName = c.CountryName
                }).ToList();

            result.Regions = response.Regions.ToList()
                .Select(c => new Region()
                {
                    RegionCode = c.RegionCode,
                    RegionName = c.RegionName
                }).ToList();

            return result;
        }
    }
    #endregion

    #region OData
    public class ODataCountryRegionProvider : BaseODataProvider, ICountryRegionService
    {
        public ODataCountryRegionProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public List<Country> GetCountries()
        {
            if (ApiSettings.IsEnterprise) return new SqlCountryRegionProvider(ApiSettings).GetCountries();
            else return new WebServiceCountryRegionProvider(ApiSettings).GetCountries();
        }
        public List<Region> GetRegions(string CountryCode)
        {
            if (ApiSettings.IsEnterprise) return new SqlCountryRegionProvider(ApiSettings).GetRegions(CountryCode);
            else return new WebServiceCountryRegionProvider(ApiSettings).GetRegions(CountryCode);
        }
        public CountryRegionCollection GetCountryRegions(string CountryCode)
        {
            if (ApiSettings.IsEnterprise) return new SqlCountryRegionProvider(ApiSettings).GetCountryRegions(CountryCode);
            else return new WebServiceCountryRegionProvider(ApiSettings).GetCountryRegions(CountryCode);
        }
    }
    #endregion

    #region Sql
    public class SqlCountryRegionProvider : BaseSqlProvider, ICountryRegionService
    {
        public SqlCountryRegionProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public List<Country> GetCountries()
        {
            var result = new List<Country>();

            var data = GetContext().GetTable(@"
                    SELECT
                        CountryCode,
                        CountryDescription
                    FROM Countries
                ");
            if(data == null) return null;

            foreach (DataRow row in data.Rows)
            {
                result.Add(new Country()
                {
                    CountryCode = row["CountryCode"].ToString(),
                    CountryName = row["CountryDescription"].ToString()
                });
            }

            return result;
        }
        public List<Region> GetRegions(string CountryCode)
        {
            var result = new List<Region>();

            var data = GetContext().GetTable(@"
                    SELECT
                        RegionCode,
                        RegionDescriptione
                    FROM CountryRegions
                    WHERE CountryCode = {0}
                ", CountryCode);
            if(data == null) return null;

            foreach (DataRow row in data.Rows)
            {
                result.Add(new Region()
                {
                    RegionCode = row["RegionCode"].ToString(),
                    RegionName = row["RegionDescription"].ToString()
                });
            }

            return result;
        }
        public CountryRegionCollection GetCountryRegions(string CountryCode)
        {
            var result = new CountryRegionCollection();

            var data = GetContext().GetSet(@"
                    SELECT
                        CountryCode,
                        CountryDescription
                    FROM Countries

                    SELECT
                        RegionCode,
                        RegionDescription
                    FROM CountryRegions
                    WHERE CountryCode = {0}
                ", CountryCode);
            if(data == null) return null;

            // Assemble the countries
            var countriesData = data.Tables[0];
            foreach (DataRow row in countriesData.Rows)
            {
                result.Countries.Add(new Country()
                {
                    CountryCode = row["CountryCode"].ToString(),
                    CountryName = row["CountryDescription"].ToString()
                });
            }

            // Assemble the regions
            var regionsData = data.Tables[1];
            foreach (DataRow row in regionsData.Rows)
            {
                result.Regions.Add(new Region()
                {
                    RegionCode = row["RegionCode"].ToString(),
                    RegionName = row["RegionDescription"].ToString()
                });
            }

            return result;
        }
    }
    #endregion
}