namespace Common.Api.ExigoOData
{
    public partial class PointAccount
    {
        public static explicit operator ExigoService.PointAccount(PointAccount PointAccount)
        {
            var model = new ExigoService.PointAccount();
            if (PointAccount == null) return model;

            model.PointAccountID          = PointAccount.PointAccountID;
            model.PointAccountDescription = PointAccount.PointAccountDescription;

            return model;
        }
    }
}