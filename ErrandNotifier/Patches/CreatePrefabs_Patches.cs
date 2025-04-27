using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Patches {
   public class CreatePrefabs_Patches {
      [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
      public static class CreatePrefabs1_Patch {
         public static void Postfix() {
            Prefabs.CreateFilterTogglePrefabs();
         }
      }

      [HarmonyPatch(typeof(MainMenu), "OnPrefabInit")]
      public static class CreatePrefabs2_Patch {
         public static void Postfix() {
            Prefabs.CreateCheckboxPrefab();
         }
      }

      [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
      public static class CreatePrefabs3_Patch {
         public static void Postfix() {
            Prefabs.CreateOutlinedCheckboxPrefab();
         }
      }
   }
}
