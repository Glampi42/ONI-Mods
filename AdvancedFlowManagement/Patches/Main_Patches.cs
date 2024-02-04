using AdvancedFlowManagement;
using Database;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using static ConduitFlow;

namespace AdvancedFlowManagement.Patches {
   public class Main_Patches {
      [HarmonyPatch(typeof(Db), "Initialize")]
      public static class Db_Initialize_Patch {
         public static void Postfix() {
            //-------------Saving all crossingIDs and their unrotated versions-------------DOWN
            for(byte id = 0; id < 81; id++)
            {
               if(Utils.CountCrossingConnections(id) > 1)
               {
                  byte smallestnum = id;
                  byte previousnum = id;
                  byte rotation_CCW = 0;
                  for(byte i = 1; i < 4; i++)
                  {
                     byte currentnum = (byte)(previousnum / 3 + (previousnum % 3) * 27);
                     if(currentnum < smallestnum)
                     {
                        smallestnum = currentnum;
                        rotation_CCW = i;
                     }
                     previousnum = currentnum;
                  }
                  Main.allCrossingIDs.Add(byteDLUR_To_stringDLRU(id), (byteDLUR_To_stringDLRU(smallestnum), rotation_CCW));
               }
            }
            //-------------Saving all crossingIDs and their unrotated versions-------------UP

            MYSPRITES.SaveSprite("afm_input_round");
            MYSPRITES.SaveSprite("afm_output_round");
            MYSPRITES.SaveSprite("afm_crossing_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingThick_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingInput_U_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingOutput_U_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingInput_R_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingOutput_R_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingInput_L_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingOutput_L_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingInput_D_ui", "/ui");
            MYSPRITES.SaveSprite("afm_crossingOutput_D_ui", "/ui");
            foreach(string id in Main.allCrossingIDs.Keys)
            {
               if(Main.allCrossingIDs[id].Item2 == 0)// if isUnrotated
                  MYSPRITES.SaveSprite("afm_crossing_" + id, "/crossings");
            }

            string byteDLUR_To_stringDLRU(byte DLUR) {
               byte d = (byte)(DLUR / 27 % 3);
               byte l = (byte)(DLUR / 9 % 3);
               byte u = (byte)(DLUR / 3 % 3);
               byte r = (byte)(DLUR % 3);
               return d.ToString() + l.ToString() + r.ToString() + u.ToString();
            }
         }
      }

