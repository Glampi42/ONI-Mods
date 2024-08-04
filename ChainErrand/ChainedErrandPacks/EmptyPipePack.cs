using ChainErrand.ChainHierarchy;
using ChainErrand.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   public class EmptyPipePack : AChainedErrandPack<EmptyConduitWorkable, ChainedErrand_EmptyConduitWorkable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(EmptyConduitWorkable).GetMethod("CreateWorkChore", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));
         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(EmptyConduitWorkable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
            chainedErrand.chore == null)
         {
            chainedErrand.ConfigureChorePrecondition(__instance.chore);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(EmptyConduitWorkable).GetMethod("CancelEmptying", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CancelEmptyingPostfix(default));

         var targetMethod2 = typeof(EmptyConduitWorkable).GetMethod("OnWorkTick", Utils.GeneralBindingFlags);
         var postfix2 = SymbolExtensions.GetMethodInfo(() => OnWorkTickPostfix(default, default, default));

         return new([new GPatchInfo(targetMethod, null, postfix), new GPatchInfo(targetMethod2, null, postfix2)]);
      }
      private static void CancelEmptyingPostfix(EmptyConduitWorkable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            chainedErrand.Remove(true);
         }
      }
      private static void OnWorkTickPostfix(Worker worker, float dt, EmptyConduitWorkable __instance) {
         if(__instance.chore == null)
         {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
            {
               chainedErrand.Remove(true);
            }

            // removing the old chore's ChainNumber (it doesn't happen automatically because the GameObject with the errand doesn't get destroyed):
            if(Main.chainOverlay != default && Main.chainOverlay.IsEnabled)
            {
               Main.chainOverlay.RemoveChainNumber(__instance.gameObject, __instance);
            }
         }
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out EmptyConduitWorkable emptyPipe) &&
            emptyPipe.chore != null)
         {
            errands.Add(emptyPipe);
            return true;
         }

         return false;
      }

      public override Chore GetChoreFromErrand(EmptyConduitWorkable errand) {
         return errand.chore;
      }
   }
}
