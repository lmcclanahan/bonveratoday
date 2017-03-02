
namespace Common.Api.ExigoOData
{
    public partial class BinaryNode
    {
        public static explicit operator ExigoService.TreeNode(BinaryNode node)
        {
            var model = new ExigoService.TreeNode();
            if (node == null) return model;

            model.CustomerID       = node.CustomerID;
            model.ParentCustomerID = node.ParentID;
            model.Level            = node.Level;
            model.PlacementID      = node.Placement;
            model.IndentedSort     = node.IndentedSort;

            return model;
        }
    }
}