using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.PatchManager;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedFlowManagement.Patches {
   class SideScreen_Patches {
      [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
      public static class AddSideScreen_Patch {
         public static void Postfix() {
            string targetClassName = null;
            bool insertBefore = true;
            GameObject uiPrefab = null;

            DetailsScreen instance = DetailsScreen.Instance;
            if(instance == null)
            {
               Debug.LogWarning("DetailsScreen is not yet initialized, try a postfix on DetailsScreen.OnPrefabInit");
               return;
            }

            List<DetailsScreen.SideScreenRef> list = DetailsScreen.Instance.sideScreens;
            GameObject configScreensParent = DetailsScreen.Instance.noConfigSideScreen.gameObject.GetParent();
            string name = typeof(FlowConfigurationSideScreen).Name;
            if(configScreensParent != null && list != null)
            {
               DetailsScreen.SideScreenRef sideScreenRef = new DetailsScreen.SideScreenRef();
               GameObject flowConfigurationSideScreen = PUIElements.CreateUI(configScreensParent, name);
               flowConfigurationSideScreen.AddComponent<BoxLayoutGroup>().Params = new BoxLayoutParams {
                  Direction = PanelDirection.Vertical,
                  Alignment = TextAnchor.UpperCenter,
                  Margin = new RectOffset(1, 1, 0, 1)
               };
               var val = flowConfigurationSideScreen.AddComponent<FlowConfigurationSideScreen>();
               if(uiPrefab != null)
               {
                  val.ContentContainer = uiPrefab;
                  uiPrefab.transform.SetParent(flowConfigurationSideScreen.transform);
               }

               sideScreenRef.name = name;
               sideScreenRef.offset = Vector2.zero;
               sideScreenRef.screenPrefab = val;
               sideScreenRef.screenInstance = val;
               InsertSideScreenContent(list, sideScreenRef, targetClassName, insertBefore);
            }
         }
      }

      private static void InsertSideScreenContent(IList<DetailsScreen.SideScreenRef> screens, DetailsScreen.SideScreenRef newScreen, string targetClassName, bool insertBefore) {
         if(screens == null)
         {
            throw new ArgumentNullException("screens");
         }

         if(newScreen == null)
         {
            throw new ArgumentNullException("newScreen");
         }

         if(string.IsNullOrEmpty(targetClassName))
         {
            screens.Add(newScreen);
            return;
         }

         int count = screens.Count;
         bool flag = false;
         for(int i = 0; i < count; i++)
         {
            DetailsScreen.SideScreenRef sideScreenRef = screens[i];
            SideScreenContent sideScreenContent = sideScreenRef.screenPrefab;
            if(!(sideScreenContent != null))
            {
               continue;
            }

            SideScreenContent[] componentsInChildren = sideScreenContent.GetComponentsInChildren<SideScreenContent>();
            if(componentsInChildren == null || componentsInChildren.Length < 1)
            {
               Debug.LogWarning("Could not find SideScreenContent on side screen: " + sideScreenRef.name);
            }
            else if(componentsInChildren[0].GetType().FullName == targetClassName)
            {
               if(insertBefore)
               {
                  screens.Insert(i, newScreen);
               }
               else if(i >= count - 1)
               {
                  screens.Add(newScreen);
               }
               else
               {
                  screens.Insert(i + 1, newScreen);
               }

               flag = true;
               break;
            }
         }

         if(!flag)
         {
            Debug.LogWarning("No side screen with class name {0} found!".F(targetClassName));
            screens.Add(newScreen);
         }
      }
   }
}
