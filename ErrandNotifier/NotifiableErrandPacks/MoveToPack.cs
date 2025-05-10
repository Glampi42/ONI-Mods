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

         var targetMethod2 = typeof(MovePickupableChore.States).GetMethod("<InitializeStates>b__16_5", Utils.GeneralBindingFlags);// inner lambda expression inside of success.Enter([...]) inside of InitializeStates()
         var prefix2 = SymbolExtensions.GetMethodInfo(() => OnChoreSuccessPrefix(default));

         var targetMethod3 = typeof(ElementSplitterComponents).GetMethod(nameof(ElementSplitterComponents.OnTake), Utils.GeneralBindingFlags, null, [typeof(Pickupable), typeof(HandleVector<int>.Handle), typeof(float)], null);
         var prefix3 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePrefix(default, default, default, default, out InstancesLibrary.Action_Movable));
         var postfix3 = SymbolExtensions.GetMethodInfo(() => OnChunkTakePostfix(default, default, default, default, default, ref InstancesLibrary.Action_Movable));

         return [new GPatchInfo(targetMethod, prefix, null), new GPatchInfo(targetMethod2, prefix2, null), new GPatchInfo(targetMethod3, prefix3, postfix3)];
      }
      private static void ClearMovePrefix(Movable __instance) {
         Debug.Log("$$$ClearMove1");
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
         {
            Debug.Log("$$$ClearMove2");
            notifiableErrand.Remove(false);
         }

         // removing the old chore's UISymbol (it doesn't happen automatically because the GameObject with the errand doesn't get destroyed):
         if(Main.notifierOverlay != default && Main.notifierOverlay.IsEnabled)
         {
            Main.notifierOverlay.RemoveUISymbol(__instance.StorageProxy?.gameObject, __instance);
         }
      }
      //private static IEnumerable<CodeInstruction> OnDeliveryCompleteTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
      //   List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

      //   int clearMoveIndex = -1;
      //   for(int i = 0; i < codes.Count; i++)
      //   {
      //      if(codes[i].Calls(SymbolExtensions.GetMethodInfo(() => ((Movable)default).ClearMove())))
      //      {
      //         clearMoveIndex = i - 1;// compensating for ldloc (loading the Movable onto stack)

      //         break;
      //      }
      //   }
      //   if(clearMoveIndex == -1)
      //      throw new Exception(Main.debugPrefix + "The ClearMove() method could not be found");

      //   List<CodeInstruction> codesCluster = new List<CodeInstruction>();

      //   // redirecting the labels to the new instructions (last I checked there are no labels that should be redirected, but just for safety):
      //   List<Label> labels = codes[clearMoveIndex].labels;
      //   codes[clearMoveIndex].labels.Clear();

      //   codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 1).WithLabels(labels));// load Movable
      //   codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => OnDeliveryComplete(default))));

      //   codes.InsertRange(clearMoveIndex, codesCluster);

      //   return codes.AsEnumerable();
      //}
      //private static void OnDeliveryComplete(Movable movable) {
      //   Debug.Log("$$$OnDeliveryComplete1");
      //   if(movable.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand))
      //   {
      //      Debug.Log("$$$OnDeliveryComplete2");
      //      notifiableErrand.Remove(true);// triggering the notification since the errand was successfully completed (doesn't happen automatically because the Movable's GO doesn't get destroyed)
      //   }
      //}
      private static void OnChoreSuccessPrefix(MovePickupableChore.StatesInstance smi) {
         Debug.Log("OnChoreSuccessPostfix1");
         if(smi != null)
         {
            Debug.Log("OnChoreSuccessPostfix2");
            GameObject movable_go = smi.sm.pickup.Get(smi);
            if(movable_go != null && movable_go.TryGetComponent(out NotifiableErrand_Movable notifiableErrand) && notifiableErrand.enabled)
            {
               Debug.Log("OnChoreSuccessPostfix3");
               notifiableErrand.Remove(true);// trying to trigger the notification (doesn't happen automatically because the Movable's GO doesn't get destroyed)
            }
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
