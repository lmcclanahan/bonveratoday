
namespace Common.Api.ExigoOData
{
    public partial class EnrollerNode
    {
        public static explicit operator ExigoService.TreeNode(EnrollerNode node)
        {
            var model = new ExigoService.TreeNode();
            if (node == null) return model;

            model.CustomerID       = node.CustomerID;
            model.ParentCustomerID = node.EnrollerID;
            model.Level            = node.Level;
            model.PlacementID      = 0;
            model.IndentedSort     = node.IndentedSort;

            return model;
        }
    }
}