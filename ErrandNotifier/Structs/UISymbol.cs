using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateClasses;
using UnityEngine;

namespace ErrandNotifier.Structs {
   public struct UISymbol {
      private LocText displayedLocText;
      private GameObject parent;
      private Workable relatedErrand;
      private NotificationType notificationType;

      private Vector3 positionOffset;
      private KAnimControllerBase kAnimControllerBase;

      public UISymbol(LocText locText, GameObject parent, Workable relatedErrand, NotificationType nType) {
         displayedLocText = locText;
         this.parent = parent;
         this.relatedErrand = relatedErrand;
         CachePositionOffset();
         kAnimControllerBase = parent.GetComponent<KAnimControllerBase>();
         UpdatePosition();
         displayedLocText.transform.SetAsLastSibling();

         RectTransform rectTransform = displayedLocText.GetComponent<RectTransform>();
         rectTransform.sizeDelta = (Vector2)rectTransform.InverseLocalScale();

         displayedLocText.gameObject.SetActive(true);

         this.notificationType = default;
         UpdateSymbol(nType);
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

      public void UpdateSymbol(NotificationType nType) {
         notificationType = nType;

         if(num > 0)
         {
            displayedLocText.text = "<b>" + number.ToString() + "</b>" +
               MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_STYLE_START + Utils.GetPostfixForLinkNumber(num - 1, false/*for technical reasons this can't be localized properly without looking weird(sad smiley)*/) + MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_STYLE_END;
         }
         else
         {
            displayedLocText.text = MYSTRINGS.UI.CHAINNUMBERS.NO_CHAIN_NUMBER;
         }

         //TODO set color
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

      public GameObject GetParent() {
         return parent;
      }
      public Workable GetRelatedErrand() {
         return relatedErrand;
      }
   }
}