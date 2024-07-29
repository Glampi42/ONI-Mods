using ChainErrand.Strings;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateClasses;
using UnityEngine;

namespace ChainErrand.Structs {
   public struct ChainNumber {
      private LocText displayedLocText;
      private GameObject parent;
      private Workable relatedErrand;
      private int number;

      private Vector3 positionOffset;
      private KAnimControllerBase kAnimControllerBase;

      public ChainNumber(LocText locText, GameObject parent, Workable relatedErrand, Color color, int number) {
         displayedLocText = locText;
         this.parent = parent;
         this.relatedErrand = relatedErrand;
         CachePositionOffset();
         UpdatePosition();
         displayedLocText.transform.SetAsLastSibling();

         kAnimControllerBase = parent.GetComponent<KAnimControllerBase>();

         displayedLocText.color = color;

         RectTransform rectTransform = displayedLocText.GetComponent<RectTransform>();
         rectTransform.sizeDelta = (Vector2)rectTransform.InverseLocalScale();

         displayedLocText.gameObject.SetActive(true);

         this.number = default;
         UpdateNumber(number);
      }
      private void CachePositionOffset() {
         if(parent.TryGetComponent(out Prioritizable prioritizable))
         {
            positionOffset = Vector3.zero;
            positionOffset.x += prioritizable.iconOffset.x;
            positionOffset.y += prioritizable.iconOffset.y;
         }
         else
         {
            positionOffset = Vector3.zero;
         }
      }

      public void UpdateNumber(int num) {
         number = num;

         if(num > 0)
         {
            displayedLocText.text = "<b>" + number.ToString() + "</b>" +
               MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_STYLE_START + Utils.GetPostfixForLinkNumber(num - 1) + MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_STYLE_END;
         }
         else
         {
            displayedLocText.text = MYSTRINGS.UI.CHAINNUMBERS.NO_CHAIN_NUMBER;
         }

         UpdateNumberSize();
      }
      private void UpdateNumberSize() {
         float fontSize = Utils.GetFontSizeFromLinkNumber(number - 1);

         displayedLocText.fontSize = fontSize;
      }

      public void UpdateColor(Color color) {
         displayedLocText.color = color;
      }

      public void UpdatePosition() {
         if(parent.IsNullOrDestroyed())
            return;

         displayedLocText.transform.position = kAnimControllerBase == null ? parent.transform.position : kAnimControllerBase.GetWorldPivot();
         displayedLocText.transform.position += positionOffset;
      }

      public void UpdateVisibility(bool visible) {
         displayedLocText.gameObject.SetActive(visible);
      }

      public LocText GetLocText() {
         return displayedLocText;
      }

      public Workable GetRelatedErrand() {
         return relatedErrand;
      }
   }
}