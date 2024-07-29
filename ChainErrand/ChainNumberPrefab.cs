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
      public const float boundsHeightDelta = 0.1f;
      public static readonly Vector2f boundsYOffset = new Vector2f(0.0f, -0.04f);

      public static LocText GetChainNumberPrefab() {
         LocText chainNumberPrefab = UnityEngine.Object.Instantiate(OverlayScreen.Instance.powerLabelPrefab);
         chainNumberPrefab.name = "ChainNumber";
         chainNumberPrefab.raycastTarget = false;
         //UnityEngine.Object.Destroy(chainNumberPrefab.GetComponent<ContentSizeFitter>());
         UnityEngine.Object.Destroy(chainNumberPrefab.GetComponent<ToolTip>());
         UnityEngine.Object.Destroy(chainNumberPrefab.transform.GetChildSafe(0)?.gameObject);
         chainNumberPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
         chainNumberPrefab.font = Localization.GetFont("GRAYSTROKE OUTLINE SDF");
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
   }
}
