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
               public static LocString SHOWCROSSINGS = "Show Junctions";
               public static LocString HIDECROSSINGS = "Hide Junctions";
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
