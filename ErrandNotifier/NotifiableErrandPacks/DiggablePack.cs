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
   public class DiggablePack : ANotifiableErrandPack<Diggable, NotifiableErrand_Diggable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Diggable).GetMethod("OnSpawn", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(Diggable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
         {
            chainedErrand.ConfigureChorePrecondition(__instance.chore);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         return null;// the GameObject gets destroyed in either case
      }

      public override List<GPatchInfo> OnAutoChain_Patch() {
         var targetMethod = typeof(DigTool).GetMethod(nameof(DigTool.PlaceDig), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnPlaceDig(default, default, default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void OnPlaceDig(int cell, int animationDelay, GameObject __result) {
         if(__result != null && __result.TryGetComponent(out Diggable diggable))
         {
            AutoChainUtils.TryAddToAutomaticChain(__result, diggable);
         }
      }

      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Diggable diggable))
         {
            errands.Add(diggable);
            return true;
         }

         return false;
      }

      public override Chore GetChoreFromErrand(Diggable errand) {
         return errand.chore;
      }
   }
}
