﻿using System;
using System.Collections.Generic;
using System.Linq;
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

      public static readonly Color grayBackgroundColor = new Color32(73, 73, 73, byte.MaxValue);
      public static readonly ColorStyleSetting whiteToggleSetting;// gets darker when hovering over it/activating it

      public static readonly float noChainMarkerFontSize = 21f;
      public static readonly float maxChainNumberFontSize = 28f;
      public static readonly float minChainNumberFontSize = 13f;
      public static readonly double chainNumberDecreaseRate = 0.16;// modifies how quickly the font size goes from max to min for increasing chain numbers
      public static readonly float outlineWidthMultiplier = 0.009f;// used to get Chain Numbers' outline width from font size

      public static readonly Color DefaultChainNumberColor = PUITuning.Colors.ButtonPinkStyle.activeColor;

      public static readonly Chore.Precondition ChainedErrandPrecondition = new() {
         id = nameof(ChainedErrandPrecondition),
         description = MYSTRINGS.UI.CHOREPRECONDITION.NOTFIRSTLINK,
         fn = (ref Chore.Precondition.Context context, object _) => {
            if(context.chore.TryGetCorrespondingChainedErrand(context.chore.prioritizable.gameObject, out ChainedErrand chainedErrand))
            {
               return chainedErrand.parentLink == null || chainedErrand.parentLink.linkNumber == 0;
            }

            return false;
         }
      };

      public static PAction chainTool_binding;

      public static ChainOverlay chainOverlay;
      public static ChainTool chainTool;

      public static bool IsGameLoaded = false;

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