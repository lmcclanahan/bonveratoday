namespace ExigoService
{
    public class GetPeriodsRequest
    {
        public int? CustomerID { get; set; }
        public int PeriodTypeID { get; set; }
        public int[] PeriodIDs { get; set; }
    }
}