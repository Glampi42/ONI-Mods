using ErrandNotifier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using static MathUtil;
using UnityEngine;

namespace ErrandNotifier.NotificationsHierarchy {
   public class GNotification {
      public int notificationID;
      public GNotificationType type;
      private HashSet<NotifiableErrand> errands;

      public GNotification(int notificationID, GNotificationType type) {
         this.notificationID = notificationID;
         this.type = type;
         errands = new();
      }

      public void AddErrands(Dictionary<GameObject, HashSet<Workable>> newErrands) {
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

                     //notifiableErrand.ConfigureChorePrecondition();
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

      public void Remove(bool removeFromNotificationsContainer) {
         foreach(var errand in errands)
         {
            errand.Remove(false);
         }
         errands.Clear();


         if(removeFromNotificationsContainer)
         {
            NotificationsContainer.RemoveNotification(this, false);
         }
      }
   }
}
