using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<Rank> GetRanks()
        {
            var context = Exigo.OData();
            var apiRanks = context.Ranks
                .OrderBy(c => c.RankID);

            foreach(var apiRank in apiRanks)
            {
                yield return (Rank)apiRank;
            }
        }
        public static Rank GetRank(int rankID)
        {
            return GetRanks()
                .Where(c => c.RankID == rankID)
                .FirstOrDefault();
        }

        public static IEnumerable<Rank> GetNextRanks(int rankID)
        {
            return GetRanks()
                .Where(c => c.RankID > rankID)
                .OrderBy(c => c.RankID)
                .ToList();
        }
        public static Rank GetNextRank(int rankID)
        {
            return GetNextRanks(rankID).FirstOrDefault();
        }

        public static IEnumerable<Rank> GetPreviousRanks(int rankID)
        {
            return GetRanks()
                .Where(c => c.RankID < rankID)
                .OrderByDescending(c => c.RankID)
                .ToList();
        }
        public static Rank GetPreviousRank(int rankID)
        {
            return GetPreviousRanks(rankID).FirstOrDefault();
        }

        public static CustomerRankCollection GetCustomerRanks(GetCustomerRanksRequest request)
        {
            var result = new CustomerRankCollection();
            var context = Exigo.OData();

            // Get the highest rank achieved in the customer table
            var highestRankAchieved = context.Customers
                .Where(c => c.CustomerID == request.CustomerID)
                .Select(c => new
                {
                    c.Rank
                }).FirstOrDefault();
            if (highestRankAchieved != null)
            {
                result.HighestPaidRankInAnyPeriod = (Rank)highestRankAchieved.Rank;
            }

            // Get the period ranks
            var query = context.PeriodVolumes
                .Where(c => c.CustomerID == request.CustomerID)
                .Where(c => c.PeriodTypeID == request.PeriodTypeID);

            if (request.PeriodID != null)
            {
                query = query.Where(c => c.PeriodID == (int)request.PeriodID);
            }
            else
            {
                query = query.Where(c => c.Period.IsCurrentPeriod);
            }

            var periodRanks = query.Select(c => new
            {
                c.Rank,
                c.PaidRank
            }).FirstOrDefault();

            if (periodRanks != null)
            {
                if (periodRanks.PaidRank != null)
                {
                    result.CurrentPeriodRank = (Rank)periodRanks.PaidRank;
                }
                if (periodRanks.Rank != null)
                {
                    result.HighestPaidRankUpToPeriod = (Rank)periodRanks.Rank;
                }
            }

            return result;
        }
    }
}