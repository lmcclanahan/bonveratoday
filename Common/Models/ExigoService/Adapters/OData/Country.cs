
namespace Common.Api.ExigoOData
{
    public partial class Country
    {
        public static explicit operator ExigoService.Country(Country country)
        {
            var model = new ExigoService.Country();
            if (country == null) return model;

            model.CountryCode = country.CountryCode;
            model.CountryName = country.CountryDescription;

            return model;
        }
    }
}