using System;

namespace ExigoService
{
    public class GetNewestDistributorsRequest : DataRequest
    {
        public GetNewestDistributorsRequest()
            : base()
        {
            MaxLevel = 10;
        }

        public int CustomerID { get; set; }
        public int MaxLevel { get; set; }
    }
}