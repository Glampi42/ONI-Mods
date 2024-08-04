/*
 * Copyright 2024 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using ChainErrand.ChainHierarchy;
using ChainErrand.Enums;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand {
   public class ChainTool : DragTool {
      private static Color32 CREATE_VISUALIZER_COLOR = new Color32(255, 172, 52, 255);
      private static GameObject CREATE_AREA_GO;
      private static Texture2D CREATE_CURSOR;

      private static Color32 DELETE_VISUALIZER_COLOR = new Color32(231, 54, 51, 255);
      private static GameObject DELETE_AREA_GO;
      private static Texture2D DELETE_CURSOR;


      private int selectedChain;
      private int selectedLink;
      private bool insertNewLink;

      private SpriteRenderer visualizerRenderer;
      private ChainToolMode currentMode;

      private bool _isStyleDelete = false;
      private bool IsStyleDelete {
         get {
            return _isStyleDelete;
         }
         set {
            bool isSame = _isStyleDelete == value;
            _isStyleDelete = value;

            if(isSame)
               return;

            if(value)
            {
               this.visualizerRenderer.color = DELETE_VISUALIZER_COLOR;

               this.areaVisualizer = DELETE_AREA_GO;
               this.areaVisualizerSpriteRenderer = areaVisualizer.GetComponent<SpriteRenderer>();
               CREATE_AREA_GO.SetActive(false);

               this.cursor = DELETE_CURSOR;
               this.boxCursor = DELETE_CURSOR;
               SetCursor(cursor, cursorOffset, CursorMode.Auto);
            }
            else
            {
               this.visualizerRenderer.color = CREATE_VISUALIZER_COLOR;

               this.areaVisualizer = CREATE_AREA_GO;
               this.areaVisualizerSpriteRenderer = areaVisualizer.GetComponent<SpriteRenderer>();
               DELETE_AREA_GO.SetActive(false);

               this.cursor = CREATE_CURSOR;
               this.boxCursor = CREATE_CURSOR;
               SetCursor(cursor, cursorOffset, CursorMode.Auto);
            }
         }
      }

      public ChainToolMode GetToolMode() {
         return currentMode;
      }
      public void SetToolMode(ChainToolMode mode) {
         if(mode == currentMode)
            return;

         currentMode = mode;
         switch(currentMode)
         {
            case ChainToolMode.CREATE_CHAIN:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("ce_create_chain");
               IsStyleDelete = false;
               break;

            case ChainToolMode.CREATE_LINK:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("ce_create_link");
               IsStyleDelete = false;
               break;

            case ChainToolMode.DELETE_CHAIN:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("ce_delete_chain");
               IsStyleDelete = true;
               break;

            case ChainToolMode.DELETE_LINK:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("ce_delete_link");
               IsStyleDelete = true;
               break;
         }

         if(Main.chainOverlay != default)
         {
            Main.chainOverlay.UpdateAllChainNumbers();
         }
      }

      public int GetSelectedChain() {
         return selectedChain;
      }
      /// <summary>
      /// Changes the selected chain according to the targetNum argument. The selectedLink will also be updated to be the last link of the newly selected chain.
      /// </summary>
      /// <param name="targetNum">The argument that tells which chain should be selected</param>
      public void SetSelectedChain(int targetNum) {
         if(ChainsContainer.ChainsCount > 0)
         {
            if(targetNum < 0)
            {
               selectedChain = ChainsContainer.ChainsCount - 1;// for looping around
            }
            else if(targetNum >= ChainsContainer.ChainsCount)
            {
               selectedChain = 0;// for looping around
            }
            else
            {
               selectedChain = targetNum;
            }
         }
         else
         {
            selectedChain = 0;
         }

         // updating selected link:
         if(ChainsContainer.TryGetChain(selectedChain, out Chain chain))
         {
            SetSelectedLink(chain.LastLinkNumber() + 1, true);
         }

         if(Main.chainOverlay != default)
         {
            Main.chainOverlay.UpdateAllChainNumbers();
         }
      }

      public int GetSelectedLink() {
         return selectedLink;
      }
      public bool GetInsertNewLink() {
         return insertNewLink;
      }
      /// <summary>
      /// Changes the selected link according to the targetNum argument.
      /// </summary>
      /// <param name="targetNum">The argument that tells which link should be selected</param>
      /// <param name="insertNewLink">Whether the link should be inserted or extended</param>
      public void SetSelectedLink(int targetNum, bool insertNewLink) {
         if(ChainsContainer.TryGetChain(selectedChain, out Chain chain))
         {
            if(targetNum < 0)
               targetNum = int.MaxValue;// -1 == last link, for the ease of use

            selectedLink = Mathf.Clamp(targetNum, 0, chain.LastLinkNumber() + 1);
            this.insertNewLink = selectedLink > chain.LastLinkNumber() || insertNewLink;
         }
         else
         {
            selectedLink = 0;
            this.insertNewLink = false;
         }
      }



      public ChainTool() {
         selectedChain = 0;
         selectedLink = 0;
         insertNewLink = false;
      }

      public override string GetConfirmSound() {
         string sound = base.GetConfirmSound();

         if(IsStyleDelete)
            sound = "Tile_Confirm_NegativeTool";

         return sound;
      }

      public override string GetDragSound() {
         string sound = base.GetDragSound();

         if(IsStyleDelete)
            sound = "Tile_Drag_NegativeTool";

         return sound;
      }

      public override void OnActivateTool() {
         base.OnActivateTool();
         SetMode(Mode.Box);

         var menu = ChainToolMenu.Instance;
         if(!menu.HasOptions)
            menu.PopulateMenu();
         menu.ShowMenu();

         ToolMenu.Instance.PriorityScreen.Show(false);
         OverlayScreen.Instance.ToggleOverlay(ChainOverlay.ID);
      }

      public override void OnCleanUp() {
         base.OnCleanUp();
         PUtil.LogDebug(Main.debugPrefix + "Destroying ChainTool");
      }

      public override void OnDeactivateTool(InterfaceTool newTool) {
         base.OnDeactivateTool(newTool);

         ChainToolMenu.Instance.HideMenu();
         ToolMenu.Instance.PriorityScreen.Show(false);
         OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID);

         if(!ModConfig.Instance.DisableUIHelp)
         {
            SetToolMode(ChainToolMode.CREATE_CHAIN);

            if(ChainToolMenu.Instance != default)
            {
               ChainToolMenu.Instance.UpdateToolModeSelectionDisplay();
            }
         }
      }

      public override void OnPrefabInit() {
         base.OnPrefabInit();

         // store prefabs:
         CREATE_AREA_GO = Util.KInstantiate(DigTool.Instance.areaVisualizer, gameObject, "ChainToolCreateAreaVisualizer");
         CREATE_AREA_GO.SetLayerRecursively(LayerMask.NameToLayer("UI"));// same layer as DigTool's visualizer
         CREATE_AREA_GO.SetActive(false);
         DELETE_AREA_GO = Util.KInstantiate(CancelTool.Instance.areaVisualizer, gameObject, "ChainToolDeleteAreaVisualizer");
         DELETE_AREA_GO.SetLayerRecursively(LayerMask.NameToLayer("UI"));// same layer as DigTool's visualizer
         DELETE_AREA_GO.SetActive(false);

         CREATE_CURSOR = DigTool.Instance.cursor;
         DELETE_CURSOR = CancelTool.Instance.cursor;

         // hover card:
         gameObject.AddComponent<ChainToolHover>();

         // cursor:
         this.cursor = CREATE_CURSOR;
         this.boxCursor = cursor;

         // area visualizer:
         this.areaVisualizer = CREATE_AREA_GO;
         this.areaVisualizerSpriteRenderer = areaVisualizer.GetComponent<SpriteRenderer>();
         this.areaVisualizerTextPrefab = DisinfectTool.Instance.areaVisualizerTextPrefab;// == no text

         // visualizer (single cell icon):
         visualizer = new GameObject("ChainToolVisualizer");
         visualizer.SetActive(false);

         GameObject offsetObject = new GameObject("IconOffset");
         offsetObject.SetActive(true);
         visualizerRenderer = offsetObject.AddComponent<SpriteRenderer>();
         visualizerRenderer.color = CREATE_VISUALIZER_COLOR;
         visualizerRenderer.sprite = MYSPRITES.GetSprite("ce_create_chain");
         visualizerRenderer.enabled = true;

         offsetObject.transform.SetParent(visualizer.transform);
         offsetObject.transform.localPosition = new Vector3(-Grid.HalfCellSizeInMeters, 0);
         var sprite = visualizerRenderer.sprite;
         offsetObject.transform.localScale = new Vector3(
             Grid.CellSizeInMeters / (sprite.texture.width / sprite.pixelsPerUnit),
             Grid.CellSizeInMeters / (sprite.texture.height / sprite.pixelsPerUnit)
         );

         visualizer.SetLayerRecursively(LayerMask.NameToLayer("UI"));// same layer as DigTool's visualizer
      }


      public override void OnDragComplete(Vector3 cursorDown, Vector3 cursorUp) {
         base.OnDragComplete(cursorDown, cursorUp);
         Debug.Log("OnDragComplete");

         Vector2I dragMin = new Vector2I((int)Math.Floor(Math.Min(cursorDown.x, cursorUp.x)), (int)Math.Floor(Math.Min(cursorDown.y, cursorUp.y)));
         Vector2I dragMax = new Vector2I((int)Math.Ceiling(Math.Max(cursorDown.x, cursorUp.x)), (int)Math.Ceiling(Math.Max(cursorDown.y, cursorUp.y)));
         Extents extents = new Extents(dragMin.x, dragMin.y, dragMax.x - dragMin.x, dragMax.y - dragMin.y);

         Dictionary<GameObject, HashSet<Workable>> collectedErrands = new();

         var collectedGOs = Utils.CollectPrioritizableObjects(extents);

         ErrandsSearchMode searchMode;
         if(currentMode == ChainToolMode.CREATE_CHAIN || currentMode == ChainToolMode.CREATE_LINK)
         {
            searchMode = ErrandsSearchMode.ERRANDS_OUTSIDE_CHAIN;// only searching errands that can be added to a new chain
         }
         else
         {
            searchMode = ErrandsSearchMode.ERRANDS_INSIDE_CHAIN;// only searching errands that are already in a chain
         }

         foreach(var go in collectedGOs)
         {
            HashSet<Workable> errands = go.CollectFilteredErrands(out _, searchMode: searchMode);
            if(errands != null && errands.Count > 0)
            {
               collectedErrands.Add(go, errands);
            }
         }

         Debug.Log("ChainTool collectedErrands Count: " + collectedErrands.Count);
         if(collectedErrands.Count > 0)
         {
            switch(currentMode)
            {
               case ChainToolMode.CREATE_CHAIN:
                  ChainToolUtils.CreateNewChain(collectedErrands);
                  break;

               case ChainToolMode.CREATE_LINK:
                  ChainToolUtils.CreateOrExpandLink(collectedErrands);
                  break;

               case ChainToolMode.DELETE_CHAIN:
                  ChainToolUtils.DeleteChains(new HashSet<Workable>(collectedErrands.Values.SelectMany(x => x)));
                  break;

               case ChainToolMode.DELETE_LINK:
                  ChainToolUtils.DeleteErrands(new HashSet<Workable>(collectedErrands.Values.SelectMany(x => x)));
                  break;
            }

            ChainToolMenu.Instance?.UpdateNumberSelectionDisplay();
         }
      }

      public override void OnDragTool(int cell, int distFromOrigin) {
         if(Main.chainOverlay == null)
            return;

         // Invoked when the tool drags over a cell
         if(Grid.IsValidCell(cell))
         {
#if DEBUG
            // Log what we are about to do
            var xy = Grid.CellToXY(cell);
            PUtil.LogDebug("{0} at cell ({1:D},{2:D})".F(ToolMenu.Instance.toolParameterMenu.GetLastEnabledFilter(), xy.X, xy.Y));
#endif
         }
      }
   }
}