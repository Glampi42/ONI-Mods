using ErrandNotifier.Custom;
using ErrandNotifier.NotificationsHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class MoveToPack : ANotifiableErrandPack<Movable, NotifiableErrand_Movable> {
      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(Movable).GetMethod(nameof(Movable.ClearMove), Utils.GeneralBindingFlags);
         var prefix = SymbolExtensions.GetMethodInfo(() => ClearMovePrefix(default));

         var targetMethod2 = typeof(ElementSplitterComponents).GetMethod(nameof(ElementSplitterComponents.OnTake), Utils.GeneralBindingFlags, null, [typeof(Pickupable), typeof(HandleVector<int>.Handle), typeof(float)], null);
         var prefix2 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePrefix(default, default, default, default, out InstancesLibrary.Action_Movable));
         var postfix2 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePostfix(default, default, default, default, default, ref InstancesLibrary.Action_Movable));

         return [new GPatchInfo(targetMethod, prefix, null), new GPatchInfo(targetMethod2, prefix2, postfix2)];
      }
      private static void ClearMovePrefix(Movable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            notifiableErrand.Remove(false);
         }

         // removing the old chore's UISymbol (it doesn't happen automatically because the GameObject with the errand doesn't get destroyed):
         if(Main.notifierOverlay != default && Main.notifierOverlay.IsEnabled)
         {
            Main.notifierOverlay.RemoveUISymbol(__instance.StorageProxy?.gameObject, __instance);
         }
      }

      private static void OnChunkTakePrefix(Pickupable pickupable, HandleVector<int>.Handle handle, float amount, ElementSplitterComponents __instance, out System.Action<Movable> __state) {// occurs when a material chunk or its portion gets picked up by a dupe
         __state = null;

         Movable parentMovable = pickupable?.GetComponent<Movable>();
         if(parentMovable != null && parentMovable.IsMarkedForMove && parentMovable.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            __state = (newMovable) => {
               // adding the split chunk to the same notification its parent is/was in:
               Dictionary<GameObject, HashSet<Workable>> newErrands = new();
               newErrands.Add(parentMovable.StorageProxy.gameObject, new([newMovable]));
               notifiableErrand.parentNotification.AddErrands(newErrands);
            };
         }
      }
      private static void OnChunkTakePostfix(Pickupable pickupable, HandleVector<int>.Handle handle, float amount, ElementSplitterComponents __instance, Pickupable __result,
         ref System.Action<Movable> __state) {
         Movable movable = __result?.GetComponent<Movable>();
         if(movable != null)
         {
            if(__state != null)
            {
               // adding the split chunk to the same notification its parent is/was in:
               __state(movable);
            }

            if(movable.IsMarkedForMove)
            {
               if(Main.notifierOverlay != default && Main.notifierOverlay.IsEnabled)
               {
                  Main.notifierOverlay.UpdateErrand(movable.StorageProxy?.GetComponent<CancellableMove>());
               }
            }
         }
      }


      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out CancellableMove cancellableMove) &&
               cancellableMove.movingObjects.Count > 0)
         {
            foreach(var movable in cancellableMove.movingObjects)
            {
               errands.Add(movable.Get());
            }
            errandReference = cancellableMove;
            return true;
         }

         return false;
      }
   }
}
