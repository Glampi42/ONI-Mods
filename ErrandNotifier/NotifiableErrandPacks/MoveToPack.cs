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
         var targetMethod = typeof(CancellableMove).GetMethod(nameof(CancellableMove.OnCancel), Utils.GeneralBindingFlags, null, [typeof(Movable)], null);
         var prefix1 = SymbolExtensions.GetMethodInfo(() => OnCancelPrefix(default, default, out InstancesLibrary.List_Movable));
         var postfix1 = SymbolExtensions.GetMethodInfo(() => OnCancelPostfix(default, default, ref InstancesLibrary.List_Movable));

         var targetMethod2 = typeof(Movable).GetMethod(nameof(Movable.ClearMove), Utils.GeneralBindingFlags);
         var prefix2 = SymbolExtensions.GetMethodInfo(() => ClearMovePrefix(default));

         var targetMethod3 = typeof(ElementSplitterComponents).GetMethod(nameof(ElementSplitterComponents.OnTake), Utils.GeneralBindingFlags, null, [typeof(Pickupable), typeof(HandleVector<int>.Handle), typeof(float)], null);
         var prefix3 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePrefix(default, default, default, default, out InstancesLibrary.Action_Movable));
         var postfix3 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePostfix(default, default, default, default, default, ref InstancesLibrary.Action_Movable));

         return [new GPatchInfo(targetMethod, prefix1, postfix1), new GPatchInfo(targetMethod2, prefix2, null), new GPatchInfo(targetMethod3, prefix3, postfix3)];
      }
      private static void OnCancelPrefix(Movable cancel_movable, CancellableMove __instance, out List<Ref<Movable>> __state) {
         __state = new();
         foreach(var movable in __instance.movables)
            __state.Add(movable);
      }
      private static void OnCancelPostfix(Movable cancel_movable, CancellableMove __instance, ref List<Ref<Movable>> __state) {
         foreach(var movable in __state)
         {
            if(movable?.Get() != null && !__instance.movables.Contains(movable))// if Movable was removed from the list
            {
               if(movable.Get().TryGetComponent(out NotifiableErrand_Movable notifiableErrand) && notifiableErrand.enabled)
               {
                  notifiableErrand.Remove(false);
               }
            }
         }
      }
      private static void ClearMovePrefix(Movable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            notifiableErrand.Remove(true);
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
