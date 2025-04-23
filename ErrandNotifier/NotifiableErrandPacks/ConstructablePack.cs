using ErrandNotifier.NotifiableErrandPacks;
using ErrandNotifier.Custom;
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
using ErrandNotifier.NotificationsHierarchy;

namespace ErrandNotifier.NotifiableErrandPacks {
   public class ConstructablePack : ANotifiableErrandPack<Constructable, NotifiableErrand_Constructable> {
      public override List<GPatchInfo> OnChoreCreate_Patch() {
         var targetMethod = typeof(Constructable).GetMethod(nameof(Constructable.PlaceDiggables), Utils.GeneralBindingFlags);
         var postfix = SymbolExtensions.GetMethodInfo(() => OnPlaceDiggables(default));

         return [new GPatchInfo(targetMethod, null, postfix)];
      }
      private static void OnPlaceDiggables(Constructable __instance) {
         if(__instance.TryGetCorrespondingNotifiableErrand(out NotifiableErrand ne))
         {
            if(__instance.buildChore != null)
            {
               //chainedErrand.ConfigureChorePrecondition(__instance.buildChore);
            }

            // adding the placed diggables to the same chain & link the construction errand is in:
            __instance.building.RunOnArea(cell => {
               Diggable diggable = Diggable.GetDiggable(cell);

               if(diggable.IsNullOrDestroyed() || !diggable.enabled)
                  return;

               Dictionary<GameObject, HashSet<Workable>> newErrands = new();
               newErrands.Add(diggable.gameObject, new([diggable]));
               //chainedErrand.parentLink.parentChain.CreateOrExpandLink(chainedErrand.parentLink.linkNumber, false, newErrands);
            });
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
