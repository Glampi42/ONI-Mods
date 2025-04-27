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

using ErrandNotifier.Enums;
using ErrandNotifier.NotificationsHierarchy;
using ErrandNotifier.Strings;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier {
   public class NotifierTool : DragTool {
      private static Color32 CREATE_VISUALIZER_COLOR = new Color32(255, 172, 52, 255);
      private static GameObject CREATE_AREA_GO;
      private static Texture2D CREATE_CURSOR;

      private static Color32 DELETE_VISUALIZER_COLOR = new Color32(231, 54, 51, 255);
      private static GameObject DELETE_AREA_GO;
      private static Texture2D DELETE_CURSOR;


      private int selectedNotification;
      private GNotification selectedGNotification;
      // settings of a new notification:
      private string notificationName_new;
      private string tooltip_new;
      private GNotificationType type_new;
      private bool pause_new;
      private bool zoom_new;

      private SpriteRenderer visualizerRenderer;
      private NotifierToolMode currentMode;

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

      public NotifierToolMode GetToolMode() {
         return currentMode;
      }
      public void SetToolMode(NotifierToolMode mode) {
         if(mode == currentMode)
            return;

         currentMode = mode;
         switch(currentMode)
         {
            case NotifierToolMode.CREATE_NOTIFICATION:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("en_create_notification");
               IsStyleDelete = false;
               break;

            case NotifierToolMode.ADD_ERRAND:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("en_add_errand");
               IsStyleDelete = false;
               break;

            case NotifierToolMode.DELETE_NOTIFICATION:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("en_delete_notification");
               IsStyleDelete = true;
               break;

            case NotifierToolMode.REMOVE_ERRAND:
               visualizerRenderer.sprite = MYSPRITES.GetSprite("en_remove_errand");
               IsStyleDelete = true;
               break;
         }

         if(Main.notifierOverlay != default)
         {
            Main.notifierOverlay.UpdateAllUISymbols();
         }
      }

      public int GetSelectedNotification() {
         return selectedNotification;
      }
      public void SetSelectedNotification(int targetNum) {
         if(NotificationsContainer.NotificationsCount > 0)
         {
            if(targetNum < 0)
            {
               selectedNotification = NotificationsContainer.NotificationsCount - 1;// for looping around
            }
            else if(targetNum >= NotificationsContainer.NotificationsCount)
            {
               selectedNotification = 0;// for looping around
            }
            else
            {
               selectedNotification = targetNum;
            }
         }
         else
         {
            selectedNotification = -1;
         }

         selectedGNotification = NotificationsContainer.GetNotification(selectedNotification);

         if(Main.notifierOverlay != default)
         {
            Main.notifierOverlay.UpdateAllUISymbols();
         }
      }

      public string GetName() {
         return currentMode == NotifierToolMode.CREATE_NOTIFICATION ? notificationName_new : selectedGNotification != null ? selectedGNotification.name : default;
      }
      public void SetName(string name) {
         if(currentMode == NotifierToolMode.CREATE_NOTIFICATION)
            notificationName_new = name;
         else
            if(selectedGNotification != null)
            selectedGNotification.name = name;
      }

      public string GetTooltip() {
         return currentMode == NotifierToolMode.CREATE_NOTIFICATION ? tooltip_new : selectedGNotification != null ? selectedGNotification.tooltip : default;
      }
      public void SetTooltip(string tooltip) {
         if(currentMode == NotifierToolMode.CREATE_NOTIFICATION)
            tooltip_new = tooltip;
         else
            if(selectedGNotification != null)
            selectedGNotification.tooltip = tooltip;
      }

      public GNotificationType GetNotificationType() {
         return currentMode == NotifierToolMode.CREATE_NOTIFICATION ? type_new : selectedGNotification != null ? selectedGNotification.type : default;
      }
      public void SetNotificationType(GNotificationType type) {
         if(currentMode == NotifierToolMode.CREATE_NOTIFICATION)
            type_new = type;
         else
            if(selectedGNotification != null)
            selectedGNotification.type = type;
      }

      public bool GetShouldPause() {
         return currentMode == NotifierToolMode.CREATE_NOTIFICATION ? pause_new : selectedGNotification != null ? selectedGNotification.pause : default;
      }
      public void SetShouldPause(bool pause) {
         if(currentMode == NotifierToolMode.CREATE_NOTIFICATION)
            pause_new = pause;
         else
            if(selectedGNotification != null)
            selectedGNotification.pause = pause;
      }

      public bool GetShouldZoom() {
         return currentMode == NotifierToolMode.CREATE_NOTIFICATION ? zoom_new : selectedGNotification != null ? selectedGNotification.zoom : default;
      }
      public void SetShouldZoom(bool zoom) {
         if(currentMode == NotifierToolMode.CREATE_NOTIFICATION)
            zoom_new = zoom;
         else
            if(selectedGNotification != null)
            selectedGNotification.zoom = zoom;
      }

      /// <summary>
      /// Resets the settings stored for the new notification to the default values.
      /// </summary>
      public void ResetNewNotification() {
         notificationName_new = MYSTRINGS.UI.NOTIFIERTOOLMENU.NAME_DEFAULT;
         tooltip_new = MYSTRINGS.UI.NOTIFIERTOOLMENU.TOOLTIP_DEFAULT;
         type_new = GNotificationType.POP;
         pause_new = false;
         zoom_new = false;
      }



      public NotifierTool() {
         selectedNotification = 0;
         selectedGNotification = NotificationsContainer.GetNotification(selectedNotification);
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

         var menu = NotifierToolMenu.Instance;
         if(!menu.HasOptions)
            menu.PopulateMenu();
         menu.ShowMenu();

         ToolMenu.Instance.PriorityScreen.Show(false);
         OverlayScreen.Instance.ToggleOverlay(NotifierOverlay.ID);
      }

      public override void OnCleanUp() {
         base.OnCleanUp();
         PUtil.LogDebug(Main.debugPrefix + "Destroying NotifierTool");
      }

      public override void OnDeactivateTool(InterfaceTool newTool) {
         base.OnDeactivateTool(newTool);

         NotifierToolMenu.Instance?.HideMenu();
         ToolMenu.Instance.PriorityScreen.Show(false);
         OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID);

         SetToolMode(NotifierToolMode.CREATE_NOTIFICATION);

         if(NotifierToolMenu.Instance != default)
         {
            NotifierToolMenu.Instance.UpdateNotifierMenuDisplay();
         }
      }

      public override void OnPrefabInit() {
         base.OnPrefabInit();

         // store prefabs:
         CREATE_AREA_GO = Util.KInstantiate(DigTool.Instance.areaVisualizer, gameObject, "NotifierToolCreateAreaVisualizer");
         CREATE_AREA_GO.SetLayerRecursively(LayerMask.NameToLayer("UI"));// same layer as DigTool's visualizer
         CREATE_AREA_GO.SetActive(false);
         DELETE_AREA_GO = Util.KInstantiate(CancelTool.Instance.areaVisualizer, gameObject, "NotifierToolDeleteAreaVisualizer");
         DELETE_AREA_GO.SetLayerRecursively(LayerMask.NameToLayer("UI"));// same layer as DigTool's visualizer
         DELETE_AREA_GO.SetActive(false);

         CREATE_CURSOR = DigTool.Instance.cursor;
         DELETE_CURSOR = CancelTool.Instance.cursor;

         // hover card:
         gameObject.AddComponent<NotifierToolHover>();

         // cursor:
         this.cursor = CREATE_CURSOR;
         this.boxCursor = cursor;

         // area visualizer:
         this.areaVisualizer = CREATE_AREA_GO;
         this.areaVisualizerSpriteRenderer = areaVisualizer.GetComponent<SpriteRenderer>();
         this.areaVisualizerTextPrefab = DisinfectTool.Instance.areaVisualizerTextPrefab;// == no text

         // visualizer (single cell icon):
         visualizer = new GameObject("NotifierToolVisualizer");
         visualizer.SetActive(false);

         GameObject offsetObject = new GameObject("IconOffset");
         offsetObject.SetActive(true);
         visualizerRenderer = offsetObject.AddComponent<SpriteRenderer>();
         visualizerRenderer.color = CREATE_VISUALIZER_COLOR;
         visualizerRenderer.sprite = MYSPRITES.GetSprite("en_create_notification");
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

         Vector2I dragMin = new Vector2I((int)Math.Floor(Math.Min(cursorDown.x, cursorUp.x)), (int)Math.Floor(Math.Min(cursorDown.y, cursorUp.y)));
         Vector2I dragMax = new Vector2I((int)Math.Ceiling(Math.Max(cursorDown.x, cursorUp.x)), (int)Math.Ceiling(Math.Max(cursorDown.y, cursorUp.y)));
         Extents extents = new Extents(dragMin.x, dragMin.y, dragMax.x - dragMin.x, dragMax.y - dragMin.y);

         Dictionary<GameObject, HashSet<Workable>> collectedErrands = new();

         var collectedGOs = Utils.CollectPrioritizableObjects(extents);

         ErrandsSearchMode searchMode;
         if(currentMode == NotifierToolMode.CREATE_NOTIFICATION || currentMode == NotifierToolMode.ADD_ERRAND)
         {
            searchMode = ErrandsSearchMode.ERRANDS_OUTSIDE_NOTIFICATION;// only searching errands that can be added to a new notification
         }
         else
         {
            searchMode = ErrandsSearchMode.ERRANDS_INSIDE_NOTIFICATION;// only searching errands that are already in a notification
         }

         foreach(var go in collectedGOs)
         {
            HashSet<Workable> errands = go.CollectFilteredErrands(out _, searchMode: searchMode);
            if(errands != null && errands.Count > 0)
            {
               collectedErrands.Add(go, errands);
            }
         }

         if(collectedErrands.Count > 0)
         {
            switch(currentMode)
            {
               case NotifierToolMode.CREATE_NOTIFICATION:
                  NotifierToolUtils.CreateNewNotification(collectedErrands);
                  break;

               case NotifierToolMode.ADD_ERRAND:
                  NotifierToolUtils.AddErrands(collectedErrands);
                  break;

               case NotifierToolMode.DELETE_NOTIFICATION:
                  NotifierToolUtils.DeleteNotifications(new HashSet<Workable>(collectedErrands.Values.SelectMany(x => x)));
                  break;

               case NotifierToolMode.REMOVE_ERRAND:
                  NotifierToolUtils.RemoveErrands(new HashSet<Workable>(collectedErrands.Values.SelectMany(x => x)));
                  break;
            }
         }
      }

      public override void OnDragTool(int cell, int distFromOrigin) {
         if(Main.notifierOverlay == null)
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