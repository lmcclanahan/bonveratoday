using Common.Api.ExigoWebService;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<Country> GetCountries()
        {
            var context = Exigo.OData();
            var records = new List<Country>();

            // Get the nodes recursively until we have them all
            int resultsPerCall = 50;
            int lastResultCount = resultsPerCall;
            int callsMade = 0;

            while (lastResultCount == resultsPerCall)
            {
                var results = context.Countries
                    .OrderBy(c => c.SortOrder)
                    .Skip(callsMade * resultsPerCall)
                    .Take(resultsPerCall)
                    .Select(c => c)
                    .ToList();

                results.ForEach(c => records.Add((Country)c));

                callsMade++;
                lastResultCount = results.Count;
            }

            foreach (var record in records)
            {
                yield return record;
            }
        }
        public static IEnumerable<Region> GetRegions(string CountryCode)
        {
            var context = Exigo.OData();
            var records = new List<Region>();

            // Get the nodes recursively until we have them all
            int resultsPerCall = 50;
            int lastResultCount = resultsPerCall;
            int callsMade = 0;

            while (lastResultCount == resultsPerCall)
            {
                var results = context.CountryRegions
                    .Where(c => c.CountryCode == CountryCode)
                    .OrderBy(c => c.SortOrder)
                    .Skip(callsMade * resultsPerCall)
                    .Take(resultsPerCall)
                    .Select(c => c)
                    .ToList();

                results.ForEach(c => records.Add((Region)c));

                callsMade++;
                lastResultCount = results.Count;
            }

            foreach (var record in records)
            {
                yield return record;
            }
        }
        public static CountryRegionCollection GetCountryRegions(string CountryCode)
        {
            var result = new CountryRegionCollection();

            using (var context = Exigo.WebService())
            {
                var response = context.GetCountryRegions(new GetCountryRegionsRequest()
                {
                    CountryCode = CountryCode
                });

                result.Countries = response.Countries.Select(c => new Country()
                {
                    CountryCode = c.CountryCode,
                    CountryName = c.CountryName
                });

                result.Regions = response.Regions.Select(c => new Region()
                {
                    RegionCode = c.RegionCode,
                    RegionName = c.RegionName
                });
            }

            return result;
        }
    }
}