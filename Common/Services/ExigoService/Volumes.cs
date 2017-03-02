using Common.Api.ExigoOData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static VolumeCollection GetCustomerVolumes(GetCustomerVolumesRequest request)
        {
            //2015-08-04
            //Ivan Sanchez
            //60
            //Added the changes Travis and I did for PHP to avoid the dashboard error
            var requestedSpecificVolumes = (request.VolumeIDs != null && request.VolumeIDs.Length > 0);

            var baseQuery = Exigo.OData().PeriodVolumes.Expand("Period,Rank,PaidRank");
            var query = baseQuery
                .Where(c => c.CustomerID == request.CustomerID)
                .Where(c => c.PeriodTypeID == request.PeriodTypeID);

            // Determine which period ID to use
            if (request.PeriodID != null)
            {
                query = query.Where(c => c.PeriodID == (int)request.PeriodID);
            }
            else
            {
                query = query.Where(c => c.Period.IsCurrentPeriod);
            }

            var data = query.FirstOrDefault();
            var result = (VolumeCollection)data;

            return result;
        }    
    }
}