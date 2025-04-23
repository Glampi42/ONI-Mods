using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STRINGS.MISC.STATUSITEMS;

namespace ErrandNotifier.Patches {
   /// <summary>
   /// This class contains patches that update errands' display in the NotifierOverlay.
   /// </summary>
   public class UpdateErrandsInOverlay_Patches {
      [HarmonyPatch(typeof(SimCellOccupier), "OnCleanUp")]
      public static class OnTileDestroyed_Patch {
         public static void Postfix(SimCellOccupier __instance) {
            if(Main.notifierOverlay != null && Main.notifierOverlay.IsEnabled)
            {
               Main.notifierOverlay.UpdateTile(true, __instance);
            }
         }
      }
   }
}
