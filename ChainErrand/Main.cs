using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChainErrand;
using ChainErrand.Enums;
using PeterHan.PLib.Actions;
using UnityEngine;

namespace ChainErrand {
   public static class Main {
      public const string debugPrefix = "[ChainErrand] > ";

      public static readonly Color grayBackgroundColor = new Color32(73, 73, 73, byte.MaxValue);
      public static readonly float maxChainNumberFontSize = 30f;
      public static readonly float minChainNumberFontSize = 13f;
      public static readonly double chainNumberDecreaseRate = 0.16;// modifies how quickly the font size goes from max to min for increasing chain numbers
      public static readonly System.Random random = new System.Random();

      public static PAction chainTool_binding;

      public static ChainOverlay chainOverlay;
      public static ChainTool chainTool;
   }
}
