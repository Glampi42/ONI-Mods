using ChainErrand.ChainHierarchy;
using ChainErrand.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   public class MoveToPack : AChainedErrandPack<Movable, ChainedErrand_Movable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(CancellableMove).GetMethod(nameof(CancellableMove.OnSpawn), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnSpawnPostfix(default));

         var targetMethod2 = typeof(CancellableMove).GetMethod(nameof(CancellableMove.SetMovable), Utils.GeneralBindingFlags);
         var postfix2 = SymbolExtensions.GetMethodInfo(() => SetMovablePostfix(default, default));

         var targetMethod3 = typeof(ElementSplitterComponents).GetMethod(nameof(ElementSplitterComponents.OnTake), Utils.GeneralBindingFlags, null, [typeof(Pickupable), typeof(HandleVector<int>.Handle), typeof(float)], null);
         var prefix3 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePrefix(default, default, default, default, out InstancesLibrary.Action_Movable));
         var postfix3 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePostfix(default, default, default, default, default, ref InstancesLibrary.Action_Movable));

         var targetMethod4 = typeof(CancellableMove).GetMethod(nameof(CancellableMove.OnChoreEnd), Utils.GeneralBindingFlags);
         var postfix4 = SymbolExtensions.GetMethodInfo(() => OnChoreEndPostfix(default, default));

         var targetMethod5 = typeof(MovePickupableChore.States).GetMethod("<InitializeStates>b__16_4", Utils.GeneralBindingFlags);// inner lambda expression inside of success.Enter([...]) inside of InitializeStates()
         var postfix5 = SymbolExtensions.GetMethodInfo(() => OnChoreSuccessPostfix(default));

         return [new GPatchInfo(targetMethod, null, postfix), new GPatchInfo(targetMethod2, null, postfix2), new GPatchInfo(targetMethod3, prefix3, postfix3),
         new GPatchInfo(targetMethod4, null, postfix4), new GPatchInfo(targetMethod5, null, postfix5)];
      }
      private static void OnSpawnPostfix(CancellableMove __instance) {
         if(__instance.fetchChore != null)
         {
            GameObject movable_go = __instance.fetchChore.smi.sm.pickupablesource.Get(__instance.fetchChore.smi);
            if(movable_go != null && movable_go.TryGetComponent(out ChainedErrand_Movable chainedErrand) && chainedErrand.enabled)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.fetchChore);
            }
         }
      }
      private static void SetMovablePostfix(Movable movable, CancellableMove __instance) {
         if(__instance.fetchChore != null)
         {
            GameObject movable_go = __instance.fetchChore.smi.sm.pickupablesource.Get(__instance.fetchChore.smi);
            if(movable_go != null && movable.gameObject == movable_go/*the newly added Movable is the one this fetchChore is referencing*/ &&
               movable_go.TryGetComponent(out ChainedErrand_Movable chainedErrand) && chainedErrand.enabled)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.fetchChore);
            }
         }
      }

      private static void OnChunkTakePrefix(Pickupable pickupable, HandleVector<int>.Handle handle, float amount, ElementSplitterComponents __instance, out System.Action<Movable> __state) {// occurs when a material chunk or its portion gets picked up by a dupe
         __state = null;

         Movable parentMovable = pickupable?.GetComponent<Movable>();
         if(parentMovable != null && parentMovable.IsMarkedForMove && parentMovable.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            __state = (newMovable) => {
               // adding the split chunk to the same chain its parent is/was in:
               Dictionary<GameObject, HashSet<Workable>> newErrands = new();
               newErrands.Add(parentMovable.StorageProxy.gameObject, new([newMovable]));
               chainedErrand.parentLink.parentChain.CreateOrExpandLink(chainedErrand.parentLink.linkNumber, false, newErrands);
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
               // adding the split chunk to the same chain its parent is/was in:
               __state(movable);
            }

            if(movable.IsMarkedForMove)
            {
               if(Main.chainOverlay != default && Main.chainOverlay.IsEnabled)
               {
                  Main.chainOverlay.UpdateErrand(movable.StorageProxy?.GetComponent<CancellableMove>());
               }
            }
         }
      }

      private static void OnChoreEndPostfix(Chore chore, CancellableMove __instance) {
         if(__instance.fetchChore != null)// after a chore ends, a new one gets created if the MoveTo errand has more objects that need to be carried
         {
            GameObject movable_go = __instance.fetchChore.smi.sm.pickupablesource.Get(__instance.fetchChore.smi);
            if(movable_go != null && movable_go.TryGetComponent(out ChainedErrand_Movable chainedErrand) && chainedErrand.enabled)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.fetchChore);
            }
         }
      }
      private static void OnChoreSuccessPostfix(MovePickupableChore.StatesInstance smi) {
         if(smi != null && !smi.sm.IsDeliveryComplete(smi))// after a chore succeeds, a new one gets created if the MoveTo errand has more objects that need to be carried
         {
            GameObject movable_go = smi.sm.pickupablesource.Get(smi);
            if(movable_go != null && movable_go.TryGetComponent(out ChainedErrand_Movable chainedErrand) && chainedErrand.enabled)
            {
               chainedErrand.ConfigureChorePrecondition(smi.master);
            }
         }
      }


      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(CancellableMove).GetMethod(nameof(CancellableMove.OnCancel), Utils.GeneralBindingFlags, null, [typeof(Movable)], null);
         var prefix = SymbolExtensions.GetMethodInfo(() => OnCancelPrefix(default, default, out InstancesLibrary.List_Movable));
         var postfix1 = SymbolExtensions.GetMethodInfo(() => OnCancelPostfix(default, default, ref InstancesLibrary.List_Movable));

         var targetMethod2 = typeof(Movable).GetMethod(nameof(Movable.ClearMove), Utils.GeneralBindingFlags);
         var prefix2 = SymbolExtensions.GetMethodInfo(() => ClearMovePrefix(default));

         return [new GPatchInfo(targetMethod, prefix, postfix1), new GPatchInfo(targetMethod2, prefix2, null)];
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
               if(movable.Get().TryGetComponent(out ChainedErrand_Movable chainedErrand) && chainedErrand.enabled)
               {
                  chainedErrand.Remove(true);
               }
            }
         }
      }
      private static void ClearMovePrefix(Movable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            chainedErrand.Remove(true);
         }

         // removing the old chore's ChainNumber (it doesn't happen automatically because the GameObject with the errand doesn't get destroyed):
         if(Main.chainOverlay != default && Main.chainOverlay.IsEnabled)
         {
            Main.chainOverlay.RemoveChainNumber(__instance.StorageProxy?.gameObject, __instance);
         }
      }

      public override List<GPatchInfo> OnAutoChain_Patch() {
         var targetMethod = typeof(Movable).GetMethod(nameof(Movable.MarkForMove), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnMarkForMove(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void OnMarkForMove(Movable __instance) {
         AutoChainUtils.TryAddToAutomaticChain(__instance.gameObject, __instance);
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

      public override Chore GetChoreFromErrand(Movable errand) {
         if(errand.StorageProxy?.TryGetComponent(out CancellableMove cancellableMove) ?? false)
         {
            GameObject movable_go = cancellableMove.fetchChore.smi.sm.pickupablesource.Get(cancellableMove.fetchChore.smi);
            if(movable_go == errand.gameObject)
            {
               return cancellableMove.fetchChore;
            }
            // else the fetchChore doesn't relate to this errand
         }
         return null;
      }
   }
}
