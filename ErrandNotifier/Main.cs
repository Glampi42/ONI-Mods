using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PeterHan.PLib.Actions;
using PeterHan.PLib.UI;
using UnityEngine;

namespace ErrandNotifier {
   public static class Main {
      public const string debugPrefix = "[ErrandNotifier] > ";

      public static readonly Assembly Assembly = typeof(Main).Assembly;

      public static readonly Color grayBackgroundColor = new Color32(73, 73, 73, byte.MaxValue);
      public static readonly ColorStyleSetting whiteToggleSetting;// gets darker when hovering over it/activating it

      public static float outlineWidthMultiplier = 0.0127f;// used to get UISymbols' outline width from font size

      public static PAction notifierTool_binding;

      public static NotifierOverlay notifierOverlay;
      public static NotifierTool notifierTool;


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
