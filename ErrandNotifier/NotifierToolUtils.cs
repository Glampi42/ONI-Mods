using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ErrandNotifier.Enums;

namespace ErrandNotifier {
   public static class NotifierToolUtils {
      /// <summary>
      /// Checks whether a GameObject isn't filtered out by the NotifierTool's filters and collects the errands related to it.
      /// </summary>
      /// <param name="errand_go">The GameObject</param>
      /// <param name="errandReference">Component attached to the GameObject that represents the errand (is used in NotifierOverlay)</param>
      /// <param name="searchMode">Determines which kinds of errands should be collected</param>
      /// <returns>The collected errands.</returns>
      public static HashSet<Workable> CollectFilteredErrands(this GameObject errand_go, out KMonoBehaviour errandReference, ErrandsSearchMode searchMode = ErrandsSearchMode.ALL_ERRANDS) {
         errandReference = default;
         var errands = new HashSet<Workable>();

         if(errand_go == null)
            return errands;

         //foreach(NotifierToolFilter filter in Enum.GetValues(typeof(NotifierToolFilter)))
         //{
         //   if(filter.IsOn())
         //   {
         //      switch(filter)
         //      {
         //         case NotifierToolFilter.ALL:
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Constructable)).CollectErrands(errand_go, errands, ref errandReference);
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Deconstructable)).CollectErrands(errand_go, errands, ref errandReference);
         //            // pipes can have multiple errands(deconstruct + empty)

         //            if(ChainedErrandPackRegistry.GetChainedErrandPack(typeof(EmptyConduitWorkable)).CollectErrands(errand_go, errands, ref errandReference))
         //               break;// buildings can't have other errands

         //            if(ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Diggable)).CollectErrands(errand_go, errands, ref errandReference))
         //               break;// digging markers can't have other errands

         //            if(ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Moppable)).CollectErrands(errand_go, errands, ref errandReference))
         //               break;// mopping markers can't have other errands

         //            if(ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Movable)).CollectErrands(errand_go, errands, ref errandReference))
         //               break;// moveto markers can't have other errands
         //            break;

         //         case NotifierToolFilter.CONSTRUCTION:
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Constructable)).CollectErrands(errand_go, errands, ref errandReference);
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Deconstructable)).CollectErrands(errand_go, errands, ref errandReference);
         //            break;

         //         case NotifierToolFilter.DIG:
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Diggable)).CollectErrands(errand_go, errands, ref errandReference);
         //            break;

         //         case NotifierToolFilter.MOP:
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Moppable)).CollectErrands(errand_go, errands, ref errandReference);
         //            break;

         //         case NotifierToolFilter.EMPTY_PIPE:
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(EmptyConduitWorkable)).CollectErrands(errand_go, errands, ref errandReference);
         //            break;

         //         case NotifierToolFilter.MOVE_TO:
         //            ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Movable)).CollectErrands(errand_go, errands, ref errandReference);
         //            break;

         //         case NotifierToolFilter.STANDARD_BUILDINGS:
         //         case NotifierToolFilter.LIQUID_PIPES:
         //         case NotifierToolFilter.GAS_PIPES:
         //         case NotifierToolFilter.CONVEYOR_RAILS:
         //         case NotifierToolFilter.WIRES:
         //         case NotifierToolFilter.AUTOMATION:
         //         case NotifierToolFilter.BACKWALLS:
         //            if(errand_go.TryGetComponent(out Building building))
         //            {
         //               ObjectLayer objLayer = building.Def.ObjectLayer;
         //               if(Utils.ObjectLayersFromNotifierToolFilter(filter).Contains(objLayer))
         //               {
         //                  ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Constructable)).CollectErrands(errand_go, errands, ref errandReference);
         //                  ChainedErrandPackRegistry.GetChainedErrandPack(typeof(Deconstructable)).CollectErrands(errand_go, errands, ref errandReference);
         //               }
         //            }
         //            break;
         //      }

         //      break;
         //   }
         //}

         if(searchMode != ErrandsSearchMode.ALL_ERRANDS)
         {
            // filtering out errands that are/aren't already chained up:
            errands = new(errands.Where(errand => {
               if(errand.TryGetCorrespondingNotifiableErrand(out _))
               {
                  return searchMode == ErrandsSearchMode.ERRANDS_INSIDE_NOTIFICATION;
               }
               return searchMode == ErrandsSearchMode.ERRANDS_OUTSIDE_NOTIFICATION;
            }));
         }

         if(errandReference == default)
            errandReference = errands.FirstOrDefault();

         return errands;
      }

      /// <summary>
      /// Tries to retrieve the notification ID from the input text.
      /// </summary>
      /// <param name="text">The text</param>
      /// <returns>The notification ID.</returns>
      public static int InterpretNotificationID(string text) {
         int result;
         if(!int.TryParse(text, out result))
         {
            result = 0;
         }

         return result;
      }
   }
}
