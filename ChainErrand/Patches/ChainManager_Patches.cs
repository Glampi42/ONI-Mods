using ChainErrand.ChainHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.Patches {
   /// <summary>
   /// This class contains patches that create, update and monitor the existing chains.
   /// </summary>
   public class ChainManager_Patches {
      [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
      public static class SaveGame_OnPrefabInit_Patch {
         public static void Postfix(SaveGame __instance) {
            __instance.gameObject.AddOrGet<ChainsContainer>();
         }
      }
   }
}
