using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Strings {
   public class MYSTRINGS : LocalizeStrings {
      public class UI {
         public class TOOLS {
            public class NOTIFIER {
               public static LocString NAME = "Notifier";
            }
            public class FILTERLAYERS {
               public class MOP {
                  public static LocString NAME = "Mopping";
                  public static LocString TOOLTIP = "Target <style=\"KKeyword\">Mopping</style> errands only";
               }
               public class EMPTYPIPE {
                  public static LocString TOOLTIP = "Target <style=\"KKeyword\">Empty Pipe</style> errands only";
               }
               public class MOVETO {
                  public static LocString TOOLTIP = "Target <style=\"KKeyword\">Relocate To</style> errands only";
               }
            }
         }
         public class TOOLTIPS {
            public static LocString NOTIFIERBUTTON = "Assign a notification to one/many errand(s) that will be triggered upon the errand's completion {Hotkey}";
         }
         public class NOTIFIERTOOLMENU {
            public static LocString TITLE = "Errand Notifier";

            public static LocString CREATEBUTTON = "Create";
            public static LocString DELETEBUTTON = "Delete";

            public static LocString CREATENOTIFICATION = "Create Notification";
            public static LocString CREATENOTIFICATION_TOOLTIP = "Create a new notification assigned to the selected errands\n You can configure the new notification before creating it";
            public static LocString ADDERRAND = "Manage Notification";
            public static LocString ADDERRAND_HOVER = "Add Errand";
            public static LocString ADDERRAND_TOOLTIP = "Add new errands to an already existing notification, or change the notification's settings";

            public static LocString NOTIFICATIONID = "Notification ID:";
            public static LocString NOTIFICATIONID_TOOLTIP = "ID of the notification that will be modified\n\"NEW\" means that you configure a new notification\n\"NONE\" means that there are no notifications to modify";

            public static LocString NOTIFICATIONID_NEW = "NEW";
            public static LocString NOTIFICATIONID_NOTFOUND = "NONE";

            public static LocString GOTONOTIFICATION_TOOLTIP = "Go to this notification";

            public static LocString NAME = "Name:";
            public static LocString NAME_TOOLTIP = "How the notification will appear when it will be triggered";
            public static LocString NAME_DEFAULT = "Errand is completed";
            public static LocString TOOLTIP = "Tooltip:";
            public static LocString TOOLTIP_TOOLTIP = "What will be displayed when hovered over the notification";
            public static LocString TOOLTIP_DEFAULT = "Press to zoom to the errand's location";
            public static LocString TYPE = "Type:";
            public static LocString TYPE_TOOLTIP = "Type of the notification";
            public static LocString PAUSE = "Pause:";
            public static LocString PAUSE_TOOLTIP = "Whether the game should be paused upon the errands' completion";
            public static LocString ZOOM = "Zoom:";
            public static LocString ZOOM_TOOLTIP = "Whether the camera should zoom to the errands upon their completion";

            public static LocString DELETENOTIFICATION = "Delete Notification";
            public static LocString DELETENOTIFICATION_TOOLTIP = "Delete a notification";
            public static LocString REMOVEERRAND = "Delete Errand";
            public static LocString REMOVEERRAND_TOOLTIP = "Delete selected errands from the notification assigned to them";
         }
      }
   }
}