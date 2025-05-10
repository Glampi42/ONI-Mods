using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ChainErrand;
using ChainErrand.ChainHierarchy;
using ChainErrand.Enums;
using ChainErrand.Strings;
using PeterHan.PLib.Actions;
using PeterHan.PLib.UI;
using UnityEngine;

namespace ChainErrand {
   public static class Main {
      public const string debugPrefix = "[ChainErrand] > ";

      public static readonly Assembly Assembly = typeof(Main).Assembly;

      public static readonly Color grayBackgroundColor = new Color32(73, 73, 73, byte.MaxValue);
      public static readonly ColorStyleSetting whiteToggleSetting;// gets darker when hovering over it/activating it

      public static readonly Color autoChainVignetteColor = new Color(0f, 0f, 1f, 0.4f);

      public static readonly float noChainMarkerFontSize = 21f;
      public static readonly float maxChainNumberFontSize = 28f;
      public static readonly float minChainNumberFontSize = 13f;
      public static readonly double chainNumberDecreaseRate = 0.16;// modifies how quickly the font size goes from max to min for increasing chain numbers
      public static float outlineWidth = 0.36f;

      public static readonly Color DefaultChainNumberColor = PUITuning.Colors.ButtonPinkStyle.activeColor;

      public static Chore.Precondition ChainedErrandPrecondition = new() {
         id = nameof(ChainedErrandPrecondition),
         description = MYSTRINGS.UI.CHOREPRECONDITION.NOTFIRSTLINK,
         fn = (ref Chore.Precondition.Context context, object preconditionEnabled) => {
            if(preconditionEnabled == null || !(bool)preconditionEnabled)
               return true;

            if(context.chore.masterPriority.priority_class == PriorityScreen.PriorityClass.topPriority)
               return true;

            GameObject go;
            if(context.chore is MovePickupableChore moveChore)
            {
               go = moveChore.smi.sm.pickupablesource.Get(moveChore.smi);// MoveTo errand's prioritizable isn't attached to the GameObject that has the errand
            }
            else
            {
               go = context.chore.prioritizable.gameObject;
            }
            if(go != null && context.chore.TryGetCorrespondingChainedErrand(go, out ChainedErrand chainedErrand))
            {
               return chainedErrand.parentLink == null || chainedErrand.parentLink.linkNumber == 0;
            }

            return true;// this return shouldn't normally be reached, but if it is reached - no need to block the execution of the chore
         }
      };

      public static Notification autoChainNotification = null;

      public static PAction chainTool_binding;

      public static ChainOverlay chainOverlay;
      public static ChainTool chainTool;

      public static bool autoChainEnabled = false;


      static Main() {
         Color gray = new Color(0.784f, 0.784f, 0.784f, 1f);
         Color darkerGray = new Color(0.695f, 0.695f, 0.695f, 1f);

         whiteToggleSetting = ScriptableObject.CreateInstance<ColorStyleSetting>();
         whiteToggleSetting.activeColor = darkerGray;
         whiteToggleSetting.inactiveColor = Color.white;
         whiteToggleSetting.hoverColor = gray;
         whiteToggleSetting.disabledActiveColor = darkerGray;
         whiteToggleSetting.disabledColor = gray;
         whiteToggleSetting.disabledhoverColor = darkerGray;
      }
   }
}
