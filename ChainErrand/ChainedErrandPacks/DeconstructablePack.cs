using ChainErrand.ChainHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using ChainErrand.Custom;

namespace ChainErrand.ChainedErrandPacks {
   public class DeconstructablePack : AChainedErrandPack<Deconstructable, ChainedErrand_Deconstructable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Deconstructable).GetMethod("QueueDeconstruction", Utils.GeneralBindingFlags, null, [typeof(bool)], null);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default, default));
         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(bool userTriggered, Deconstructable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
            chainedErrand.chore == null)
         {
            chainedErrand.ConfigureChorePrecondition(__instance.chore);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(Deconstructable).GetMethod("CancelDeconstruction", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => DeletePostfix(default));
         return new([new GPatchInfo(targetMethod, null, postfix)]);
      }
      private static void DeletePostfix(Deconstructable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            chainedErrand.Remove(true);
         }
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Deconstructable deconstructable) &&
            deconstructable.IsMarkedForDeconstruction())
         {
            errands.Add(deconstructable);
            return true;
         }

         return false;
      }

      public override Chore GetChoreFromErrand(Deconstructable errand) {
         return errand.chore;
      }
   }
}
