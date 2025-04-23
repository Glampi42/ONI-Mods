using ErrandNotifier.Enums;
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
      private GNotificationType notificationType;

      private Vector3 positionOffset;
      private KAnimControllerBase kAnimControllerBase;

      public UISymbol(LocText locText, GameObject parent, Workable relatedErrand, GNotificationType nType) {
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

      public void UpdateSymbol(GNotificationType nType) {
         notificationType = nType;

         string symbol = "";
         Color color = Color.white;
         float size = 0f;
         switch(nType)
         {
            case GNotificationType.NONE:
               symbol = "*";
               color = PUITuning.Colors.ButtonPinkStyle.activeColor;
               size = 21f;
               break;

            case GNotificationType.POP:
               symbol = "•";
               color = Color.blue;
               size = 21f;
               break;

            case GNotificationType.BOING_BOING:
               symbol = "!";
               color = Color.red;
               size = 21f;
               break;

            case GNotificationType.AHH:
               symbol = "!!";
               color = new Color(0.7f, 0f, 0f, 1f);
               size = 21f;
               break;
         }

         displayedLocText.text = symbol;
         displayedLocText.color = color;
         displayedLocText.fontSize = size;
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