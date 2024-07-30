using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.ChainHierarchy {
   /// <summary>
   /// Stores all existing chains.
   /// </summary>
   public static class ChainsContainer {
      private static List<Chain> chains = new();

      public static int ChainsCount => chains.Count;

      public static void StoreChain(Chain chain, int atIndex = -1) {
         chains.Add(chain);
         UpdateAllChainIDs();

         ChainToolMenu.Instance.UpdateNumberSelectionDisplay();
      }

      public static void RemoveChain(Chain chain) {
         if(chain == null)
            return;

         chain.Remove(false);

         chains.Remove(chain);
         UpdateAllChainIDs();
      }

      public static bool TryGetChain(int chainID, out Chain chain) {
         chain = null;

         if(chainID > -1 && chainID < chains.Count)
         {
            chain = chains[chainID];
         }

         return chain != null;
      }

      private static void UpdateAllChainIDs() {
         for(int index = 0; index < chains.Count; index++)
         {
            chains[index].chainID = index;
         }
      }
   }
}
