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
            public static LocString NOTIFIERBUTTON = "Assign a notification to an errand that will be triggered upon the errand's completion {Hotkey}";
         }
         public class NOTIFIERTOOLMENU {
            public static LocString TITLE = "Errand Notifier";

            public static LocString CREATEBUTTON = "Create";
            public static LocString DELETEBUTTON = "Delete";

            public static LocString CREATENOTIFICATION = "Create Notification";
            public static LocString CREATENOTIFICATION_TOOLTIP = "Create a new notification assigned to the selected errands";
            public static LocString ADDERRAND = "Manage Notification";
            public static LocString ADDERRAND_TOOLTIP = "Add new errands to an already existing notification, or change the notification's settings";

            public static LocString NOTIFICATIONID = "Notification ID:";
            public static LocString NOTIFICATIONID_TOOLTIP = "ID of the notification that should be modified";

            public static LocString NOTIFICATIONID_NOTFOUND = "NO NOTIFICATIONS";

            public static LocString DELETENOTIFICATION = "Delete Notification";
            public static LocString DELETENOTIFICATION_TOOLTIP = "Delete a notification";
            public static LocString REMOVEERRAND = "Delete Errand";
            public static LocString REMOVEERRAND_TOOLTIP = "Delete selected errands from the notification assigned to them";
         }
      }
   }
}