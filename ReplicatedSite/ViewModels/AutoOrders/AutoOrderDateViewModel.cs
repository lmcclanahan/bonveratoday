using Common.Api.ExigoWebService;
using System;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels.AutoOrders
{
    public class AutoOrderDateViewModel
    {
        public int AutoorderID { get; set; }
        public DateTime NextDate { get; set; }
        public FrequencyType Frequency { get; set; }
        public DateTime CreatedDate { get; set; }

        public Dictionary<FrequencyType, string> AvailableFrequencyTypes
        {
            get
            {
                return new Dictionary<FrequencyType, string> 
                { 
                    { FrequencyType.Monthly, "Monthly" }
                };
            }
        }
    }
}