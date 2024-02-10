using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay {
   public class TreeNode {
      private readonly Dictionary<string, TreeNode> _children = new Dictionary<string, TreeNode>();

      public readonly string Name;
      public TreeNode Parent { get; private set; }

      public TreeNode(string name) {
         Name = name;
      }

      public TreeNode AddChild(TreeNode child, bool returnNewNode = false) {
         if(child.Parent != null)
         {
            child.Parent._children.Remove(child.Name);
         }

         child.Parent = this;
         _children.Add(child.Name, child);

         return returnNewNode ? child : this;
      }

      public bool IsRoot() {
         return Parent == null;
      }

      public bool HasChild(string childName, out TreeNode child) {
         child = default;
         if(_children.ContainsKey(childName))
         {
            child = _children[childName];
         }

         return child != default;
      }
      public List<TreeNode> GetAllChildren() {
         return _children.Values.ToList();
      }
   }
}
