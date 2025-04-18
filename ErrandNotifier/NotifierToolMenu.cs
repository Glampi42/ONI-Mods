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
using ErrandNotifier.Notifications;
using ErrandNotifier.Strings;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoronoiTree;

namespace ErrandNotifier {
   public sealed class NotifierToolMenu : KMonoBehaviour {
      /// <summary>
		/// The singleton instance of this class.
		/// </summary>
		public static NotifierToolMenu Instance { get; private set; }

      /// <summary>
      /// Initializes the instance of this class.
      /// </summary>
      public static void CreateInstance() {
         var parameterMenu = new GameObject("SettingsChangeParams");
         var originalMenu = ToolMenu.Instance.toolParameterMenu;
         if(originalMenu != null)
            parameterMenu.transform.SetParent(originalMenu.transform.parent);
         // The content is not actually a child of tool menu's GO, so this GO can be plain
         Instance = parameterMenu.AddComponent<NotifierToolMenu>();
         parameterMenu.SetActive(true);
         parameterMenu.SetActive(false);
      }

      public static void DestroyInstance() {
         var inst = Instance;
         if(inst != null)
         {
            inst.ClearMenu();
            Destroy(inst.gameObject);
         }
         Instance = null;
      }

      public bool HasOptions {
         get {
            return options.Count > 0;
         }
      }

      /// <summary>
      /// The parent of each layer checkbox.
      /// </summary>
      private GameObject choiceList;
      /// <summary>
      /// The filter menu.
      /// </summary>
      private GameObject content;
      /// <summary>
      /// The checkboxes for each layer.
      /// </summary>
      private readonly IDictionary<NotifierToolFilter, NotifierToolMenuOption> options;


      private GameObject toolsPanel;
      private GameObject createButton;
      private GameObject deleteButton;

      private GameObject createPanel;
      private GameObject notificationConfigurationPanel;
      private GameObject notificationIDLabel;
      private GameObject IDDecrease;
      private GameObject IDIncrease;

      private TMP_InputField IDField;

      private GameObject deletePanel;

      public Dictionary<NotifierToolMode, MultiToggle> modeToggles = new Dictionary<NotifierToolMode, MultiToggle>(Enum.GetNames(typeof(NotifierToolMode)).Length);


      public NotifierToolMenu() {
         options = new Dictionary<NotifierToolFilter, NotifierToolMenuOption>();
      }

      /// <summary>
      /// Switches between the createPanel and deletePanel.
      /// </summary>
      /// <param name="create">Determines which panel should be activated</param>
      private void SwitchCreateOrDelete(bool create) {
         if(this.content == null)
            return;

         if(create)
         {
            if(Main.notifierTool.GetToolMode() == NotifierToolMode.DELETE_NOTIFICATION)
            {
               Main.notifierTool.SetToolMode(NotifierToolMode.CREATE_NOTIFICATION);
            }
            else if(Main.notifierTool.GetToolMode() == NotifierToolMode.REMOVE_ERRAND)
            {
               Main.notifierTool.SetToolMode(NotifierToolMode.ADD_ERRAND);
            }
            // else no actions required
         }
         else
         {
            if(Main.notifierTool.GetToolMode() == NotifierToolMode.CREATE_NOTIFICATION)
            {
               Main.notifierTool.SetToolMode(NotifierToolMode.DELETE_NOTIFICATION);
            }
            else if(Main.notifierTool.GetToolMode() == NotifierToolMode.ADD_ERRAND)
            {
               Main.notifierTool.SetToolMode(NotifierToolMode.REMOVE_ERRAND);
            }
            // else no actions required
         }

         UpdateToolModeSelectionDisplay();
      }

