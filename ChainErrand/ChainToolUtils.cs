using ChainErrand.ChainHierarchy;
using ChainErrand.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ChainErrand.Strings.MYSTRINGS.UI.TOOLS;

namespace ChainErrand {
   public static class ChainToolUtils {
      /// <summary>
      /// Checks whether a GameObject isn't filtered out by the ChainTool's filters and collects the errands related to it.
      /// </summary>
      /// <param name="errand_go">The GameObject</param>
      /// <param name="errandReference">Component attached to the GameObject that represents the errand (is used in ChainOverlay)</param>
      /// <param name="searchMode">Determines which kinds of errands should be collected</param>
      /// <returns>The collected errands.</returns>
      public static HashSet<Workable> CollectFilteredErrands(this GameObject errand_go, out KMonoBehaviour errandReference, ErrandsSearchMode searchMode = ErrandsSearchMode.ALL_ERRANDS) {
         errandReference = default;
         var errands = new HashSet<Workable>();
         int collectedErrands = 0;

         if(errand_go == null)
            return errands;

         foreach(ChainToolFilter filter in Enum.GetValues(typeof(ChainToolFilter)))
         {
            if(filter.IsOn())
            {
               switch(filter)
               {
                  case ChainToolFilter.ALL:
                     CheckConstruction();// pipes can have multiple errands(deconstruct + empty)

                     if(CheckEmptyPipe())
                        break;// buildings can't have other errands

                     if(CheckDigging())
                        break;// digging markers can't have other errands

                     if(CheckMopping())
                        break;// mopping markers can't have other errands

                     if(CheckMoveTo(ref errandReference))
                        break;// moveto markers can't have other errands
                     break;

                  case ChainToolFilter.CONSTRUCTION:
                     CheckConstruction();
                     break;

                  case ChainToolFilter.DIG:
                     CheckDigging();
                     break;

                  case ChainToolFilter.MOP:
                     CheckMopping();
                     break;

                  case ChainToolFilter.EMPTY_PIPE:
                     CheckEmptyPipe();
                     break;

                  case ChainToolFilter.MOVE_TO:
                     CheckMoveTo(ref errandReference);
                     break;

                  case ChainToolFilter.STANDARD_BUILDINGS:
                  case ChainToolFilter.LIQUID_PIPES:
                  case ChainToolFilter.GAS_PIPES:
                  case ChainToolFilter.CONVEYOR_RAILS:
                  case ChainToolFilter.WIRES:
                  case ChainToolFilter.AUTOMATION:
                  case ChainToolFilter.BACKWALLS:
                     if(errand_go.TryGetComponent(out Building building))
                     {
                        ObjectLayer objLayer = building.Def.ObjectLayer;
                        if(Utils.ObjectLayersFromChainToolFilter(filter).Contains(objLayer))
                        {
                           CheckConstruction();
                        }
                     }
                     break;
               }

               break;
            }
         }

         if(searchMode != ErrandsSearchMode.ALL_ERRANDS)
         {
            // filtering out errands that are/aren't already chained up:
            errands = new(errands.Where(errand => {
               if(errand.TryGetCorrespondingChainedErrand(out _))
               {
                  return searchMode == ErrandsSearchMode.ERRANDS_INSIDE_CHAIN;
               }
               return searchMode == ErrandsSearchMode.ERRANDS_OUTSIDE_CHAIN;
            }));
         }

         if(errandReference == default)
            errandReference = errands.FirstOrDefault();

         return errands;


         bool CheckConstruction() {
            collectedErrands = 0;

            if(errand_go.TryGetComponent(out Constructable constructable))
            {
               errands.Add(constructable);
               collectedErrands++;
            }
            else if(errand_go.TryGetComponent(out Deconstructable deconstructable) &&
               deconstructable.IsMarkedForDeconstruction())
            {
               errands.Add(deconstructable);
               collectedErrands++;
            }

            return collectedErrands > 0;
         }

         bool CheckDigging() {
            collectedErrands = 0;

            if(errand_go.TryGetComponent(out Diggable diggable))
            {
               errands.Add(diggable);
               collectedErrands++;
            }

            return collectedErrands > 0;
         }

         bool CheckMopping() {
            collectedErrands = 0;

            if(errand_go.TryGetComponent(out Moppable moppable))
            {
               errands.Add(moppable);
               collectedErrands++;
            }

            return collectedErrands > 0;
         }

         bool CheckEmptyPipe() {
            collectedErrands = 0;

            if(errand_go.TryGetComponent(out EmptyConduitWorkable emptyPipe) &&
               emptyPipe.chore != null)
            {
               errands.Add(emptyPipe);
               collectedErrands++;
            }

            return collectedErrands > 0;
         }

         bool CheckMoveTo(ref KMonoBehaviour specialErrand_inner) {
            specialErrand_inner = default;
            collectedErrands = 0;

            if(errand_go.TryGetComponent(out CancellableMove cancellableMove) &&
               cancellableMove.movingObjects.Count > 0)
            {
               foreach(var movable in cancellableMove.movingObjects)
               {
                  errands.Add(movable.Get());
               }
               specialErrand_inner = cancellableMove;
               collectedErrands += cancellableMove.movingObjects.Count;
            }

            return collectedErrands > 0;
         }
      }

