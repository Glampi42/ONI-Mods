using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Strings {
   public class STRINGS : RegisterLocalizeStrings {
      public class GLAMPISTRINGS {
         public class MODCONFIG {
            public static LocString PERSISTENTNOTIFICATIONS = "Persistent Notifications";
            public static LocString PERSISTENTNOTIFICATIONS_TOOLTIP = "If enabled, the notifications will stay on screen until the player dismisses them.";
            public static LocString DEFAULTPAUSE = "Default Pause Setting";
            public static LocString DEFAULTPAUSE_TOOLTIP = "Determines the default setting of the \"pause\" option for new notifications (unchecked = don't pause, checked = pause).";
            public static LocString DEFAULTZOOM = "Default Zoom Setting";
            public static LocString DEFAULTZOOM_TOOLTIP = "Determines the default setting of the \"zoom\" option for new notifications (unchecked = don't zoom, checked = zoom).";
         }
      }
   }
}
