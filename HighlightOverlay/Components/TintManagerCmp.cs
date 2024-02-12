using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Components {
   public class TintManagerCmp : KMonoBehaviour {
      public Color actualTintColor;// stores whatever tint color is set to the object externally
      public Color shownTintColor = default;

      private bool preventRecursion = false;
      public KBatchedAnimController animController;

      public override void OnSpawn() {
         base.OnSpawn();

         animController = this.GetComponent<KBatchedAnimController>();

         if(animController == null)
            throw new Exception(Main.debugPrefix + "Tried adding TintManager to a GameObject without a KBatchedAnimController");

         actualTintColor = animController.TintColour;

         if(animController.OnTintChanged != null)
            animController.OnTintChanged += ManageTintChange;
         else
            animController.OnTintChanged = ManageTintChange;

         enabled = false;
      }

      private void ManageTintChange(Color newTint) {
         if(!enabled)
         {
            actualTintColor = newTint;
         }
         else
         {
            if(preventRecursion)
               return;// not sure if this is a good practice, but it works so yeah

            actualTintColor = newTint;

            preventRecursion = true;
            animController.TintColour = shownTintColor;
            preventRecursion = false;
         }
      }

      public void SetTintColor(Color color) {
         enabled = true;

         shownTintColor = color;

         preventRecursion = true;
         animController.TintColour = shownTintColor;
         preventRecursion = false;
      }

      public void ResetTintColor() {
         preventRecursion = true;
         animController.TintColour = actualTintColor;
         preventRecursion = false;

         shownTintColor = default;
         enabled = false;
      }
   }
}
