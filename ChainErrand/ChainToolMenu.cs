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
using ChainErrand.Strings;
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
      private readonly IDictionary<ChainToolFilter, ChainToolMenuOption> options;


      private GameObject toolsPanel;
      private GameObject createButton;
      private GameObject deleteButton;

      private GameObject createPanel;
      private GameObject numberSelectorPanel;
      private GameObject chainNumberLabel;
      private GameObject chainNumDecrease;
      private GameObject chainNumIncrease;
      private GameObject linkNumberLabel;
      private GameObject linkNumDecrease;
      private GameObject linkNumIncrease;

      private TMP_InputField chainNumberField;
      private TMP_InputField linkNumberField;

      private GameObject deletePanel;

      public Dictionary<ChainToolMode, MultiToggle> modeToggles = new Dictionary<ChainToolMode, MultiToggle>(Enum.GetNames(typeof(ChainToolMode)).Length);


      public ChainToolMenu() {
         options = new Dictionary<ChainToolFilter, ChainToolMenuOption>();
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
            if(Main.chainTool.GetToolMode() == ChainToolMode.DELETE_CHAIN)
            {
               Main.chainTool.SetToolMode(ChainToolMode.CREATE_CHAIN);
            }
            else if(Main.chainTool.GetToolMode() == ChainToolMode.DELETE_LINK)
            {
               Main.chainTool.SetToolMode(ChainToolMode.CREATE_LINK);
            }
            // else no actions required
         }
         else
         {
            if(Main.chainTool.GetToolMode() == ChainToolMode.CREATE_CHAIN)
            {
               Main.chainTool.SetToolMode(ChainToolMode.DELETE_CHAIN);
            }
            else if(Main.chainTool.GetToolMode() == ChainToolMode.CREATE_LINK)
            {
               Main.chainTool.SetToolMode(ChainToolMode.DELETE_LINK);
            }
            // else no actions required
         }

         UpdateToolModeSelectionDisplay();
      }

      /// <summary>
      /// Updates the visual elements of the menu that represent the selected chain tool mode.
      /// </summary>
      public void UpdateToolModeSelectionDisplay() {
         if(this.content == null)
            return;

         ChainToolMode currentMode = Main.chainTool.GetToolMode();

         bool createPanelActive = currentMode == ChainToolMode.CREATE_CHAIN || currentMode == ChainToolMode.CREATE_LINK;
         PButton.SetButtonEnabled(createButton, !createPanelActive);
         PButton.SetButtonEnabled(deleteButton, createPanelActive);
         createPanel.SetActive(createPanelActive);
         deletePanel.SetActive(!createPanelActive);

         foreach(var toggleMode in modeToggles.Keys)
         {
            modeToggles[toggleMode].ChangeState(toggleMode == currentMode ? 1 : 0);

            if(toggleMode == ChainToolMode.CREATE_LINK)
            {
               this.numberSelectorPanel.SetActive(toggleMode == currentMode);
            }
         }
      }

      /// <summary>
      /// Updates the visual elements of the menu that represent the selected chain ID and link number.
      /// </summary>
      public void UpdateNumberSelectionDisplay() {
         if(this.content == null)
            return;

         if(ChainsContainer.ChainsCount > 1)
         {
            SetArrowButtonEnabled(chainNumDecrease, true);
            SetArrowButtonEnabled(chainNumIncrease, true);
         }
         else// there are no other chains to switch to
         {
            SetArrowButtonEnabled(chainNumDecrease, false);
            SetArrowButtonEnabled(chainNumIncrease, false);
         }
         if(ChainsContainer.ChainsCount > 0)
         {
            SetTextFieldText(chainNumberField, Main.chainTool.GetSelectedChain().ToString());
         }
         else
         {
            SetTextFieldText(chainNumberField, MYSTRINGS.UI.CHAINTOOLSMENU.CHAINNUMBER_NOTFOUND);
         }

         if(ChainsContainer.TryGetChain(Main.chainTool.GetSelectedChain(), out Chain chain))
         {
            SetArrowButtonEnabled(linkNumDecrease, Main.chainTool.GetSelectedLink() > 0 || !Main.chainTool.GetInsertNewLink());// true if previous link(s) exist
            SetArrowButtonEnabled(linkNumIncrease, Main.chainTool.GetSelectedLink() <= chain.LastLinkNumber());// true if next link(s) exist

            if(Main.chainTool.GetInsertNewLink() || Main.chainTool.GetSelectedLink() > chain.LastLinkNumber())
            {
               if(Main.chainTool.GetSelectedLink() > chain.LastLinkNumber())
               {
                  // appending link at the end of the chain won't show a fraction (3.5) but a whole number (4th)
                  SetTextFieldText(linkNumberField, (Main.chainTool.GetSelectedLink() + 1/*0th link -> 1st link*/).ToString() + Utils.GetPostfixForLinkNumber(Main.chainTool.GetSelectedLink(), true));
               }
               else
               {
                  SetTextFieldText(linkNumberField, (Main.chainTool.GetSelectedLink()/*0th link -> 0.5 (because the link should be inserted before the selected link)*/).ToString() + ".5");
               }
            }
            else
            {
               SetTextFieldText(linkNumberField, (Main.chainTool.GetSelectedLink() + 1/*0th link -> 1st link*/).ToString() + Utils.GetPostfixForLinkNumber(Main.chainTool.GetSelectedLink(), true));
            }
         }
         else
         {
            SetArrowButtonEnabled(linkNumDecrease, false);
            SetArrowButtonEnabled(linkNumIncrease, false);

            SetTextFieldText(linkNumberField, MYSTRINGS.UI.CHAINTOOLSMENU.LINKNUMBER_NOTFOUND);
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

         if(Main.chainOverlay != null && Main.chainOverlay.IsEnabled)
         {
            Main.chainOverlay.UpdateOverlay();
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
         chainToolsMenu.AddOnRealize(menu => {
            UpdateNumberSelectionDisplay();
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
         toolsPanel.AddOnRealize(go => {
            this.toolsPanel = go;

            // adding tooltips should be done here because the GameObject of toolsPanel is realized after the labels' GameObjects
            this.chainNumberLabel.AddFilterMenuToolTip(MYSTRINGS.UI.CHAINTOOLSMENU.CHAINNUMBER_TOOLTIP, this.toolsPanel.rectTransform());
            this.linkNumberLabel.AddFilterMenuToolTip(MYSTRINGS.UI.CHAINTOOLSMENU.LINKNUMBER_TOOLTIP, this.toolsPanel.rectTransform());
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
                  Main.chainTool.SetToolMode(ChainToolMode.CREATE_CHAIN);
                  UpdateToolModeSelectionDisplay();
               });

               var createLink = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.createPanel, true);
               ConfigureToggle(createLink, ChainToolMode.CREATE_LINK, MYSTRINGS.UI.CHAINTOOLSMENU.CREATELINK, MYSTRINGS.UI.CHAINTOOLSMENU.CREATELINK_TOOLTIP, false, () => {
                  //onClick:
                  Main.chainTool.SetToolMode(ChainToolMode.CREATE_LINK);
                  UpdateToolModeSelectionDisplay();
               });

               this.numberSelectorPanel.transform.SetAsLastSibling();// this panel should be below the toggles
            };
            Prefabs.RunAfterPrefabsInit(addCreateToggles, nameof(Prefabs.FilterToggleReversedPrefab));
         });

         PPanel numberSelectorPanel = new PPanel("ChainAndLinkNumbers") {
            Alignment = TextAnchor.MiddleLeft,
            Direction = PanelDirection.Vertical,
            FlexSize = new Vector2(1f, 1f),
            Spacing = 3,
            Margin = new RectOffset(0, 0, 5, 1),
         };
         numberSelectorPanel.AddOnRealize(go => {
            this.numberSelectorPanel = go;
            this.numberSelectorPanel.SetActive(false);
         });

         PLabel chainNumberLabel = new PLabel("ChainNumberLabel") {
            TextAlignment = TextAnchor.MiddleLeft,
            TextStyle = PUITuning.Fonts.TextLightStyle,
            Text = MYSTRINGS.UI.CHAINTOOLSMENU.CHAINNUMBER,
         };
         chainNumberLabel.AddOnRealize(label => this.chainNumberLabel = label);

         PPanel chainNumberInput = new PPanel("ChainNumberInput") {
            Alignment = TextAnchor.MiddleCenter,
            Direction = PanelDirection.Horizontal,
            FlexSize = new Vector2(1f, 1f),
            Spacing = 8,
         };
         chainNumberInput.AddOnRealize(panel => {
            chainNumDecrease = Util.KInstantiateUI(Prefabs.ArrowLeftButtonPrefab, panel);
            chainNumDecrease.name = "ChainDecrease";
            ConfigureArrowButton(chainNumDecrease, true, () => {
               Main.chainTool.SetSelectedChain(Main.chainTool.GetSelectedChain() - 1);
               UpdateNumberSelectionDisplay();
            });

            chainNumIncrease = Util.KInstantiateUI(Prefabs.ArrowRightButtonPrefab, panel);
            chainNumIncrease.name = "ChainIncrease";
            ConfigureArrowButton(chainNumIncrease, false, () => {
               Main.chainTool.SetSelectedChain(Main.chainTool.GetSelectedChain() + 1);
               UpdateNumberSelectionDisplay();
            });
         });

         PTextField chainField = new PTextField("ChainField") {
            Type = PTextField.FieldType.Integer,
         };
         chainField.AddOnRealize(field => {
            chainNumberField = field.GetComponent<TMP_InputField>();

            var layoutElem = field.AddOrGet<LayoutElement>();
            layoutElem.minHeight = 24f;
            layoutElem.minWidth = 100f;
         });
         chainField.OnTextChanged = (GameObject textField, string text) => {
            int chainNum = ChainToolUtils.InterpretChainNumber(text);
            Main.chainTool.SetSelectedChain(chainNum);
            UpdateNumberSelectionDisplay();
         };

         chainNumberInput.AddChild(chainField);

         PLabel linkNumberLabel = new PLabel("LinkNumberLabel") {
            TextAlignment = TextAnchor.MiddleLeft,
            TextStyle = PUITuning.Fonts.TextLightStyle,
            Text = MYSTRINGS.UI.CHAINTOOLSMENU.LINKNUMBER,
         };
         linkNumberLabel.AddOnRealize(label => this.linkNumberLabel = label);

         PPanel linkNumberInput = new PPanel("LinkNumberInput") {
            Alignment = TextAnchor.MiddleCenter,
            Direction = PanelDirection.Horizontal,
            FlexSize = new Vector2(1f, 1f),
            Spacing = 8,
         };
         linkNumberInput.AddOnRealize(panel => {
            linkNumDecrease = Util.KInstantiateUI(Prefabs.ArrowLeftButtonPrefab, panel);
            linkNumDecrease.name = "LinkDecrease";
            ConfigureArrowButton(linkNumDecrease, true, () => {
               if(Main.chainTool.GetInsertNewLink())
               {
                  Main.chainTool.SetSelectedLink(Main.chainTool.GetSelectedLink() - 1, false);
               }
               else
               {
                  Main.chainTool.SetSelectedLink(Main.chainTool.GetSelectedLink(), true);
               }
               UpdateNumberSelectionDisplay();
            });

            linkNumIncrease = Util.KInstantiateUI(Prefabs.ArrowRightButtonPrefab, panel);
            linkNumIncrease.name = "LinkIncrease";
            ConfigureArrowButton(linkNumIncrease, false, () => {
               if(Main.chainTool.GetInsertNewLink())
               {
                  Main.chainTool.SetSelectedLink(Main.chainTool.GetSelectedLink(), false);
               }
               else
               {
                  Main.chainTool.SetSelectedLink(Main.chainTool.GetSelectedLink() + 1, true);
               }
               UpdateNumberSelectionDisplay();
            });
         });

         PTextField linkField = new PTextField("LinkField") {
            Type = PTextField.FieldType.Float
         };
         linkField.AddOnRealize(field => {
            linkNumberField = field.GetComponent<TMP_InputField>();

            var layoutElem = field.AddOrGet<LayoutElement>();
            layoutElem.minHeight = 24f;
            layoutElem.minWidth = 100f;
         });
         linkField.OnTextChanged = (GameObject textField, string text) => {
            var linkNum = ChainToolUtils.InterpretLinkNumber(text);
            Main.chainTool.SetSelectedLink(linkNum.linkNumber, linkNum.insertNewLink);
            UpdateNumberSelectionDisplay();
         };

         linkNumberInput.AddChild(linkField);

         numberSelectorPanel.AddChild(chainNumberLabel).AddChild(chainNumberInput).AddChild(linkNumberLabel).AddChild(linkNumberInput);
         createPanel.AddChild(numberSelectorPanel);

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
                  Main.chainTool.SetToolMode(ChainToolMode.DELETE_CHAIN);
                  UpdateToolModeSelectionDisplay();
               });

               var deleteLink = Util.KInstantiateUI(Prefabs.FilterToggleReversedPrefab, this.deletePanel, true);
               ConfigureToggle(deleteLink, ChainToolMode.DELETE_LINK, MYSTRINGS.UI.CHAINTOOLSMENU.DELETELINK, MYSTRINGS.UI.CHAINTOOLSMENU.DELETELINK_TOOLTIP, false, () => {
                  //onClick:
                  Main.chainTool.SetToolMode(ChainToolMode.DELETE_LINK);
                  UpdateToolModeSelectionDisplay();
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
      /// <param name="initialState">The starting state: 1 is on, 0 is off</param>
      /// <param name="onClick">The action that should be done onClick</param>
      private void ConfigureToggle(GameObject toggle, ChainToolMode mode, string text, string tooltip, bool initialState, System.Action onClick) {
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
      /// Populates the menu with the available destroy modes.
      /// </summary>
      internal void PopulateMenu() {
         int i = 0;
         var prefab = ToolMenu.Instance.toolParameterMenu.widgetPrefab;
         ClearMenu();
         foreach(ChainToolFilter filter in Enum.GetValues(typeof(ChainToolFilter)))
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

      public ToolParameterMenu.ToggleState GetToggleState(ChainToolFilter filter) {
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
         UpdateNumberSelectionDisplay();
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
         public ChainToolFilter Filter { get; }

         public ChainToolMenuOption(ChainToolFilter filter, GameObject checkbox) {
            Checkbox = checkbox ?? throw new ArgumentNullException(nameof(checkbox));
            Filter = filter;
            State = ToolParameterMenu.ToggleState.Off;
         }
      }
   }
}
