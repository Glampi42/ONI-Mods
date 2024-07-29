using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ChainErrand.Strings.MYSTRINGS.UI.TOOLS;

namespace ChainErrand.ChainHierarchy {
   public class Chain {
      public int chainID;
      public Color chainColor;
      private List<Link> links;

      public Chain(int chainID, Color chainColor) {
         this.chainID = chainID;
         this.chainColor = chainColor;
         links = new();
      }

      /// <summary>
      /// Creates a new link / expands an existing one with the specified errands.
      /// </summary>
      /// <param name="linkNumber">The number of the specified link</param>
      /// <param name="insertNewLink">Whether a new link should be inserted at the specified index, or the specified link should be expanded</param>
      /// <param name="linkErrands">The errands to be added to the link</param>
      public void CreateOrExpandLink(int linkNumber, bool insertNewLink, Dictionary<GameObject, HashSet<Workable>> linkErrands) {
         Link link;
         if(links.Count == 0 || insertNewLink || linkNumber >= links.Count)
         {
            int insertNumber = Math.Min(linkNumber, links.Count);
            link = new Link(this, insertNumber);
            links.Insert(insertNumber, link);
         }
         else
         {
            link = links[linkNumber];
         }
         UpdateAllLinkNumbers();

         HashSet<ChainedErrand> newErrands = new();
         foreach(var pair in linkErrands)
         {
            foreach(var errand in pair.Value)
            {
               ChainedErrand chainedErrand = errand.gameObject.AddComponent<ChainedErrand>();
               chainedErrand.parentLink = link;
               chainedErrand.errand = errand;
               chainedErrand.chainNumberBearer = pair.Key;

               chainedErrand.ConfigureChorePrecondition();
               chainedErrand.UpdateChainNumber();

               newErrands.Add(chainedErrand);
            }
         }

         link.errands.AddRange(newErrands);
      }

      public bool TryGetLink(int linkNum, out Link link) {
         link = default;

         if(linkNum > -1 && linkNum < links.Count)
         {
            link = links[linkNum];
            return true;
         }

         return false;
      }

      public void RemoveLink(Link link) {
         links.Remove(link);
         UpdateAllLinkNumbers();
      }

      public int LastLinkNumber() {
         return links.Count - 1;
      }

      private void UpdateAllLinkNumbers() {
         for(int index = 0; index < links.Count; index++)
         {
            bool changed = links[index].linkNumber != index;

            links[index].linkNumber = index;

            if(changed)
               links[index].UpdateChainNumbers();
         }
      }

      public void Remove(bool removeFromChainsContainer) {
         foreach(var link in links)
         {
            link.Remove(false);
         }
         links.Clear();

         if(removeFromChainsContainer)
         {
            ChainsContainer.Instance.RemoveChain(this);

            if(chainID < Main.chainTool.GetSelectedChain())
            {
               Main.chainTool.SetSelectedChain(Main.chainTool.GetSelectedChain() - 1);// to keep the same chain selected
            }
            else
            {
               Main.chainTool.SetSelectedChain(Main.chainTool.GetSelectedChain());// making sure the selected chain is inside of the chains count and updating the selected link
            }
            ChainToolMenu.Instance.UpdateNumberSelectionDisplay();
         }
      }
   }
}
