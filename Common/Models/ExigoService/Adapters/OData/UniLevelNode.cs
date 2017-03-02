namespace Common.Api.ExigoOData
{
    public partial class UniLevelNode
    {
        public static explicit operator ExigoService.TreeNode(UniLevelNode node)
        {
            var model = new ExigoService.TreeNode();
            if (node == null) return model;

            model.CustomerID       = node.CustomerID;
            model.ParentCustomerID = node.SponsorID;
            model.Level            = node.Level;
            model.PlacementID      = node.Placement;
            model.IndentedSort     = node.IndentedSort;

            return model;
        }
    }
}