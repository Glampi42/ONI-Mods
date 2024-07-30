using ChainErrand.ChainHierarchy;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STRINGS.MISC.STATUSITEMS;

namespace ChainErrand.Patches {
   /// <summary>
   /// This class contains patches that update errands' display in the ChainOverlay.
   /// </summary>
   public class UpdateErrands_Patches {
      //------------------------Create------------------------DOWN
      [HarmonyPatch(typeof(Constructable), "PlaceDiggables")]
      public static class OnConstructableChoreCreate_Patch {
         public static void Postfix(Constructable __instance) {
            if(__instance.buildChore != null && __instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
               chainedErrand.chore == null)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.buildChore);
            }
         }
      }

      [HarmonyPatch(typeof(Deconstructable), "QueueDeconstruction")]
      [HarmonyPatch([ typeof(bool) ])]
      public static class OnDeconstructableChoreCreate_Patch {
         public static void Postfix(bool userTriggered, Deconstructable __instance) {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
               chainedErrand.chore == null)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.chore);
            }
         }
      }

      [HarmonyPatch(typeof(Diggable), "OnSpawn")]
      public static class OnDiggableChoreCreate_Patch {
         public static void Postfix(Diggable __instance) {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
               chainedErrand.chore == null)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.chore);
            }
         }
      }

      [HarmonyPatch(typeof(Moppable), "OnSpawn")]
      public static class OnMoppableChoreCreate_Patch {
         public static void Postfix(Moppable __instance) {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
               chainedErrand.chore == null)
            {
               chainedErrand.ConfigureChorePrecondition();
            }
         }
      }

      [HarmonyPatch(typeof(EmptyConduitWorkable), "CreateWorkChore")]
      public static class OnEmptyPipeChoreCreate_Patch {
         public static void Postfix(EmptyConduitWorkable __instance) {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
               chainedErrand.chore == null)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.chore);
            }
         }
      }

      [HarmonyPatch(typeof(Movable), "MarkForMove")]
      public static class OnMoveToChoreCreate_Patch {
         public static void Postfix(Movable __instance) {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
               chainedErrand.chore == null)
            {
               chainedErrand.ConfigureChorePrecondition();
            }
         }
      }
      //------------------------Create------------------------UP
      //------------------------Remove------------------------DOWN
      [HarmonyPatch(typeof(EmptyConduitWorkable), "OnWorkTick")]
      public static class OnEmptyPipe_Patch {
         public static void Postfix(Worker worker, float dt, EmptyConduitWorkable __instance) {
            if(__instance.chore == null)
            {
               if(Main.chainOverlay != null && Main.chainOverlay.IsEnabled)
               {
                  Main.chainOverlay.UpdateErrand(__instance);
               }

               if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
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
      //------------------------Remove------------------------UP


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
