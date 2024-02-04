using AdvancedFlowManagement.Strings;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedFlowManagement {
   public class MYSTRINGS : LocalizeStrings {
      public class OVERLAYITEMS {
         public class JUNCTION {
            public static LocString NAME = "Junction";
            public static LocString TOOLTIP = "A pipe section with more than 2 connections";
         }
         public class JUNCTIONINPUT {
            public static LocString NAME = "Junction Input";
            public static LocString TOOLTIP_LIQUID = "Direction from which liquid enters the junction";
            public static LocString TOOLTIP_GAS = "Direction from which gas enters the junction";
         }
         public class JUNCTIONOUTPUT {
            public static LocString NAME = "Junction Output";
            public static LocString TOOLTIP_LIQUID = "Direction in which liquid escapes the junction";
            public static LocString TOOLTIP_GAS = "Direction in which gas escapes the junction";
         }
         public class ILLEGALJUNCTION {
            public static LocString NAME = "Illegal Junction";
            public static LocString TOOLTIP_LIQUID = "A junction which liquid cannot enter or escape";
            public static LocString TOOLTIP_GAS = "A junction which gas cannot enter or escape";
         }
      }
      public class SIDESCREEN {
         public static LocString TITLE = "Flow Configuration";
         public class DIRECTIONSCREEN {
            public static LocString DIRECTION = "Direction";
            public static LocString SWITCHDIRECTION = "Switch Flow Direction";
            public static LocString FIXEDDIRECTION = "Fixed Flow Direction";
         }
         public class PRIORITYSCREEN {
            public static LocString PRIORITY = "Priority";
            public static LocString SHOWPRIORITIES = "Show flow priorities of:";
            public static LocString INPUTS = "Inputs";
            public static LocString OUTPUTS = "Outputs";
            public static LocString BOTH = "Both";
            public static LocString CHANGEPRIORITY = "Change Flow Priority";
         }
      }
   }
}