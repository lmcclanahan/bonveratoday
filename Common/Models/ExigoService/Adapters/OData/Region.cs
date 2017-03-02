namespace Common.Api.ExigoOData
{
    public partial class CountryRegion
    {
        public static explicit operator ExigoService.Region(CountryRegion region)
        {
            var model = new ExigoService.Region();
            if (region == null) return model;

            model.RegionCode = region.RegionCode;
            model.RegionName = region.RegionDescription;

            return model;
        }
    }
}