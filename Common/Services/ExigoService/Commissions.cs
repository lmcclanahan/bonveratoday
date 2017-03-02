using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<ICommission> GetCommissionList(int customerID)
        {
            // Historical Commissions
            var historicalCommissions = GetHistoricalCommissionList(customerID);
            foreach (var commission in historicalCommissions)
            {
                yield return commission;
            }
        }
        public static IEnumerable<ICommission> GetHistoricalCommissionList(int customerID)
        {
            // Historical Commissions
            var commissions = Exigo.OData().Commissions.Expand("CommissionRun/Period").Expand("PaidRank")
                .Where(c => c.CustomerID == customerID)
                .OrderByDescending(c => c.CommissionRunID);
            if (commissions != null)
            {
                foreach (var commission in commissions)
                {
                    yield return (HistoricalCommission)commission;
                }
            }
        }

        public static IEnumerable<ICommission> GetCommissionPeriodList(int customerID)
        {
            // Historical Commissions
            var commissions = Exigo.OData().Commissions.Expand("CommissionRun/Period")
                .Where(c => c.CustomerID == customerID)
                .OrderByDescending(c => c.CommissionRunID);

            if (commissions != null)
            {
                foreach (var commission in commissions)
                {
                    yield return (HistoricalCommission)commission;
                }
            }
        }

        public static HistoricalCommission GetCustomerHistoricalCommission(int customerID, int commissionRunID)
        {
            // Get the commission record
            var commission = Exigo.OData().Commissions.Expand("CommissionRun/Period").Expand("PaidRank")
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.CommissionRunID == commissionRunID)
                .FirstOrDefault();
            if (commission == null) return null;
            var result = (HistoricalCommission)commission;


            // Get the volumes
            result.Volumes = GetCustomerVolumes(new GetCustomerVolumesRequest
            {
                CustomerID = customerID,
                PeriodID = result.Period.PeriodID,
                PeriodTypeID = result.Period.PeriodTypeID,
                VolumeIDs = new int[] { 1, 2, 3, 4, 16 }
            });

            return result;
        }
        public static IEnumerable<RealTimeCommission> GetCustomerRealTimeCommissions(GetCustomerRealTimeCommissionsRequest request)
        {
             
            var results = new List<RealTimeCommission>();


            // Get the commission record
            var realtimeresponse = Exigo.WebService().GetRealTimeCommissions(new Common.Api.ExigoWebService.GetRealTimeCommissionsRequest
            {
                CustomerID = request.CustomerID
            });
            if (realtimeresponse.Commissions.Length == 0) return results;


            // Get the unique periods for each of the commission results
            var periods = new List<Period>();
            var periodRequests = new List<GetPeriodsRequest>();
            foreach (var commissionResponse in realtimeresponse.Commissions)
            {
                var periodID = commissionResponse.PeriodID;
                var periodTypeID = commissionResponse.PeriodType;

                var req = periodRequests.Where(c => c.PeriodTypeID == periodTypeID).FirstOrDefault();
                if (req == null)
                {
                    periodRequests.Add(new GetPeriodsRequest()
                    {
                        PeriodTypeID = periodTypeID,
                        PeriodIDs = new int[] { periodID }
                    });
                }
                else
                {
                    var ids = req.PeriodIDs.ToList();
                    ids.Add(periodID);
                    req.PeriodIDs = ids.Distinct().ToArray();
                }
            }
            foreach (var req in periodRequests)
            {
                var responses = GetPeriods(req);
                foreach (var response in responses)
                {
                    periods.Add(response);
                }
            }


            // Get the volumes for each unique period
            var volumeCollections = new List<VolumeCollection>();
            foreach (var period in periods)
            {
                volumeCollections.Add(GetCustomerVolumes(new GetCustomerVolumesRequest
                {
                    CustomerID   = request.CustomerID,
                    PeriodID     = period.PeriodID,
                    PeriodTypeID = period.PeriodTypeID,
                    VolumeIDs    = request.VolumeIDs
                }));
            }

            // Process each commission response 
            try
            {
                foreach (var commission in realtimeresponse.Commissions)
                {
                    var typedCommission = (RealTimeCommission)commission;

                    typedCommission.Period = periods
                        .Where(c => c.PeriodTypeID == commission.PeriodType)
                        .Where(c => c.PeriodID == commission.PeriodID)
                        .FirstOrDefault();

                    typedCommission.Volumes = volumeCollections
                        .Where(c => c.Period.PeriodTypeID == typedCommission.Period.PeriodTypeID)
                        .Where(c => c.Period.PeriodID == typedCommission.Period.PeriodID)
                        .FirstOrDefault();

                    typedCommission.PaidRank = typedCommission.Volumes.PayableAsRank;

                    results.Add(typedCommission);
                }

                return results.OrderByDescending(c => c.Period.StartDate);
            }
            catch { return results; }
        }
    }
}