
namespace Common.Api.ExigoOData
{
    public partial class CustomerPointAccount
    {
        public static explicit operator ExigoService.CustomerPointAccount(CustomerPointAccount PointAccount)
        {
            var model = new ExigoService.CustomerPointAccount();
            if (PointAccount == null) return model;

            model.PointAccountID = PointAccount.PointAccountID;
            model.CustomerID     = PointAccount.CustomerID;
            model.Balance        = PointAccount.PointBalance;

            if (PointAccount.PointAccount != null)
            {
                model.PointAccountDescription = PointAccount.PointAccount.PointAccountDescription;
            }

            return model;
        }
    }
}