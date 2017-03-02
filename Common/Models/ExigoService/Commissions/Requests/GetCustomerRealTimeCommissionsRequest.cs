namespace ExigoService
{
    public class GetCustomerRealTimeCommissionsRequest
    {
        public int CustomerID { get; set; }
        public int[] VolumeIDs { get; set; }
    }
}