using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ErrandNotifier.Enums;
using ErrandNotifier.NotifiableErrandPacks;
using ErrandNotifier.NotificationsHierarchy;

namespace ErrandNotifier {
   public static class NotifierToolUtils {
      /// <summary>
      /// Checks whether a GameObject isn't filtered out by the NotifierTool's filters and collects the errands related to it.
      /// </summary>
      /// <param name="errand_go">The GameObject</param>
      /// <param name="errandReference">Component attached to the GameObject that represents the errand (is used in NotifierOverlay)</param>
      /// <param name="searchMode">Determines which kinds of errands should be collected</param>
      /// <returns>The collected errands.</returns>
      public static HashSet<Workable> CollectFilteredErrands(this GameObject errand_go, out KMonoBehaviour errandReference, ErrandsSearchMode searchMode = ErrandsSearchMode.ALL_ERRANDS) {
         errandReference = default;
         var errands = new HashSet<Workable>();

         if(errand_go == null)
            return errands;

         foreach(NotifierToolFilter filter in Enum.GetValues(typeof(NotifierToolFilter)))
         {
            if(filter.IsOn())
            {
               switch(filter)
               {
                  case NotifierToolFilter.ALL:
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Constructable)).CollectErrands(errand_go, errands, ref errandReference);
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Deconstructable)).CollectErrands(errand_go, errands, ref errandReference);
                     // pipes can have multiple errands(deconstruct + empty)

                     if(NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(EmptyConduitWorkable)).CollectErrands(errand_go, errands, ref errandReference))
                        break;// buildings can't have other errands

                     if(NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(EmptySolidConduitWorkable)).CollectErrands(errand_go, errands, ref errandReference))
                        break;// buildings can't have other errands

                     if(NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Diggable)).CollectErrands(errand_go, errands, ref errandReference))
                        break;// digging markers can't have other errands

                     if(NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Moppable)).CollectErrands(errand_go, errands, ref errandReference))
                        break;// mopping markers can't have other errands

                     if(NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Movable)).CollectErrands(errand_go, errands, ref errandReference))
                        break;// moveto markers can't have other errands
                     break;

                  case NotifierToolFilter.CONSTRUCTION:
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Constructable)).CollectErrands(errand_go, errands, ref errandReference);
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Deconstructable)).CollectErrands(errand_go, errands, ref errandReference);
                     break;

                  case NotifierToolFilter.DIG:
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Diggable)).CollectErrands(errand_go, errands, ref errandReference);
                     break;

                  case NotifierToolFilter.MOP:
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Moppable)).CollectErrands(errand_go, errands, ref errandReference);
                     break;

                  case NotifierToolFilter.EMPTY_PIPE:
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(EmptyConduitWorkable)).CollectErrands(errand_go, errands, ref errandReference);
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(EmptySolidConduitWorkable)).CollectErrands(errand_go, errands, ref errandReference);
                     break;

                  case NotifierToolFilter.MOVE_TO:
                     NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Movable)).CollectErrands(errand_go, errands, ref errandReference);
                     break;

                  case NotifierToolFilter.STANDARD_BUILDINGS:
                  case NotifierToolFilter.LIQUID_PIPES:
                  case NotifierToolFilter.GAS_PIPES:
                  case NotifierToolFilter.CONVEYOR_RAILS:
                  case NotifierToolFilter.WIRES:
                  case NotifierToolFilter.AUTOMATION:
                  case NotifierToolFilter.BACKWALLS:
                     if(errand_go.TryGetComponent(out Building building))
                     {
                        ObjectLayer objLayer = building.Def.ObjectLayer;
                        if(Utils.ObjectLayersFromNotifierToolFilter(filter).Contains(objLayer))
                        {
                           NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Constructable)).CollectErrands(errand_go, errands, ref errandReference);
                           NotifiableErrandPackRegistry.GetNotifiableErrandPack(typeof(Deconstructable)).CollectErrands(errand_go, errands, ref errandReference);
                        }
                     }
                     break;
               }

               break;
            }
         }

         if(searchMode != ErrandsSearchMode.ALL_ERRANDS)
         {
            // filtering out errands that are/aren't already assigned to a notification:
            errands = new(errands.Where(errand => {
               if(errand.TryGetCorrespondingNotifiableErrand(out _))
               {
                  return searchMode == ErrandsSearchMode.ERRANDS_INSIDE_NOTIFICATION;
               }
               return searchMode == ErrandsSearchMode.ERRANDS_OUTSIDE_NOTIFICATION;
            }));
         }

         if(errandReference == default)
            errandReference = errands.FirstOrDefault();

         return errands;
      }

      /// <summary>
      /// Returns the currently active NotifierToolFilter selected in the NotifierToolMenu.
      /// </summary>
      /// <returns>The filter.</returns>
      public static NotifierToolFilter GetCurrentFilter() {
         foreach(NotifierToolFilter filter in Enum.GetValues(typeof(NotifierToolFilter)))
         {
            if(filter.IsOn())
            {
               return filter;
            }
         }

         return NotifierToolFilter.ALL;
      }

      /// <summary>
      /// Creates a new notification and assigns it to the specified errands.
      /// </summary>
      /// <param name="errands">The errands</param>
      public static void CreateNewNotification(Dictionary<GameObject, HashSet<Workable>> errands) {
         Debug.Log("CreateNewNotification");
         GNotification n = NotificationsContainer.CreateNewNotification();
         n.AddErrands(errands);

         Main.notifierTool.SetSelectedNotification(n.notificationID);

         NotifierToolMenu.Instance?.modeToggles[NotifierToolMode.ADD_ERRAND].onClick();// switching to Add Errand

         Main.notifierTool.ResetNewNotification();// clear the new-notification settings after creating one
      }

      /// <summary>
      /// Adds the errands to an already existing notification. The notification ID is taken from the Notifier Tool.
      /// </summary>
      /// <param name="errands">The errands to be added to the notification</param>
      public static void AddErrands(Dictionary<GameObject, HashSet<Workable>> errands) {
         if(NotificationsContainer.TryGetNotification(Main.notifierTool.GetSelectedNotification(), out GNotification n))
         {
            n.AddErrands(errands);
         }
      }

      /// <summary>
      /// Deletes all notifications that are assigned to the specified errands.
      /// </summary>
      /// <param name="errands">The errands</param>
      public static void DeleteNotifications(HashSet<Workable> errands) {
         HashSet<GNotification> nsToDelete = new();
         foreach(var errand in errands)
         {
            if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand nE))
            {
               nsToDelete.Add(nE.parentNotification);
            }
         }

         foreach(var n in nsToDelete)
         {
            NotificationsContainer.RemoveAndTriggerNotification(n, Utils.InvalidLocation);// the notification was deleted manually -> shouldn't be triggered
         }
      }

      /// <summary>
      /// Removes the specified errands from the notifications assigned to them. If the notification isn't assigned to any other errands, then it will be deleted.
      /// </summary>
      /// <param name="errands">The errands</param>
      public static void RemoveErrands(HashSet<Workable> errands) {
         foreach(var errand in errands)
         {
            if(errand.TryGetCorrespondingNotifiableErrand(out NotifiableErrand nE))
            {
               nE.Remove(false);
            }
         }
      }

      /// <summary>
      /// Tries to retrieve the notification ID from the input text.
      /// </summary>
      /// <param name="text">The text</param>
      /// <returns>The notification ID.</returns>
      public static int InterpretNotificationID(string text) {
         int result;
         if(!int.TryParse(text, out result))
         {
            result = 0;
         }

         return result;
      }
   }
}
