using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.Strings {
   public class STRINGS : RegisterLocalizeStrings {
      public class GLAMPISTRINGS {
         public class MODCONFIG {
            //public static LocString ALLOWBUILDINGMATERIALSDELIVERY = "Allow Building Materials Delivery";
            //public static LocString ALLOWBUILDINGMATERIALSDELIVERY_TOOLTIP = "If enabled, dupes will deliver building materials even to building errands that are\n" +
            //   "not in the first link (they still won't be built until the previous errands are executed).";
            public static LocString DISABLEAUTOCHAINVIGNETTE = "Disable Auto Chain Vignette";
            public static LocString DISABLEAUTOCHAINVIGNETTE_TOOLTIP = "If enabled, the blue vignette around the screen will not be shown when auto chain is enabled.\n" +
               "It is recommended to leave this option in its defaut state for your own convenience\n(you can forget that auto chain is enabled if there is no vignette).";
            public static LocString DISABLEUIHELP = "Disable UI Help";
            public static LocString DISABLEUIHELP_TOOLTIP = "If enabled, all UI help such as automatically switching from \"Create Chain\" to \"Create/Expand Link\"\n" +
               "or automatically increasing the link number will be disabled.";
         }
      }
   }
}
