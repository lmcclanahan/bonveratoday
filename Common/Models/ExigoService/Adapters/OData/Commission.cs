
namespace Common.Api.ExigoOData
{
    public partial class Commission
    {
        public static explicit operator ExigoService.HistoricalCommission(Commission commission)
        {
            var model = new ExigoService.HistoricalCommission();
            if (commission == null) return model;

            model.CustomerID        = commission.CustomerID;
            model.CurrencyCode      = commission.CurrencyCode;
            model.Total             = commission.Total;
            model.Period            = (ExigoService.Period)commission.CommissionRun.Period;

            model.CommissionRunID   = commission.CommissionRunID;
            model.Earnings          = commission.Earnings;
            model.PreviousBalance   = commission.PreviousBalance;
            model.BalanceForward    = commission.BalanceForward;
            model.Fee               = commission.Fee;

            model.PaidRank          = (ExigoService.Rank)commission.PaidRank;

            return model;
        }
    }
}