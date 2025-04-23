using ErrandNotifier.Custom;
using ErrandNotifier.NotificationsHierarchy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class DiggablePack : ANotifiableErrandPack<Diggable, NotifiableErrand_Diggable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Diggable).GetMethod("OnSpawn", Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => CreatePostfix(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void CreatePostfix(Diggable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand chainedErrand))
         {
            //chainedErrand.ConfigureChorePrecondition(__instance.chore);
         }
      }

      public override List<GPatchInfo> OnChoreDelete_Patch() {
         return null;// the GameObject gets destroyed in either case
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
