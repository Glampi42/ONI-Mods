using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighlightOverlay.Components {
   public class TintManagerCmp : KMonoBehaviour {
      private Color actualTintColor;// stores whatever tint color is set to the object externally
      private Color shownTintColor = default;

      private bool ignoreChange = true;
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
      }

      private void ManageTintChange(Color newTint) {
         if(ignoreChange)
            return;

         actualTintColor = newTint;

         ignoreChange = true;
         animController.TintColour = shownTintColor;
         ignoreChange = false;
      }

      public void SetTintColor(Color color) {
         ignoreChange = true;

         shownTintColor = color;
         animController.TintColour = color;

         ignoreChange = false;// not sure if this is a good practice, but it works so yeah
      }

      public void ResetTintColor() {
         ignoreChange = true;
         animController.TintColour = actualTintColor;
         ignoreChange = false;

         shownTintColor = default;
         ignoreChange = true;
      }
   }
}
