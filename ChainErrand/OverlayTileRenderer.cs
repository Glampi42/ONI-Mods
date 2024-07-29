using HarmonyLib;
using Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Rendering.BlockTileRenderer;

namespace ChainErrand {
   /// <summary>
   /// This class is responsible for making tiles appear normal color when the ChainOverlay is on.<br></br>
   /// It works by creating custom RenderInfo for each type of tile which renders them on the masked layer.
   /// </summary>
   public static class OverlayTileRenderer {
      public static Dictionary<KeyValuePair<BuildingDef, RenderInfoLayer>, RenderInfo> overlayRenderInfos = new();

      public static void RenderTile(int cell, BuildingDef tileDef, bool isReplacement, SimHashes element) {
         KeyValuePair<BuildingDef, RenderInfoLayer> key = new KeyValuePair<BuildingDef, RenderInfoLayer>(tileDef, GetRenderInfoLayer(isReplacement, element));
         
         if(!overlayRenderInfos.TryGetValue(key, out RenderInfo renderInfo))
         {
            renderInfo = new RenderInfo(World.Instance.blockTileRenderer, isReplacement ? (int)tileDef.ReplacementLayer : (int)tileDef.TileLayer,
               Main.chainOverlay.targetLayer, tileDef, element);
            overlayRenderInfos.Add(key, renderInfo);
         }
         if(!renderInfo.occupiedCells.ContainsKey(cell))
         {
            renderInfo.AddCell(cell);
         }
      }

      public static void UnrenderTile(int cell, BuildingDef tileDef, bool isReplacement, SimHashes element) {
         if(!overlayRenderInfos.TryGetValue(new KeyValuePair<BuildingDef, RenderInfoLayer>(tileDef, GetRenderInfoLayer(isReplacement, element)), out RenderInfo renderInfo))
            return;

         renderInfo.RemoveCell(cell);
      }


      public static void FreeResources() {
         foreach(var renderInfo in overlayRenderInfos.Values)
         {
            if(renderInfo != null)
               renderInfo.FreeResources();
         }
         overlayRenderInfos.Clear();
      }

      [HarmonyPatch(typeof(BlockTileRenderer), "Render")]
      public static class BlockTileRenderer_Render_Patch {
         public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            List<CodeInstruction> codesCluster = new List<CodeInstruction>();
            codesCluster.Add(new CodeInstruction(OpCodes.Ldarg_0));// load BlockTileRenderer
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 7));// load x
            codesCluster.Add(new CodeInstruction(OpCodes.Ldloc_S, 6));// load y
            codesCluster.Add(new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => RenderOverlayTiles(default, default, default))));

            int afterRenderIndex = -1;
            for(int i = 0; i < codes.Count; i++)
            {
               if(codes[i].Calls(SymbolExtensions.GetMethodInfo(() => ((RenderInfo)default).Render(default, default))))
               {
                  afterRenderIndex = i + 1;
                  break;
               }
            }
            if(afterRenderIndex == -1)
               throw new Exception(Main.debugPrefix + "Render() method could not be found");

            codes.InsertRange(afterRenderIndex, codesCluster);

            return codes.AsEnumerable();
         }
      }
      private static void RenderOverlayTiles(BlockTileRenderer tileRenderer, int chunk_x, int chunk_y) {
         if(World.Instance.blockTileRenderer != tileRenderer)
            return;// just for safety

         foreach(var renderInfo in overlayRenderInfos.Values)
         {
            if(renderInfo != null)
            {
               renderInfo.Rebuild(tileRenderer, chunk_x, chunk_y, MeshUtil.vertices, MeshUtil.uvs, MeshUtil.indices, MeshUtil.colours);
               renderInfo.Render(chunk_x, chunk_y);
            }
         }
      }

      [HarmonyPatch(typeof(BlockTileRenderer), "Rebuild")]
      public static class BlockTileRenderer_Rebuild_Patch {
         public static void Postfix(ObjectLayer layer, int cell, BlockTileRenderer __instance) {
            if(World.Instance.blockTileRenderer != __instance)
               return;// just for safety

            foreach(var keyValuePair in overlayRenderInfos)
            {
               if(keyValuePair.Key.Key.TileLayer == layer)
                  keyValuePair.Value.MarkDirty(cell);
            }
         }
      }
   }
}
