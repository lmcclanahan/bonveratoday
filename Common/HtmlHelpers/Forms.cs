﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using ExigoService;
using Dapper;

namespace Common.HtmlHelpers
{
    public static class FormHtmlHelpers
    {
        public static IEnumerable<SelectListItem> BirthYears(this HtmlHelper helper, int maxYearOffset = 18, int yearCount = 100, int defaultYear = 0)
        {
            var years = new List<SelectListItem>();
            var startDate = DateTime.Now.AddYears(-maxYearOffset);
            var endDate = startDate.AddYears(-yearCount);
            defaultYear = (defaultYear == 0) ? DateTime.Now.Year : defaultYear;

            for (var year = startDate.Year; year >= endDate.Year; year--)
            {
                years.Add(new SelectListItem()
                {
                    Text = year.ToString(),
                    Value = year.ToString(),
                    Selected = (year == defaultYear)
                });
            }

            return years.AsEnumerable();
        }
        public static IEnumerable<SelectListItem> Days(this HtmlHelper helper, int days = 31, int defaultDay = 0)
        {
            for (int i = 1; i <= days; i++)
            {
                yield return new SelectListItem()
                {
                    Text = i.ToString(),
                    Value = i.ToString(),
                    Selected = i == defaultDay
                };
            }
        }
        public static IEnumerable<SelectListItem> Months(this HtmlHelper helper, int defaultMonth = 0)
        {
            return DateTimeFormatInfo
                       .CurrentInfo
                       .MonthNames
                       .Where(m => !string.IsNullOrEmpty(m))
                       .Select((monthName, index) => new SelectListItem
                       {
                           Value = (index + 1).ToString(),
                           Text = ((index + 1) + " - " + monthName).ToString(),
                           Selected = (index + 1) == defaultMonth
                       });
        }
        public static IEnumerable<SelectListItem> Years(this HtmlHelper helper, int startYear, int years = 100)
        {
            for (int i = 0; i <= years; i++)
            {
                yield return new SelectListItem()
                {
                    Text = (startYear - i).ToString(),
                    Value = (startYear - i).ToString()
                };
            }
        }

        public static IEnumerable<SelectListItem> ExpirationYears(this HtmlHelper helper, int yearCount = 20)
        {
            var years = new List<SelectListItem>();

            for (var year = DateTime.Now.Year; year <= DateTime.Now.AddYears(yearCount).Year; year++)
            {
                years.Add(new SelectListItem()
                {
                    Text = year.ToString(),
                    Value = year.ToString(),
                    Selected = (year == DateTime.Now.Year)
                });
            }

            return years.AsEnumerable();
        }

        public static IEnumerable<SelectListItem> Languages(this HtmlHelper helper, int defaultLanguageID = 0)
        {
            var response = Exigo.GetLanguages();

            return response.Select(c => new SelectListItem()
            {
                Text = c.LanguageDescription,
                Value = c.LanguageID.ToString(),
                Selected = c.LanguageID == defaultLanguageID
            });
        }

        //20170130 DV. This is used in RS only
        public static IEnumerable<SelectListItem> Countries(this HtmlHelper helper, string defaultCountryCode = "US")
        {
            return Countries(helper,null,true,null,defaultCountryCode);
        }

        //20170130 DV. This is used in BO only for distributors
        public static IEnumerable<SelectListItem> Countries(this HtmlHelper helper, bool disableHT, int CustomerID, string defaultCountryCode = "US")
        {
            return Countries(helper, null, disableHT, CustomerID, defaultCountryCode);
        }
  

        public static IEnumerable<SelectListItem> Countries(this HtmlHelper helper, IEnumerable<string> countryCodes, bool disableHT, int? CustomerID, string defaultCountryCode = "US")
        {
            var apiCountries = Exigo.GetCountries();
            var countries = new List<Country>();
            
            //20170118 82825 DV. Reminder to self. Consider moving Field5 to the Identity object since it could be handy throughout entire project later.
            if (!disableHT) { 
            using (var context = Exigo.Sql())
            {
                var Field5 = context.Query<string>(@"
                                    SELECT Field5 FROM Customers WHERE CustomerID = @customerID
                                    ", new { CustomerID }).FirstOrDefault();

                if (Field5.IsNotNullOrEmpty()&&Field5=="1") //That is, the value is likely a 1 for true. Most customers will have either an empty "" value or a value of 0 for Field5 if the CSR unchecks the Field5 user-defined field in Exigo Admin
                {
                        Country US = new Country();
                    US.CountryCode = "US";
                    US.CountryName = "United States";
                    Country HT = new Country();
                    HT.CountryCode = "HT";
                    HT.CountryName = "Haiti";
                    countries.Add(US);
                    countries.Add(HT);

                    return countries.Select(c => new SelectListItem()
                    {
                        Text = c.CountryName,
                        Value = c.CountryCode,
                        Selected = c.CountryCode == defaultCountryCode
                    });
                }
                }
            }

            var markets = GlobalSettings.Markets.AvailableMarkets;

            // compare the countries in the Countries table for the company with the list of available markets in the Settings file
            foreach (var market in markets)
            {
                foreach (var country in market.Countries)
                {
                    var countryMatch = apiCountries.Where(c => c.CountryCode == country).FirstOrDefault();

                    // ensure no duplicates are added
                    if (!countries.Any(c => c.CountryCode == countryMatch.CountryCode))
                    {
                        countries.Add(countryMatch);
                    }
                }
            }

            if (countryCodes != null && countryCodes.Count() > 0)
            {
                countries = countries.Where(c => countryCodes.Contains(c.CountryCode)).ToList();
            }

            return countries.Select(c => new SelectListItem()
            {
                Text = c.CountryName,
                Value = c.CountryCode,
                Selected = c.CountryCode == defaultCountryCode
            });
        }

        public static IEnumerable<SelectListItem> Regions(this HtmlHelper helper, string countryCode, string defaultRegionCode = "")
        {
            var response = Exigo.GetRegions(countryCode);

            return response.Select(c => new SelectListItem()
            {
                Text = c.RegionName,
                Value = c.RegionCode,
                Selected = c.RegionCode == defaultRegionCode
            });
        }

        public static MvcHtmlString CountryOptions(this HtmlHelper helper, string defaultCountryCode = "US")
        {
            var response = Exigo.GetCountries();

            var html = new StringBuilder();
            foreach (var country in response)
            {
                html.AppendFormat("<option value='{0}' {2}>{1}</option>"
                    , country.CountryCode
                    , country.CountryName
                    , country.CountryCode.Equals(defaultCountryCode, StringComparison.InvariantCultureIgnoreCase) ? "selected" : "");
            }

            return new MvcHtmlString(html.ToString());
        }
        public static MvcHtmlString RegionOptions(this HtmlHelper helper, string countryCode, string defaultRegionCode = "")
        {
            var response = Exigo.GetRegions(countryCode);

            if (response.Count() > 1)
            {
                response = response.Where(c => !c.RegionCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var html = new StringBuilder();
            foreach (var region in response)
            {
                html.AppendFormat("<option value='{0}' {2}>{1}</option>"
                    , region.RegionCode
                    , region.RegionName
                    , region.RegionCode.Equals(defaultRegionCode, StringComparison.InvariantCultureIgnoreCase) ? "selected" : "");
            }

            return new MvcHtmlString(html.ToString());
        }
    }
}