using ErrandNotifier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using static MathUtil;
using UnityEngine;
using ErrandNotifier.Structs;

namespace ErrandNotifier.NotificationsHierarchy {
   public class GNotification {
      public int notificationID;
      public string name;
      public string tooltip;
      private GNotificationType _type;
      public GNotificationType type {
         get => _type;
         set {
            _type = value;
            UpdateUISymbols();
         }
      }
      public bool pause;
      public bool zoom;

      private HashSet<NotifiableErrand> errands = new();

      public GNotification(int notificationID, string name, string tooltip, GNotificationType type, bool pause, bool zoom) {
         Debug.Log("GNotification constructor");
         this.notificationID = notificationID;
         this.name = name;
         this.tooltip = tooltip;
         this.type = type;
         this.pause = pause;
         this.zoom = zoom;
      }

      public void AddErrands(Dictionary<GameObject, HashSet<Workable>> newErrands) {
         Debug.Log("AddErrands");
         foreach(var pair in newErrands)
         {
            foreach(var errand in pair.Value)
            {
               if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand, true))
               {
                  if(!notifiableErrand.enabled)
                  {
                     notifiableErrand.enabled = true;
                     notifiableErrand.parentNotification = this;
                     notifiableErrand.uiSymbolBearer = new Ref<KPrefabID>(pair.Key.GetComponent<KPrefabID>());

                     notifiableErrand.UpdateUISymbol();

                     errands.Add(notifiableErrand);
                  }
                  else
                  {
                     Debug.LogWarning(Main.debugPrefix + $"Tried to add errand of type {errand.GetType()} of the GameObject {pair.Key.name} to a notification, but it is already in a notification");
                  }
               }
               else
               {
                  Debug.LogWarning(Main.debugPrefix + $"Tried to add errand of type {errand.GetType()} of the GameObject {pair.Key.name} to a notification, but it didn't have a related NotifiableErrand component");
               }
            }
         }
      }
      public void AddErrand(NotifiableErrand errand) {
         errands.Add(errand);
      }

      public HashSet<NotifiableErrand> GetErrands() {
         return errands;
      }

      public void UpdateUISymbols() {
         Debug.Log("UpdateUISymbols");
         if(Main.notifierOverlay != default)
         {
            foreach(var errand in errands)
            {
               errand.UpdateUISymbol();
            }
         }
      }

      /// <summary>
      /// Deletes the GNotification. If the location argument is valid, a Notification will be created and shown in NotificationScreen.
      /// </summary>
      /// <param name="notificationLocation">The location to which the camera will zoom when the Notification will be clicked. If this location is not valid, then the Notification won't be created.</param>
      public void Remove(WorldPosition notificationLocation) {
         foreach(var errand in errands)
         {
            errand.Remove(false, false);
         }
         errands.Clear();


         if(notificationLocation.worldID != -1)
         {
            NotificationsContainer.RemoveAndTriggerNotification(this, notificationLocation);
         }
      }
   }
}