      [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
      public static class SaveGame_OnPrefabInit_Patch {
         public static void Postfix(SaveGame __instance) {
            __instance.gameObject.AddOrGet<SaveModState>();
         }
      }

      [HarmonyPatch(typeof(Game), "OnSpawn")]
      public static class Game_OnSpawn_Patch {
         public static void Postfix() {
            CrossingSprite.CreateCrossingIconPrefab();
         }
      }

      [HarmonyPatch(typeof(Game), "OnDestroy")]
      public static class Game_OnDestroy_Patch {
         public static void Postfix() {
            Main.crossings_liquid.Clear();
            Main.crossings_gas.Clear();
            Main.buffers_liquid.Clear();
            Main.buffers_gas.Clear();
            Main.customFlowConduits_liquid.Clear();
            Main.customFlowConduits_gas.Clear();
            Main.endpoints_liquid.Clear();
            Main.endpoints_gas.Clear();
            Main.crossingsNetworks_liquid.Clear();
            Main.crossingsNetworks_gas.Clear();
            Main.showCrossings_Liquid = true;
            Main.showCrossings_Gas = true;
            Main.deserializedCmps_liquid = false;
            Main.deserializedCmps_gas = false;
            CrossingSprite.crossingIconPrefab = null;
         }
      }

      [HarmonyPatch(typeof(ConduitFlow), "OnUtilityNetworksRebuilt")]
      public static class OnNetworksRebuilt_Patch {
         public static void Postfix(IList<UtilityNetwork> networks/*as far as I'm concerned, these networks are the same as __instance.networks, but I'm not 100% sure*/,
            ICollection<int> root_nodes, ConduitFlow __instance) {
            bool ignoreUpdate = __instance.soaInfo.NumEntries == 0;

            if(!ignoreUpdate)
            {
               // registering & unregistering crossings, redirecting flow directions:
               CrossingsUpdates_Patches.PostProcessNetworksRebuild(root_nodes, __instance);

               FlowPriorityManagement_Patches.RecalculateUpdateOrder(__instance.networks, __instance);
               FlowPriorityManagement_Patches.SaveCustomFlowConduits(__instance.networks, __instance);
            }
         }
      }

      [HarmonyPatch(typeof(BuildingStatusItems), "CreateStatusItems")]
      public static class CreateBufferContentsSI_Patch {
         public static void Postfix() {
            Main.bufferContentsSI = new StatusItem("BufferStorage", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.LiquidConduits.ID);
            Main.bufferContentsSI.resolveStringCallback = (str, data) => {
               BufferStorageCmp bufferStorageCmp = data as BufferStorageCmp;
               if(bufferStorageCmp == null)
                  return str;

               if(!Utils.ConduitTypeToBuffersSet(bufferStorageCmp.conduitType).Contains(bufferStorageCmp.conduitCell))
                  return str;

               ConduitContents bufferContents = bufferStorageCmp.bufferStorage[0];
               string newValue = (string)global::STRINGS.BUILDING.STATUSITEMS.PIPECONTENTS.EMPTY;
               if((double)bufferContents.mass > 0.0)
               {
                  Element elementByHash = ElementLoader.FindElementByHash(bufferContents.element);
                  newValue = string.Format((string)global::STRINGS.BUILDING.STATUSITEMS.PIPECONTENTS.CONTENTS, (object)GameUtil.GetFormattedMass(bufferContents.mass), (object)elementByHash.name, (object)GameUtil.GetFormattedTemperature(bufferContents.temperature));
                  if(OverlayScreen.Instance != null && OverlayScreen.Instance.mode == OverlayModes.Disease.ID && bufferContents.diseaseIdx != byte.MaxValue)
                     newValue += string.Format((string)global::STRINGS.BUILDING.STATUSITEMS.PIPECONTENTS.CONTENTS_WITH_DISEASE, (object)GameUtil.GetFormattedDisease(bufferContents.diseaseIdx, bufferContents.diseaseCount, true));
               }
               str = str.Replace("{Contents}", newValue);
               return str;
            };
            Main.bufferContentsSI.resolveTooltipCallback = (str, data) => {
               BufferStorageCmp bufferStorageCmp = data as BufferStorageCmp;
               if(bufferStorageCmp == null)
                  return str;

               if(!Utils.ConduitTypeToBuffersSet(bufferStorageCmp.conduitType).Contains(bufferStorageCmp.conduitCell))
                  return str;

               string contents = "";
               for(int i = 0; i < bufferStorageCmp.bufferStorage.Length; i++)
               {
                  ConduitContents bufferContents = bufferStorageCmp.bufferStorage[i];
                  string newValue = (string)global::STRINGS.BUILDING.STATUSITEMS.PIPECONTENTS.EMPTY;
                  if((double)bufferContents.mass > 0.0)
                  {
                     Element elementByHash = ElementLoader.FindElementByHash(bufferContents.element);
                     newValue = string.Format((string)global::STRINGS.BUILDING.STATUSITEMS.PIPECONTENTS.CONTENTS, (object)GameUtil.GetFormattedMass(bufferContents.mass), (object)elementByHash.name, (object)GameUtil.GetFormattedTemperature(bufferContents.temperature));
                     if(OverlayScreen.Instance != null && OverlayScreen.Instance.mode == OverlayModes.Disease.ID && bufferContents.diseaseIdx != byte.MaxValue)
                        newValue += string.Format((string)global::STRINGS.BUILDING.STATUSITEMS.PIPECONTENTS.CONTENTS_WITH_DISEASE, (object)GameUtil.GetFormattedDisease(bufferContents.diseaseIdx, bufferContents.diseaseCount, true));

                     contents += "\n" + newValue;
                  }
                  else if(i == 0)
                  {
                     contents += "\n" + newValue;
                  }
               }
               str = str.Replace("{Contents}", contents);
               return str;
            };
         }
      }
   }
}
