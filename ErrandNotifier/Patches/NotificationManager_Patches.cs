using ErrandNotifier.NotifiableErrandPacks;
using ErrandNotifier.NotificationsHierarchy;
using HarmonyLib;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.Patches {
   /// <summary>
   /// This class contains patches that create, update and monitor the existing notifications as well as serialize them.
   /// </summary>
   public class NotificationManager_Patches {
      /// <summary>
      /// Adding the NotifiableErrand component to the prefabs.
      /// </summary>
      [HarmonyPatch(typeof(Assets), "OnPrefabInit")]
      [HarmonyPriority(Priority.Low)]
      public static class AddNotifiableErrandCmp_Patch {
         public static void Postfix() {
            foreach(var prefab in Assets.Prefabs)
            {
               IEnumerable<Type> errandTypes = NotifiableErrandPackRegistry.AllErrandTypes();
               foreach(var errandType in errandTypes)
               {
                  if(prefab.gameObject.TryGetComponent(errandType, out _))
                  {
                     var notifiableErrand = prefab.gameObject.AddComponent(NotifiableErrandPackRegistry.GetNotifiableErrandPack(errandType).GetNotifiableErrandType()) as NotifiableErrand;
                     if(notifiableErrand == null)
                        throw new Exception(Main.debugPrefix + $"Failed to add NotifiableErrand component for errand of type {errandType}");

                     notifiableErrand.enabled = false;
                  }
               }
            }
         }
      }

      [HarmonyPatch(typeof(Game), "Load")]
      public static class ClearEmptyNotifications_Patch {
         public static void Postfix(Deserializer deserializer) {
            SerializationUtils.CleanupEmptyDeserializedNotifications();
         }
      }
   }
}
