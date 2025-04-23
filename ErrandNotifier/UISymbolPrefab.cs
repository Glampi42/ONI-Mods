using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace ErrandNotifier {
   public static class UISymbolPrefab {
      private static LocText uiSymbolPrefab = null;

      public static LocText GetUISymbolPrefab() {
         if(uiSymbolPrefab != null)
            return uiSymbolPrefab;

         uiSymbolPrefab = UnityEngine.Object.Instantiate(OverlayScreen.Instance.powerLabelPrefab);
         uiSymbolPrefab.name = "Notification_UISymbol";
         uiSymbolPrefab.raycastTarget = false;
         //UnityEngine.Object.Destroy(uiSymbolPrefab.GetComponent<ContentSizeFitter>());
         UnityEngine.Object.Destroy(uiSymbolPrefab.GetComponent<ToolTip>());
         UnityEngine.Object.Destroy(uiSymbolPrefab.transform.GetChildSafe(0)?.gameObject);
         uiSymbolPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

         var font = Localization.GetFont("GRAYSTROKE OUTLINE SDF");
         if(font == null)//TODO import own font
         {
            font = Localization.GetFont("GRAYSTROKE REGULAR SDF");// some localizations don't have the outline font for some reason (but it looks better than the regular because it doesn't have artifacts with big outlines)
            Main.outlineWidthMultiplier = 0.0191f;// this font has a different scale for the outline
         }

         uiSymbolPrefab.font = font;
         uiSymbolPrefab.alignment = TextAlignmentOptions.Center;
         uiSymbolPrefab.fontSize = 28f;
         uiSymbolPrefab.outlineColor = Color.white;
         uiSymbolPrefab.outlineWidth = Main.outlineWidthMultiplier * uiSymbolPrefab.fontSize;
         uiSymbolPrefab.characterSpacing = -1f;
         uiSymbolPrefab.lineSpacing = -10f;
         uiSymbolPrefab.enableKerning = true;
         uiSymbolPrefab.enableWordWrapping = true;
         uiSymbolPrefab.overflowMode = TextOverflowModes.Overflow;
         uiSymbolPrefab.UpdateMeshPadding();

         return uiSymbolPrefab;
      }

      public static void DestroyPrefab() {
         uiSymbolPrefab = null;
      }
   }
}
