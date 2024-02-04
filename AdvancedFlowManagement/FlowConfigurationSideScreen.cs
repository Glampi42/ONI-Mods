using AdvancedFlowManagement;
using AdvancedFlowManagement.Patches;
using PeterHan.PLib.Detours;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedFlowManagement {
   class FlowConfigurationSideScreen : SideScreenContent {
      private static readonly UnityEngine.Color klei_black = new UnityEngine.Color(0.24f, 0.24f, 0.28f, 1f);
      private static readonly StatePresentationSetting sps = new StatePresentationSetting() {
         color = klei_black,
         use_color_on_hover = false,
         image_target = null,
         name = "Priority1"
      };

      private static readonly ToggleState[] priorityStates = new ToggleState[] {
            // Priority 0
            new ToggleState()
            {
                color = klei_black,
                sprite = Assets.GetSprite("icon_priority_down"),
                use_color_on_hover = false,
                additional_display_settings = new StatePresentationSetting[] { sps }
            },
            // Priority 1
            new ToggleState()
            {
                color = klei_black,
                sprite = Assets.GetSprite("icon_priority_flat"),
                use_color_on_hover = false,
                additional_display_settings = new StatePresentationSetting[] { sps }
            },
            // Priority 2
            new ToggleState()
            {
                color = klei_black,
                sprite = Assets.GetSprite("icon_priority_up"),
                use_color_on_hover = false,
                additional_display_settings = new StatePresentationSetting[] { sps }
            },
            // Priority 3
            new ToggleState()
            {
                color = klei_black,
                sprite = Assets.GetSprite("icon_priority_up_2"),
                use_color_on_hover = false,
                additional_display_settings = new StatePresentationSetting[] { sps }
            }
        };

      private static readonly ColorStyleSetting directionsColor;
      private static readonly ColorStyleSetting directionsDisabledColor;
      private static readonly ColorStyleSetting directionsIllegalColor;
      private static readonly ColorStyleSetting directionsIllegalDisabledColor;
      private static Color transparentize(Color color) {
         color.a = 0.9f;
         return color;
      }

      static FlowConfigurationSideScreen() {
         directionsColor = (ColorStyleSetting)ScriptableObject.CreateInstance(typeof(ColorStyleSetting));
         directionsColor.activeColor = transparentize(CrossingSprite.normalColor);
         directionsColor.inactiveColor = transparentize(CrossingSprite.normalColor);
         directionsColor.hoverColor = transparentize(CrossingSprite.normalColor);

         directionsDisabledColor = (ColorStyleSetting)ScriptableObject.CreateInstance(typeof(ColorStyleSetting));
         directionsDisabledColor.activeColor = transparentize(CrossingSprite.dimColor);
         directionsDisabledColor.inactiveColor = transparentize(CrossingSprite.dimColor);
         directionsDisabledColor.hoverColor = transparentize(CrossingSprite.dimColor);

         directionsIllegalColor = (ColorStyleSetting)ScriptableObject.CreateInstance(typeof(ColorStyleSetting));
         directionsIllegalColor.activeColor = transparentize(CrossingSprite.highlightedColor);
         directionsIllegalColor.inactiveColor = transparentize(CrossingSprite.highlightedColor);
         directionsIllegalColor.hoverColor = transparentize(CrossingSprite.highlightedColor);

         directionsIllegalDisabledColor = (ColorStyleSetting)ScriptableObject.CreateInstance(typeof(ColorStyleSetting));
         directionsIllegalDisabledColor.activeColor = transparentize(CrossingSprite.dimHighlightedColor);
         directionsIllegalDisabledColor.inactiveColor = transparentize(CrossingSprite.dimHighlightedColor);
         directionsIllegalDisabledColor.hoverColor = transparentize(CrossingSprite.dimHighlightedColor);
      }

      public static FlowConfigurationSideScreen Instance;

      private CrossingCmp crossingCmp;
      private byte crossing_rotation;

      public GameObject directionButton;
      public GameObject directionPanel;
      public (GameObject, ImageToggleState, KToggle)[] directionToggles_DLRU = new (GameObject, ImageToggleState, KToggle)[4];
      public GameObject[] directionSpacers_LR = new GameObject[2];
      public ImageToggleState crossingCircleIcon;

      public GameObject priorityButton;
      public GameObject priorityPanel;
      public GameObject inputsButton;
      public GameObject outputsButton;
      public GameObject bothButton;
      private bool showInputsPri;
      private bool showOutputsPri;
      public GameObject[] priorityToggles_DLRU_unrotated = new GameObject[5];
      public GameObject[] prioritySpacers_DLRU_unrotated = new GameObject[5];
      public Image togglesPanelImage;

      private bool isUpdating = false;

      public override void OnPrefabInit() {
         RectOffset rectOffset = new RectOffset(0, 0, 4, 4);
         BoxLayoutGroup component = this.gameObject.GetComponent<BoxLayoutGroup>();
         if(component != null)
            component.Params = new BoxLayoutParams() {
               Alignment = TextAnchor.MiddleCenter,
               Margin = rectOffset
            };
         PPanel mainpanel = new PPanel("MainPanel") {
            Alignment = TextAnchor.MiddleCenter,
            Direction = PanelDirection.Vertical,
            Margin = rectOffset,
            Spacing = 15,
            FlexSize = Vector2.right
         };

         PRelativePanel dirorpri = new PRelativePanel("DirectionOrPriority") {
            FlexSize = Vector2.right
         };
         PButton direction = new PButton("DirectionButton");
         direction.FlexSize = Vector2.right;
         direction.SetKleiPinkStyle();
         direction.Color.disabledColor = PUITuning.Colors.ButtonPinkStyle.activeColor;
         direction.Color.disabledhoverColor = PUITuning.Colors.ButtonPinkStyle.hoverColor;
         direction.Margin = new RectOffset(8, 8, 8, 8);
         direction.Text = MYSTRINGS.SIDESCREEN.DIRECTIONSCREEN.DIRECTION;
         direction.AddOnRealize(go => this.directionButton = go);
         direction.AddOnRealize(realized => PButton.SetButtonEnabled(realized, false));
         direction.OnClick = go => SwitchFlowConfigCategory(true);
         PButton priority = new PButton("PriorityButton");
         priority.FlexSize = Vector2.right;
         priority.SetKleiPinkStyle();
         priority.Color.disabledColor = PUITuning.Colors.ButtonPinkStyle.activeColor;
         priority.Color.disabledhoverColor = PUITuning.Colors.ButtonPinkStyle.hoverColor;
         priority.Margin = new RectOffset(8, 8, 8, 8);
         priority.Text = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.PRIORITY;
         priority.AddOnRealize(go => this.priorityButton = go);
         priority.OnClick = go => SwitchFlowConfigCategory(false);

         dirorpri.AddChild(new PSpacer()).AddChild(direction).AddChild(new PSpacer()).AddChild(priority).AddChild(new PSpacer());
         dirorpri.SetLeftEdge(direction, 0.127322f).SetRightEdge(direction, 0.436339f).SetLeftEdge(priority, 0.563661f).SetRightEdge(priority, 0.872678f);

         //------------------Setting direction panel------------------DOWN
         PPanel dirPanel = new PPanel("DirectionPanel") {
            Alignment = TextAnchor.UpperCenter,
            Direction = PanelDirection.Vertical,
            FlexSize = Vector2.up,
            Spacing = 1
         };
         dirPanel.AddOnRealize(go => this.directionPanel = go);
         PToggle downDir = new PToggle {
            ToolTip = MYSTRINGS.SIDESCREEN.DIRECTIONSCREEN.SWITCHDIRECTION,
            InactiveSprite = MYSPRITES.GetSprite("afm_crossingInput_D_ui"),
            ActiveSprite = MYSPRITES.GetSprite("afm_crossingOutput_D_ui"),
            Color = directionsColor,
            Size = new Vector2(40f, 40f),
            OnStateChanged = (go, state) => TrySwitchFlowDirection(go, state)
         };
         downDir.AddOnRealize(go => directionToggles_DLRU[0] = (go, go.GetComponent<ImageToggleState>(), go.GetComponent<KToggle>()));
         PToggle leftDir = new PToggle {
            ToolTip = MYSTRINGS.SIDESCREEN.DIRECTIONSCREEN.SWITCHDIRECTION,
            InactiveSprite = MYSPRITES.GetSprite("afm_crossingInput_L_ui"),
            ActiveSprite = MYSPRITES.GetSprite("afm_crossingOutput_L_ui"),
            Color = directionsColor,
            Size = new Vector2(40f, 40f),
            OnStateChanged = (go, state) => TrySwitchFlowDirection(go, state)
         };
         leftDir.AddOnRealize(go => directionToggles_DLRU[1] = (go, go.GetComponent<ImageToggleState>(), go.GetComponent<KToggle>()));
         PToggle upDir = new PToggle {
            ToolTip = MYSTRINGS.SIDESCREEN.DIRECTIONSCREEN.SWITCHDIRECTION,
            InactiveSprite = MYSPRITES.GetSprite("afm_crossingInput_U_ui"),
            ActiveSprite = MYSPRITES.GetSprite("afm_crossingOutput_U_ui"),
            Color = directionsColor,
            Size = new Vector2(40f, 40f),
            OnStateChanged = (go, state) => TrySwitchFlowDirection(go, state)
         };
         upDir.AddOnRealize(go => directionToggles_DLRU[3] = (go, go.GetComponent<ImageToggleState>(), go.GetComponent<KToggle>()));
         PToggle rightDir = new PToggle {
            ToolTip = MYSTRINGS.SIDESCREEN.DIRECTIONSCREEN.SWITCHDIRECTION,
            InactiveSprite = MYSPRITES.GetSprite("afm_crossingInput_R_ui"),
            ActiveSprite = MYSPRITES.GetSprite("afm_crossingOutput_R_ui"),
            Color = directionsColor,
            Size = new Vector2(40f, 40f),
            OnStateChanged = (go, state) => TrySwitchFlowDirection(go, state)
         };
         rightDir.AddOnRealize(go => directionToggles_DLRU[2] = (go, go.GetComponent<ImageToggleState>(), go.GetComponent<KToggle>()));
         PToggle crossingCircleIcon = new PToggle() {
            InactiveSprite = MYSPRITES.GetSprite("afm_crossing_ui"),
            Color = directionsColor,
            Size = new Vector2(88f, 88f)
         };
         crossingCircleIcon.AddOnRealize(go => { this.crossingCircleIcon = go.GetComponent<ImageToggleState>(); go.GetComponent<KToggle>().interactable = false; });
         PSpacer leftSpacer = new PSpacer() { PreferredSize = new Vector2(40f, 40f), FlexSize = Vector2.zero };
         PSpacer rightSpacer = new PSpacer() { PreferredSize = new Vector2(40f, 40f), FlexSize = Vector2.zero };
         leftSpacer.OnRealize += go => (directionSpacers_LR[0] = go).SetActive(false);
         rightSpacer.OnRealize += go => (directionSpacers_LR[1] = go).SetActive(false);

         dirPanel.AddChild(upDir).AddChild(new PPanel("Middle") { Alignment = TextAnchor.MiddleCenter, Direction = PanelDirection.Horizontal, Spacing = 1 }
             .AddChild(leftSpacer).AddChild(leftDir).AddChild(crossingCircleIcon).AddChild(rightDir).AddChild(rightSpacer)).AddChild(downDir);
         //------------------Setting direction panel------------------UP
         //------------------Setting priority panel------------------DOWN
         PPanel priPanel = new PPanel("PriorityPanel") {
            Alignment = TextAnchor.UpperCenter,
            Direction = PanelDirection.Vertical,
            FlexSize = Vector2.up,
            Spacing = 0
         };
         priPanel.AddOnRealize(go => this.priorityPanel = go);
         priPanel.AddOnRealize(panel => panel.SetActive(false));

         PLabel priorityLabel = new PLabel();
         priorityLabel.Text = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.SHOWPRIORITIES;
         priorityLabel.TextStyle = PUITuning.Fonts.TextDarkStyle;

         PRelativePanel inputsoroutputs = new PRelativePanel("InputsOrOutputs") {
            FlexSize = Vector2.right
         };
         PButton inputs = new PButton("InputsButton");
         inputs.FlexSize = Vector2.right;
         inputs.SetKleiBlueStyle();
         inputs.Color.disabledColor = PUITuning.Colors.ButtonBlueStyle.activeColor;
         inputs.Color.disabledhoverColor = PUITuning.Colors.ButtonBlueStyle.hoverColor;
         inputs.Margin = new RectOffset(4, 4, 6, 6);
         inputs.Text = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.INPUTS;
         inputs.AddOnRealize(go => this.inputsButton = go);
         inputs.OnClick = go => SwitchPriorityVisibility(true, false);
         PButton outputs = new PButton("OutputsButton");
         outputs.FlexSize = Vector2.right;
         outputs.SetKleiBlueStyle();
         outputs.Color.disabledColor = PUITuning.Colors.ButtonBlueStyle.activeColor;
         outputs.Color.disabledhoverColor = PUITuning.Colors.ButtonBlueStyle.hoverColor;
         outputs.Margin = new RectOffset(4, 4, 6, 6);
         outputs.Text = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.OUTPUTS;
         outputs.AddOnRealize(go => this.outputsButton = go);
         outputs.OnClick = go => SwitchPriorityVisibility(false, true);
         PButton both = new PButton("BothButton");
         both.SetKleiBlueStyle();
         both.Color.disabledColor = PUITuning.Colors.ButtonBlueStyle.activeColor;
         both.Color.disabledhoverColor = PUITuning.Colors.ButtonBlueStyle.hoverColor;
         both.Margin = new RectOffset(6, 6, 6, 6);
         both.Text = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.BOTH;
         both.AddOnRealize(go => this.bothButton = go);
         both.AddOnRealize(realized => { PButton.SetButtonEnabled(realized, false); showInputsPri = showOutputsPri = true; });
         both.OnClick = go => SwitchPriorityVisibility(true, true);

         inputsoroutputs.AddChild(new PSpacer()).AddChild(inputs).AddChild(new PSpacer()).AddChild(both).AddChild(new PSpacer()).AddChild(outputs).AddChild(new PSpacer());
         inputsoroutputs.SetLeftEdge(inputs, 0.1f).SetRightEdge(inputs, 0.365573f).SetLeftEdge(both, 0.405573f).SetRightEdge(both, 0.594427f)
            .SetLeftEdge(outputs, 0.634427f).SetRightEdge(outputs, 0.9f);

         PPanel togglesPanel = new PPanel() {
            Alignment = TextAnchor.UpperCenter,
            Direction = PanelDirection.Vertical,
            FlexSize = Vector2.up,
            Spacing = 69
         };
         togglesPanel.BackImage = crossingCmp.crossingIcon.GetComponentInChildren<Image>().sprite;// first update -> only needed for the sprite to get displayed
         togglesPanel.AddOnRealize(go => togglesPanelImage = go.GetComponent<Image>());

         PCheckBox downPri = new PCheckBox() {
            ToolTip = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.CHANGEPRIORITY,
            OnChecked = (go, state) => TrySwitchFlowPriority(go, state)
         };
         downPri.AddOnRealize(go => SetupPriorityToggle(go, 0));
         PCheckBox leftPri = new PCheckBox() {
            ToolTip = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.CHANGEPRIORITY,
            OnChecked = (go, state) => TrySwitchFlowPriority(go, state)
         };
         leftPri.AddOnRealize(go => SetupPriorityToggle(go, 1));
         PCheckBox upPri = new PCheckBox() {
            ToolTip = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.CHANGEPRIORITY,
            OnChecked = (go, state) => TrySwitchFlowPriority(go, state)
         };
         upPri.AddOnRealize(go => SetupPriorityToggle(go, 3));
         PCheckBox rightPri = new PCheckBox() {
            ToolTip = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.CHANGEPRIORITY,
            OnChecked = (go, state) => TrySwitchFlowPriority(go, state)
         };
         rightPri.AddOnRealize(go => SetupPriorityToggle(go, 2));
         PCheckBox buildingEndpointPri = new PCheckBox() {
            ToolTip = MYSTRINGS.SIDESCREEN.PRIORITYSCREEN.CHANGEPRIORITY,
            OnChecked = (go, state) => TrySwitchFlowPriority(go, state)
         };
         buildingEndpointPri.AddOnRealize(go => SetupPriorityToggle(go, 4));

         PSpacer downPriSpacer = new PSpacer() { PreferredSize = new Vector2(20f, 20f), FlexSize = Vector2.zero };
         PSpacer leftPriSpacer = new PSpacer() { PreferredSize = new Vector2(20f, 20f), FlexSize = Vector2.zero };
         PSpacer upPriSpacer = new PSpacer() { PreferredSize = new Vector2(20f, 20f), FlexSize = Vector2.zero };
         PSpacer rightPriSpacer = new PSpacer() { PreferredSize = new Vector2(20f, 20f), FlexSize = Vector2.zero };
         PSpacer centerPriSpacer = new PSpacer() { PreferredSize = new Vector2(20f, 20f), FlexSize = Vector2.zero };
         downPriSpacer.OnRealize += go => (prioritySpacers_DLRU_unrotated[0] = go).SetActive(false);
         leftPriSpacer.OnRealize += go => (prioritySpacers_DLRU_unrotated[1] = go).SetActive(false);
         upPriSpacer.OnRealize += go => (prioritySpacers_DLRU_unrotated[3] = go).SetActive(false);
         rightPriSpacer.OnRealize += go => (prioritySpacers_DLRU_unrotated[2] = go).SetActive(false);
         centerPriSpacer.OnRealize += go => (prioritySpacers_DLRU_unrotated[4] = go).SetActive(false);

         togglesPanel.AddChild(upPriSpacer).AddChild(upPri).AddChild(new PPanel("Middle") { Alignment = TextAnchor.MiddleCenter, Direction = PanelDirection.Horizontal, Spacing = 69 }
             .AddChild(leftPriSpacer).AddChild(leftPri).AddChild(buildingEndpointPri).AddChild(centerPriSpacer).AddChild(rightPri).AddChild(rightPriSpacer)).AddChild(downPri).AddChild(downPriSpacer);

         priPanel.AddChild(priorityLabel).AddChild(new PSpacer() { FlexSize = Vector2.zero, PreferredSize = new Vector2(1f, 10f) }).AddChild(inputsoroutputs)
             .AddChild(new PSpacer() { FlexSize = Vector2.zero, PreferredSize = new Vector2(1f, 10f) }).AddChild(togglesPanel);
         //------------------Setting priority panel------------------UP

         this.ContentContainer = mainpanel.AddChild(dirorpri).AddChild(dirPanel).AddChild(priPanel).AddTo(this.gameObject);
         base.OnPrefabInit();
         UpdateSideScreen();
         Instance = this;


         void SetupPriorityToggle(GameObject go, int index) {
            MultiToggle toggle = go.GetComponent<MultiToggle>();
            toggle.states = priorityStates;
            priorityToggles_DLRU_unrotated[index] = go;
            if(index == 4)
               PCheckBox.SetCheckState(go, 1);// a fix for an edge-case visual bug
         }

         void SwitchFlowConfigCategory(bool toDirection) {
            if(this.ContentContainer == null)
               return;
            PButton.SetButtonEnabled(directionButton, !toDirection);
            PButton.SetButtonEnabled(priorityButton, toDirection);
            directionPanel.SetActive(toDirection);
            priorityPanel.SetActive(!toDirection);
         }

         void TrySwitchFlowDirection(GameObject toggle_go, bool newState) {
            if(this.ContentContainer == null)
               return;
            if(isUpdating)
               return;// modern problems require modern solutions
            if(directionToggles_DLRU[0].Item1 == null)
               return;
            int pipeDirection = Array.IndexOf(directionToggles_DLRU, directionToggles_DLRU.First(value => toggle_go.Equals(value.Item1)));
            if(pipeDirection == -1)
               return;

            Utils.SwitchFlowDirection(crossingCmp, (ConduitFlow.FlowDirections)(1 << pipeDirection), true);
            Utils.UpdateCrossingDirection(crossingCmp, (sbyte)pipeDirection);
            ConduitFlow conduitFlow = Utils.ConduitTypeToConduitFlow(crossingCmp.conduitType);
            FlowPriorityManagement_Patches.RecalculateUpdateOrder(conduitFlow.GetNetwork(conduitFlow.GetConduit(crossingCmp.crossingCell)), conduitFlow);

            UpdateSideScreen();
         }

         void SwitchPriorityVisibility(bool showInputsPriority, bool showOutputsPriority) {
            if(this.ContentContainer == null)
               return;
            PButton.SetButtonEnabled(inputsButton, !showInputsPriority || showOutputsPriority);
            PButton.SetButtonEnabled(outputsButton, showInputsPriority || !showOutputsPriority);
            PButton.SetButtonEnabled(bothButton, !showInputsPriority || !showOutputsPriority);
            showInputsPri = showInputsPriority;
            showOutputsPri = showOutputsPriority;

            for(sbyte unrotated_i = 0; unrotated_i < 4; unrotated_i++)
            {
               char flowDirection_DLRU = Utils.GetFlowDirection(crossingCmp, unrotated_i);
               int rotatedi = Rotate(unrotated_i, true);
               if((showInputsPri && !(flowDirection_DLRU == '2')) || (showOutputsPri && (flowDirection_DLRU == '2')))
               {
                  if(flowDirection_DLRU != '0')
                  {
                     priorityToggles_DLRU_unrotated[rotatedi].SetActive(true);
                     prioritySpacers_DLRU_unrotated[rotatedi].SetActive(false);
                  }
               }
               else
               {
                  priorityToggles_DLRU_unrotated[rotatedi].SetActive(false);
                  prioritySpacers_DLRU_unrotated[rotatedi].SetActive(true);
               }
            }
            if(Utils.TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type) &&
               ((showInputsPri && !(endpoint_type == Endpoint.Sink)) || (showOutputsPri && (endpoint_type == Endpoint.Sink))))
            {
               priorityToggles_DLRU_unrotated[4].SetActive(true);
               prioritySpacers_DLRU_unrotated[4].SetActive(false);
            }
            else
            {
               priorityToggles_DLRU_unrotated[4].SetActive(false);
               prioritySpacers_DLRU_unrotated[4].SetActive(true);
            }
         }

         void TrySwitchFlowPriority(GameObject toggle_go, int oldState) {
            if(this.ContentContainer == null)
               return;
            if(isUpdating)
               return;// modern problems require modern solutions
            if(priorityToggles_DLRU_unrotated[0] == null)
               return;
            int unrotatedFlowDirection = Array.IndexOf(priorityToggles_DLRU_unrotated, toggle_go);
            if(unrotatedFlowDirection == -1)
               return;

            Utils.StoreFlowPriority(crossingCmp, (sbyte)Rotate(unrotatedFlowDirection, false), (sbyte)(oldState > 2 ? 0 : ++oldState));
            Utils.UpdateShouldManageFlowPriorities(crossingCmp);
            ConduitFlow conduitFlow = Utils.ConduitTypeToConduitFlow(crossingCmp.conduitType);
            FlowPriorityManagement_Patches.RecalculateUpdateOrder(conduitFlow.GetNetwork(conduitFlow.GetConduit(crossingCmp.crossingCell)), conduitFlow);

            UISounds.PlaySound(UISounds.Sound.ClickObject);

            UpdateSideScreen();
         }
      }

      private void UpdateSideScreen() {
         if(this.ContentContainer == null)
            return;// if this method is triggered before PrefabInit, do nothing
         isUpdating = true;
         //-------------Updating direction panel-------------DOWN
         for(sbyte direction = 0; direction < 4; direction++)
         {
            GameObject toggle_go = directionToggles_DLRU[direction].Item1;
            ImageToggleState image = directionToggles_DLRU[direction].Item2;
            KToggle ktoggle = directionToggles_DLRU[direction].Item3;
            char flowDirection_URLD = Utils.GetFlowDirection(crossingCmp, direction);
            if(flowDirection_URLD == '0')
            {
               toggle_go.SetActive(false);
               if(direction == 1)
                  directionSpacers_LR[0].SetActive(true);// left Spacer
               if(direction == 2)
                  directionSpacers_LR[1].SetActive(true);// right Spacer
            }
            else
            {
               toggle_go.SetActive(true);
               if(direction == 1)
                  directionSpacers_LR[0].SetActive(false);// left Spacer
               if(direction == 2)
                  directionSpacers_LR[1].SetActive(false);// right Spacer

               PipeEnding pipeEnding = Utils.FollowPipe(crossingCmp, direction);
               if((pipeEnding.type == PipeEnding.Type.CROSSING && pipeEnding.endingCell != crossingCmp.crossingCell) ||
                   (pipeEnding.type == PipeEnding.Type.SINK && (flowDirection_URLD == '1')) || (pipeEnding.type == PipeEnding.Type.SOURCE && (flowDirection_URLD == '2')))
               {
                  PUIElements.SetToolTip(toggle_go, MYSTRINGS.SIDESCREEN.DIRECTIONSCREEN.SWITCHDIRECTION);
                  image.SetColorStyle(crossingCmp.isIllegal ? directionsIllegalColor : directionsColor);
                  ktoggle.interactable = true;
                  PToggle.SetToggleState(toggle_go, flowDirection_URLD == '2');// SetToggleState when ktoggle.interactable == true!
               }
               else
               {
                  PUIElements.SetToolTip(toggle_go, MYSTRINGS.SIDESCREEN.DIRECTIONSCREEN.FIXEDDIRECTION);
                  image.SetColorStyle(crossingCmp.isIllegal ? directionsIllegalDisabledColor : directionsDisabledColor);
                  if(!ktoggle.interactable)
                     ktoggle.interactable = true;
                  PToggle.SetToggleState(toggle_go, flowDirection_URLD == '2');// SetToggleState when ktoggle.interactable == true!
                  ktoggle.interactable = false;
               }
            }
         }
         crossingCircleIcon.SetColorStyle(crossingCmp.isIllegal ? directionsIllegalColor : directionsColor);
         //-------------Updating direction panel-------------UP
         //-------------Updating priority panel-------------DOWN
         Image crossingImage = crossingCmp.crossingIcon.GetComponentInChildren<Image>();
         togglesPanelImage.sprite = crossingImage.sprite;
         togglesPanelImage.transform.rotation = crossingImage.transform.rotation;
         Color temp = crossingCmp.isIllegal ? CrossingSprite.highlightedColor : CrossingSprite.normalColor;
         temp.a = 0.7f;
         togglesPanelImage.color = temp;

         Utils.ToUnrotatedCrossingID(crossingCmp.crossingID, out crossing_rotation);
         Quaternion counter_rotation = Quaternion.AngleAxis(crossing_rotation * -90f, Vector3.forward);
         for(sbyte unrotated_i = 0; unrotated_i < 4; unrotated_i++)
         {
            char flowDirection_DLRU = Utils.GetFlowDirection(crossingCmp, unrotated_i);
            int rotated_i = Rotate(unrotated_i, true);
            GameObject toggle_go = priorityToggles_DLRU_unrotated[rotated_i];
            if(flowDirection_DLRU == '0' || !((showInputsPri && !(flowDirection_DLRU == '2')) || (showOutputsPri && (flowDirection_DLRU == '2'))))
            {
               toggle_go.SetActive(false);
               prioritySpacers_DLRU_unrotated[rotated_i].SetActive(true);
            }
            else
            {
               toggle_go.SetActive(true);
               prioritySpacers_DLRU_unrotated[rotated_i].SetActive(false);
            }
            if(flowDirection_DLRU != '0')
            {
               toggle_go.transform.localRotation = counter_rotation;

               sbyte flowPriority = Utils.GetFlowPriority(crossingCmp, unrotated_i);
               if(flowPriority == -1)// set default value
               {
                  PCheckBox.SetCheckState(toggle_go, 1);
               }
               else
               {
                  PCheckBox.SetCheckState(toggle_go, flowPriority);
               }
            }
         }
         if(Utils.TryGetRealEndpointType(crossingCmp, out Endpoint endpoint_type))
         {
            priorityToggles_DLRU_unrotated[4].transform.localRotation = counter_rotation;

            sbyte flowPriority = Utils.GetFlowPriority(crossingCmp, 4);
            if(flowPriority == -1)// set default value
            {
               if(endpoint_type == Endpoint.Sink)
               {
                  PCheckBox.SetCheckState(priorityToggles_DLRU_unrotated[4], 2);
               }
               else
               {
                  PCheckBox.SetCheckState(priorityToggles_DLRU_unrotated[4], 0);
               }
            }
            else
            {
               PCheckBox.SetCheckState(priorityToggles_DLRU_unrotated[4], flowPriority);
            }

            if((showInputsPri && !(endpoint_type == Endpoint.Sink)) || (showOutputsPri && (endpoint_type == Endpoint.Sink)))
            {
               priorityToggles_DLRU_unrotated[4].SetActive(true);
               prioritySpacers_DLRU_unrotated[4].SetActive(false);
            }
            else
            {
               priorityToggles_DLRU_unrotated[4].SetActive(false);
               prioritySpacers_DLRU_unrotated[4].SetActive(true);
            }
         }
         else
         {
            priorityToggles_DLRU_unrotated[4].SetActive(false);
            prioritySpacers_DLRU_unrotated[4].SetActive(true);
         }
         //-------------Updating priority panel-------------UP
         isUpdating = false;
      }

      private int Rotate(int unrotated_DLRU, bool clockwise) {
         if(unrotated_DLRU == 4)
            return 4;// shouldn't rotate that direction
         int[] directions;
         if(clockwise)
            directions = new int[]{ 0, 1, 3, 2 };
         else
            directions = new int[]{ 2, 3, 1, 0 };
         int index = Array.IndexOf(directions, unrotated_DLRU);
         return directions[(index + crossing_rotation) % directions.Length];
      }

      public override bool IsValidForTarget(GameObject target) {
         Conduit crossing = target?.GetComponent<Conduit>();
         return crossing != null && Utils.ConduitTypeToCrossingsSet(crossing.type).Contains(crossing.Cell);
      }

      public override void SetTarget(GameObject target) {
         crossingCmp = target?.GetComponent<CrossingCmp>();
         Utils.ToUnrotatedCrossingID(crossingCmp.crossingID, out crossing_rotation);
         UpdateSideScreen();
      }

      public override void ClearTarget() {
         crossingCmp = null;
      }

      public override int GetSideScreenSortOrder() => -30;

      public override string GetTitle() => MYSTRINGS.SIDESCREEN.TITLE;
   }
}
