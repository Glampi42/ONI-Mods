using ErrandNotifier.Enums;
using ErrandNotifier.NotificationsHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier {
   public static class SerializationUtils {
      private static object reconstructNotificationLock = new object();
      public static void ReconstructNotification(int notificationID, NotifiableErrand notifiableErrand, GNotificationType notificationType) {
         lock(reconstructNotificationLock)// in case deserialization happens in parallel
         {
            if(notificationID == -1 || notifiableErrand.IsNullOrDestroyed())
               return;

            GNotification n;
            if(!NotificationsContainer.TryGetNotification(notificationID, out n))
            {
               n = new GNotification(notificationID, notificationType);
               NotificationsContainer.StoreNotification(n, notificationID);
            }

            notifiableErrand.parentNotification = n;
            notifiableErrand.enabled = true;
         }
      }

      /// <summary>
      /// Removes empty notifications. They might emerge because a serialized errand is gone (save file modified, errand cancelled etc.).
      /// </summary>
      public static void CleanupEmptyDeserializedNotifications() {
         for(int notificationID = 0; notificationID < NotificationsContainer.NotificationsCount; notificationID++)
         {
            GNotification n = NotificationsContainer.GetNotification(notificationID);
            if(n == null)
            {
               Debug.LogWarning(Main.debugPrefix + $"The entire notification with notificationID {notificationID} is missing");
            }
         }
         NotificationsContainer.RemoveNullNotifications();
      }
   }
}