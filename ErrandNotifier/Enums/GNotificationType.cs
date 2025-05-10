using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Enums {
   public enum GNotificationType {
      NONE,
      BOING_BOING,
      POP,
      AHH
   }

   public static class GNotificationExtensions {
      public static NotificationType ToNotificationType(this GNotificationType gType) {
         switch(gType)
         {
            case GNotificationType.POP:
               return NotificationType.Neutral;

            case GNotificationType.BOING_BOING:
               return NotificationType.Bad;

            case GNotificationType.AHH:
               return NotificationType.DuplicantThreatening;

            default:
               throw new ArgumentException(Main.debugPrefix + $"No corresponding NotificationType for GNotificationType {gType}");
         }
      }
   }
}
