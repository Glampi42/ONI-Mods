using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChainErrand.Strings.MYSTRINGS.UI.TOOLS;

namespace ChainErrand.ChainHierarchy {
   public class Link {
      public Chain parentChain;

      public int linkNumber;
      public HashSet<ChainedErrand> errands;

      public Link(Chain parentChain, int linkNumber) {
         this.parentChain = parentChain;
         this.linkNumber = linkNumber;
         errands = new();
      }

      public void UpdateChainNumbers() {
         if(Main.chainOverlay != default)
         {
            foreach(var errand in errands)
            {
               errand.UpdateChainNumber();
            }
         }
      }

      public void Remove(bool tryRemoveChain) {
         foreach(var errand in errands)
         {
            errand.Remove(false);
         }
         errands.Clear();

         if(tryRemoveChain)
         {
            parentChain.RemoveLink(this);

            if(parentChain.chainID == Main.chainTool.GetSelectedChain())
            {
               if(linkNumber < Main.chainTool.GetSelectedLink())
               {
                  Main.chainTool.SetSelectedLink(Main.chainTool.GetSelectedLink() - 1, Main.chainTool.GetInsertNewLink());// to keep the same link selected
               }
               else if(Main.chainTool.GetSelectedLink() > parentChain.LastLinkNumber())
               {
                  Main.chainTool.SetSelectedLink(parentChain.LastLinkNumber() + 1, true);
               }
               ChainToolMenu.Instance.UpdateNumberSelectionDisplay();
            }

            if(parentChain.LastLinkNumber() < 0)
            {
               parentChain.Remove(true);
            }
         }
      }
   }
}
