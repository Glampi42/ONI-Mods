using ChainErrand.ChainHierarchy;
using ChainErrand.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   public class ConstructablePack : AChainedErrandPack<Constructable, ChainedErrand_Constructable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Constructable).GetMethod("PlaceDiggables", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));
         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(Constructable __instance) {
         if(__instance.buildChore != null && __instance.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand) &&
            chainedErrand.chore == null)
         {
            chainedErrand.ConfigureChorePrecondition(__instance.buildChore);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         return null;// the GameObject gets destroyed in either case
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
