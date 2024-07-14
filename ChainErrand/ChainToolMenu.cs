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

using ChainErrand.Enums;
using ChainErrand.Strings;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VoronoiTree;
using static CrewJobsEntry;

namespace ChainErrand {
   public sealed class ChainToolMenu : KMonoBehaviour {
      /// <summary>
		/// The singleton instance of this class.
		/// </summary>
		public static ChainToolMenu Instance { get; private set; }

      /// <summary>
      /// Initializes the instance of this class.
      /// </summary>
      public static void CreateInstance() {
         var parameterMenu = new GameObject("SettingsChangeParams");
         var originalMenu = ToolMenu.Instance.toolParameterMenu;
         if(originalMenu != null)
            parameterMenu.transform.SetParent(originalMenu.transform.parent);
         // The content is not actually a child of tool menu's GO, so this GO can be plain
         Instance = parameterMenu.AddComponent<ChainToolMenu>();
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
      private readonly IDictionary<ChainToolFilters.Filter, ChainToolMenuOption> options;


      private GameObject createButton;
      private GameObject deleteButton;
      private GameObject createPanel;
      private GameObject deletePanel;

      private Dictionary<ChainToolMode, MultiToggle> modeToggles = new Dictionary<ChainToolMode, MultiToggle>(Enum.GetNames(typeof(ChainToolMode)).Length);


      public ChainToolMenu() {
         options = new Dictionary<ChainToolFilters.Filter, ChainToolMenuOption>();
      }

      private void SwitchCreateOrDelete(bool create) {
         if(this.content == null)
            return;
         PButton.SetButtonEnabled(createButton, !create);
         PButton.SetButtonEnabled(deleteButton, create);
         createPanel.SetActive(create);
         deletePanel.SetActive(!create);

         // managing toggles:
         if(create)
         {
            if(modeToggles[ChainToolMode.DELETE_CHAIN].CurrentState == 1)
            {
               modeToggles[ChainToolMode.CREATE_CHAIN].onClick.Invoke();
               modeToggles[ChainToolMode.DELETE_CHAIN].ChangeState(0);
            }
            else if(modeToggles[ChainToolMode.DELETE_LINK].CurrentState == 1)
            {
               modeToggles[ChainToolMode.CREATE_LINK].onClick.Invoke();
               modeToggles[ChainToolMode.DELETE_LINK].ChangeState(0);
            }
         }
         else
         {
            if(modeToggles[ChainToolMode.CREATE_CHAIN].CurrentState == 1)
            {
               modeToggles[ChainToolMode.DELETE_CHAIN].onClick.Invoke();
               modeToggles[ChainToolMode.CREATE_CHAIN].ChangeState(0);
            }
            else if(modeToggles[ChainToolMode.CREATE_LINK].CurrentState == 1)
            {
               modeToggles[ChainToolMode.DELETE_LINK].onClick.Invoke();
               modeToggles[ChainToolMode.CREATE_LINK].ChangeState(0);
            }
         }
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
            content.SetActive(false);
      }

      /// <summary>
      /// Updates the visible checkboxes to correspond with the layer settings.
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
         //TODO update overlay filters
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
         base.OnPrefabInit();

         var baseMenu = ToolMenu.Instance.toolParameterMenu;
         var filterMenuPrefab = baseMenu.content;
         var oldestParent = filterMenuPrefab.GetParent();

         //------------------Creating chain tool menu------------------DOWN
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
         //------------------Setting Chain Tools menu------------------DOWN
         PPanel chainToolsMenu = new PPanel("ChainToolsMenu") {
            Alignment = TextAnchor.MiddleCenter,
            Direction = PanelDirection.Vertical,
            FlexSize = Vector2.zero,
         };

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
            Text = MYSTRINGS.UI.CHAINTOOLSMENU.TITLE,
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
         createButton.Text = MYSTRINGS.UI.CHAINTOOLSMENU.CREATEBUTTON;
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
         deleteButton.Text = MYSTRINGS.UI.CHAINTOOLSMENU.DELETEBUTTON;
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
               var createChain = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.createPanel, true);
               ConfigureToggle(createChain, ChainToolMode.CREATE_CHAIN, MYSTRINGS.UI.CHAINTOOLSMENU.CREATECHAIN, MYSTRINGS.UI.CHAINTOOLSMENU.CREATECHAIN_TOOLTIP, true, () => {
                  //onClick:
                  if(modeToggles[ChainToolMode.CREATE_CHAIN].CurrentState != 0)
                     return;

                  modeToggles[ChainToolMode.CREATE_CHAIN].ChangeState(1);
                  modeToggles[ChainToolMode.CREATE_LINK].ChangeState(0);

                  Main.chainTool.SetToolMode(ChainToolMode.CREATE_CHAIN);
               });

