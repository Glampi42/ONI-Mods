using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.Strings {
   public class MYSTRINGS : LocalizeStrings {
      public class UI {
         public class TOOLS {
            public class CHAIN {
               public static LocString NAME = "Chain";
            }
            public class FILTERLAYERS {
               //public class SWEEP {
               //   public static LocString NAME = "Sweeping";
               //   public static LocString TOOLTIP = "Target <style=\"KKeyword\">Sweeping</style> errands only";
               //}
               public class MOP {
                  public static LocString NAME = "Mopping";
                  public static LocString TOOLTIP = "Target <style=\"KKeyword\">Mopping</style> errands only";
               }
               public class EMPTYPIPE {
                  public static LocString TOOLTIP = "Target <style=\"KKeyword\">Empty Pipe</style> errands only";
               }
               public class MOVETO {
                  public static LocString TOOLTIP = "Target <style=\"KKeyword\">Move To</style> errands only";
               }
            }
         }
         public class TOOLTIPS {
            public static LocString CHAINBUTTON = "Chain up errands to be executed in some specific order {Hotkey}";
         }
         public class CHAINTOOLSMENU {
            public static LocString TITLE = "Chain Tools";

            public static LocString CREATEBUTTON = "Create";
            public static LocString DELETEBUTTON = "Delete";

            public static LocString CREATECHAIN = "Create Chain";
            public static LocString CREATECHAIN_TOOLTIP = "Create the first link of a new chain";
            public static LocString CREATELINK = "Create/Expand Link";
            public static LocString CREATELINK_TOOLTIP = "Create a new link at the end/inside of an existing chain, or expand an existing link";

            public static LocString CHAINNUMBER = "Chain ID:";
            public static LocString CHAINNUMBER_TOOLTIP = "ID of the chain that should be modified";
            public static LocString LINKNUMBER = "Link Nr.:";
            public static LocString LINKNUMBER_TOOLTIP = "Number of the link\nIf the number is whole, an existing link will be expanded\n" +
               "If the number is a fraction, a new link will be inserted inbetween";

            public static LocString CHAINNUMBER_NOTFOUND = "NO CHAINS";
            public static LocString LINKNUMBER_NOTFOUND = "NO LINKS";

            public static LocString DELETECHAIN = "Delete Chain";
            public static LocString DELETECHAIN_TOOLTIP = "Delete an entire chain";
            public static LocString DELETELINK = "Delete Errand";
            public static LocString DELETELINK_TOOLTIP = "Delete selected errands from the chain they are in";
         }
         public class CHAINNUMBERS {
            public static LocString NO_CHAIN_NUMBER = "*";

            public static LocString POSTFIX_STYLE_START = "<sup><u>";
            public static LocString POSTFIX_STYLE_END = "</sup></u>";

            public static LocString POSTFIX_1 = "st";
            public static LocString POSTFIX_2 = "nd";
            public static LocString POSTFIX_3 = "rd";
            public static LocString POSTFIX_4 = "th";
            public static LocString POSTFIX_5 = "th";
            public static LocString POSTFIX_6 = "th";
            public static LocString POSTFIX_7 = "th";
            public static LocString POSTFIX_8 = "th";
            public static LocString POSTFIX_9 = "th";
            public static LocString POSTFIX_10 = "th";
            public static LocString POSTFIX_DEFAULT = "th";

            // english versions of the postfixes:
            public static LocString UNTRANSLATED_POSTFIX_1 = "st";
            public static LocString UNTRANSLATED_POSTFIX_2 = "nd";
            public static LocString UNTRANSLATED_POSTFIX_3 = "rd";
            public static LocString UNTRANSLATED_POSTFIX_4 = "th";
            public static LocString UNTRANSLATED_POSTFIX_5 = "th";
            public static LocString UNTRANSLATED_POSTFIX_6 = "th";
            public static LocString UNTRANSLATED_POSTFIX_7 = "th";
            public static LocString UNTRANSLATED_POSTFIX_8 = "th";
            public static LocString UNTRANSLATED_POSTFIX_9 = "th";
            public static LocString UNTRANSLATED_POSTFIX_10 = "th";
            public static LocString UNTRANSLATED_POSTFIX_DEFAULT = "th";
         }
         public class CHOREPRECONDITION {
            public static LocString NOTFIRSTLINK = "Must be in the 1st link";
         }
         public class AUTOCHAINBUTTON {
            public static LocString TOOLTIP_HEADER = "Toggle Auto Chain";
            public static LocString TOOLTIP_CONTENT = "If enabled, all new errands (that can be added to a chain) will be chained up.\nSwitch off and on to create a new chain.\n\n" +
               "Errands that were created simultaneously and construct errands that were created with one stroke (without releasing the left mouse button) will be added to the same link.";
         }
         public class AUTOCHAINNOTIFICATION {
            public static LocString NAME = "Auto Chain Enabled";
            public static LocString TOOLTIP = "All newly created errands will be chained up\n\nPress to disable";
         }
      }
   }
}