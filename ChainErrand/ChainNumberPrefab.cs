using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace ChainErrand {
   public static class ChainNumberPrefab {
      private static LocText chainNumberPrefab = null;

      public static LocText GetChainNumberPrefab() {
         if(chainNumberPrefab != null)
            return chainNumberPrefab;

         chainNumberPrefab = UnityEngine.Object.Instantiate(OverlayScreen.Instance.powerLabelPrefab);
         chainNumberPrefab.name = "ChainNumber";
         chainNumberPrefab.raycastTarget = false;
         //UnityEngine.Object.Destroy(chainNumberPrefab.GetComponent<ContentSizeFitter>());
         UnityEngine.Object.Destroy(chainNumberPrefab.GetComponent<ToolTip>());
         UnityEngine.Object.Destroy(chainNumberPrefab.transform.GetChildSafe(0)?.gameObject);
         chainNumberPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

         var font = Localization.GetFont("GRAYSTROKE OUTLINE SDF");
         if(font == null)
         {
            font = Localization.GetFont("GRAYSTROKE REGULAR SDF");// some localizations don't have the outline font for some reason (but it looks better than the regular because it doesn't have artifacts with big outlines)
            Main.outlineWidthMultiplier = 0.0191f;// this font has other scale for the outline
         }

         chainNumberPrefab.font = font;
         chainNumberPrefab.alignment = TextAlignmentOptions.Center;
         chainNumberPrefab.fontSize = Main.maxChainNumberFontSize;
         chainNumberPrefab.outlineColor = Color.white;
         chainNumberPrefab.outlineWidth = Main.outlineWidthMultiplier * chainNumberPrefab.fontSize;
         chainNumberPrefab.characterSpacing = -1f;
         chainNumberPrefab.lineSpacing = -10f;
         chainNumberPrefab.enableKerning = true;
         chainNumberPrefab.enableWordWrapping = true;
         chainNumberPrefab.overflowMode = TextOverflowModes.Overflow;
         chainNumberPrefab.UpdateMeshPadding();

         return chainNumberPrefab;
      }

      public static void DestroyPrefab() {
         chainNumberPrefab = null;
      }
   }
}
