namespace Common.Api.ExigoOData
{
    public partial class Rank
    {
        public static explicit operator ExigoService.Rank(Rank rank)
        {
            var model = new ExigoService.Rank();
            if (rank == null) return model;

            model.RankID          = rank.RankID;
            model.RankDescription = rank.RankDescription;

            return model;
        }
    }
}