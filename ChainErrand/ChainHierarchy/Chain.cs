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
      /// <param name="linkErrands">The errands to be added to the link; the GameObject is the chain number bearer</param>
      /// <param name="forceInsertAtLinkNumber">If true, a link will be inserted(not expanded) at the desired index even if it surpasses the current links count</param>
      /// <returns>The newly created/expanded link.</returns>
      public Link CreateOrExpandLink(int linkNumber, bool insertNewLink, Dictionary<GameObject, HashSet<Workable>> linkErrands, bool forceInsertAtLinkNumber = false) {
         Link link;
         if(!forceInsertAtLinkNumber)
         {
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
         }
         else
         {
            if(linkNumber > links.Count)
            {
               int repeat = linkNumber - links.Count;
               for(int i = 0; i < repeat; i++)
                  links.Add(null);// add temporary null entries to enable inserting the link at the desired index
            }

            if(linkNumber < links.Count && links[linkNumber] == null)
            {
               links.RemoveAt(linkNumber);// replacing the null entry with this chain
            }
            link = new Link(this, linkNumber);
            links.Insert(linkNumber, link);
         }
         UpdateAllLinkNumbers();

         if(linkErrands != null)
         {
            HashSet<ChainedErrand> newErrands = new();
            foreach(var pair in linkErrands)
            {
               foreach(var errand in pair.Value)
               {
                  if(errand.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand, true))
                  {
                     if(!chainedErrand.enabled)
                     {
                        chainedErrand.enabled = true;
                        chainedErrand.parentLink = link;
                        chainedErrand.chainNumberBearer = new Ref<KPrefabID>(pair.Key.GetComponent<KPrefabID>());

                        chainedErrand.ConfigureChorePrecondition();
                        chainedErrand.UpdateChainNumber();

                        newErrands.Add(chainedErrand);
                     }
                     else
                     {
                        Debug.LogWarning(Main.debugPrefix + $"Tried to add errand of type {errand.GetType()} of the GameObject {pair.Key.name} to a chain, but it is already in a chain");
                     }
                  }
                  else
                  {
                     Debug.LogWarning(Main.debugPrefix + $"Tried to add errand of type {errand.GetType()} of the GameObject {pair.Key.name} to a chain, but it didn't have a related ChainedErrand component");
                  }
               }
            }

            link.errands.AddRange(newErrands);
         }

         return link;
      }

      public bool TryGetLink(int linkNum, out Link link) {
         link = GetLink(linkNum);
         return link != null;
      }
      public Link GetLink(int linkNum) {
         if(linkNum > -1 && linkNum < links.Count)
         {
            return links[linkNum];
         }
         return null;
      }

      public void RemoveLink(Link link) {
         links.Remove(link);
         UpdateAllLinkNumbers();
      }

      public void RemoveNullLinks() {
         links = links.Where(x => x != null).ToList();
         UpdateAllLinkNumbers();
      }

      public int LastLinkNumber() {
         return links.Count - 1;
      }

      private void UpdateAllLinkNumbers() {
         for(int index = 0; index < links.Count; index++)
         {
            if(links[index] == null)
               continue;

            int previousNum = links[index].linkNumber;

            links[index].linkNumber = index;

            if(previousNum != index)
            {
               links[index].UpdateChainNumbers();

               if(previousNum == 0)
               {
                  foreach(var chainedErrand in links[index].errands)
                  {
                     chainedErrand.InterruptChore("New link was inserted in front");
                  }
               }
            }
         }
      }

      public void Remove(bool removeFromChainsContainer) {
         foreach(var link in links)
         {
            if(link == null)
               continue;

            link.Remove(false);
         }
         links.Clear();

         if(removeFromChainsContainer)
         {
            ChainsContainer.RemoveChain(this);
         }
      }
   }
}
