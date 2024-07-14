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
      private int number;

      public ChainNumber(LocText locText, GameObject parent, Color color, int number) {
         displayedLocText = locText;
         displayedLocText.transform.position = parent.transform.position;
         displayedLocText.transform.SetAsLastSibling();

         displayedLocText.color = color;

         RectTransform rectTransform = displayedLocText.GetComponent<RectTransform>();
         rectTransform.sizeDelta = (Vector2)rectTransform.InverseLocalScale();

         displayedLocText.gameObject.SetActive(true);

         this.number = default;
         UpdateNumber(number);
      }

      public void UpdateNumber(int num) {
         number = num;

         string numText = number.ToString();

         string postfix;
         switch(num)
         {
            case 1:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_1;
               break;
            case 2:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_2;
               break;
            case 3:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_3;
               break;
            case 4:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_4;
               break;
            case 5:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_5;
               break;
            case 6:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_6;
               break;
            case 7:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_7;
               break;
            case 8:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_8;
               break;
            case 9:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_9;
               break;
            case 10:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_10;
               break;
            default:
               postfix = MYSTRINGS.UI.CHAINNUMBERS.POSTFIX_DEFAULT;
               break;
         }
         displayedLocText.text = "<b>" + numText + "</b>" + postfix;

         UpdateSize();
      }

      private void UpdateSize() {
         float fontSize = Utils.GetFontSizeFromLinkNumber(number);

         displayedLocText.fontSize = fontSize;
      }
   }
}