      /// <summary>
      /// Updates the visual elements of the menu that represent the selected notifier tool mode.
      /// </summary>
      public void UpdateToolModeSelectionDisplay() {
         if(this.content == null)
            return;

         NotifierToolMode currentMode = Main.notifierTool.GetToolMode();

         bool createPanelActive = currentMode == NotifierToolMode.CREATE_NOTIFICATION || currentMode == NotifierToolMode.ADD_ERRAND;
         PButton.SetButtonEnabled(createButton, !createPanelActive);
         PButton.SetButtonEnabled(deleteButton, createPanelActive);
         createPanel.SetActive(createPanelActive);
         deletePanel.SetActive(!createPanelActive);

         foreach(var toggleMode in modeToggles.Keys)
         {
            modeToggles[toggleMode].ChangeState(toggleMode == currentMode ? 1 : 0);

            if(toggleMode == NotifierToolMode.ADD_ERRAND)
            {
               this.notificationConfigurationPanel.SetActive(toggleMode == currentMode);
            }
         }
      }

      /// <summary>
      /// Updates the visual elements of the menu that represent the selected notification's ID and its properties.
      /// </summary>
      public void UpdateNotificationConfigDisplay() {
         if(this.content == null)
            return;

         if(NotificationsContainer.NotificationsCount > 1)
         {
            SetArrowButtonEnabled(IDDecrease, true);
            SetArrowButtonEnabled(IDIncrease, true);
         }
         else// there are no other notifications to switch to
         {
            SetArrowButtonEnabled(IDDecrease, false);
            SetArrowButtonEnabled(IDIncrease, false);
         }
         if(NotificationsContainer.NotificationsCount > 0)
         {
            SetTextFieldText(IDField, Main.notifierTool.GetSelectedNotification().ToString());
         }
         else
         {
            SetTextFieldText(IDField, MYSTRINGS.UI.NOTIFIERTOOLMENU.NOTIFICATIONID_NOTFOUND);
         }
      }
      private void SetArrowButtonEnabled(GameObject button, bool enabled) {
         var disabled = button.GetChildSafe(1);
         if(disabled != null)
         {
            disabled.SetActive(!enabled);
         }
      }
      /// <summary>
      /// Sets text of a text field.
      /// </summary>
      /// <param name="textField">The text field</param>
      /// <param name="text">The new text</param>
      private void SetTextFieldText(TMP_InputField textField, string text) {
         if(textField == null)
            return;

         textField.text = text;
      }

      /// <summary>
      /// Removes all entries from the menu and hides it.
      /// </summary>
      public void ClearMenu() {
         HideMenu();
         foreach(var option in options)
            Destroy(option.Value.Checkbox);
         options.Clear();
      }

      /// <summary>
      /// Hides the menu without destroying it.
      /// </summary>
      public void HideMenu() {
         if(content != null)
         {
            content.SetActive(false);
         }
      }

      /// <summary>
      /// Updates the visible checkboxes to correspond with the layer settings and runs other filter-related logic.
      /// </summary>
      private void OnChange() {
         foreach(var option in options.Values)
         {
            var checkbox = option.Checkbox;
            switch(option.State)
            {
               case ToolParameterMenu.ToggleState.On:
                  PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_CHECKED);
                  break;
               case ToolParameterMenu.ToggleState.Off:
                  PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_UNCHECKED);
                  break;
               case ToolParameterMenu.ToggleState.Disabled:
               default:
                  PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
                  break;
            }
         }

