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
               public static LocString SWEEP = "Sweeping";
               public static LocString MOP = "Mopping";
            }
         }
         public class TOOLTIPS {
            public static LocString CHAINBUTTON = "Chain up errands to be executed in some specific order {Hotkey}";
         }
         public class CHAINTOOLSMENU {
            public static LocString TITLE = "Chain Tools";

            public static LocString CREATEBUTTON = "Create";
            public static LocString DELETEBUTTON = "Delete";

            public static LocString CREATECHAIN = "Create chain";
            public static LocString CREATECHAIN_TOOLTIP = "Create the first link of a new chain";
            public static LocString CREATELINK = "Create/modify link";
            public static LocString CREATELINK_TOOLTIP = "Create a new link at the end/inside of an existing chain, or modify an existing link";

            public static LocString DELETECHAIN = "Delete chain";
            public static LocString DELETECHAIN_TOOLTIP = "Delete an entire chain";
            public static LocString DELETELINK = "Delete errand";
            public static LocString DELETELINK_TOOLTIP = "Delete selected errands from the chain they are in";
         }
         public class CHAINNUMBERS {
            public static LocString POSTFIX_1 = "<sup><u>st</sup></u>";
            public static LocString POSTFIX_2 = "<sup><u>nd</sup></u>";
            public static LocString POSTFIX_3 = "<sup><u>rd</sup></u>";
            public static LocString POSTFIX_4 = "<sup><u>th</sup></u>";
            public static LocString POSTFIX_5 = "<sup><u>th</sup></u>";
            public static LocString POSTFIX_6 = "<sup><u>th</sup></u>";
            public static LocString POSTFIX_7 = "<sup><u>th</sup></u>";
            public static LocString POSTFIX_8 = "<sup><u>th</sup></u>";
            public static LocString POSTFIX_9 = "<sup><u>th</sup></u>";
            public static LocString POSTFIX_10 = "<sup><u>th</sup></u>";
            public static LocString POSTFIX_DEFAULT = "<sup><u>th</sup></u>";
         }
      }
   }
}