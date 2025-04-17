using ErrandNotifier.Components;
using ErrandNotifier.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.ChainedErrandPacks {
   public class EmptyPipePack : ANotifiableErrandPack<EmptyConduitWorkable, NotifiableErrand_EmptyConduitWorkable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(EmptyConduitWorkable).GetMethod("CreateWorkChore", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));
         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(EmptyConduitWorkable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
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
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
         {
            chainedErrand.Remove(true);
         }
      }
      private static void OnWorkTickPostfix(WorkerBase worker, float dt, EmptyConduitWorkable __instance) {
         if(__instance.chore == null)
         {
            if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
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

      public override List<GPatchInfo> OnAutoChain_Patch() {
         var targetMethod = typeof(EmptyConduitWorkable).GetMethod(nameof(EmptyConduitWorkable.CreateWorkChore), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnMarkForEmpty(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void OnMarkForEmpty(EmptyConduitWorkable __instance) {
         AutoChainUtils.TryAddToAutomaticChain(__instance.gameObject, __instance);
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