               var createLink = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.createPanel, true);
               ConfigureToggle(createLink, ChainToolMode.CREATE_LINK, MYSTRINGS.UI.CHAINTOOLSMENU.CREATELINK, MYSTRINGS.UI.CHAINTOOLSMENU.CREATELINK_TOOLTIP, false, () => {
                  //onClick:
                  if(modeToggles[ChainToolMode.CREATE_LINK].CurrentState != 0)
                     return;

                  modeToggles[ChainToolMode.CREATE_LINK].ChangeState(1);
                  modeToggles[ChainToolMode.CREATE_CHAIN].ChangeState(0);

                  Main.chainTool.SetToolMode(ChainToolMode.CREATE_LINK);
               });
            };
            Prefabs.RunAfterPrefabsInit(addCreateToggles, nameof(Prefabs.FilterToggleReversedPrefab));
         });

         //PPanel createChain = new PPanel("CreateChain") {
         //   Alignment = TextAnchor.MiddleLeft,
         //   Direction = PanelDirection.Horizontal,
         //   FlexSize = Vector2.right,
         //   Spacing = 5
         //};
         //PCheckBox createChainToggle = new PCheckBox("CreateChainToggle");
         //createChainToggle.InitialState = PCheckBox.STATE_PARTIAL;
         //createChainToggle.CheckColor = PUITuning.Colors.ComponentDarkStyle;
         //createChainToggle.AddOnRealize(toggle => {
         //   var checkboxBorder = toggle.transform.GetChildSafe(0)?.GetChildSafe(0)?.GetComponent<Image>();
         //   if(checkboxBorder != null)
         //   {
               
         //   }
         //});
         //PLabel createChainLabel = new PLabel("CreateChainLabel");
         //createChainLabel.Text = MYSTRINGS.UI.CHAINTOOLSMENU.CREATECHAIN;
         //createChainLabel.ToolTip = MYSTRINGS.UI.CHAINTOOLSMENU.CREATECHAIN_TOOLTIP;
         //createChainLabel.FlexSize = Vector2.right;
         //createChainLabel.TextAlignment = TextAnchor.MiddleLeft;
         //createChainLabel.TextStyle = PUITuning.Fonts.TextLightStyle;

         //createChain.AddChild(createChainToggle).AddChild(createChainLabel);

         //PPanel createLink = new PPanel("CreateLink") {
         //   Alignment = TextAnchor.MiddleLeft,
         //   Direction = PanelDirection.Horizontal,
         //   FlexSize = Vector2.right,
         //   Spacing = 5
         //};
         //PCheckBox createLinkToggle = new PCheckBox("CreateLinkToggle");
         //createLinkToggle.InitialState = PCheckBox.STATE_UNCHECKED;
         //PLabel createLinkLabel = new PLabel("CreateLinkLabel");
         //createLinkLabel.Text = MYSTRINGS.UI.CHAINTOOLSMENU.CREATELINK;
         //createLinkLabel.ToolTip = MYSTRINGS.UI.CHAINTOOLSMENU.CREATELINK_TOOLTIP;
         //createLinkLabel.FlexSize = Vector2.right;
         //createLinkLabel.TextAlignment = TextAnchor.MiddleLeft;
         //createLinkLabel.TextStyle = PUITuning.Fonts.TextLightStyle;

         //createLink.AddChild(createLinkToggle).AddChild(createLinkLabel);

         //createPanel.AddChild(createChain).AddChild(createLink);
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
               var deleteChain = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.deletePanel, true);
               ConfigureToggle(deleteChain, ChainToolMode.DELETE_CHAIN, MYSTRINGS.UI.CHAINTOOLSMENU.DELETECHAIN, MYSTRINGS.UI.CHAINTOOLSMENU.DELETECHAIN_TOOLTIP, false, () => {
                  //onClick:
                  if(modeToggles[ChainToolMode.DELETE_CHAIN].CurrentState != 0)
                     return;

                  modeToggles[ChainToolMode.DELETE_CHAIN].ChangeState(1);
                  modeToggles[ChainToolMode.DELETE_LINK].ChangeState(0);

                  Main.chainTool.SetToolMode(ChainToolMode.DELETE_CHAIN);
               });

               var deleteLink = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.deletePanel, true);
               ConfigureToggle(deleteLink, ChainToolMode.DELETE_LINK, MYSTRINGS.UI.CHAINTOOLSMENU.DELETELINK, MYSTRINGS.UI.CHAINTOOLSMENU.DELETELINK_TOOLTIP, false, () => {
                  //onClick:
                  if(modeToggles[ChainToolMode.DELETE_LINK].CurrentState != 0)
                     return;

                  modeToggles[ChainToolMode.DELETE_LINK].ChangeState(1);
                  modeToggles[ChainToolMode.DELETE_CHAIN].ChangeState(0);

                  Main.chainTool.SetToolMode(ChainToolMode.DELETE_LINK);
               });
            };
            Prefabs.RunAfterPrefabsInit(addDeleteToggles, nameof(Prefabs.FilterToggleReversedPrefab));
         });
         //------------------Setting delete panel------------------UP
         toolsPanel.AddChild(createOrDelete).AddChild(createPanel).AddChild(deletePanel);

         chainToolsMenu.AddChild(title).AddChild(toolsPanel).AddTo(horizontal);
         //------------------Setting Chain Tools menu------------------UP
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
         //------------------Creating chain tool menu------------------UP
      }
      /// <summary>
      /// Fixes up the toggle to look nice in the Chain Tools menu.
      /// </summary>
      /// <param name="toggle">The toggle's GameObject</param>
      /// <param name="mode">ChainToolMode associated with this toggle</param>
      /// <param name="text">The text to show next to the toggle</param>
      /// <param name="tooltip">The tooltip to show when hovering over the text</param>
      private void ConfigureToggle(GameObject toggle, ChainToolMode mode, string text, string tooltip, bool initialState, System.Action onClick) {
         var createLinkLabel = toggle.GetComponentInChildren<LocText>();
         createLinkLabel.text = text;
         createLinkLabel.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
         createLinkLabel.gameObject.AddSimpleTooltip(tooltip);
         var layoutElem = createLinkLabel.GetComponent<LayoutElement>();
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
      /// Populates the menu with the available destroy modes.
      /// </summary>
      internal void PopulateMenu() {
         int i = 0;
         var prefab = ToolMenu.Instance.toolParameterMenu.widgetPrefab;
         ClearMenu();
         foreach(var filter in ChainToolFilters.allFilters)
         {
            // Create prefab based on existing Klei menu
            var widgetPrefab = Util.KInstantiateUI(prefab, choiceList, true);
            PUIElements.SetText(widgetPrefab, filter.Name);
            var toggle = widgetPrefab.GetComponentInChildren<MultiToggle>();
            if(toggle != null)
            {
               var checkbox = toggle.gameObject;
               // Set initial state, note that ChangeState is only called by SetCheckState
               // if it appears to be different, but since this executes before the
               // parent is active it must be set to something different
               var option = new ChainToolMenuOption(filter, checkbox);
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

      public ToolParameterMenu.ToggleState GetToggleState(ChainToolFilters.Filter filter) {
         if(!options.TryGetValue(filter, out ChainToolMenuOption option))
            return ToolParameterMenu.ToggleState.Off;

         return option.State;
      }

      /// <summary>
      /// Shows the menu.
      /// </summary>
      public void ShowMenu() {
         content.SetActive(true);
         OnChange();
      }

      /// <summary>
      /// Stores available settings change tools and their current menu states.
      /// </summary>
      private sealed class ChainToolMenuOption {
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
         public ChainToolFilters.Filter Filter { get; }

         public ChainToolMenuOption(ChainToolFilters.Filter filter, GameObject checkbox) {
            Checkbox = checkbox ?? throw new ArgumentNullException(nameof(checkbox));
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            State = ToolParameterMenu.ToggleState.Off;
         }
      }
   }
}
