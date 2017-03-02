using Exigo.Api;
using Exigo.Api.Base;
using Exigo.Api.Extensions;
using Exigo.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CommissionService : ICommissionService
    {
        public ICommissionService Provider { get; set; }
        public CommissionService(ICommissionService Provider = null)
        {
            if (Provider != null)
            {
                this.Provider = Provider;
            }
            else
            {
                var defaultApiSettings = new DefaultApiSettings();
                if(defaultApiSettings.IsEnterprise) this.Provider = new SqlCommissionProvider(defaultApiSettings);
                else this.Provider = new ODataCommissionProvider(defaultApiSettings);
            }
        }

        public Rank GetRank(int RankID)
        {
            return Provider.GetRank(RankID);
        }
        public Rank GetHighestRankAchieved(int CustomerID)
        {
            return Provider.GetHighestRankAchieved(CustomerID);
        }
        public Rank GetHighestRankAchievedInCurrentPeriod(int CustomerID, int PeriodTypeID)
        {
            return Provider.GetHighestRankAchievedInCurrentPeriod(CustomerID, PeriodTypeID);
        }
        public Rank GetCurrentRank(int CustomerID, int PeriodTypeID)
        {
            return Provider.GetCurrentRank(CustomerID, PeriodTypeID);
        }
        public List<Rank> GetRanks()
        {
            return Provider.GetRanks();
        }

        public PeriodType GetPeriodType(int PeriodTypeID)
        {
            return Provider.GetPeriodType(PeriodTypeID);
        }
        public List<PeriodType> GetPeriodTypes()
        {
            return Provider.GetPeriodTypes();
        }

        public Period GetPeriod(int PeriodID, int PeriodTypeID)
        {
            return Provider.GetPeriod(PeriodID, PeriodTypeID);
        }
        public Period GetCurrentPeriod(int PeriodTypeID)
        {
            return Provider.GetCurrentPeriod(PeriodTypeID);
        }
        public List<Period> GetPeriods(int PeriodTypeID)
        {
            return Provider.GetPeriods(PeriodTypeID);
        }
        public List<Period> GetPeriods(int PeriodTypeID, DateTime StartDate)
        {
            return Provider.GetPeriods(PeriodTypeID, StartDate);
        }
    }

    public interface ICommissionService
    {
        Rank GetRank(int RankID);
        Rank GetHighestRankAchieved(int CustomerID);
        Rank GetHighestRankAchievedInCurrentPeriod(int CustomerID, int PeriodTypeID);
        Rank GetCurrentRank(int CustomerID, int PeriodTypeID);
        List<Rank> GetRanks();

        PeriodType GetPeriodType(int PeriodTypeID);
        List<PeriodType> GetPeriodTypes();

        Period GetPeriod(int PeriodID, int PeriodTypeID);
        Period GetCurrentPeriod(int PeriodTypeID);
        List<Period> GetPeriods(int PeriodTypeID);
        List<Period> GetPeriods(int PeriodTypeID, DateTime StartDate);
    }

    #region Web Service
    public class WebServiceCommissionProvider : BaseWebServiceProvider, ICommissionService
    {
        public WebServiceCommissionProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public Rank GetRank(int RankID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetRank(RankID);
            else return new ODataCommissionProvider(ApiSettings).GetRank(RankID);
        }
        public Rank GetHighestRankAchieved(int CustomerID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetHighestRankAchieved(CustomerID);
            else return new ODataCommissionProvider(ApiSettings).GetHighestRankAchieved(CustomerID);
        }
        public Rank GetHighestRankAchievedInCurrentPeriod(int CustomerID, int PeriodTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetHighestRankAchievedInCurrentPeriod(CustomerID, PeriodTypeID);
            else return new ODataCommissionProvider(ApiSettings).GetHighestRankAchievedInCurrentPeriod(CustomerID, PeriodTypeID);
        }
        public Rank GetCurrentRank(int CustomerID, int PeriodTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetCurrentRank(CustomerID, PeriodTypeID);
            else return new ODataCommissionProvider(ApiSettings).GetCurrentRank(CustomerID, PeriodTypeID);
        }
        public List<Rank> GetRanks()
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetRanks();
            else return new ODataCommissionProvider(ApiSettings).GetRanks();
        }

        public PeriodType GetPeriodType(int PeriodTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetPeriodType(PeriodTypeID);
            else return new ODataCommissionProvider(ApiSettings).GetPeriodType(PeriodTypeID);
        }
        public List<PeriodType> GetPeriodTypes()
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetPeriodTypes();
            else return new ODataCommissionProvider(ApiSettings).GetPeriodTypes();
        }

        public Period GetPeriod(int PeriodID, int PeriodTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetPeriod(PeriodID, PeriodTypeID);
            else return new ODataCommissionProvider(ApiSettings).GetPeriod(PeriodID, PeriodTypeID);
        }
        public Period GetCurrentPeriod(int PeriodTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetCurrentPeriod(PeriodTypeID);
            else return new ODataCommissionProvider(ApiSettings).GetCurrentPeriod(PeriodTypeID);
        }
        public List<Period> GetPeriods(int PeriodTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetPeriods(PeriodTypeID);
            else return new ODataCommissionProvider(ApiSettings).GetPeriods(PeriodTypeID);
        }
        public List<Period> GetPeriods(int PeriodTypeID, DateTime StartDate)
        {
            if(ApiSettings.IsEnterprise) return new SqlCommissionProvider(ApiSettings).GetPeriods(PeriodTypeID, StartDate);
            else return new ODataCommissionProvider(ApiSettings).GetPeriods(PeriodTypeID, StartDate);
        }
    }
    #endregion

    #region OData
    public class ODataCommissionProvider : BaseODataProvider, ICommissionService
    {
        public ODataCommissionProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public Rank GetRank(int RankID)
        {
            var result = new Rank();

            var response = GetContext().Ranks
                .Where(c => c.RankID == RankID)
                .FirstOrDefault();
            if(response == null) return null;

            result.RankID = response.RankID;
            result.RankDescription = response.RankDescription;

            return result;
        }
        public Rank GetHighestRankAchieved(int CustomerID)
        {
            var result = new Rank();

            var response = GetContext().Customers
                .Where(c => c.CustomerID == CustomerID)
                .Select(c => new 
                {
                    RankID = c.RankID,
                    RankDescription = c.Rank.RankDescription
                })
                .FirstOrDefault();
            if(response == null) return null;

            result.RankID = response.RankID;
            result.RankDescription = GlobalUtilities.Coalesce(response.RankDescription, "Unknown");

            return result;
        }
        public Rank GetHighestRankAchievedInCurrentPeriod(int CustomerID, int PeriodTypeID)
        {
            var result = new Rank();

            var response = GetContext().PeriodVolumes
                .Where(c => c.CustomerID == CustomerID)
                .Where(c => c.PeriodTypeID == PeriodTypeID)
                .Where(c => c.Period.IsCurrentPeriod)
                .Select(c => new 
                {
                    RankID = c.RankID,
                    RankDescription = c.Rank.RankDescription
                })
                .FirstOrDefault();
            if(response == null) return null;

            result.RankID = response.RankID;
            result.RankDescription = GlobalUtilities.Coalesce(response.RankDescription, "Unknown");

            return result;
        }
        public Rank GetCurrentRank(int CustomerID, int PeriodTypeID)
        {
            var result = new Rank();

            var response = GetContext().PeriodVolumes
                .Where(c => c.CustomerID == CustomerID)
                .Where(c => c.PeriodTypeID == PeriodTypeID)
                .Where(c => c.Period.IsCurrentPeriod)
                .Select(c => new 
                {
                    RankID = c.PaidRankID,
                    RankDescription = c.PaidRank.RankDescription
                })
                .FirstOrDefault();
            if(response == null) return null;

            result.RankID = response.RankID;
            result.RankDescription = GlobalUtilities.Coalesce(response.RankDescription, "Unknown");

            return result;
        }
        public List<Rank> GetRanks()
        {
            var result = new List<Rank>();

            var response = GetContext().Ranks.ToList();
            if(response == null) return null;

            result = response.Select(c => new Rank()
            {
                RankID = c.RankID,
                RankDescription = c.RankDescription
            }).ToList();

            return result;
        }

        public PeriodType GetPeriodType(int PeriodTypeID)
        {
            var result = new PeriodType();

            var response = GetContext().PeriodTypes
                .Where(c => c.PeriodTypeID == PeriodTypeID)
                .FirstOrDefault();
            if(response == null) return null;

            result.PeriodTypeID = response.PeriodTypeID;
            result.PeriodTypeDescription = response.PeriodTypeDescription;

            return result;
        }
        public List<PeriodType> GetPeriodTypes()
        {
            var result = new List<PeriodType>();

            var response = GetContext().PeriodTypes
                .ToList();
            if(response == null) return null;

            result = response.Select(c => new PeriodType()
            {
                PeriodTypeID = c.PeriodTypeID,
                PeriodTypeDescription = c.PeriodTypeDescription
            }).ToList();

            return result;
        }

        public Period GetPeriod(int PeriodID, int PeriodTypeID)
        {
            var result = new Period();

            var response = GetContext().Periods.Expand("PeriodType")
                .Where(c => c.PeriodTypeID == PeriodTypeID)
                .Where(c => c.PeriodID == PeriodID)
                .FirstOrDefault();
            if(response == null) return null;

            result.PeriodID = response.PeriodID;
            result.PeriodType = new PeriodType()
            {
                PeriodTypeID = response.PeriodTypeID,
                PeriodTypeDescription = response.PeriodType.PeriodTypeDescription
            };
            result.PeriodDescription = response.PeriodDescription;
            result.StartDate = response.StartDate;
            result.EndDate = response.EndDate;

            return result;
        }
        public Period GetCurrentPeriod(int PeriodTypeID)
        {
            var result = new Period();

            var response = GetContext().Periods.Expand("PeriodType")
                .Where(c => c.PeriodTypeID == PeriodTypeID)
                .Where(c => c.IsCurrentPeriod)
                .FirstOrDefault();
            if(response == null) return null;

            result.PeriodID = response.PeriodID;
            result.PeriodType = new PeriodType()
            {
                PeriodTypeID = response.PeriodTypeID,
                PeriodTypeDescription = response.PeriodType.PeriodTypeDescription
            };
            result.PeriodDescription = response.PeriodDescription;
            result.StartDate = response.StartDate;
            result.EndDate = response.EndDate;

            return result;
        }
        public List<Period> GetPeriods(int PeriodTypeID)
        {
            var result = new List<Period>();

            // Get all records recursively
            var maxRecordsPerCall = GlobalSettings.OData.MaxRecordsPerCall;
            int lastResultCount = maxRecordsPerCall;
            int completedCallCount = 0;

            while (lastResultCount == maxRecordsPerCall)
            {
                var response = GetContext().Periods.Expand("PeriodType")
                    .Where(c => c.PeriodTypeID == PeriodTypeID)
                    .Where(c => c.StartDate < DateTime.Now)
                    .OrderByDescending(c => c.PeriodID)
                    .Skip(completedCallCount * maxRecordsPerCall)
                    .Take(maxRecordsPerCall)
                    .Select(c => new Period()
                    {
                        PeriodID = c.PeriodID,
                        PeriodType = new PeriodType()
                        {
                            PeriodTypeID = c.PeriodTypeID,
                            PeriodTypeDescription = c.PeriodType.PeriodTypeDescription
                        },
                        PeriodDescription = c.PeriodDescription,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate
                    })
                    .ToList();

                response.ForEach(c => result.Add(c));

                completedCallCount++;
                lastResultCount = response.Count;
            }

            return result;
        }
        public List<Period> GetPeriods(int PeriodTypeID, DateTime StartDate)
        {
            var result = new List<Period>();

            // Get all records recursively
            var maxRecordsPerCall = GlobalSettings.OData.MaxRecordsPerCall;
            int lastResultCount = maxRecordsPerCall;
            int completedCallCount = 0;

            while (lastResultCount == maxRecordsPerCall)
            {
                var response = GetContext().Periods.Expand("PeriodType")
                    .Where(c => c.PeriodTypeID == PeriodTypeID)
                    .Where(c => c.StartDate < DateTime.Now)
                    .Where(c => c.EndDate > StartDate)
                    .OrderByDescending(c => c.PeriodID)
                    .Skip(completedCallCount * maxRecordsPerCall)
                    .Take(maxRecordsPerCall)
                    .Select(c => new Period()
                    {
                        PeriodID = c.PeriodID,
                        PeriodType = new PeriodType()
                        {
                            PeriodTypeID = c.PeriodTypeID,
                            PeriodTypeDescription = c.PeriodType.PeriodTypeDescription
                        },
                        PeriodDescription = c.PeriodDescription,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate
                    })
                    .ToList();

                response.ForEach(c => result.Add(c));

                completedCallCount++;
                lastResultCount = response.Count;
            }

            return result;
        }
    }
    #endregion

    #region Sql
    public class SqlCommissionProvider : BaseSqlProvider, ICommissionService
    {
        public SqlCommissionProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public Rank GetRank(int RankID)
        {
            var result = new Rank();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM Ranks
                    WHERE RankID = {0}
                ", RankID))
            {
                if(!reader.Read()) return null;

                result.RankID = reader.GetInt32("RankID");
                result.RankDescription = reader.GetString("RankDescription");
            }

            return result;
        }
        public Rank GetHighestRankAchieved(int CustomerID)
        {
            var result = new Rank();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        RankID = COALESCE(c.RankID, 1),
                        RankDescription = COALESCE(r.RankDescription, 'Unknown')
                    FROM Customers c
                    LEFT JOIN Ranks r
                        ON r.RankID = c.RankID
                    WHERE c.CustomerID = {0}
                ", CustomerID))
            {
                if(!reader.Read()) return null;

                result.RankID = reader.GetInt32("RankID");
                result.RankDescription = reader.GetString("RankDescription");
            }

            return result;
        }
        public Rank GetHighestRankAchievedInCurrentPeriod(int CustomerID, int PeriodTypeID)
        {
            var result = new Rank();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        RankID = COALESCE(pv.RankID, 1),
                        RankDescription = COALESCE(r.RankDescription, 'Unknown')
                    FROM PeriodVolumes pv
                    LEFT JOIN Ranks r
                        ON r.RankID = pv.RankID
                    LEFT JOIN Periods p
                        ON p.PeriodID = pv.PeriodID
                        AND p.PeriodTypeID = pv.PeriodTypeID
                    WHERE pv.CustomerID = {0}
                        AND pv.PeriodTypeID = {1}
                        AND p.StartDate < {2}
                        AND p.EndDate >= {2}                        
                ", CustomerID, PeriodTypeID, DateTime.Now))
            {
                if(!reader.Read()) return null;

                result.RankID = reader.GetInt32("RankID");
                result.RankDescription = reader.GetString("RankDescription");
            }

            return result;
        }
        public Rank GetCurrentRank(int CustomerID, int PeriodTypeID)
        {
            var result = new Rank();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        RankID = COALESCE(pv.PaidRankID, 1),
                        RankDescription = COALESCE(r.RankDescription, 'Unknown')
                    FROM PeriodVolumes pv
                    LEFT JOIN Ranks r
                        ON r.RankID = pv.PaidRankID
                    LEFT JOIN Periods p
                        ON p.PeriodID = pv.PeriodID
                        AND p.PeriodTypeID = pv.PeriodTypeID
                    WHERE pv.CustomerID = {0}
                        AND pv.PeriodTypeID = {1}
                        AND p.StartDate < {2}
                        AND p.EndDate >= {2}                        
                ", CustomerID, PeriodTypeID, DateTime.Now))
            {
                if(!reader.Read()) return null;

                result.RankID = reader.GetInt32("RankID");
                result.RankDescription = reader.GetString("RankDescription");
            }

            return result;
        }
        public List<Rank> GetRanks()
        {
            var result = new List<Rank>();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM Ranks
                "))
            {
                while (reader.Read())
                {
                    result.Add(new Rank()
                    {
                        RankID = reader.GetInt32("RankID"),
                        RankDescription = reader.GetString("RankDescription")
                    });
                }
            }

            return result;
        }

        public PeriodType GetPeriodType(int PeriodTypeID)
        {
            var result = new PeriodType();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM PeriodTypes
                    WHERE PeriodTypeID = {0}
                ", PeriodTypeID))
            {
                if(!reader.Read()) return null;

                result.PeriodTypeID = reader.GetInt32("PeriodTypeID");
                result.PeriodTypeDescription = reader.GetString("PeriodTypeDescription");
            }

            return result;
        }
        public List<PeriodType> GetPeriodTypes()
        {
            var result = new List<PeriodType>();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM PeriodTypes
                "))
            {
                while (reader.Read())
                {
                    result.Add(new PeriodType()
                    {
                        PeriodTypeID = reader.GetInt32("PeriodTypeID"),
                        PeriodTypeDescription = reader.GetString("PeriodTypeDescription")
                    });
                }
            }

            return result;
        }

        public Period GetPeriod(int PeriodID, int PeriodTypeID)
        {
            var result = new Period();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        p.*,
                        pt.PeriodTypeDescription
                    FROM Periods p
                    INNER JOIN PeriodTypes pt
                        ON pt.PeriodTypeID = p.PeriodTypeID
                    WHERE 
                        p.PeriodID = {0}
                        AND p.PeriodTypeID = {1}
                ", PeriodID, PeriodTypeID))
            {
                if(!reader.Read()) return null;

                result.PeriodID = reader.GetInt32("PeriodID");
                result.PeriodType = new PeriodType()
                {
                    PeriodTypeID = reader.GetInt32("PeriodTypeID"),
                    PeriodTypeDescription = reader.GetString("PeriodTypeDescription")
                };
                result.PeriodDescription = reader.GetString("PeriodDescription");
                result.StartDate = reader.GetDateTime("StartDate");
                result.EndDate = reader.GetDateTime("EndDate");
            }

            return result;
        }
        public Period GetCurrentPeriod(int PeriodTypeID)
        {
            var result = new Period();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        p.*,
                        pt.PeriodTypeDescription
                    FROM Periods p
                    INNER JOIN PeriodTypes pt
                        ON pt.PeriodTypeID = p.PeriodTypeID
                    WHERE 
                        p.PeriodTypeID = {0}
                        AND p.StartDate < {1}
                        AND p.EndDate >= {1}
                ", PeriodTypeID, DateTime.Now))
            {
                if(!reader.Read()) return null;

                result.PeriodID = reader.GetInt32("PeriodID");
                result.PeriodType = new PeriodType()
                {
                    PeriodTypeID = reader.GetInt32("PeriodTypeID"),
                    PeriodTypeDescription = reader.GetString("PeriodTypeDescription")
                };
                result.PeriodDescription = reader.GetString("PeriodDescription");
                result.StartDate = reader.GetDateTime("StartDate");
                result.EndDate = reader.GetDateTime("EndDate");
            }

            return result;
        }
        public List<Period> GetPeriods(int PeriodTypeID)
        {
            var result = new List<Period>();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        p.*,
                        pt.PeriodTypeDescription
                    FROM Periods p
                    INNER JOIN PeriodTypes pt
                        ON pt.PeriodTypeID = p.PeriodTypeID
                    WHERE 
                        p.PeriodTypeID = {0}
                        AND p.StartDate < {1}
                ", PeriodTypeID, DateTime.Now))
            {
                while (reader.Read())
                {
                    result.Add(new Period()
                    {
                        PeriodID = reader.GetInt32("PeriodID"),
                        PeriodType = new PeriodType()
                        {
                            PeriodTypeID = reader.GetInt32("PeriodTypeID"),
                            PeriodTypeDescription = reader.GetString("PeriodTypeDescription")
                        },
                        PeriodDescription = reader.GetString("PeriodDescription"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate")
                    });
                }
            }

            return result;
        }
        public List<Period> GetPeriods(int PeriodTypeID, DateTime StartDate)
        {
            var result = new List<Period>();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        p.*,
                        pt.PeriodTypeDescription
                    FROM Periods p
                    INNER JOIN PeriodTypes pt
                        ON pt.PeriodTypeID = p.PeriodTypeID
                    WHERE 
                        p.PeriodTypeID = {0}
                        AND p.StartDate < {1}
                        AND p.EndDate > {2}
                ", PeriodTypeID, DateTime.Now, StartDate))
            {
                while (reader.Read())
                {
                    result.Add(new Period()
                    {
                        PeriodID = reader.GetInt32("PeriodID"),
                        PeriodType = new PeriodType()
                        {
                            PeriodTypeID = reader.GetInt32("PeriodTypeID"),
                            PeriodTypeDescription = reader.GetString("PeriodTypeDescription")
                        },
                        PeriodDescription = reader.GetString("PeriodDescription"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate")
                    });
                }
            }

            return result;
        }
    }
    #endregion
}
