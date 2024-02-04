using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Patches {
   public class CreatePrefabs_Patches {
      [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
      public static class CreatePrefabs1_Patch {
         public static void Postfix() {
            Prefabs.CreateFilterTogglePrefabs();
            Prefabs.CreateLabelPrefab();
         }
      }

      [HarmonyPatch(typeof(MainMenu), "OnPrefabInit")]
      public static class CreatePrefabs2_Patch {
         public static void Postfix() {
            Prefabs.CreateCheckboxPrefab();
         }
      }
   }
}
