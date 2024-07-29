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
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainsContainer : KMonoBehaviour {
      //[Serialize]
      private List<Chain> chains = new();

      public int ChainsCount => chains.Count;

      public void StoreChain(Chain chain) {
         chains.Add(chain);
         UpdateAllChainIDs();

         ChainToolMenu.Instance.UpdateNumberSelectionDisplay();
      }

      public void RemoveChain(Chain chain) {
         if(chain == null)
            return;

         chain.Remove(false);

         chains.Remove(chain);
         UpdateAllChainIDs();
      }

      public bool TryGetChain(int chainID, out Chain chain) {
         chain = default;

         if(chainID > -1 && chainID < chains.Count)
         {
            chain = chains[chainID];
            return true;
         }

         return false;
      }

      public static ChainsContainer Instance { get; private set; }
      public ChainsContainer() {
         Instance = this;
      }

      private void UpdateAllChainIDs() {
         for(int index = 0; index < chains.Count; index++)
         {
            chains[index].chainID = index;
         }
      }
   }
}