         if(Main.notifierOverlay != null && Main.notifierOverlay.IsEnabled)
         {
            Main.notifierOverlay.UpdateOverlay();
         }
      }

      /// <summary>
      /// When an option is selected, updates the state of other options if necessary and
      /// refreshes the UI.
      /// </summary>
      /// <param name="target">The option check box that was clicked.</param>
      private void OnClick(GameObject target) {
         foreach(var option in options.Values)
            if(option.Checkbox == target)
            {
               if(option.State == ToolParameterMenu.ToggleState.Off)
               {
                  // Set to on and all others to off
                  foreach(var disableOption in options.Values)
                     if(disableOption != option)
                        disableOption.State = ToolParameterMenu.ToggleState.Off;
                  option.State = ToolParameterMenu.ToggleState.On;
                  OnChange();
               }
               break;
            }
      }

      public override void OnCleanUp() {
         ClearMenu();
         if(content != null)
            Destroy(content);
         base.OnCleanUp();
      }

      public override void OnPrefabInit() {
         Prefabs.CreateArrowButtonsPrefab();

         base.OnPrefabInit();

         var baseMenu = ToolMenu.Instance.toolParameterMenu;
         var filterMenuPrefab = baseMenu.content;
         var oldestParent = filterMenuPrefab.GetParent();

         //------------------Creating notifier tool menu------------------DOWN
         content = new GameObject("MainPanel");
         content.AddOrGet<RectTransform>();
         content.SetParent(oldestParent);

         // very specific settings, DON'T TOUCH (if something looks useless, then DON'T TOUCH IT (but yeah, I also have no idea why that's necessary)):
         var vlayoutGroup = content.AddComponent<VerticalLayoutGroup>();
         vlayoutGroup.childAlignment = TextAnchor.LowerRight;
         vlayoutGroup.childControlHeight = true;
         vlayoutGroup.childControlWidth = true;
         vlayoutGroup.childForceExpandHeight = false;
         vlayoutGroup.childForceExpandWidth = false;
         vlayoutGroup.spacing = 0f;

         // additional GO for horizontal layout
         var horizontal = new GameObject("HorizontalLayoutGO");
         horizontal.SetParent(content);
         var hlayoutGroup = horizontal.AddComponent<HorizontalLayoutGroup>();
         hlayoutGroup.childAlignment = TextAnchor.LowerRight;
         hlayoutGroup.childControlHeight = true;
         hlayoutGroup.childControlWidth = true;
         hlayoutGroup.childForceExpandHeight = false;
         hlayoutGroup.childForceExpandWidth = false;
         hlayoutGroup.spacing = 20f;
         //------------------Setting Notifier Tool menu------------------DOWN
         PPanel notifierToolMenu = new PPanel("NotifierToolMenu") {
            Alignment = TextAnchor.MiddleCenter,
            Direction = PanelDirection.Vertical,
            FlexSize = Vector2.zero,
         };
         notifierToolMenu.AddOnRealize(menu => {
            UpdateNotificationConfigDisplay();
         });

         PPanel title = new PPanel("Title") {
            Alignment = TextAnchor.MiddleCenter,
            FlexSize = Vector2.right,
            BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor,
            BackImage = PUITuning.Images.BoxBorder,
            ImageMode = Image.Type.Sliced
         };
         title.AddOnRealize(go => {
            var layoutElem = go.AddOrGet<LayoutElement>();
            layoutElem.minWidth = 200f;// same width as Tool Filter menu
         });

         PLabel titleText = new PLabel("Text") {
            Text = MYSTRINGS.UI.NOTIFIERTOOLMENU.TITLE,
            TextStyle = PUITuning.Fonts.TextLightStyle,
            Margin = new RectOffset(25, 25, 3, 3),
         };

         title.AddChild(titleText);

         PPanel toolsPanel = new PPanel("ToolsPanel") {
            Alignment = TextAnchor.MiddleCenter,
            Direction = PanelDirection.Vertical,
            FlexSize = new Vector2(1f, 1f),
            BackColor = Main.grayBackgroundColor,
            BackImage = Assets.GetSprite("web_stack"),// black 1 pixel outline except for the upper edge
            ImageMode = Image.Type.Sliced,
            Margin = new RectOffset(9, 9, 10, 4),
            Spacing = 10
         };
         toolsPanel.AddOnRealize(go => {
            this.toolsPanel = go;

            // adding tooltips should be done here because the GameObject of toolsPanel is realized after the labels' GameObjects
            this.notificationIDLabel.AddFilterMenuToolTip(MYSTRINGS.UI.NOTIFIERTOOLMENU.NOTIFICATIONID_TOOLTIP, this.toolsPanel.rectTransform());
            this.IDField.gameObject.AddFilterMenuToolTip(MYSTRINGS.UI.NOTIFIERTOOLMENU.NOTIFICATIONID_TOOLTIP, this.toolsPanel.rectTransform());
         });

         PRelativePanel createOrDelete = new PRelativePanel("CreateOrDeletePanel") {
            FlexSize = Vector2.right,
         };

         PButton createButton = new PButton("CreateButton");
         createButton.FlexSize = Vector2.right;
         createButton.SetKleiPinkStyle();
         createButton.TextStyle = PUITuning.Fonts.TextLightStyle;
         createButton.Color.disabledColor = PUITuning.Colors.ButtonPinkStyle.activeColor;
         createButton.Color.disabledhoverColor = PUITuning.Colors.ButtonPinkStyle.hoverColor;
         createButton.Margin = new RectOffset(8, 8, 4, 4);
         createButton.Text = MYSTRINGS.UI.NOTIFIERTOOLMENU.CREATEBUTTON;
         createButton.AddOnRealize(go => this.createButton = go);
         createButton.AddOnRealize(realized => PButton.SetButtonEnabled(realized, false));
         createButton.OnClick = go => SwitchCreateOrDelete(true);
         PButton deleteButton = new PButton("DeleteButton");
         deleteButton.FlexSize = Vector2.right;
         deleteButton.SetKleiPinkStyle();
         deleteButton.TextStyle = PUITuning.Fonts.TextLightStyle;
         deleteButton.Color.disabledColor = PUITuning.Colors.ButtonPinkStyle.activeColor;
         deleteButton.Color.disabledhoverColor = PUITuning.Colors.ButtonPinkStyle.hoverColor;
         deleteButton.Margin = new RectOffset(8, 8, 4, 4);
         deleteButton.Text = MYSTRINGS.UI.NOTIFIERTOOLMENU.DELETEBUTTON;
         deleteButton.AddOnRealize(go => this.deleteButton = go);
         deleteButton.AddOnRealize(realized => PButton.SetButtonEnabled(realized, true));
         deleteButton.OnClick = go => SwitchCreateOrDelete(false);

         createOrDelete.AddChild(createButton).AddChild(new PSpacer()).AddChild(deleteButton);
         createOrDelete.SetLeftEdge(createButton, 0f).SetRightEdge(createButton, 0.45f).SetLeftEdge(deleteButton, 0.55f).SetRightEdge(deleteButton, 1f);
         //------------------Setting create panel------------------DOWN
         PPanel createPanel = new PPanel("CreatePanel") {
            Alignment = TextAnchor.MiddleLeft,
            Direction = PanelDirection.Vertical,
            FlexSize = new Vector2(1f, 1f),
            Spacing = 3,
            Margin = new RectOffset(0, 0, 0, 0)
         };
         createPanel.AddOnRealize(go => {
            this.createPanel = go;

            System.Action addCreateToggles = () => {
               var createNotification = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.createPanel, true);
               ConfigureToggle(createNotification, NotifierToolMode.CREATE_NOTIFICATION, MYSTRINGS.UI.NOTIFIERTOOLMENU.CREATENOTIFICATION, MYSTRINGS.UI.NOTIFIERTOOLMENU.CREATENOTIFICATION_TOOLTIP, true, () => {
                  //onClick:
                  Main.notifierTool.SetToolMode(NotifierToolMode.CREATE_NOTIFICATION);
                  UpdateToolModeSelectionDisplay();
               });

               var addErrand = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.createPanel, true);
               ConfigureToggle(addErrand, NotifierToolMode.ADD_ERRAND, MYSTRINGS.UI.NOTIFIERTOOLMENU.ADDERRAND, MYSTRINGS.UI.NOTIFIERTOOLMENU.ADDERRAND_TOOLTIP, false, () => {
                  //onClick:
                  Main.notifierTool.SetToolMode(NotifierToolMode.ADD_ERRAND);
                  UpdateToolModeSelectionDisplay();
               });

               this.notificationConfigurationPanel.transform.SetAsLastSibling();// this panel should be below the toggles
            };
            Prefabs.RunAfterPrefabsInit(addCreateToggles, nameof(Prefabs.FilterToggleReversedPrefab));
         });

         PPanel notificationConfigPanel = new PPanel("NotificationConfigurationPanel") {
            Alignment = TextAnchor.MiddleLeft,
            Direction = PanelDirection.Vertical,
            FlexSize = new Vector2(1f, 1f),
            Spacing = 3,
            Margin = new RectOffset(0, 0, 5, 1),
         };
         notificationConfigPanel.AddOnRealize(go => {
            this.notificationConfigurationPanel = go;
            this.notificationConfigurationPanel.SetActive(false);
         });

         PLabel notificationIDLabel = new PLabel("NotificationIDLabel") {
            TextAlignment = TextAnchor.MiddleLeft,
            TextStyle = PUITuning.Fonts.TextLightStyle,
            Text = MYSTRINGS.UI.NOTIFIERTOOLMENU.NOTIFICATIONID,
         };
         notificationIDLabel.AddOnRealize(label => this.notificationIDLabel = label);

         PPanel notificationIDInput = new PPanel("NotificationIDInput") {
            Alignment = TextAnchor.MiddleCenter,
            Direction = PanelDirection.Horizontal,
            FlexSize = new Vector2(1f, 1f),
            Spacing = 8,
         };
         notificationIDInput.AddOnRealize(panel => {
            IDDecrease = Util.KInstantiateUI(Prefabs.ArrowLeftButtonPrefab, panel);
            IDDecrease.name = "IDDecrease";
            ConfigureArrowButton(IDDecrease, true, () => {
               Main.notifierTool.SetSelectedNotification(Main.notifierTool.GetSelectedNotification() - 1);
               UpdateNotificationConfigDisplay();
            });

            IDIncrease = Util.KInstantiateUI(Prefabs.ArrowRightButtonPrefab, panel);
            IDIncrease.name = "IDIncrease";
            ConfigureArrowButton(IDIncrease, false, () => {
               Main.notifierTool.SetSelectedNotification(Main.notifierTool.GetSelectedNotification() + 1);
               UpdateNotificationConfigDisplay();
            });
         });

         PTextField idField = new PTextField("IDField") {
            Type = PTextField.FieldType.Integer,
         };
         idField.AddOnRealize(field => {
            IDField = field.GetComponent<TMP_InputField>();

            var layoutElem = field.AddOrGet<LayoutElement>();
            layoutElem.minHeight = 24f;
            layoutElem.minWidth = 100f;
         });
         idField.OnTextChanged = (GameObject textField, string text) => {
            int notificationID = NotifierToolUtils.InterpretNotificationID(text);
            Main.notifierTool.SetSelectedNotification(notificationID);
            UpdateNotificationConfigDisplay();
         };

         notificationIDInput.AddChild(idField);

         notificationConfigPanel.AddChild(notificationIDLabel).AddChild(notificationIDInput);
         createPanel.AddChild(notificationConfigPanel);
         //------------------Setting create panel------------------UP
         //------------------Setting delete panel------------------DOWN
         PPanel deletePanel = new PPanel("DeletePanel") {
            Alignment = TextAnchor.MiddleLeft,
            Direction = PanelDirection.Vertical,
            FlexSize = new Vector2(1f, 1f),
            Spacing = 3
         };
         deletePanel.AddOnRealize(go => {
            this.deletePanel = go;
            this.deletePanel.SetActive(false);

            System.Action addDeleteToggles = () => {
               var deleteNotification = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.deletePanel, true);
               ConfigureToggle(deleteNotification, NotifierToolMode.DELETE_NOTIFICATION, MYSTRINGS.UI.NOTIFIERTOOLMENU.DELETENOTIFICATION, MYSTRINGS.UI.NOTIFIERTOOLMENU.DELETENOTIFICATION_TOOLTIP, false, () => {
                  //onClick:
                  Main.notifierTool.SetToolMode(NotifierToolMode.DELETE_NOTIFICATION);
                  UpdateToolModeSelectionDisplay();
               });

               var removeErrand = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.deletePanel, true);
               ConfigureToggle(removeErrand, NotifierToolMode.REMOVE_ERRAND, MYSTRINGS.UI.NOTIFIERTOOLMENU.REMOVEERRAND, MYSTRINGS.UI.NOTIFIERTOOLMENU.REMOVEERRAND_TOOLTIP, false, () => {
                  //onClick:
                  Main.notifierTool.SetToolMode(NotifierToolMode.REMOVE_ERRAND);
                  UpdateToolModeSelectionDisplay();
               });
            };
            Prefabs.RunAfterPrefabsInit(addDeleteToggles, nameof(Prefabs.FilterToggleReversedPrefab));
         });
         //------------------Setting delete panel------------------UP
         toolsPanel.AddChild(createOrDelete).AddChild(createPanel).AddChild(deletePanel);

         notifierToolMenu.AddChild(title).AddChild(toolsPanel).AddTo(horizontal);
         //------------------Setting Notifier Tool menu------------------UP
         //------------------Setting Tool Filter menu------------------DOWN
         // required for a correct filter menu display
         GameObject filterMenuContainer = new GameObject("FilterMenuContainer");
         filterMenuContainer.SetParent(horizontal);
         vlayoutGroup = filterMenuContainer.AddComponent<VerticalLayoutGroup>();
         vlayoutGroup.childControlHeight = false;
         vlayoutGroup.childControlWidth = false;
         vlayoutGroup.childForceExpandHeight = false;
         vlayoutGroup.childForceExpandWidth = false;

         GameObject toolFilterMenu_go = Util.KInstantiateUI(filterMenuPrefab, filterMenuContainer, false);
         toolFilterMenu_go.rectTransform().offsetMax = new Vector2(0.0f, 400f);

         if(toolFilterMenu_go.rectTransform().childCount > 1)
         {
            // Add buttons to the chooser
            choiceList = toolFilterMenu_go.rectTransform().GetChild(1).gameObject;
         }
         //------------------Setting Tool Filter menu------------------UP
         var transform = content.rectTransform();
         // Bump up the offset to allow more space
         transform.offsetMin = new Vector2(0.0f, 100.0f);
         transform.offsetMax = new Vector2(0.0f, 400.0f);
         transform.SetAsFirstSibling();
         HideMenu();
         //------------------Creating notifier tool menu------------------UP
      }
      /// <summary>
      /// Fixes up the toggle to look nice in the Notifier Tools menu.
      /// </summary>
      /// <param name="toggle">The toggle's GameObject</param>
      /// <param name="mode">NotifierToolMode associated with this toggle</param>
      /// <param name="text">The text to show next to the toggle</param>
      /// <param name="tooltip">The tooltip to show when hovering over the text</param>
      /// <param name="initialState">The starting state: 1 is on, 0 is off</param>
      /// <param name="onClick">The action that should be done onClick</param>
      private void ConfigureToggle(GameObject toggle, NotifierToolMode mode, string text, string tooltip, bool initialState, System.Action onClick) {
         toggle.AddFilterMenuToolTip(tooltip, toolsPanel.rectTransform());

         var toggleLabel = toggle.GetComponentInChildren<LocText>();
         toggleLabel.text = text;
         toggleLabel.fontStyle = TMPro.FontStyles.Normal;
         toggleLabel.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
         var layoutElem = toggleLabel.GetComponent<LayoutElement>();
         layoutElem.minWidth = 20f;

         GameObject collider;
         if((collider = toggle.transform.FindRecursively("collider")?.gameObject) != null)
         {
            UnityEngine.Object.Destroy(collider);
         }

         if(toggle.TryGetComponent(out HorizontalLayoutGroup hlayoutGroup))
         {
            hlayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            hlayoutGroup.childForceExpandWidth = false;
            hlayoutGroup.spacing = 7f;
            hlayoutGroup.padding = new RectOffset(0, 0, 0, 0);
         }

         MultiToggle mToggle = toggle.GetComponentInChildren<MultiToggle>();
         modeToggles.Add(mode, mToggle);
         mToggle.onClick += onClick;
         mToggle.ChangeState(initialState ? 1 : 0);
         if(initialState)
            mToggle.onClick.Invoke();
      }
      /// <summary>
      /// Adds onClick action and fixes the position of the button.
      /// </summary>
      /// <param name="button">The button</param>
      /// <param name="leftButton">If true, the sibling index of the button will be set to 0; otherwise, it will be set to the biggest value (the button will be on the right)</param>
      /// <param name="onClick">The action that should be done onClick</param>
      private void ConfigureArrowButton(GameObject button, bool leftButton, System.Action onClick) {
         if(leftButton)
         {
            button.transform.SetAsFirstSibling();
         }
         else
         {
            button.transform.SetAsLastSibling();
         }

         if(button.TryGetComponent(out MultiToggle mToggle))
         {
            mToggle.onClick += onClick;
         }
      }

      /// <summary>
      /// Populates the menu with the available modes.
      /// </summary>
      internal void PopulateMenu() {
         int i = 0;
         var prefab = ToolMenu.Instance.toolParameterMenu.widgetPrefab;
         ClearMenu();
         foreach(NotifierToolFilter filter in Enum.GetValues(typeof(NotifierToolFilter)))
         {
            // Create prefab based on existing Klei menu
            var widgetPrefab = Util.KInstantiateUI(prefab, choiceList, true);
            PUIElements.SetText(widgetPrefab, filter.Name());
            widgetPrefab.AddFilterMenuToolTip(filter.Tooltip(), choiceList.rectTransform(), wrapWidth: 256);

            var toggle = widgetPrefab.GetComponentInChildren<MultiToggle>();
            if(toggle != null)
            {
               var checkbox = toggle.gameObject;
               // Set initial state, note that ChangeState is only called by SetCheckState
               // if it appears to be different, but since this executes before the
               // parent is active it must be set to something different
               var option = new NotifierToolMenuOption(filter, checkbox);
               PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
               if(i == 0)
               {
                  option.State = ToolParameterMenu.ToggleState.On;
               }
               options.Add(filter, option);
               toggle.onClick += () => OnClick(checkbox);
            }
            else
               PUtil.LogWarning(Main.debugPrefix + "Could not find tool menu checkbox!");

            i++;
         }
      }

      /// <summary>
      /// Sets all check boxes to the same value.
      /// </summary>
      /// <param name="toggleState">The toggle state to set.</param>
      public void SetAll(ToolParameterMenu.ToggleState toggleState) {
         foreach(var option in options)
            option.Value.State = toggleState;
         OnChange();
      }

      public ToolParameterMenu.ToggleState GetToggleState(NotifierToolFilter filter) {
         if(!options.TryGetValue(filter, out NotifierToolMenuOption option))
            return ToolParameterMenu.ToggleState.Off;

         return option.State;
      }

      /// <summary>
      /// Shows the menu.
      /// </summary>
      public void ShowMenu() {
         content.SetActive(true);
         OnChange();
         UpdateNotificationConfigDisplay();
      }

      /// <summary>
      /// Stores available settings change tools and their current menu states.
      /// </summary>
      private sealed class NotifierToolMenuOption {
         /// <summary>
         /// The check box in the UI.
         /// </summary>
         public GameObject Checkbox { get; }

         /// <summary>
         /// The current option state.
         /// </summary>
         public ToolParameterMenu.ToggleState State { get; set; }

         /// <summary>
         /// The filter representing this option.
         /// </summary>
         public NotifierToolFilter Filter { get; }

         public NotifierToolMenuOption(NotifierToolFilter filter, GameObject checkbox) {
            Checkbox = checkbox ?? throw new ArgumentNullException(nameof(checkbox));
            Filter = filter;
            State = ToolParameterMenu.ToggleState.Off;
         }
      }
   }
}
