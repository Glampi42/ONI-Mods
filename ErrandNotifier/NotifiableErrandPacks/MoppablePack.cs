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
   public class MoppablePack : ANotifiableErrandPack<Moppable, NotifiableErrand_Moppable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Moppable).GetMethod(nameof(Moppable.OnSpawn), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(Moppable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
         {
            chainedErrand.ConfigureChorePrecondition();
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         return null;// the GameObject gets destroyed in either case
      }

      public override List<GPatchInfo> OnAutoChain_Patch() {
         var targetMethod = typeof(Moppable).GetMethod(nameof(Moppable.OnPrefabInit), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix2(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix2(Moppable __instance) {
         AutoChainUtils.TryAddToAutomaticChain(__instance.gameObject, __instance);
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Moppable moppable))
         {
            errands.Add(moppable);
            return true;
         }

         return false;
      }

      public override Chore GetChoreFromErrand(Moppable errand) {
         if(errand.TryGetComponent(out StateMachineController controller))
         {
            var workChore = (WorkChore<Moppable>.StatesInstance)controller.stateMachines.FirstOrDefault(sm => sm.GetType() == typeof(WorkChore<Moppable>.StatesInstance));
            return (Chore)workChore?.master;
         }
         return null;
      }
   }
}
