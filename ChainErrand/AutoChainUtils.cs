using ChainErrand.ChainHierarchy;
using ChainErrand.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand {
   /// <summary>
   /// This class contains methods & fields that are related with the automatic creation of chains.
   /// </summary>
   public static class AutoChainUtils {
      private static Chain automaticChain = null;

      /// <summary>
      /// Whether the errands that are being added to the automatic chain should be added to a single new link.
      /// </summary>
      public static bool bundleErrands = false;
      /// <summary>
      /// Signifies whether the first errand in the bundle was added to a new link (all following errands will be added to the same link this one was added to).
      /// </summary>
      public static bool firstBundledErrand = default;

      /// <summary>
      /// Tries to add the newly created errand to a chain. If the "auto chain" toggle is off, this action won't be performed. This method automatically handles the bundling of errands
      /// (= if errands were created simultaneously, they will be added to the same link).
      /// </summary>
      /// <param name="chainNumberBearer">The GameObject that will hold the chain number</param>
      /// <param name="errand">The newly created errand</param>
      public static void TryAddToAutomaticChain(GameObject chainNumberBearer, Workable errand) {
         if(!Main.autoChainEnabled)
            return;

         if(automaticChain == null)
         {
            automaticChain = ChainsContainer.CreateNewChain();
         }

         var errands = new Dictionary<GameObject, HashSet<Workable>>() {
               {
                  chainNumberBearer,
                  new HashSet<Workable>([errand])
               }
            };
         if(bundleErrands)
         {
            if(firstBundledErrand)
            {
               automaticChain.CreateOrExpandLink(automaticChain.LastLinkNumber() + 1, true, errands);// the first errand in the bundle should be added to a new link
               firstBundledErrand = false;
            }
            else
            {
               automaticChain.CreateOrExpandLink(automaticChain.LastLinkNumber(), false, errands);// the following errands in the bundle after the first one should be added to the same link
            }
         }
         else
         {
            automaticChain.CreateOrExpandLink(automaticChain.LastLinkNumber() + 1, true, errands);
         }

         Main.chainTool.SetSelectedChain(automaticChain.chainID);
      }

      /// <summary>
      /// Deletes the reference to the automatically created chain.
      /// </summary>
      /// <param name="removedChain">The chain that was removed. If left null, the reference will be removed in either case</param>
      public static void NullifyAutomaticChain(Chain removedChain = null) {
         if(removedChain == null || automaticChain == removedChain)
            automaticChain = null;
      }

      public static void ToggleAutoChain() {
         Main.autoChainEnabled = !Main.autoChainEnabled;

         if(AutoChain_Patches.autoChainToggle != null)
         {
            AutoChain_Patches.autoChainToggle.ChangeState(Main.autoChainEnabled ? 1 : 0);
         }

         if(!Main.autoChainEnabled)
            AutoChainUtils.NullifyAutomaticChain();

         // managing notification:
         if(Main.autoChainEnabled)
         {
            NotificationManager.Instance.AddNotification(Main.autoChainNotification);
         }
         else
         {
            NotificationManager.Instance.RemoveNotification(Main.autoChainNotification);
         }

         // adding blue vignette:
         if(!ModConfig.Instance.DisableAutoChainVignette && Vignette.Instance.image != null)
         {
            if(Main.autoChainEnabled)
            {
               if(Vignette.Instance.image.color == Vignette.Instance.defaultColor)
               {
                  Vignette.Instance.SetColor(Main.autoChainVignetteColor);
               }
            }
            else
            {
               if(Vignette.Instance.image.color == Main.autoChainVignetteColor)
               {
                  Vignette.Instance.SetColor(Vignette.Instance.defaultColor);
               }
            }
         }
      }
   }
}