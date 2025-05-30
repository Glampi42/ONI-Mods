﻿using ErrandNotifier.NotifiableErrandPacks;
using ErrandNotifier.Custom;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier {
   public class MyUserMod2 : UserMod2 {
      public override void OnLoad(Harmony harmony) {
         base.OnLoad(harmony);

         PUtil.InitLibrary();

         Main.notifierTool_binding = new PActionManager().CreateAction("glampi.NotifierTool", (LocString)"NotifierTool", new PKeyBinding(KKeyCode.N, Modifier.Shift));
         new POptions().RegisterOptions(this, typeof(ModConfig));

         // patching the pathes defined in NotifiableErrandPacks:
         foreach(var pack in NotifiableErrandPackRegistry.AllPacks())
         {
            var deletePatches = pack.OnChoreDelete_Patch();
            if(deletePatches != null)
            {
               foreach(var patch in deletePatches)
               {
                  TryPatch(harmony, patch);
               }
            }
         }
      }
      private static void TryPatch(Harmony harmony, GPatchInfo patchInfo) {
         try
         {
            harmony.Patch(patchInfo.patchedMethod, patchInfo.prefix, patchInfo.postfix, patchInfo.transpiler);
         }
         catch(Exception ex)
         {
            Debug.LogError(Main.debugPrefix + $"Could not patch method {patchInfo.patchedMethod.FullDescription()};");
            Debug.LogError(Main.debugPrefix + $"Prefix that couldn't be patched: {patchInfo.prefix?.method.FullDescription() ?? "null"}");
            Debug.LogError(Main.debugPrefix + $"Postfix that couldn't be patched: {patchInfo.postfix?.method.FullDescription() ?? "null"}");
            Debug.LogError(Main.debugPrefix + $"Transpiler that couldn't be patched: {patchInfo.transpiler?.method.FullDescription() ?? "null"}");
            throw ex;
         }
      }
   }
}
