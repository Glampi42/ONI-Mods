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
      public static TMP_FontAsset graystroke_outline;// font that supports big outlines because of big padding between the glyphs
      public static TMP_FontAsset graystroke_outline_italic;

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

         chainNumberPrefab.font = graystroke_outline;// I had to import custom font because the default in-game one looks bad when applying big outlines
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
