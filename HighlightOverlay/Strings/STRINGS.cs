using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Strings {
   public class STRINGS : RegisterLocalizeStrings {
      public class GLAMPISTRINGS {
         public class MODCONFIG {
            public static LocString HIGHLIGHTBURRIEDGEYSERS = "Highlight Burried Geysers";
            public static LocString HIGHLIGHTBURRIEDGEYSERS_TOOLTIP = "If turned on, fully burried geysers will be highlighted(partially burried geysers will be highlighted in either case)";
            public static LocString ALLOWNOTPAUSED = "Allow Highlighting While Not Paused";
            public static LocString ALLOWNOTPAUSED_TOOLTIP = "If turned on, lets you use the Highlight Overlay while the game is not paused.\n\n" +
               "Because the highlighting doesn't update when the world changes(a new building gets built, an ice tile melts, duplicant delivers something to a storage bin etc.)," +
               "it will get incorrect over time if this option is turned on. If you are aware of this fact, you can freely turn this option on with no side effects.";
         }
      }
   }
}
