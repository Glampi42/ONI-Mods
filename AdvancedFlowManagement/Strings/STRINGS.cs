using AdvancedFlowManagement.Strings;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedFlowManagement.Strings {
   public class STRINGS : RegisterLocalizeStrings {
      public class UI {
         public class TOOLS {
            public class FILTERLAYERS {
               public class SHOWCROSSINGS {
                  public static LocString NAME = "Show Junctions";
                  public static LocString TOOLTIP = "Make junctions visible";
               }
               public class HIDECROSSINGS {
                  public static LocString NAME = "Hide Junctions";
                  public static LocString TOOLTIP = "Make junctions invisible";
               }
            }
         }
      }
      public class BUILDING {
         public class STATUSITEMS {
            public class BUFFERSTORAGE {
               public static LocString NAME = "Buffer Contents: {Contents}";
               public static LocString TOOLTIP = "This pipe's buffer contains: {Contents}";
            }
         }
      }
   }
}
