using ErrandNotifier.Custom;
using ErrandNotifier.Enums;
using ErrandNotifier.Structs;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ErrandNotifier {
   public class NotifierOverlay : OverlayModes.Mode {
      public static readonly HashedString ID = "glampi_NotifierOverlay";

      public override HashedString ViewMode() => ID;

      public override string GetSoundName() => nameof(OverlayModes.Priorities);

      private bool isEnabled = false;
      public bool IsEnabled => isEnabled;

      public readonly int targetUILayer;
      public readonly int targetLayer;
      private readonly int layerMask;

      private UIPool<LocText> uiSymbolsPool;
      private HashSet<KMonoBehaviour> visibleErrands = new();
      private HashSet<GameObject> notVisibleErrands = new();
      public UISymbolsCollection uiSymbols = new();

      // default layers:
      private int digDefaultLayer = -1;
      private float digDefaultZ = -1f;
      private int mopDefaultLayer = -1;
      private float mopDefaultZ = -1f;
      private int moveToDefaultLayer = -1;
      private float moveToDefaultZ = -1f;

      private bool ghostTilesOnDefaultLayer = true;


      public NotifierOverlay() {
         targetUILayer = LayerMask.NameToLayer("MaskedOverlay");
         targetLayer = LayerMask.NameToLayer("MaskedOverlayBG");
         layerMask = LayerMask.GetMask("MaskedOverlay", "MaskedOverlayBG");
      }

      public override void Enable() {
         base.Enable();

         Camera.main.cullingMask |= layerMask;
         SelectTool.Instance.SetLayerMask(SelectTool.Instance.GetDefaultLayerMask() | layerMask);

         UpdateOverlay();

         uiSymbolsPool = new UIPool<LocText>(UISymbolPrefab.GetUISymbolPrefab());

         isEnabled = true;
      }

      public override void Disable() {
         base.Disable();

         Camera.main.cullingMask &= ~layerMask;
         SelectTool.Instance.ClearLayerMask();

         UpdateOverlay(true);

         uiSymbols.Clear();
         uiSymbolsPool.ClearAll();

         isEnabled = false;
      }

      public override void Update() {
         Grid.GetVisibleExtents(out Vector2I vis_min, out Vector2I vis_max);
         Utils.ClampToActiveWorldBounds(ref vis_min);
         Utils.ClampToActiveWorldBounds(ref vis_max);

         // cleanup:
         visibleErrands.Remove(null);// HashSets can contain one null item at most
         notVisibleErrands.Remove(null);
         visibleErrands.RemoveWhere(errand => errand.isNull);
         Dictionary<GameObject, HashSet<UISymbol>> uiSymbolsToRemove = new();
         foreach(var pair in uiSymbols.GetAllUISymbols())
         {
            foreach(var uiSymbol in pair.Value)
            {
               if(uiSymbol.GetParent().IsNullOrDestroyed() || uiSymbol.GetRelatedErrand().isNull)
               {
                  if(!uiSymbolsToRemove.ContainsKey(pair.Key))
                     uiSymbolsToRemove.Add(pair.Key, new HashSet<UISymbol>(1));

                  uiSymbolsToRemove[pair.Key].Add(uiSymbol);
               }
            }
         }
         foreach(var pair in uiSymbolsToRemove)
         {
            foreach(var uiSymbol in pair.Value)
            {
               RemoveUISymbol(pair.Key, uiSymbol);
            }
         }
         OverlayModes.Mode.ClearOutsideViewObjects(this.visibleErrands, vis_min, vis_max, null, errand => {// don't use RemoveOffscreenTargets()
            if(errand != null)
            {
               ResetErrandGODisplay(errand, false);
               RemoveAttachedUISymbols(errand.gameObject);
            }
         });
         // no need to clear notVisibleErrands

         Extents gatherExtents = new Extents(vis_min.x, vis_min.y, vis_max.x - vis_min.x, vis_max.y - vis_min.y);

         HashSet<GameObject> visibleErrands_GOs = new(visibleErrands.Select(errand => errand.isNull ? null : errand.gameObject));
         foreach(GameObject obj in Utils.CollectPrioritizableObjects(gatherExtents))
         {
            if(visibleErrands_GOs.Contains(obj) || notVisibleErrands.Contains(obj))
               continue;

            var errands = obj.CollectFilteredErrands(out KMonoBehaviour errandReference);
            if(errands != null && errands.Count > 0)
            {
               AddToVisible(errandReference, errands);
            }
            else
            {
               notVisibleErrands.Add(obj);
            }
         }

         //updating UISymbols' position (in case their parent GOs moved):
         //foreach(var uiSymbol in uiSymbols.GetAllUISymbolsFlattened())
         //{
         //   uiSymbol.UpdatePosition();
         //}
      }
      private void AddToVisible(KMonoBehaviour errandRef, HashSet<Workable> collectedErrands) {
         if(errandRef.IsNullOrDestroyed())
            return;

         visibleErrands.Add(errandRef);

         // moving objects to the masked layer for them to appear in normal color:
         if(errandRef.TryGetComponent(out KBatchedAnimController animController))
         {
            // normal buildings:
            if(!animController.IsNullOrDestroyed())
            {
               animController.SetLayer(targetLayer);
            }
         }
         else if(Utils.IsTile(errandRef.gameObject, out SimCellOccupier cellOccupier))
         {
            // tiles:
            UpdateTile(false, cellOccupier);
            // ghost tiles are managed separately
         }
         else if(errandRef is Diggable)
         {
            // dig errands:
            GameObject digIcon = ((Diggable)errandRef).childRenderer.gameObject;

            if(digDefaultLayer == -1)
               digDefaultLayer = digIcon.layer;
            if(digDefaultZ == -1)
               digDefaultZ = digIcon.transform.position.z;

            digIcon.layer = targetUILayer;
            digIcon.transform.position = new Vector3(digIcon.transform.position.x, digIcon.transform.position.y, Grid.GetLayerZ(Grid.SceneLayer.FXFront));
         }
         else if(errandRef is Moppable)
         {
            // mop errands:
            GameObject mopIcon = ((Moppable)errandRef).childRenderer.gameObject;

            if(mopDefaultLayer == -1)
               mopDefaultLayer = mopIcon.layer;
            if(mopDefaultZ == -1)
               mopDefaultZ = mopIcon.transform.position.z;

            mopIcon.layer = targetUILayer;
            mopIcon.transform.position = new Vector3(mopIcon.transform.position.x, mopIcon.transform.position.y, Grid.GetLayerZ(Grid.SceneLayer.FXFront));
         }
         else if(errandRef is CancellableMove)
         {
            // moveTo errands:
            GameObject moveToIcon = ((CancellableMove)errandRef).GetComponentInChildren<EasingAnimations>().gameObject;

            if(moveToDefaultLayer == -1)
               moveToDefaultLayer = moveToIcon.layer;
            if(moveToDefaultZ == -1)
               moveToDefaultZ = moveToIcon.transform.position.z;

            moveToIcon.layer = targetUILayer;
            moveToIcon.transform.position = new Vector3(moveToIcon.transform.position.x, moveToIcon.transform.position.y, Grid.GetLayerZ(Grid.SceneLayer.FXFront));
         }

         // creating the chain number(s):
         foreach(var errand in collectedErrands)
         {
            CreateUISymbol(errandRef.gameObject, errand);
         }
      }

      private void RemoveFromVisible(KMonoBehaviour errand, bool update) {
         if(visibleErrands.Contains(errand))
         {
            ResetErrandGODisplay(errand, update);
            RemoveAttachedUISymbols(errand.gameObject);
            visibleErrands.Remove(errand);
         }
      }
      private void RemoveAllFromVisible(bool update) {
         foreach(var errand in visibleErrands)
         {
            ResetErrandGODisplay(errand, update);
            RemoveAttachedUISymbols(errand.gameObject);
         }
         visibleErrands.Clear();
      }
      /// <summary>
      /// Resets the visual parameters of the GameObject of the errand.
      /// </summary>
      /// <param name="errand">The errand</param>
      /// <param name="update">If true, it means the reset is done for updating the display, not for disabling it (is used to prevent flicker on tiles)</param>
      private void ResetErrandGODisplay(KMonoBehaviour errand, bool update) {
         if(errand.TryGetComponent(out KBatchedAnimController animController))
         {
            // buildings, debris:
            ResetDisplayValues(animController);
         }
         else if(Utils.IsTile(errand.gameObject, out SimCellOccupier cellOccupier))
         {
            // tiles:
            if(!update)
               UpdateTile(true, cellOccupier);
            // ghost tiles are managed separately
         }
         else if(errand is Diggable)
         {
            // dig errands:
            GameObject digIcon = ((Diggable)errand).childRenderer.gameObject;

            digIcon.layer = digDefaultLayer;
            digIcon.transform.position = new Vector3(digIcon.transform.position.x, digIcon.transform.position.y, digDefaultZ);
         }
         else if(errand is Moppable)
         {
            // mop errands:
            GameObject mopIcon = ((Moppable)errand).childRenderer.gameObject;

            mopIcon.layer = mopDefaultLayer;
            mopIcon.transform.position = new Vector3(mopIcon.transform.position.x, mopIcon.transform.position.y, mopDefaultZ);
         }
         else if(errand is CancellableMove)
         {
            // moveTo errands:
            GameObject moveToIcon = ((CancellableMove)errand).GetComponentInChildren<EasingAnimations>().gameObject;

            moveToIcon.layer = moveToDefaultLayer;
            moveToIcon.transform.position = new Vector3(moveToIcon.transform.position.x, moveToIcon.transform.position.y, moveToDefaultZ);
         }
      }

      /// <summary>
      /// Updates the display of all tiles (both normal and ghost tiles).
      /// </summary>
      /// <param name="disablingOverlay">Should be true if the update happens when the overlay is getting disabled</param>
      private void UpdateTiles(bool disablingOverlay = false) {
         if(!disablingOverlay && (NotifierToolFilter.ALL.IsOn() || NotifierToolFilter.CONSTRUCTION.IsOn() || NotifierToolFilter.STANDARD_BUILDINGS.IsOn()))
         {
            MoveGhostTilesToMaskedLayer();
         }
         else
         {
            ResetGhostTiles();
            OverlayTileRenderer.FreeResources();// updating normal tiles
         }
      }
      private void MoveGhostTilesToMaskedLayer() {
         if(ghostTilesOnDefaultLayer)
         {
            var blockTileRenderer = World.Instance.blockTileRenderer;
            foreach(var renderInfoEntry in blockTileRenderer.renderInfo)
            {
               if(renderInfoEntry.Key.Value == Rendering.BlockTileRenderer.RenderInfoLayer.UnderConstruction ||
                  renderInfoEntry.Key.Value == Rendering.BlockTileRenderer.RenderInfoLayer.Replacement)
               {
                  renderInfoEntry.Value.renderLayer = targetLayer;
               }
            }

            ghostTilesOnDefaultLayer = false;
         }
      }
      private void ResetGhostTiles() {
         if(!ghostTilesOnDefaultLayer)
         {
            var blockTileRenderer = World.Instance.blockTileRenderer;
            foreach(var renderInfoEntry in blockTileRenderer.renderInfo)
            {
               if(renderInfoEntry.Key.Value == Rendering.BlockTileRenderer.RenderInfoLayer.UnderConstruction ||
                  renderInfoEntry.Key.Value == Rendering.BlockTileRenderer.RenderInfoLayer.Replacement)
               {
                  renderInfoEntry.Value.renderLayer = LayerMask.NameToLayer("Construction");// default layer
               }
            }

            ghostTilesOnDefaultLayer = true;
         }
      }
      //--------------------------Chain Numbers - UTILS--------------------------DOWN
      public void CreateUISymbol(GameObject parentGO, Workable relatedErrand, NotificationType nType) {
         if(parentGO == null || relatedErrand == null)
            return;

         UISymbol uiSymbol = new UISymbol(uiSymbolsPool.GetFreeElement(GameScreenManager.Instance.worldSpaceCanvas), parentGO, relatedErrand, nType);
         uiSymbols.Add(parentGO, uiSymbol);

         if(relatedErrand.TryGetCorrespondingNotifiableErrand(out ChainedErrand chainedErrand))
         {
            UpdateUISymbol(uiSymbol, chainedErrand.parentLink);
         }
         else
         {
            UpdateUISymbol(uiSymbol, null);
         }
      }

      public void UpdateAllUISymbols() {
         foreach(var chainNumber in uiSymbols.GetAllChainNumbersFlattened())
         {
            if(chainNumber.GetRelatedErrand().TryGetCorrespondingChainedErrand(out ChainedErrand chainedErrand))
            {
               UpdateChainNumber(chainNumber, chainedErrand.parentLink);
            }
            else
            {
               UpdateChainNumber(chainNumber, null);
            }
         }
      }

      public void UpdateUISymbol(GameObject parentGO, Workable relatedErrand, Link link) {
         if(parentGO.IsNullOrDestroyed() || relatedErrand.IsNullOrDestroyed())
            return;

         if(uiSymbols.TryGetChainNumber(parentGO, relatedErrand, out ChainNumber chainNumber))
         {
            UpdateUISymbol(chainNumber, link);
         }
      }
      public void UpdateUISymbol(ChainNumber chainNumber, Link link) {
         chainNumber.UpdateColor(link == null ? Main.DefaultChainNumberColor : link.parentChain.chainColor);
         chainNumber.UpdateNumber(link == null ? 0 : link.linkNumber + 1/*0th link -> 1st link*/);

         bool shouldBeVisible;
         if(link == null)// if not in a chain
         {
            shouldBeVisible = Main.chainTool.GetToolMode() == Enums.ChainToolMode.CREATE_CHAIN || Main.chainTool.GetToolMode() == Enums.ChainToolMode.CREATE_LINK;
         }
         else// if in a chain
         {
            if(Main.chainTool.GetToolMode() == Enums.ChainToolMode.CREATE_LINK)
            {
               shouldBeVisible = (link.parentChain?.chainID ?? -1) == Main.chainTool.GetSelectedChain();
            }
            else// any other mode
            {
               shouldBeVisible = true;
            }
         }
         chainNumber.UpdateVisibility(shouldBeVisible);
      }

      public void RemoveAttachedUISymbols(GameObject parentGO) {
         foreach(var chainNum in uiSymbols.GetAttachedChainNumbers(parentGO))
         {
            uiSymbolsPool.ClearElement(chainNum.GetLocText());
         }
         uiSymbols.RemoveAttached(parentGO);
      }

      public void RemoveUISymbol(GameObject parentGO, Workable relatedErrand) {
         if(uiSymbols.TryGetChainNumber(parentGO, relatedErrand, out UISymbol uiSymbol))
         {
            RemoveUISumbol(parentGO, uiSymbol);
         }
      }
      public void RemoveUISymbol(GameObject parentGO, UISymbol uiSymbol) {
         uiSymbolsPool.ClearElement(uiSymbol.GetLocText());
         uiSymbols.Remove(parentGO, uiSymbol);
      }
      //--------------------------Chain Numbers - UTILS--------------------------UP

      /// <summary>
      /// Clears stored data and forces the overlay to get updated.
      /// </summary>
      /// <param name="disablingOverlay">Should be true if the update happens when the overlay is getting disabled</param>
      public void UpdateOverlay(bool disablingOverlay = false) {
         RemoveAllFromVisible(!disablingOverlay);
         notVisibleErrands.Clear();

         UpdateTiles(disablingOverlay);
      }

      /// <summary>
      /// Updates a specific errand's display.
      /// </summary>
      /// <param name="errandReference">The component that represents the errand that should be updated</param>
      public void UpdateErrand(KMonoBehaviour errandReference) {
         if(errandReference == null)
            return;

         RemoveFromVisible(errandReference, true);
         notVisibleErrands.Remove(errandReference.gameObject);
      }

      /// <summary>
      /// Updates the specified tile's display.
      /// </summary>
      /// <param name="remove">If true, the tile's display will be disabled. Otherwise, the display will be added</param>
      /// <param name="cellOccupier">The tile's SimCellOccupier</param>
      public void UpdateTile(bool remove, SimCellOccupier cellOccupier) {
         foreach(int cell in cellOccupier.building.PlacementCells)
         {
            if(remove)
            {
               OverlayTileRenderer.UnrenderTile(cell, cellOccupier.building.Def, false, cellOccupier.building.GetVisualizationElementID(cellOccupier.primaryElement));
            }
            else
            {
               OverlayTileRenderer.RenderTile(cell, cellOccupier.building.Def, false, cellOccupier.building.GetVisualizationElementID(cellOccupier.primaryElement));
            }
         }
      }
   }
}