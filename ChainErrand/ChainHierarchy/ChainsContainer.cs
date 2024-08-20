using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChainErrand.Strings.MYSTRINGS.UI.TOOLS;

namespace ChainErrand.ChainHierarchy {
   /// <summary>
   /// Stores all existing chains.
   /// </summary>
   public static class ChainsContainer {
      private static List<Chain> chains = new();

      public static int ChainsCount => chains.Count;

      public static Chain CreateNewChain() {
         Chain newChain = new Chain(ChainsCount, Utils.RandomChainColor());
         StoreChain(newChain);

         return newChain;
      }

      public static void StoreChain(Chain chain, int atIndex = -1) {
         if(atIndex == -1)
         {
            chains.Add(chain);
         }
         else
         {
            if(atIndex > chains.Count)
            {
               int repeat = atIndex - chains.Count;
               for(int i = 0; i < repeat; i++)
                  chains.Add(null);// add temporary null entries to enable inserting the chain at the desired index
            }

            if(atIndex < chains.Count && chains[atIndex] == null)
            {
               chains.RemoveAt(atIndex);// replacing the null entry with this chain
            }
            chains.Insert(atIndex, chain);
         }
         UpdateAllChainIDs();

         ChainToolMenu.Instance?.UpdateNumberSelectionDisplay();
      }

      public static void RemoveChain(Chain chain, bool callRemoveChain = true) {
         if(chain == null)
            return;

         int chainID = chain.chainID;

         AutoChainUtils.NullifyAutomaticChain(chain);

         if(callRemoveChain)
         {
            chain.Remove(false);
         }

         chains.Remove(chain);
         UpdateAllChainIDs();

         if(chainID < Main.chainTool.GetSelectedChain())
         {
            Main.chainTool.SetSelectedChain(Main.chainTool.GetSelectedChain() - 1);// to keep the same chain selected
         }
         else
         {
            Main.chainTool.SetSelectedChain(Main.chainTool.GetSelectedChain());// making sure the selected chain is inside of the chains count and updating the selected link
         }
         ChainToolMenu.Instance?.UpdateNumberSelectionDisplay();
      }

      public static void RemoveNullChains() {
         chains = chains.Where(x => x != null).ToList();
         UpdateAllChainIDs();
      }

      public static void Clear() {
         chains.Clear();
      }

      public static bool TryGetChain(int chainID, out Chain chain) {
         chain = GetChain(chainID);
         return chain != null;
      }
      public static Chain GetChain(int chainID) {
         if(chainID > -1 && chainID < chains.Count)
         {
            return chains[chainID];
         }
         return null;
      }

      private static void UpdateAllChainIDs() {
         for(int index = 0; index < chains.Count; index++)
         {
            if(chains[index] != null)
            {
               chains[index].chainID = index;
            }
         }
      }
   }
}
