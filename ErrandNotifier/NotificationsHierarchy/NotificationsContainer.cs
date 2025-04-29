using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrandNotifier.Enums;
using ErrandNotifier.Strings;
using ErrandNotifier.Structs;
using UnityEngine;

namespace ErrandNotifier.NotificationsHierarchy {
   /// <summary>
   /// Stores all existing notifications.
   /// </summary>
   public static class NotificationsContainer {
      private static List<GNotification> notifications = new();

      public static int NotificationsCount => notifications.Count;

      /// <summary>
      /// Creates a new notification with the settings specified in the NotifierTool and stores it in the NotificationsContainer.
      /// </summary>
      /// <returns></returns>
      public static GNotification CreateNewNotification() {
         Debug.Log("NotificationsContainer.CreateNewNotification");
         GNotification newN = new GNotification(NotificationsCount,
            Main.notifierTool.GetName(),
            Main.notifierTool.GetTooltip(),
            Main.notifierTool.GetNotificationType(),
            Main.notifierTool.GetShouldPause(),
            Main.notifierTool.GetShouldZoom());
         StoreNotification(newN);

         return newN;
      }

      public static void StoreNotification(GNotification n, int atIndex = -1) {
         Debug.Log("StoreNotification");
         if(atIndex == -1)
         {
            notifications.Add(n);
         }
         else
         {
            if(atIndex > notifications.Count)
            {
               int repeat = atIndex - notifications.Count;
               for(int i = 0; i < repeat; i++)
                  notifications.Add(null);// add temporary null entries to enable inserting the notification at the desired index
            }

            if(atIndex < notifications.Count && notifications[atIndex] == null)
            {
               notifications.RemoveAt(atIndex);// replacing the null entry with this notification
            }
            notifications.Insert(atIndex, n);
         }
         UpdateAllNotificationIDs();

         NotifierToolMenu.Instance?.UpdateNotificationConfigDisplay();
      }

      /// <summary>
      /// Removes the GNotification from the container and triggers the actual Notification displayed on the NotificationScreen.
      /// </summary>
      /// <param name="n">The GNotification to remove and to use when constructing the Notification</param>
      /// <param name="notificationLocation">The location to which the camera will zoom when the Notification will be clicked. If this location is not valid, then the Notification won't be created.</param>
      public static void RemoveAndTriggerNotification(GNotification n, WorldPosition notificationLocation) {
         if(n == null)
            return;

         if(notificationLocation.worldID != -1)// location is valid -> create a Notification pointing to that location
         {
            Notification notification = new Notification(n.name, n.type.ToNotificationType(), (l, o) => n.tooltip, expires: !ModConfig.Instance.PersistentNotifications,
               show_dismiss_button: ModConfig.Instance.PersistentNotifications, custom_click_callback: location => {
                  WorldPosition pos = (WorldPosition)location;
                  Utils.MoveCamera(pos, true);
               }, custom_click_data: notificationLocation);

            NotificationScreen.Instance.AddNotification(notification);
            notification.GameTime = Time.time;
            notification.Time = KTime.Instance.UnscaledGameTime;

            // pause the game:
            if(n.pause && !SpeedControlScreen.Instance.IsPaused)
               SpeedControlScreen.Instance.Pause(false);

            // zoom the camera:
            if(n.zoom)
               Utils.MoveCamera(notificationLocation, false);
         }

         int ID = n.notificationID;

         if(notificationLocation.worldID == -1)// location is invalid -> the GNotification should be removed without creating the Notification
         {
            n.Remove(Utils.InvalidLocation);
         }

         notifications.Remove(n);
         UpdateAllNotificationIDs();

         if(ID < Main.notifierTool.GetSelectedNotification())
         {
            Main.notifierTool.SetSelectedNotification(Main.notifierTool.GetSelectedNotification() - 1);// to keep the same notification selected
         }
         else
         {
            Main.notifierTool.SetSelectedNotification(Main.notifierTool.GetSelectedNotification());// making sure the selected notification is inside of the notifications count
         }
         NotifierToolMenu.Instance?.UpdateNotificationConfigDisplay();
      }

      public static void RemoveNullNotifications() {
         notifications = notifications.Where(x => x != null).ToList();
         UpdateAllNotificationIDs();
      }

      public static void Clear() {
         notifications.Clear();
      }

      public static bool TryGetNotification(int ID, out GNotification notification) {
         notification = GetNotification(ID);
         return notification != null;
      }
      public static GNotification GetNotification(int ID) {
         if(ID > -1 && ID < notifications.Count)
         {
            return notifications[ID];
         }
         return null;
      }

      private static void UpdateAllNotificationIDs() {
         for(int index = 0; index < notifications.Count; index++)
         {
            if(notifications[index] != null)
            {
               notifications[index].notificationID = index;
            }
         }
      }
   }
}
