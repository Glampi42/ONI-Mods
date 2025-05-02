using ErrandNotifier.Custom;
using ErrandNotifier.NotificationsHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class EmptyPipeSolidPack : ANotifiableErrandPack<EmptySolidConduitWorkable, NotifiableErrand_EmptySolidConduitWorkable> {
      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(EmptySolidConduitWorkable).GetMethod(nameof(EmptySolidConduitWorkable.OnSpawn), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CancelEmptyingPostfix(default));

         var targetMethod2 = typeof(EmptySolidConduitWorkable).GetMethod(nameof(EmptySolidConduitWorkable.OnWorkTick), Utils.GeneralBindingFlags);
         var postfix2 = SymbolExtensions.GetMethodInfo(() => OnWorkTickPostfix(default, default, default));

         return new([new GPatchInfo(targetMethod, null, postfix), new GPatchInfo(targetMethod2, null, postfix2)]);
      }
      private static void CancelEmptyingPostfix(EmptySolidConduitWorkable __instance) {
         __instance.Subscribe((int)GameHashes.Cancel, OnErrandCancelDelegate);
      }
      private static readonly EventSystem.IntraObjectHandler<EmptySolidConduitWorkable> OnErrandCancelDelegate = new EventSystem.IntraObjectHandler<EmptySolidConduitWorkable>((errand, data) => {
         if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand, true))
         {
            notifiableErrand.Remove(false);
         }
      });
      private static void OnWorkTickPostfix(WorkerBase worker, float dt, EmptySolidConduitWorkable __instance) {
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
         if(gameObject.TryGetComponent(out EmptySolidConduitWorkable emptyPipe) &&
            emptyPipe.chore != null)
         {
            errands.Add(emptyPipe);
            return true;
         }

         return false;
      }
   }
}
