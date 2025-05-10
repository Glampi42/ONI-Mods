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
         this.notificationID = notificationID;
         this.name = name;
         this.tooltip = tooltip;
         this.type = type;
         this.pause = pause;
         this.zoom = zoom;
      }

      public void AddErrands(Dictionary<GameObject, HashSet<Workable>> newErrands) {
         foreach(var pair in newErrands)
         {
            foreach(var errand in pair.Value)
            {
               if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand notifiableErrand, true))
               {
                  if(notifiableErrand.IsNullOrDestroyed())
                  {
                     Debug.LogWarning(Main.debugPrefix + $"Tried to add errand of type {errand.GetType()} of the GameObject {pair.Key.name} to a notification, but the corresponding NotifiableErrand is destroyed");
                  }
                  else
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
         if(Main.notifierOverlay != default)
         {
            foreach(var errand in errands)
            {
               errand.UpdateUISymbol();
            }
         }
      }

      /// <summary>
      /// Deletes the GNotification. If tryTriggerNotification is true (and the notificationLocation is valid), a Notification will be created and shown in NotificationScreen.
      /// </summary>
      /// <param name="tryTriggerNotification">If true, the Notification will be attempted to be created</param>
      /// <param name="notificationLocation">The location to which the camera will zoom when the Notification will be clicked</param>
      /// <param name="removeUp">Shows the direction of the notification removal (is false if this GNotification is removed by the NotificationsContainer itself)</param>
      public void Remove(bool tryTriggerNotification, WorldPosition notificationLocation, bool removeUp = true) {
         foreach(var errand in errands)
         {
            errand.Remove(false, false);
         }
         errands.Clear();

         if(removeUp)
         {
            NotificationsContainer.RemoveAndTriggerNotification(this, tryTriggerNotification, notificationLocation);
         }
      }
   }
}
