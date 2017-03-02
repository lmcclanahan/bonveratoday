using ExigoService;
using System.Collections.Generic;

namespace Backoffice.Models
{
    public class UnilevelNestedTreeNode : TreeNode, INestedTreeNode<UnilevelNestedTreeNode>
    {
        public UnilevelNestedTreeNode()
        {
            this.Customer    = new Customer();
            this.CurrentRank = new Rank();

            this.Children = new List<UnilevelNestedTreeNode>();
        }

        public List<UnilevelNestedTreeNode> Children { get; set; }

        public Customer Customer { get; set; }
        public Rank CurrentRank { get; set; }
        public bool IsPersonallyEnrolled { get; set; }
        public bool HasAutoOrder { get; set; }
    }
}