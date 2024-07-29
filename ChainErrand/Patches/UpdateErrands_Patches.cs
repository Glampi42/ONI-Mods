using ChainErrand.ChainHierarchy;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.Patches {
   /// <summary>
   /// This class contains patches that update errands' display in the ChainOverlay as well as their existance in the chain
   /// if that errand was finished/deleted/just began (pipe was emptied, debris was swept to storage etc.).
   /// </summary>
   public class UpdateErrands_Patches {
      [HarmonyPatch(typeof(EmptyConduitWorkable), "OnWorkTick")]
      public static class OnEmptyPipe_Patch {
         public static void Postfix(Worker worker, float dt, EmptyConduitWorkable __instance) {
            if(__instance.chore == null)
            {
               if(Main.chainOverlay != null && Main.chainOverlay.IsEnabled)
               {
                  Main.chainOverlay.UpdateErrand(__instance);
               }

               if(__instance.emptiedPipe && __instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
               {
                  chainedErrand.Remove(true);
               }
            }
         }
      }
      [HarmonyPatch(typeof(EmptyConduitWorkable), "CancelEmptying")]
      public static class OnCancelEmptyPipe_Patch {
         public static void Postfix(EmptyConduitWorkable __instance) {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
            {
               chainedErrand.Remove(true);
            }
         }
      }

      [HarmonyPatch(typeof(Deconstructable), "CancelDeconstruction")]
      public static class OnCancelDeconstruct_Patch {
         public static void Postfix(Deconstructable __instance) {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
            {
               chainedErrand.Remove(true);
            }
         }
      }

      [HarmonyPatch(typeof(Constructable), "PlaceDiggables")]
      public static class OnConstructableChoreCreate_Patch {
         public static void Postfix(Constructable __instance) {
            if(__instance.buildChore != null && __instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
               chainedErrand.chore == null)
            {
               Debug.Log("Added chore to ChainedErrand");
               chainedErrand.ConfigureChorePrecondition(__instance.buildChore);
            }
         }
      }

      [HarmonyPatch(typeof(SimCellOccupier), "OnCleanUp")]
      public static class OnTileDestroyed_Patch {
         public static void Postfix(SimCellOccupier __instance) {
            if(Main.chainOverlay != null && Main.chainOverlay.IsEnabled)
            {
               Main.chainOverlay.UpdateTile(true, __instance);
            }
         }
      }
   }
}
