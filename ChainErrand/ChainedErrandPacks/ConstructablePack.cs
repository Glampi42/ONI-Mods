using ChainErrand.ChainHierarchy;
using ChainErrand.Custom;
using HarmonyLib;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   public class ConstructablePack : AChainedErrandPack<Constructable, ChainedErrand_Constructable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Constructable).GetMethod(nameof(Constructable.PlaceDiggables), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnPlaceDiggables(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void OnPlaceDiggables(Constructable __instance) {
         if(__instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
         {
            if(__instance.buildChore != null)
            {
               chainedErrand.ConfigureChorePrecondition(__instance.buildChore);
            }

            // adding the placed diggables to the same chain & link the construction errand is in:
            __instance.building.RunOnArea(cell => {
               Diggable diggable = Diggable.GetDiggable(cell);

               if(diggable.IsNullOrDestroyed() || !diggable.enabled)
                  return;

               Dictionary<GameObject, HashSet<Workable>> newErrands = new();
               newErrands.Add(diggable.gameObject, new([diggable]));
               chainedErrand.parentLink.parentChain.CreateOrExpandLink(chainedErrand.parentLink.linkNumber, false, newErrands);
            });
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         return null;// the GameObject gets destroyed in either case
      }

      public override List<GPatchInfo> OnAutoChain_Patch() {
         var targetMethod = typeof(BuildTool).GetMethod(nameof(BuildTool.PostProcessBuild), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnBuildingGhostCreate(default, default, default));

         var targetMethod2 = typeof(BaseUtilityBuildTool).GetMethod(nameof(BaseUtilityBuildTool.BuildPath), Utils.GeneralBindingFlags);
         var transpiler2 = SymbolExtensions.GetMethodInfo(() => OnBuildPath(default, default));

         return [new GPatchInfo(targetMethod, null, postfix), new GPatchInfo(targetMethod2, null, null, transpiler2)];
      }
      private static void OnBuildingGhostCreate(bool instantBuild, Vector3 pos, GameObject builtItem) {
         if(builtItem != null && builtItem.TryGetComponent(out Constructable constructable))
         {
            AutoChainUtils.TryAddToAutomaticChain(builtItem, constructable);
         }
      }
      /// <summary>
      /// This transpiler inserts the code that tries to add each utility in the build path to the auto chain.
      /// </summary>
      private static IEnumerable<CodeInstruction> OnBuildPath(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
         List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

         int endOfLoopIndex = -1;
         for(int i = 0; i < codes.Count; i++)
         {
            if(codes[i].opcode == OpCodes.Br)
            {
               Label loopHeadLabel = (Label) codes[i].operand;
               for(int ii = i + 1; ii < codes.Count; ii++)
               {
                  if(codes[ii].labels.Contains(loopHeadLabel))
                  {
                     endOfLoopIndex = ii - 4;// compensating for the four instructions that increase the loop variable

                     break;
                  }
               }

               break;
            }
         }
         if(endOfLoopIndex == -1)
            throw new Exception(Main.debugPrefix + "The end of the loop could not be found");

         List<CodeInstruction> codesCluster = new List<CodeInstruction>();

         codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 6));// load go (the utility that got built)
         codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => OnUtilityBuild(default))));

         // inserting the code at the end of the main for-loop that loops through all utilities that got built:
         codes.InsertRange(endOfLoopIndex, codesCluster);

         return codes.AsEnumerable();
      }
      private static void OnUtilityBuild(GameObject utility_go) {
         if(!utility_go.IsNullOrDestroyed() && utility_go.TryGetComponent(out Constructable constructable))
         {
            AutoChainUtils.TryAddToAutomaticChain(utility_go, constructable);
         }
      }


      public override bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference) {
         if(gameObject.TryGetComponent(out Constructable constructable))
         {
            errands.Add(constructable);
            return true;
         }

         return false;
      }

      public override Chore GetChoreFromErrand(Constructable errand) {
         return errand.buildChore;
      }
   }
}