      public static void CreateNewChain(Dictionary<GameObject, HashSet<Workable>> firstLinkErrands) {
         Chain chain = new Chain(ChainsContainer.Instance.ChainsCount, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 0.9f, 0.6f, 0.9f));
         ChainsContainer.Instance.StoreChain(chain);

         chain.CreateOrExpandLink(0, false, firstLinkErrands);

         Main.chainTool.SetSelectedChain(ChainsContainer.Instance.ChainsCount - 1);

         if(!ModConfig.Instance.DisableUIHelp && ChainToolMenu.Instance != default)
         {
            ChainToolMenu.Instance.modeToggles[ChainToolMode.CREATE_LINK].onClick();// switching to Create Link
         }
      }

      /// <summary>
      /// Creates a new link / expands an existing one with the specified errands. The chain ID and link number will be taken from ChainTool.
      /// </summary>
      /// <param name="linkErrands">The errands to be added to the link</param>
      public static void CreateOrExpandLink(Dictionary<GameObject, HashSet<Workable>> linkErrands) {
         if(ChainsContainer.Instance.TryGetChain(Main.chainTool.GetSelectedChain(), out Chain chain))
         {
            bool createdLastLink = Main.chainTool.GetSelectedLink() > chain.LastLinkNumber();
            chain.CreateOrExpandLink(Main.chainTool.GetSelectedLink(), Main.chainTool.GetInsertNewLink(), linkErrands);

            if(!ModConfig.Instance.DisableUIHelp && createdLastLink)
            {
               Main.chainTool.SetSelectedLink(chain.LastLinkNumber() + 1, true);
            }
         }
      }

      public static void DeleteChains(HashSet<Workable> errands) {
         Debug.Log("DeleteChains");
         HashSet<Chain> chainsToDelete = new();
         foreach(var errand in errands)
         {
            if(errand.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
            {
               chainsToDelete.Add(chainedErrand.parentLink?.parentChain);
            }
         }

         foreach(var chain in chainsToDelete)
         {
            Debug.Log("Removing chain " + chain.chainID);
            ChainsContainer.Instance.RemoveChain(chain);
         }
      }

      public static void DeleteErrands(HashSet<Workable> errands) {
         Debug.Log("DeleteErrands");
         foreach(var errand in errands)
         {
            if(errand.TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
            {
               chainedErrand.Remove(true);
            }
         }
      }

      /// <summary>
      /// Tries to retrieve the chain number from the input text.
      /// </summary>
      /// <param name="text">The text</param>
      /// <returns>The chain number.</returns>
      public static int InterpretChainNumber(string text) {
         int result;
         if(!int.TryParse(text, out result))
         {
            result = 0;
         }

         return result;
      }

      /// <summary>
      /// Tries to retrieve the link number from the input text.
      /// </summary>
      /// <param name="text">The text</param>
      /// <returns>Tuple containing the link number and whether the link should be created or extended</returns>
      public static (int linkNumber, bool insertNewLink) InterpretLinkNumber(string text) {
         float inputNum;
         if(!float.TryParse(text, out inputNum)) {
            inputNum = 1f;
         }

         bool insertNewLink = inputNum != (int)inputNum;// if the number has digits after dot

         if(!insertNewLink)
            inputNum--;// 1st link -> 0th link

         return ((int)inputNum, insertNewLink);
      }
   }
}
