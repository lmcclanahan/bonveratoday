using Common.Api.ExigoWebService;
using Common.Services;
using ExigoService;
using System;
using System.Collections.Generic;

namespace Backoffice.ViewModels.AutoOrders
{
    public class AutoOrderDateViewModel
    {
        public int AutoorderID { get; set; }
        public DateTime NextDate { get; set; }
        public int Frequency { get; set; }
        public DateTime CreatedDate { get; set; }

        public Dictionary<int, string> AvailableFrequencyTypes
        {
            get
            {
                return new Dictionary<int, string> 
                { 
                    { Exigo.GetFrequencyTypeID(FrequencyType.BiWeekly), CommonResources.FrequencyTypes(Exigo.GetFrequencyTypeID(FrequencyType.BiWeekly)) },
                    { Exigo.GetFrequencyTypeID(FrequencyType.EveryFourWeeks), CommonResources.FrequencyTypes(Exigo.GetFrequencyTypeID(FrequencyType.EveryFourWeeks)) },
                    { Exigo.GetFrequencyTypeID(FrequencyType.EverySixWeeks), CommonResources.FrequencyTypes(Exigo.GetFrequencyTypeID(FrequencyType.EverySixWeeks)) },
                    { Exigo.GetFrequencyTypeID(FrequencyType.EveryEightWeeks), CommonResources.FrequencyTypes(Exigo.GetFrequencyTypeID(FrequencyType.EveryEightWeeks)) },
                    { Exigo.GetFrequencyTypeID(FrequencyType.EveryTwelveWeeks), CommonResources.FrequencyTypes(Exigo.GetFrequencyTypeID(FrequencyType.EveryTwelveWeeks)) }                    
                };
            }
        }
    }
}