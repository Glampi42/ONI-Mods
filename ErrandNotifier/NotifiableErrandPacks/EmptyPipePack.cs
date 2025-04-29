using ErrandNotifier.Custom;
using ErrandNotifier.NotificationsHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class EmptyPipePack : ANotifiableErrandPack<EmptyConduitWorkable, NotifiableErrand_EmptyConduitWorkable> {
      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(EmptyConduitWorkable).GetMethod(nameof(EmptyConduitWorkable.CancelEmptying), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CancelEmptyingPostfix(default));

         var targetMethod2 = typeof(EmptyConduitWorkable).GetMethod(nameof(EmptyConduitWorkable.OnWorkTick), Utils.GeneralBindingFlags);
         var postfix2 = SymbolExtensions.GetMethodInfo(() => OnWorkTickPostfix(default, default, default));

         return new([new GPatchInfo(targetMethod, null, postfix), new GPatchInfo(targetMethod2, null, postfix2)]);
      }
      private static void CancelEmptyingPostfix(EmptyConduitWorkable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            notifiableErrand.Remove(false);
         }
      }
      private static void OnWorkTickPostfix(WorkerBase worker, float dt, EmptyConduitWorkable __instance) {
         if(__instance.chore == null)
         {
            if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
            {
               notifiableErrand.Remove(true);
            }

            // removing the old chore's UISymbol (it doesn't happen automatically because the GameObject with the errand doesn't get destroyed):
            if(Main.notifierOverlay != default && Main.notifierOverlay.IsEnabled)
            {
               Main.notifierOverlay.RemoveUISymbol(__instance.gameObject, __instance);
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
   }
}
