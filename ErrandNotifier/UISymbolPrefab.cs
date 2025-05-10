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

         var font = Localization.GetFont("GRAYSTROKE REGULAR SDF");

         uiSymbolPrefab.font = font;
         uiSymbolPrefab.alignment = TextAlignmentOptions.Center;
         uiSymbolPrefab.fontSize = 28f;// font size gets overriden in UISymbol struct
         uiSymbolPrefab.outlineColor = Color.white;
         uiSymbolPrefab.outlineWidth = 0.4f;
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
