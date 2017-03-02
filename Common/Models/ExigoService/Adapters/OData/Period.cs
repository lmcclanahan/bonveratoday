namespace Common.Api.ExigoOData
{
    public partial class Period
    {
        public static explicit operator ExigoService.Period(Period period)
        {
            var model = new ExigoService.Period();
            if (period == null) return model;

            model.PeriodTypeID = period.PeriodTypeID;
            model.PeriodID = period.PeriodID;
            model.PeriodDescription = period.PeriodDescription;
            model.StartDate = period.StartDate;
            model.EndDate = period.EndDate;

            return model;
        }
    }
}