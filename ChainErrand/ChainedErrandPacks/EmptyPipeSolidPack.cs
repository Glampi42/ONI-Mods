﻿using ChainErrand.ChainHierarchy;
using ChainErrand.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   public class EmptyPipeSolidPack : AChainedErrandPack<EmptySolidConduitWorkable, ChainedErrand_EmptySolidConduitWorkable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(EmptySolidConduitWorkable).GetMethod(nameof(EmptySolidConduitWorkable.CreateWorkChore), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));
         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(EmptySolidConduitWorkable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            chainedErrand.ConfigureChorePrecondition(__instance.chore);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         var targetMethod = typeof(EmptySolidConduitWorkable).GetMethod(nameof(EmptySolidConduitWorkable.CancelEmptying), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CancelEmptyingPostfix(default));

         var targetMethod2 = typeof(EmptySolidConduitWorkable).GetMethod(nameof(EmptySolidConduitWorkable.OnWorkTick), Utils.GeneralBindingFlags);
         var postfix2 = SymbolExtensions.GetMethodInfo(() => OnWorkTickPostfix(default, default, default));

         return new([new GPatchInfo(targetMethod, null, postfix), new GPatchInfo(targetMethod2, null, postfix2)]);
      }
      private static void CancelEmptyingPostfix(EmptySolidConduitWorkable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            chainedErrand.Remove(true);
         }
      }
      private static void OnWorkTickPostfix(WorkerBase worker, float dt, EmptySolidConduitWorkable __instance) {
         if(__instance.chore == null)
         {
            if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
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
         var targetMethod = typeof(EmptySolidConduitWorkable).GetMethod(nameof(EmptySolidConduitWorkable.CreateWorkChore), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnMarkForEmpty(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void OnMarkForEmpty(EmptySolidConduitWorkable __instance) {
         AutoChainUtils.TryAddToAutomaticChain(__instance.gameObject, __instance);
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

      public override Chore GetChoreFromErrand(EmptySolidConduitWorkable errand) {
         return errand.chore;
      }
   }
}
