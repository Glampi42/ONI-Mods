using ChainErrand.ChainHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand {
   public static class SerializationUtils {
      private static object reconstructChainLock = new object();
      public static void ReconstructChain(int chainID, int linkNumber, ChainedErrand chainedErrand, Color chainColor) {
         lock(reconstructChainLock)// in case deserialization happens in parallel
         {
            if(chainID == -1 || linkNumber == -1 || chainedErrand == null)
               return;

            Debug.Log($"ReconstructChain ID:{chainID}, linkNum:{linkNumber}, errandType:{chainedErrand.GetType()}");
            Chain chain;
            if(!ChainsContainer.TryGetChain(chainID, out chain))
            {
               chain = new Chain(chainID, chainColor);
               ChainsContainer.StoreChain(chain, chainID);
            }

            Link link;
            if(!chain.TryGetLink(linkNumber, out link))
            {
               link = chain.CreateOrExpandLink(linkNumber, true, null, true);
            }

            link.errands.Add(chainedErrand);

            chainedErrand.parentLink = link;
            chainedErrand.enabled = true;
         }
      }

      /// <summary>
      /// Removes all null-links from all chains and removes null-chains. They might emerge because a serialized errand is gone (save file modified, errand cancelled etc.).
      /// </summary>
      public static void CleanupEmptyDeserializedChains() {
         for(int chainID = 0; chainID < ChainsContainer.ChainsCount; chainID++)
         {
            Chain chain = ChainsContainer.GetChain(chainID);
            if(chain == null)
            {
               Debug.LogWarning(Main.debugPrefix + $"The entire chain with chainID {chainID} is missing");
            }
            else
            {
               for(int linkNum = 0; linkNum <= chain.LastLinkNumber(); linkNum++)
               {
                  Link link = chain.GetLink(linkNum);
                  if(link == null)
                  {
                     Debug.LogWarning(Main.debugPrefix + $"The entire link with link number {linkNum} of chain {chainID} is missing");
                  }
               }
               chain.RemoveNullLinks();
            }
         }
         ChainsContainer.RemoveNullChains();
      }
   }
}